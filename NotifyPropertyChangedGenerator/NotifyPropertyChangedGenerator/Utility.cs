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
        internal static Property[] GetExpandableProperties(ClassDeclarationSyntax classDeclaration, SemanticModel model)
        {
            var classAttribute = classDeclaration.GetNotifyAttribute();
            var isClassNotify = classAttribute != null;

            Property[] notifyProperties;
            if (isClassNotify)
            {
                notifyProperties = (from member in classDeclaration.Members.OfType<PropertyDeclarationSyntax>()
                                    where member.GetNonNotifyAttribute() == null
                                    select new Property(member, classAttribute: classAttribute)).ToArray();
            }
            else
            {
                notifyProperties = (from member in classDeclaration.Members.OfType<PropertyDeclarationSyntax>()
                                    let attribute = member.GetNotifyAttribute()
                                    where attribute != null
                                    select new Property(member, attribute)).ToArray();
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
                var inpc = SyntaxFactory.ParseTypeName("System.ComponentModel.INotifyPropertyChanged")
                    .WithAdditionalAnnotations(Simplifier.Annotation)
                    .WithAdditionalAnnotations(Formatter.Annotation);

                var decl = classDeclaration.AddBaseListTypes(SyntaxFactory.SimpleBaseType(inpc));
                return decl.WithIdentifier(decl.Identifier.WithTrailingTrivia());
            }
            else
            {
                return classDeclaration;
            }
        }
    }
}