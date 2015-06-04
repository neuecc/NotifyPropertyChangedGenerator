using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NotifyPropertyChangedGenerator
{
    internal static class DeclarationSyntaxExtensions
    {
        public static AttributeSyntax GetNotifyAttribute(this PropertyDeclarationSyntax p) => p.AttributeLists.GetAttribute("Notify");
        public static AttributeSyntax GetNotifyAttribute(this ClassDeclarationSyntax c) => c.AttributeLists.GetAttribute("Notify");

        public static AttributeSyntax GetNonNotifyAttribute(this PropertyDeclarationSyntax p) => p.AttributeLists.GetAttribute("NonNotify");
        public static AttributeSyntax GetNonNotifyAttribute(this ClassDeclarationSyntax c) => c.AttributeLists.GetAttribute("NonNotify");

        public static AttributeSyntax GetAttribute(this SyntaxList<AttributeListSyntax> attributes, string attributeName)
            => attributes
                .SelectMany(a => a.Attributes)
                .FirstOrDefault(a =>
                {
                    var name = a.Name.ToString();
                    return name == attributeName || name == attributeName + "Attribute";
                });

        public static CompareMethod GetCompareMethod(this ClassDeclarationSyntax c)
        {
            var a = c.GetNotifyAttribute();
            if (a == null) return CompareMethod.EqualityComparer;

            var s = a.ToString();
            return
                s.Contains(nameof(CompareMethod.None)) ? CompareMethod.None :
                s.Contains(nameof(CompareMethod.ReferenceEquals)) ? CompareMethod.ReferenceEquals :
                CompareMethod.EqualityComparer;
        }
    }
}
