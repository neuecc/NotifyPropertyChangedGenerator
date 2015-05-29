// used from NotifyPropertyChangedGenerator

using System;

namespace $rootnamespace$
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    internal sealed class NotifyAttribute : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    internal sealed class NonNotifyAttribute : Attribute
    {

    }
}