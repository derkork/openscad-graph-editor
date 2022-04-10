namespace OpenScadGraphEditor.Library.External
{
    /// <summary>
    /// Path mode used for encoding references to external files.
    /// </summary>
    public enum ExternalFilePathMode
    {
        /// <summary>
        /// This references to a file in one of OpenSCADs library folders.
        /// </summary>
        Library,
        
        /// <summary>
        /// This references to a file relative to the current project.
        /// </summary>
        Relative,
        
        /// <summary>
        /// This references to an absolute file path.
        /// </summary>
        Absolute
    }
}