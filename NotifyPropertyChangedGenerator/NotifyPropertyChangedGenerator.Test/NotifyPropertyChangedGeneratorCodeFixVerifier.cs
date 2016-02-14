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
            Locations = new[] { new DiagnosticResultLocation("Test0.cs", 3, 1) }
        };

        const string Usings = @"using System;
using System.ComponentModel;
";
        const string Attr = @"

internal enum NamingConvention
{
    Plain,
    LeadingUnderscore,
    TrailingUnderscore,
}

internal enum CompareMethod
{
    None,
    ReferenceEquals,
    EqualityComparer,
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
internal sealed class NotifyAttribute : Attribute
{
    public NotifyAttribute() { }
    public NotifyAttribute(string namingConvention = null, string compareMethod = null) { }
    public NotifyAttribute(NamingConvention namingConvention = default(NamingConvention), CompareMethod compareMethod = default(CompareMethod)) { }
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
            VerifyCSharpDiagnostic(Usings + @"public class MyClass : INotifyPropertyChanged
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
}" + Attr);
        }

        [TestMethod]
        public void SuccessForClassDecr()
        {
            VerifyCSharpDiagnostic(Usings + @"[Notify]
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
}" + Attr);
        }

        [TestMethod]
        public void DoesNotHaveInterface()
        {
            var source = Usings + @"public class MyClass
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
}" + Attr;

