using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace NotifyPropertyChangedGenerator
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NotifyPropertyChangedGeneratorCodeFixProvider)), Shared]
    public class NotifyPropertyChangedGeneratorCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(NotifyPropertyChangedGeneratorDiagnosticAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        // Add using System.ComponentModel;
        // Add INotifyPropertyChanged
        // Remove PropertyChangedEventHandler
        // Replace All notify property
        // Replace NotifyPropertyChangedGenerator region(with PropertyChangedEventHandler)

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false) as CompilationUnitSyntax;
            var model = await context.Document.GetSemanticModelAsync();

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var classDeclaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().First();
            
            HashSet<MemberDeclarationSyntax> deleteTargets;
            var region = classDeclaration.GetRegion("NotifyPropertyChangedGenerator");
            if (region == null)
            {
                deleteTargets = new HashSet<MemberDeclarationSyntax>();
            }
            else
            {
                var start = region.SpanStart;
                var endRegion = region.GetNextDirective();
                var end = endRegion.SpanStart;

                deleteTargets = new HashSet<MemberDeclarationSyntax>(
                    classDeclaration.Members.Where(x => start <= x.SpanStart && x.SpanStart <= end));
            }

            var notifyProperties = Utility.GetExpandableProperties(classDeclaration, model).ToDictionary(x => x.Syntax);

            var newMemberList = new List<MemberDeclarationSyntax>();
            foreach (var originalMember in classDeclaration.Members)
            {
                var ev = originalMember as EventFieldDeclarationSyntax;
                if (ev != null && ev.Declaration.ToString() == "PropertyChangedEventHandler PropertyChanged")
                {
                    continue;
                }
                if (deleteTargets.Contains(originalMember))
                {
                    continue;
                }
                var originalProperty = originalMember as PropertyDeclarationSyntax;
                if (originalProperty == null || !notifyProperties.ContainsKey(originalProperty))
                {
                    newMemberList.Add(originalMember);
                    continue;
                }
                var modifier = originalProperty.Modifiers.First(y =>
                {
                    var kind = y.Kind();
                    return (kind == SyntaxKind.PublicKeyword || kind == SyntaxKind.PrivateKeyword || kind == SyntaxKind.ProtectedKeyword || kind == SyntaxKind.InternalKeyword);
                });

                var p = notifyProperties[originalProperty];
                var propName = p.PropertyName;
                var fieldName = p.FieldName;

                // Check the access level of the set accessor.
                var setAccessor = originalProperty.AccessorList.Accessors.SingleOrDefault(accessor => accessor.IsKind(SyntaxKind.SetAccessorDeclaration));
                var setAccessorAccessLevel = (setAccessor?.Modifiers.Any(modifier_ => modifier_.IsKind(SyntaxKind.PrivateKeyword)) ?? true) ? "private " : "";

                var memberTree = CSharpSyntaxTree.ParseText($"{modifier} {originalProperty.Type.ToString()} {originalProperty.Identifier.ToString()} {{ get {{ return {fieldName}; }} {setAccessorAccessLevel}set {{ SetProperty(ref {fieldName}, value, {fieldName}PropertyChangedEventArgs); }} }}\r\n");
                var newMember = memberTree.GetRoot().ChildNodes().OfType<PropertyDeclarationSyntax>().First()
                    .WithAttributeLists(originalProperty.AttributeLists)
                    .WithTriviaFrom(originalProperty)
                    .WithAdditionalAnnotations(Formatter.Annotation);
                newMemberList.Add(newMember);
            }

            // Generate Region
            var fieldBuilder = new StringBuilder();
            foreach (var x in notifyProperties.Values)
            {
                var propName = x.PropertyName;
                var fieldName = x.FieldName;

                fieldBuilder.AppendLine($"private {x.Syntax.Type.ToString()} {fieldName};");
                fieldBuilder.AppendLine($"private static readonly PropertyChangedEventArgs {fieldName}PropertyChangedEventArgs = new PropertyChangedEventArgs(nameof({propName}));");
            }

            var comp = classDeclaration.GetCompareMethod();

            const string setPropertyMethodEqualityComparer = @"
private void SetProperty<T>(ref T field, T value, PropertyChangedEventArgs ev)
{
    if (!System.Collections.Generic.EqualityComparer<T>.Default.Equals(field, value))
    {
        field = value;
        PropertyChanged?.Invoke(this, ev);
    }
}
";

            const string setPropertyMethodReferenceEquals = @"
private void SetProperty<T>(ref T field, T value, PropertyChangedEventArgs ev)
{
    if (!object.ReferenceEquals(field, value))
    {
        field = value;
        PropertyChanged?.Invoke(this, ev);
    }
}
";

            const string setPropertyMethodNone = @"
private void SetProperty<T>(ref T field, T value, PropertyChangedEventArgs ev)
{
    field = value;
    PropertyChanged?.Invoke(this, ev);
}
";

            var setPropertyMethod =
                comp == CompareMethod.EqualityComparer ? setPropertyMethodEqualityComparer :
                comp == CompareMethod.ReferenceEquals ? setPropertyMethodReferenceEquals :
                setPropertyMethodNone;

            var regionSource = $@"
#region NotifyPropertyChangedGenerator

public event PropertyChangedEventHandler PropertyChanged;

{fieldBuilder.ToString()}{setPropertyMethod}

#endregion";

            var regionTree = CSharpSyntaxTree.ParseText(regionSource).GetRoot();
            var newRegionMembers = regionTree.ChildNodes()
                .OfType<MemberDeclarationSyntax>()
                .Select(x => x.WithAdditionalAnnotations(Formatter.Annotation));

            newMemberList.AddRange(newRegionMembers);

            context.RegisterCodeFix(
                CodeAction.Create("Generate NotifyProperty", new Func<CancellationToken, Task<Document>>(c =>
                {
                    var newCloseBraceToken = classDeclaration.CloseBraceToken
                        .WithLeadingTrivia(
                            SyntaxFactory.CarriageReturnLineFeed,
                            regionTree.DescendantTrivia().First(x => x.IsKind(SyntaxKind.EndRegionDirectiveTrivia)),
                            SyntaxFactory.CarriageReturnLineFeed);
                    
                    var newClassDeclaration = classDeclaration
                        .WithMembers(SyntaxFactory.List<MemberDeclarationSyntax>(newMemberList))
                        .WithCloseBraceToken(newCloseBraceToken)
                        .WithINotifyPropertyChangedInterface();

                    var newRoot = root.ReplaceNode(classDeclaration, newClassDeclaration)
                        .WithUsing("System.ComponentModel")
                        .WithAdditionalAnnotations(Formatter.Annotation);
                    var newDocument = context.Document.WithSyntaxRoot(newRoot);

                    return Task.FromResult(newDocument);
                })),
                diagnostic);
        }
    }
}