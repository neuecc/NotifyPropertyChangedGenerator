using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NotifyPropertyChangedGenerator
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NotifyPropertyChangedGeneratorDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "NotifyPropertyChangedGenerator";

        internal const string Title = "Generate notify property from NotifyAttribute.";
        internal const string MessageFormat = "Notify property is not generated yet.";
        internal const string Description = "Notify property is not generated yet.";
        internal const string Category = "Refactoring";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeSymbol, SyntaxKind.ClassDeclaration);
        }

        private static void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
        {
            var model = context.SemanticModel;
            var classDeclaration = context.Node as ClassDeclarationSyntax;

            var notifyProperties = Utility.GetExpandableProperties(classDeclaration, model);
            if (notifyProperties.Length == 0) return; // OK, is not Notify Target

            // has interface?
            if (!(classDeclaration.BaseList?.Types.Any(x =>
            {
                var t = x.Type.ToString();
                return t == "INotifyPropertyChanged" || t == "System.ComponentModel.INotifyPropertyChanged";
            }) ?? false))
            {
                goto REPORT_DIAGNOSTIC;
            }

            var region = classDeclaration.GetRegion("NotifyPropertyChangedGenerator");

            if (region == null) goto REPORT_DIAGNOSTIC;

            var start = region.SpanStart;
            var end = region.GetNextDirective().SpanStart;

            var fieldsInRegion = classDeclaration.Members
                .OfType<FieldDeclarationSyntax>()
                .Where(x => start <= x.SpanStart && x.SpanStart <= end)
                .ToArray();

            var methodsInRegion = classDeclaration.Members
                .OfType<MethodDeclarationSyntax>()
                .Where(x => start <= x.SpanStart && x.SpanStart <= end)
                .ToArray();

            if (!methodsInRegion.Where(x => x.Identifier.ToString() == "SetProperty").Any())
            {
                goto REPORT_DIAGNOSTIC;
            }

            var idInRegion = fieldsInRegion.Select(x =>
            {
                var isEventArgs = x.Declaration.Type.ToString() == "PropertyChangedEventArgs";
                if (isEventArgs)
                {
                    var len = "PropertyChangedEventArgs".Length;
                    var eventArgs = x.Declaration.Variables[0].Identifier.ToString();
                    var trimed = eventArgs.Substring(0, eventArgs.Length - len);
                    return trimed;
                }
                else
                {
                    return x.Declaration.Variables[0].Identifier.ToString();
                }
            }).Select(x => x.TrimStart('_').TrimEnd('_'))
            .Distinct().ToDictionary(x => x, StringComparer.OrdinalIgnoreCase);

            foreach (var item in notifyProperties)
            {
                if (!idInRegion.ContainsKey(item.Syntax.Identifier.ToString())) goto REPORT_DIAGNOSTIC;
            }

            return; // OK

            REPORT_DIAGNOSTIC:
            var diagnostic = Diagnostic.Create(Rule, classDeclaration.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}