            VerifyCSharpDiagnostic(source, Expected);
            VerifyCSharpFix(source, Usings + @"public class MyClass : INotifyPropertyChanged
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
}" + Attr);

        }

        [TestMethod]
        public void StandardPattern()
        {
            var source = Usings + @"public class MyClass
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
}" + Attr;

            VerifyCSharpDiagnostic(source, Expected);
            VerifyCSharpFix(source, Usings + @"public class MyClass : INotifyPropertyChanged
{
    [Notify]
    public int MyProperty { get { return myProperty; } set { SetProperty(ref myProperty, value, myPropertyPropertyChangedEventArgs); } }
    [NotifyAttribute]
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
}" + Attr);
        }

        [TestMethod]
        public void StandardPatternClass()
        {
            var source = Usings + @"[Notify]
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
}" + Attr;

            VerifyCSharpDiagnostic(source, Expected);
            VerifyCSharpFix(source, Usings + @"[Notify]
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
}" + Attr);
        }

        [TestMethod]
        public void PrivateSetterPatternClass()
        {
            var source = Usings + @"[Notify]
public class MyClass
{
    public int MyProperty { get; private set; }
    public int MyProperty2 { get; set; }
    public int MyProperty3 { get; }

    public MyClass()
    {

    }

    public void Method()
    {
    }
}" + Attr;

            VerifyCSharpDiagnostic(source, Expected);
            VerifyCSharpFix(source, Usings + @"[Notify]
public class MyClass : INotifyPropertyChanged
{
    public int MyProperty { get { return myProperty; } private set { SetProperty(ref myProperty, value, myPropertyPropertyChangedEventArgs); } }
    public int MyProperty2 { get { return myProperty2; } set { SetProperty(ref myProperty2, value, myProperty2PropertyChangedEventArgs); } }
    public int MyProperty3 { get { return myProperty3; } private set { SetProperty(ref myProperty3, value, myProperty3PropertyChangedEventArgs); } }

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
    private int myProperty3;
    private static readonly PropertyChangedEventArgs myProperty3PropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(MyProperty3));

    private void SetProperty<T>(ref T field, T value, PropertyChangedEventArgs ev)
    {
        if (!System.Collections.Generic.EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            PropertyChanged?.Invoke(this, ev);
        }
    }

    #endregion
}" + Attr);
        }

        [TestMethod]
        public void AddNewProperty()
        {
            var source = Usings + @"[Notify]
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
}" + Attr;

            VerifyCSharpDiagnostic(source, Expected);
            VerifyCSharpFix(source, Usings + @"[Notify]
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
}" + Attr);
        }

        [TestMethod]
        public void HiddenRegion()
        {
            var source = Usings + @"[Notify]
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
}" + Attr;

            VerifyCSharpDiagnostic(source, Expected);
        }

        [TestMethod]
        public void BrokenRegion()
        {
            var source = Usings + @"[Notify]
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
}" + Attr;

            VerifyCSharpDiagnostic(source, Expected);
            VerifyCSharpFix(source, Usings + @"[Notify]
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
}" + Attr);
        }

        [TestMethod]
        public void NamingConvention()
        {
            var source = Usings + @"public class MyClass
{
    [Notify]
    public int MyProperty { get; set; }
    [Notify(""LeadingUnderscore"")]
    public int MyProperty1 { get; set; }
    [NotifyAttribute(""TrailingUnderscore"")]
    public int MyProperty2 { get; set; }
    [Notify(NamingConvention.LeadingUnderscore)]
    public int MyProperty3 { get; set; }

    public MyClass()
    {

    }

    public void Method()
    {
    }
}" + Attr;

            VerifyCSharpDiagnostic(source, Expected);
            VerifyCSharpFix(source, Usings + @"public class MyClass : INotifyPropertyChanged
{
    [Notify]
    public int MyProperty { get { return myProperty; } set { SetProperty(ref myProperty, value, myPropertyPropertyChangedEventArgs); } }
    [Notify(""LeadingUnderscore"")]
    public int MyProperty1 { get { return _myProperty1; } set { SetProperty(ref _myProperty1, value, _myProperty1PropertyChangedEventArgs); } }
    [NotifyAttribute(""TrailingUnderscore"")]
    public int MyProperty2 { get { return myProperty2_; } set { SetProperty(ref myProperty2_, value, myProperty2_PropertyChangedEventArgs); } }
    [Notify(NamingConvention.LeadingUnderscore)]
    public int MyProperty3 { get { return _myProperty3; } set { SetProperty(ref _myProperty3, value, _myProperty3PropertyChangedEventArgs); } }

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
    private int _myProperty1;
    private static readonly PropertyChangedEventArgs _myProperty1PropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(MyProperty1));
    private int myProperty2_;
    private static readonly PropertyChangedEventArgs myProperty2_PropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(MyProperty2));
    private int _myProperty3;
    private static readonly PropertyChangedEventArgs _myProperty3PropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(MyProperty3));

    private void SetProperty<T>(ref T field, T value, PropertyChangedEventArgs ev)
    {
        if (!System.Collections.Generic.EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            PropertyChanged?.Invoke(this, ev);
        }
    }

    #endregion
}" + Attr);
        }

        [TestMethod]
        public void NamingConventionByClassAttribute()
        {
            var source = Usings + @"[Notify(""LeadingUnderscore"")]
public class MyClass
{
    [Notify]
    public int MyProperty { get; set; }
    [NonNotify]
    public int MyProperty1 { get; set; }
    [NotifyAttribute(""TrailingUnderscore"")]
    public int MyProperty2 { get; set; }
    public int MyProperty3 { get; set; }

    public MyClass()
    {

    }

    public void Method()
    {
    }
}" + Attr;

            VerifyCSharpDiagnostic(source, Expected);
            VerifyCSharpFix(source, Usings + @"[Notify(""LeadingUnderscore"")]
public class MyClass : INotifyPropertyChanged
{
    [Notify]
    public int MyProperty { get { return myProperty; } set { SetProperty(ref myProperty, value, myPropertyPropertyChangedEventArgs); } }
    [NonNotify]
    public int MyProperty1 { get; set; }
    [NotifyAttribute(""TrailingUnderscore"")]
    public int MyProperty2 { get { return myProperty2_; } set { SetProperty(ref myProperty2_, value, myProperty2_PropertyChangedEventArgs); } }
    public int MyProperty3 { get { return _myProperty3; } set { SetProperty(ref _myProperty3, value, _myProperty3PropertyChangedEventArgs); } }

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
    private int myProperty2_;
    private static readonly PropertyChangedEventArgs myProperty2_PropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(MyProperty2));
    private int _myProperty3;
    private static readonly PropertyChangedEventArgs _myProperty3PropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(MyProperty3));

    private void SetProperty<T>(ref T field, T value, PropertyChangedEventArgs ev)
    {
        if (!System.Collections.Generic.EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            PropertyChanged?.Invoke(this, ev);
        }
    }

    #endregion
}" + Attr);
        }

        [TestMethod]
        public void CompareMethodReferenceEquals()
        {
            var source = Usings + @"[Notify(""ReferenceEquals"")]
public class MyClass
{
    public int MyProperty { get; set; }
}" + Attr;

            VerifyCSharpDiagnostic(source, Expected);
            VerifyCSharpFix(source, Usings + @"[Notify(""ReferenceEquals"")]
public class MyClass : INotifyPropertyChanged
{
    public int MyProperty { get { return myProperty; } set { SetProperty(ref myProperty, value, myPropertyPropertyChangedEventArgs); } }

    #region NotifyPropertyChangedGenerator

    public event PropertyChangedEventHandler PropertyChanged;

    private int myProperty;
    private static readonly PropertyChangedEventArgs myPropertyPropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(MyProperty));

    private void SetProperty<T>(ref T field, T value, PropertyChangedEventArgs ev)
    {
        if (!object.ReferenceEquals(field, value))
        {
            field = value;
            PropertyChanged?.Invoke(this, ev);
        }
    }

    #endregion
}" + Attr);
        }

        [TestMethod]
        public void CompareMethodNone()
        {
            var source = Usings + @"[Notify(""None"")]
public class MyClass
{
    public int MyProperty { get; set; }
}" + Attr;

            VerifyCSharpDiagnostic(source, Expected);
            VerifyCSharpFix(source, Usings + @"[Notify(""None"")]
public class MyClass : INotifyPropertyChanged
{
    public int MyProperty { get { return myProperty; } set { SetProperty(ref myProperty, value, myPropertyPropertyChangedEventArgs); } }

    #region NotifyPropertyChangedGenerator

    public event PropertyChangedEventHandler PropertyChanged;

    private int myProperty;
    private static readonly PropertyChangedEventArgs myPropertyPropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(MyProperty));

    private void SetProperty<T>(ref T field, T value, PropertyChangedEventArgs ev)
    {
        field = value;
        PropertyChanged?.Invoke(this, ev);
    }

    #endregion
}" + Attr);
        }

        [TestMethod]
        public void NamingConventionAndCompareMethod()
        {
            var source = Usings + @"[Notify(NamingConvention.LeadingUnderscore, CompareMethod.ReferenceEquals)]
public class MyClass
{
    [Notify]
    public int MyProperty { get; set; }
    [NonNotify]
    public int MyProperty1 { get; set; }
    [NotifyAttribute(""TrailingUnderscore"")]
    public int MyProperty2 { get; set; }
    public int MyProperty3 { get; set; }

    public MyClass()
    {

    }

    public void Method()
    {
    }
}" + Attr;

            VerifyCSharpDiagnostic(source, Expected);
            VerifyCSharpFix(source, Usings + @"[Notify(NamingConvention.LeadingUnderscore, CompareMethod.ReferenceEquals)]
public class MyClass : INotifyPropertyChanged
{
    [Notify]
    public int MyProperty { get { return myProperty; } set { SetProperty(ref myProperty, value, myPropertyPropertyChangedEventArgs); } }
    [NonNotify]
    public int MyProperty1 { get; set; }
    [NotifyAttribute(""TrailingUnderscore"")]
    public int MyProperty2 { get { return myProperty2_; } set { SetProperty(ref myProperty2_, value, myProperty2_PropertyChangedEventArgs); } }
    public int MyProperty3 { get { return _myProperty3; } set { SetProperty(ref _myProperty3, value, _myProperty3PropertyChangedEventArgs); } }

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
    private int myProperty2_;
    private static readonly PropertyChangedEventArgs myProperty2_PropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(MyProperty2));
    private int _myProperty3;
    private static readonly PropertyChangedEventArgs _myProperty3PropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(MyProperty3));

    private void SetProperty<T>(ref T field, T value, PropertyChangedEventArgs ev)
    {
        if (!object.ReferenceEquals(field, value))
        {
            field = value;
            PropertyChanged?.Invoke(this, ev);
        }
    }

    #endregion
}" + Attr);
        }

        [TestMethod]
        public void LeadingUnserscoreInterface()
        {
            var source = @"using System;
using System.ComponentModel;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
internal sealed class NotifyAttribute : Attribute,
    // default option, you can customize default naming convention
    NotifyAttribute.ILeadingUnderscore
{
    // naming convention markers
    internal interface IPlain { }
    internal interface ILeadingUnderscore { }
    internal interface ITrailingUnderscore { }

    public NotifyAttribute() { }

    public NotifyAttribute(string namingConvention = null, string compareMethod = null) { }
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
internal sealed class NonNotifyAttribute : Attribute
{

}

[Notify]
public class MyClass : INotifyPropertyChanged
{
    public int MyProperty { get; set; }
    public int MyProperty2 { get; set; }
    [NonNotify]
    public int MyProperty3 { get; set; }

    public MyClass()
    {

    }

    public void Method()
    {
    }
}
";

            var fixSource = @"using System;
using System.ComponentModel;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
internal sealed class NotifyAttribute : Attribute,
    // default option, you can customize default naming convention
    NotifyAttribute.ILeadingUnderscore
{
    // naming convention markers
    internal interface IPlain { }
    internal interface ILeadingUnderscore { }
    internal interface ITrailingUnderscore { }

    public NotifyAttribute() { }

    public NotifyAttribute(string namingConvention = null, string compareMethod = null) { }
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
internal sealed class NonNotifyAttribute : Attribute
{

}

[Notify]
public class MyClass : INotifyPropertyChanged
{
    public int MyProperty { get { return _myProperty; } set { SetProperty(ref _myProperty, value, _myPropertyPropertyChangedEventArgs); } }
    public int MyProperty2 { get { return _myProperty2; } set { SetProperty(ref _myProperty2, value, _myProperty2PropertyChangedEventArgs); } }
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

    private int _myProperty;
    private static readonly PropertyChangedEventArgs _myPropertyPropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(MyProperty));
    private int _myProperty2;
    private static readonly PropertyChangedEventArgs _myProperty2PropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(MyProperty2));

    private void SetProperty<T>(ref T field, T value, PropertyChangedEventArgs ev)
    {
        if (!System.Collections.Generic.EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            PropertyChanged?.Invoke(this, ev);
        }
    }

    #endregion
}
";
            VerifyCSharpFix(source, fixSource);
        }



        [TestMethod]
        public void ChaingingComparer()
        {
            var source = @"using System;
using System.ComponentModel;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
internal sealed class NotifyAttribute : Attribute,
    // default option, you can customize default naming convention
    NotifyAttribute.ILeadingUnderscore
{
    // naming convention markers
    internal interface IPlain { }
    internal interface ILeadingUnderscore { }
    internal interface ITrailingUnderscore { }

    public NotifyAttribute() { }

    public NotifyAttribute(NotifyCompareMethod compareMethod) { }
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
internal sealed class NonNotifyAttribute : Attribute
{

}

[Notify(NotifyCompareMethod.ReferenceEquals)]
public class MyClass : INotifyPropertyChanged
{
    public int MyProperty { get; set; }
    public int MyProperty2 { get; set; }
    [NonNotify]
    public int MyProperty3 { get; set; }

    public MyClass()
    {

    }

    public void Method()
    {
    }
}
";

            var fixSource = @"using System;
using System.ComponentModel;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
internal sealed class NotifyAttribute : Attribute,
    // default option, you can customize default naming convention
    NotifyAttribute.ILeadingUnderscore
{
    // naming convention markers
    internal interface IPlain { }
    internal interface ILeadingUnderscore { }
    internal interface ITrailingUnderscore { }

    public NotifyAttribute() { }

    public NotifyAttribute(NotifyCompareMethod compareMethod) { }
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
internal sealed class NonNotifyAttribute : Attribute
{

}

[Notify(NotifyCompareMethod.ReferenceEquals)]
public class MyClass : INotifyPropertyChanged
{
    public int MyProperty { get { return _myProperty; } set { SetProperty(ref _myProperty, value, _myPropertyPropertyChangedEventArgs); } }
    public int MyProperty2 { get { return _myProperty2; } set { SetProperty(ref _myProperty2, value, _myProperty2PropertyChangedEventArgs); } }
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

    private int _myProperty;
    private static readonly PropertyChangedEventArgs _myPropertyPropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(MyProperty));
    private int _myProperty2;
    private static readonly PropertyChangedEventArgs _myProperty2PropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(MyProperty2));

    private void SetProperty<T>(ref T field, T value, PropertyChangedEventArgs ev)
    {
        if (!object.ReferenceEquals(field, value))
        {
            field = value;
            PropertyChanged?.Invoke(this, ev);
        }
    }

    #endregion
}
";
            VerifyCSharpFix(source, fixSource);
        }
    }
}