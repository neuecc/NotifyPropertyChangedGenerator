using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NotifyPropertyChangedGenerator
{
    internal class Property
    {
        public PropertyDeclarationSyntax Syntax { get; }

        public string PropertyName => propertyName ?? (propertyName = Syntax.Identifier.ToString());
        private string propertyName;

        public string FieldName => fieldName ?? (fieldName = GetFieldName());
        private string fieldName;

        private string GetFieldName()
        {
            var propName = PropertyName;
            var fieldName = Char.ToLower(propName[0]) + propName.Substring(1);

            switch (NamingConvention)
            {
                default:
                case NamingConvention.Plain: return fieldName;
                case NamingConvention.LeadingUnderscore: return "_" + fieldName;
                case NamingConvention.TrailingUnderscore: return fieldName + "_";
            }
        }

        public NamingConvention NamingConvention => namingConvension ?? (namingConvension = GetNamingConvention()).Value;

        private NamingConvention? namingConvension;

        private NamingConvention GetNamingConvention()
        {
            var a = PropertyAttribute ?? ClassAttribute;
            if (a == null) return NamingConvention.Plain;

            var s = a.ToString();
            return
                s.Contains(nameof(NamingConvention.LeadingUnderscore)) ? NamingConvention.LeadingUnderscore :
                s.Contains(nameof(NamingConvention.TrailingUnderscore)) ? NamingConvention.TrailingUnderscore :
                NamingConvention.Plain;
        }

        private AttributeSyntax PropertyAttribute => propertyAttribute ?? (propertyAttribute = Syntax.GetNotifyAttribute());
        private AttributeSyntax propertyAttribute;

        private AttributeSyntax ClassAttribute { get; }

        public Property(PropertyDeclarationSyntax syntax, AttributeSyntax attribute = null, AttributeSyntax classAttribute = null)
        {
            Syntax = syntax;
            propertyAttribute = attribute;
            ClassAttribute = classAttribute;
        }
    }
}
