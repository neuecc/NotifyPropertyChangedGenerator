// used from NotifyPropertyChangedGenerator

using System;
using System.Diagnostics;

namespace $rootnamespace$
{
    [Conditional("NEVER_USED_AT_RUNTIME")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    internal sealed class NotifyAttribute : Attribute
    {

    }

    [Conditional("NEVER_USED_AT_RUNTIME")]
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    internal sealed class NonNotifyAttribute : Attribute
    {

    }
}