namespace NotifyPropertyChangedGenerator.Annotations
{
    /// <summary>
    /// Naming convention of the generated fields.
    /// </summary>
    public enum NamingConvention
    {
        /// <summary>
        /// Uses lowerCamelCase of the original property for the generated field name.
        /// </summary>
        Plain,

        /// <summary>
        /// Uses "_" + [Plain name].
        /// </summary>
        LeadingUnderscore,

        /// <summary>
        /// Uses [Plain name] + "_".
        /// </summary>
        TrailingUnderscore,
    }
}
