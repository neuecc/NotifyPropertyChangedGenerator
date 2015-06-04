using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NotifyPropertyChangedGenerator
{
    internal static class DeclarationSyntaxExtensions
    {
        public static AttributeSyntax GetNotifyAttribute(this PropertyDeclarationSyntax p) => p.AttributeLists.GetAttribute("Notify");
        public static AttributeSyntax GetNotifyAttribute(this ClassDeclarationSyntax p) => p.AttributeLists.GetAttribute("Notify");

        public static AttributeSyntax GetNonNotifyAttribute(this PropertyDeclarationSyntax p) => p.AttributeLists.GetAttribute("NonNotify");
        public static AttributeSyntax GetNonNotifyAttribute(this ClassDeclarationSyntax p) => p.AttributeLists.GetAttribute("NonNotify");

        public static AttributeSyntax GetAttribute(this SyntaxList<AttributeListSyntax> attributes, string attributeName)
            => attributes
                .SelectMany(a => a.Attributes)
                .FirstOrDefault(a =>
                {
                    var name = a.Name.ToString();
                    return name == attributeName || name == attributeName + "Attribute";
                });
    }
}
