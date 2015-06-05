// used from NotifyPropertyChangedGenerator

using System;
using System.Diagnostics;

namespace $rootnamespace$
{
    [Conditional("NEVER_USED_AT_RUNTIME")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    internal sealed class NotifyAttribute : Attribute,
        // default option, you can customize default naming convention
        NotifyAttribute.IPlain
    {
        // naming convention markers
        internal interface IPlain { }
        internal interface ILeadingUnderscore { }
        internal interface ITrailingUnderscore { }

        /// <summary>
        /// No option.
        /// </summary>
        public NotifyAttribute() { }

        /// <summary>
        /// Specify options as string.
        /// </summary>
        /// <param name="namingConvention">Backing field naming : 'Plain' or 'LeadingUnderscore' or 'TrailingUnderscore'</param>
        /// <param name="compareMethod">Comppare kind for raise property changed : 'None' or 'ReferenceEquals' or 'EqualityComparer'</param>
        public NotifyAttribute(string namingConvention = null, string compareMethod = null) { }
    }

    [Conditional("NEVER_USED_AT_RUNTIME")]
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    internal sealed class NonNotifyAttribute : Attribute
    {

    }
}