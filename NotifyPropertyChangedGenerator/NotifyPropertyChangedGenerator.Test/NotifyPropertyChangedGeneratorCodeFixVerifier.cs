using NotifyPropertyChangedGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;

namespace NotifyPropertyChangedGenerator.Test
{
    [TestClass]
    public class NotifyPropertyChangedGeneratorCodeFixVerifier : CodeFixVerifier
    {
        readonly DiagnosticResult Expected = new DiagnosticResult
        {
            Id = NotifyPropertyChangedGeneratorDiagnosticAnalyzer.DiagnosticId,
            Message = String.Format("Notify property is not generated yet."),
            Severity = DiagnosticSeverity.Error,
            Locations = new[] { new DiagnosticResultLocation("Test0.cs", 16, 1) }
        };

        const string Attr = @"using System;
using System.ComponentModel;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
internal sealed class NotifyAttribute : Attribute
{

}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
internal sealed class NonNotifyAttribute : Attribute
{

}

";

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new NotifyPropertyChangedGeneratorDiagnosticAnalyzer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new NotifyPropertyChangedGeneratorCodeFixProvider();
        }

        [TestMethod]
        public void SuccessForPropertyDecr()
        {
            VerifyCSharpDiagnostic(Attr + @"public class MyClass : INotifyPropertyChanged
{
    [Notify]
    public int MyProperty { get { return myProperty; } set { SetProperty(ref myProperty, value, myPropertyPropertyChangedEventArgs); } }
    [NotifyAttribute]
    public int MyProperty2 { get { return myProperty2; } set { SetProperty(ref myProperty2, value, myProperty2PropertyChangedEventArgs); } }
    // NonNotify
    public int MyProperty3 { get; set; }

    public MyClass()
    {

    }

    public void Method()
    {
    }

    #region NotifyPropertyChangedGenerator

    public event PropertyChangedEventHandler PropertyChanged;

    private int myProperty;
    private static readonly PropertyChangedEventArgs myPropertyPropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(MyProperty));
    private int myProperty2;
    private static readonly PropertyChangedEventArgs myProperty2PropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(MyProperty2));

    private void SetProperty<T>(ref T field, T value, PropertyChangedEventArgs ev)
    {
        if (!System.Collections.Generic.EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            PropertyChanged?.Invoke(this, ev);
        }
    }

    #endregion
}");
        }

        [TestMethod]
        public void SuccessForClassDecr()
        {
            VerifyCSharpDiagnostic(Attr + @"[Notify]
public class MyClass : INotifyPropertyChanged
{
    public int MyProperty { get { return myProperty; } set { SetProperty(ref myProperty, value, myPropertyPropertyChangedEventArgs); } }
    public int MyProperty2 { get { return myProperty2; } set { SetProperty(ref myProperty2, value, myProperty2PropertyChangedEventArgs); } }
    [NonNotify]
    public int MyProperty3 { get; set; }

    public MyClass()
    {

    }

    public void Method()
    {
    }

    #region NotifyPropertyChangedGenerator

    public event PropertyChangedEventHandler PropertyChanged;

    private int myProperty;
    private static readonly PropertyChangedEventArgs myPropertyPropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(MyProperty));
    private int myProperty2;
    private static readonly PropertyChangedEventArgs myProperty2PropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(MyProperty2));

    private void SetProperty<T>(ref T field, T value, PropertyChangedEventArgs ev)
    {
        if (!System.Collections.Generic.EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            PropertyChanged?.Invoke(this, ev);
        }
    }

    #endregion
}");
        }

        [TestMethod]
        public void DoesNotHaveInterface()
        {
            var source = Attr + @"public class MyClass
{
    [Notify]
    public int MyProperty { get { return myProperty; } set { SetProperty(ref myProperty, value, myPropertyPropertyChangedEventArgs); } }
    [NotifyAttribute]
    public int MyProperty2 { get { return myProperty2; } set { SetProperty(ref myProperty2, value, myProperty2PropertyChangedEventArgs); } }
    // NonNotify
    public int MyProperty3 { get; set; }

    public MyClass()
    {

    }

    public void Method()
    {
    }

    #region NotifyPropertyChangedGenerator

    public event PropertyChangedEventHandler PropertyChanged;

    private int myProperty;
    private static readonly PropertyChangedEventArgs myPropertyPropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(MyProperty));
    private int myProperty2;
    private static readonly PropertyChangedEventArgs myProperty2PropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(MyProperty2));

    private void SetProperty<T>(ref T field, T value, PropertyChangedEventArgs ev)
    {
        if (!System.Collections.Generic.EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            PropertyChanged?.Invoke(this, ev);
        }
    }

    #endregion
}";

            VerifyCSharpDiagnostic(source, Expected);
            VerifyCSharpFix(source, Attr + @"public class MyClass : INotifyPropertyChanged
{
    [Notify]
    public int MyProperty { get { return myProperty; } set { SetProperty(ref myProperty, value, myPropertyPropertyChangedEventArgs); } }
    [Notify]
    public int MyProperty2 { get { return myProperty2; } set { SetProperty(ref myProperty2, value, myProperty2PropertyChangedEventArgs); } }
    // NonNotify
    public int MyProperty3 { get; set; }

    public MyClass()
    {

    }

    public void Method()
    {
    }

    #region NotifyPropertyChangedGenerator

    public event PropertyChangedEventHandler PropertyChanged;

    private int myProperty;
    private static readonly PropertyChangedEventArgs myPropertyPropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(MyProperty));
    private int myProperty2;
    private static readonly PropertyChangedEventArgs myProperty2PropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(MyProperty2));

    private void SetProperty<T>(ref T field, T value, PropertyChangedEventArgs ev)
    {
        if (!System.Collections.Generic.EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            PropertyChanged?.Invoke(this, ev);
        }
    }

    #endregion
}");

        }

        [TestMethod]
        public void StandardPattern()
        {
            var source = Attr + @"public class MyClass
{
    [Notify]
    public int MyProperty { get; set; }
    [NotifyAttribute]
    public int MyProperty2 { get; set; }

    public MyClass()
    {

    }

    public void Method()
    {
    }
}";

            VerifyCSharpDiagnostic(source, Expected);
            VerifyCSharpFix(source, Attr + @"public class MyClass : INotifyPropertyChanged
{
    [Notify]
    public int MyProperty { get { return myProperty; } set { SetProperty(ref myProperty, value, myPropertyPropertyChangedEventArgs); } }
    [Notify]
    public int MyProperty2 { get { return myProperty2; } set { SetProperty(ref myProperty2, value, myProperty2PropertyChangedEventArgs); } }

    public MyClass()
    {

    }

    public void Method()
    {
    }

    #region NotifyPropertyChangedGenerator

    public event PropertyChangedEventHandler PropertyChanged;

    private int myProperty;
    private static readonly PropertyChangedEventArgs myPropertyPropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(MyProperty));
    private int myProperty2;
    private static readonly PropertyChangedEventArgs myProperty2PropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(MyProperty2));

    private void SetProperty<T>(ref T field, T value, PropertyChangedEventArgs ev)
    {
        if (!System.Collections.Generic.EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            PropertyChanged?.Invoke(this, ev);
        }
    }

    #endregion
}");
        }

        [TestMethod]
        public void StandardPatternClass()
        {
            var source = Attr + @"[Notify]
public class MyClass
{
    public int MyProperty { get; set; }
    public int MyProperty2 { get; set; }

    public MyClass()
    {

    }

    public void Method()
    {
    }
}";

            VerifyCSharpDiagnostic(source, Expected);
            VerifyCSharpFix(source, Attr + @"[Notify]
public class MyClass : INotifyPropertyChanged
{
    public int MyProperty { get { return myProperty; } set { SetProperty(ref myProperty, value, myPropertyPropertyChangedEventArgs); } }
    public int MyProperty2 { get { return myProperty2; } set { SetProperty(ref myProperty2, value, myProperty2PropertyChangedEventArgs); } }

    public MyClass()
    {

    }

    public void Method()
    {
    }

    #region NotifyPropertyChangedGenerator

    public event PropertyChangedEventHandler PropertyChanged;

    private int myProperty;
    private static readonly PropertyChangedEventArgs myPropertyPropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(MyProperty));
    private int myProperty2;
    private static readonly PropertyChangedEventArgs myProperty2PropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(MyProperty2));

    private void SetProperty<T>(ref T field, T value, PropertyChangedEventArgs ev)
    {
        if (!System.Collections.Generic.EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            PropertyChanged?.Invoke(this, ev);
        }
    }

    #endregion
}");
        }

        [TestMethod]
        public void AddNewProperty()
        {
            var source = Attr + @"[Notify]
public class MyClass : INotifyPropertyChanged
{
    public int MyProperty { get { return myProperty; } set { SetProperty(ref myProperty, value, myPropertyPropertyChangedEventArgs); } }
    public int MyProperty2 { get { return myProperty2; } set { SetProperty(ref myProperty2, value, myProperty2PropertyChangedEventArgs); } }
    [NonNotify]
    public int MyProperty3 { get; set; }

    // Notify
    public int MyProperty4 { get; set; }

    public MyClass()
    {

    }

    public void Method()
    {
    }

    #region NotifyPropertyChangedGenerator

    public event PropertyChangedEventHandler PropertyChanged;

    private int myProperty;
    private static readonly PropertyChangedEventArgs myPropertyPropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(MyProperty));
    private int myProperty2;
    private static readonly PropertyChangedEventArgs myProperty2PropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(MyProperty2));

    private void SetProperty<T>(ref T field, T value, PropertyChangedEventArgs ev)
    {
        if (!System.Collections.Generic.EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            PropertyChanged?.Invoke(this, ev);
        }
    }

    #endregion
}";

            VerifyCSharpDiagnostic(source, Expected);
            VerifyCSharpFix(source, Attr + @"[Notify]
public class MyClass : INotifyPropertyChanged
{
    public int MyProperty { get { return myProperty; } set { SetProperty(ref myProperty, value, myPropertyPropertyChangedEventArgs); } }
    public int MyProperty2 { get { return myProperty2; } set { SetProperty(ref myProperty2, value, myProperty2PropertyChangedEventArgs); } }
    [NonNotify]
    public int MyProperty3 { get; set; }

    // Notify
    public int MyProperty4 { get { return myProperty4; } set { SetProperty(ref myProperty4, value, myProperty4PropertyChangedEventArgs); } }

    public MyClass()
    {

    }

    public void Method()
    {
    }

    #region NotifyPropertyChangedGenerator

    public event PropertyChangedEventHandler PropertyChanged;

    private int myProperty;
    private static readonly PropertyChangedEventArgs myPropertyPropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(MyProperty));
    private int myProperty2;
    private static readonly PropertyChangedEventArgs myProperty2PropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(MyProperty2));
    private int myProperty4;
    private static readonly PropertyChangedEventArgs myProperty4PropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(MyProperty4));

    private void SetProperty<T>(ref T field, T value, PropertyChangedEventArgs ev)
    {
        if (!System.Collections.Generic.EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            PropertyChanged?.Invoke(this, ev);
        }
    }

    #endregion
}");
        }

        [TestMethod]
        public void HiddenRegion()
        {
            var source = Attr + @"[Notify]
public class MyClass : INotifyPropertyChanged
{
    public int MyProperty { get { return myProperty; } set { SetProperty(ref myProperty, value, myPropertyPropertyChangedEventArgs); } }
    public int MyProperty2 { get { return myProperty2; } set { SetProperty(ref myProperty2, value, myProperty2PropertyChangedEventArgs); } }
    [NonNotify]
    public int MyProperty3 { get; set; }

    // Notify
    public int MyProperty4 { get; set; }

    public MyClass()
    {

    }

    public void Method()
    {
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private int myProperty;
    private static readonly PropertyChangedEventArgs myPropertyPropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(MyProperty));
    private int myProperty2;
    private static readonly PropertyChangedEventArgs myProperty2PropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(MyProperty2));

    private void SetProperty<T>(ref T field, T value, PropertyChangedEventArgs ev)
    {
        if (!System.Collections.Generic.EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            PropertyChanged?.Invoke(this, ev);
        }
    }
}";

            VerifyCSharpDiagnostic(source, Expected);
        }

        [TestMethod]
        public void BrokenRegion()
        {
            var source = Attr + @"[Notify]
public class MyClass : INotifyPropertyChanged
{
    public int MyProperty { get { return myProperty; } set { SetProperty(ref myProperty, value, myPropertyPropertyChangedEventArgs); } }
    public int MyProperty2 { get { return myProperty2; } set { SetProperty(ref myProperty2, value, myProperty2PropertyChangedEventArgs); } }
    [NonNotify]
    public int MyProperty3 { get; set; }

    // Notify
    public int MyProperty4 { get; set; }

    public MyClass()
    {

    }

    public void Method()
    {
    }

    #region NotifyPropertyChangedGenerator

    public event PropertyChangedEventHandler PropertyChanged;

    private int myProperty777;
    private static readonly PropertyChangedEventArgs myPropertyPropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(MyProperty));
    private int myProperty2;
    private static readonly PropertyChangedEventArgs myProperty2PropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(MyProperty2));

    private void SetProperty<T>(ref T field, T value, PropertyChangedEventArgs ev)
    {
        if (!System.Collections.Generic.EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            PropertyChanged?.Invoke(this, ev);
        }
    }

    #endregion
}";

            VerifyCSharpDiagnostic(source, Expected);
            VerifyCSharpFix(source, Attr + @"[Notify]
public class MyClass : INotifyPropertyChanged
{
    public int MyProperty { get { return myProperty; } set { SetProperty(ref myProperty, value, myPropertyPropertyChangedEventArgs); } }
    public int MyProperty2 { get { return myProperty2; } set { SetProperty(ref myProperty2, value, myProperty2PropertyChangedEventArgs); } }
    [NonNotify]
    public int MyProperty3 { get; set; }

    // Notify
    public int MyProperty4 { get { return myProperty4; } set { SetProperty(ref myProperty4, value, myProperty4PropertyChangedEventArgs); } }

    public MyClass()
    {

    }

    public void Method()
    {
    }

    #region NotifyPropertyChangedGenerator

    public event PropertyChangedEventHandler PropertyChanged;

    private int myProperty;
    private static readonly PropertyChangedEventArgs myPropertyPropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(MyProperty));
    private int myProperty2;
    private static readonly PropertyChangedEventArgs myProperty2PropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(MyProperty2));
    private int myProperty4;
    private static readonly PropertyChangedEventArgs myProperty4PropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(MyProperty4));

    private void SetProperty<T>(ref T field, T value, PropertyChangedEventArgs ev)
    {
        if (!System.Collections.Generic.EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            PropertyChanged?.Invoke(this, ev);
        }
    }

    #endregion
}");
        }
    }
}