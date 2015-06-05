namespace NotifyPropertyChangedGenerator.Annotations
{
    /// <summary>
    /// Compare method in the generated SetProperty method.
    /// </summary>
    public enum CompareMethod
    {
        /*
           - None: raises `PropertyChanged` at any time when the property set
           - ReferenceEquals: 
           - EqualityComparer: uses `EqualityComparer<T>.Default.Equals` to compare old and new values
        */
        /// <summary>
        /// Raises `PropertyChanged` at any time when the property set.
        /// </summary>
        None,

        /// <summary>
        /// Uses <see cref="object.ReferenceEquals(object, object)"/> to compare old and new values.
        /// </summary>
        ReferenceEquals,

        /// <summary>
        /// Uses <see cref="System.Collections.Generic.EqualityComparer{T}.Default"/> to compare old and new values.
        /// </summary>
        EqualityComparer,
    }
}
