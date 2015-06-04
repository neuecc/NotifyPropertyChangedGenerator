using System;
using System.Diagnostics;

namespace NotifyPropertyChangedGenerator.Annotations
{
    /// <summary>
    /// Annotates a property or all of properties in a class as included to the code generation for the NotifyPropertyChanged.
    /// </summary>
    [Conditional("NEVER_USED_AT_RUNTIME")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class NotifyAttribute : Attribute
    {
        /// <summary>
        /// No option.
        /// </summary>
        public NotifyAttribute() { }

        /// <summary>
        /// Specify options as string.
        /// </summary>
        /// <param name="namingConvention"></param>
        /// <param name="compareMethod"></param>
        public NotifyAttribute(string namingConvention = null, string compareMethod = null) { }

        /// <summary>
        /// Specify options as enum.
        /// </summary>
        /// <param name="namingConvention"></param>
        /// <param name="compareMethod"></param>
        public NotifyAttribute(NamingConvention namingConvention = default(NamingConvention), CompareMethod compareMethod = default(CompareMethod)) { }
    }

    /// <summary>
    /// Annotates a property as exluded from the code generation for the NotifyPropertyChanged.
    /// </summary>
    [Conditional("NEVER_USED_AT_RUNTIME")]
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class NonNotifyAttribute : Attribute
    {

    }
}
