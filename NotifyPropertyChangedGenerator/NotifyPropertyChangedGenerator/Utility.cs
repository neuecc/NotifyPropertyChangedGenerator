using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;

namespace NotifyPropertyChangedGenerator
{
    internal static class Utility
    {
        internal static PropertyDeclarationSyntax[] GetExpandableProperties(ClassDeclarationSyntax classDeclaration, SemanticModel model)
        {
            var isClassNotify = classDeclaration.AttributeLists
                .SelectMany(x => x.Attributes)
                .Any(x => model.GetTypeInfo(x).Type?.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat) == "NotifyAttribute");

            PropertyDeclarationSyntax[] notifyProperties;
            if (isClassNotify)
            {
                notifyProperties = (from member in classDeclaration.Members.OfType<PropertyDeclarationSyntax>()
                                    where !(from attributeList in member.AttributeLists
                                            from attribute in attributeList.Attributes
                                            where model.GetTypeInfo(attribute).Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat) == "NonNotifyAttribute"
                                            select new object())
                                           .Any()
                                    select member).ToArray();
            }
            else
            {
                notifyProperties = (from member in classDeclaration.Members.OfType<PropertyDeclarationSyntax>()
                                    from attributeList in member.AttributeLists
                                    from attribute in attributeList.Attributes
                                    where model.GetTypeInfo(attribute).Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat) == "NotifyAttribute"
                                    select member).ToArray();
            }

            return notifyProperties;
        }

        internal static RegionDirectiveTriviaSyntax GetRegion(this ClassDeclarationSyntax classDeclaration, string regionName)
        {
            var region = classDeclaration.DescendantTrivia()
                .Where(x => x.IsKind(SyntaxKind.RegionDirectiveTrivia))
                .Select(x => x.GetStructure() as RegionDirectiveTriviaSyntax)
                .FirstOrDefault(x => x.ToString().Contains(regionName));

            return region;
        }

        internal static CompilationUnitSyntax WithUsing(this CompilationUnitSyntax root, string name)
        {
            if (!root.Usings.Any(u => u.Name.ToString() == name))
            {
                root = root.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(name)).WithAdditionalAnnotations(Formatter.Annotation));
            }

            return root;
        }

        internal static ClassDeclarationSyntax WithINotifyPropertyChangedInterface(this ClassDeclarationSyntax classDeclaration)
        {

            if (!(classDeclaration.BaseList?.Types.Any(x =>
            {
                var t = x.Type.ToString();
                return t == "INotifyPropertyChanged" || t == "System.ComponentModel.INotifyPropertyChanged";
            }) ?? false))
            {
                var decl = classDeclaration.AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("System.ComponentModel.INotifyPropertyChanged")))
                    .WithAdditionalAnnotations(Simplifier.Annotation)
                    .WithAdditionalAnnotations(Formatter.Annotation);
                return decl.WithIdentifier(decl.Identifier.WithTrailingTrivia());
            }
            else
            {
                return classDeclaration;
            }
        }
    }
}