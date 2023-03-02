namespace OpenScadGraphEditor.Nodes
{
    /// <summary>
    /// A render hint for the widget rendering a port.
    /// </summary>
    public enum RenderHint
    {
        /// <summary>
        /// The default render hint. Render the port as a normal port.
        /// </summary>
        None,

        /// <summary>
        /// Renders the port as a file input port. Instead of a string widget, allows the user to select a file.
        /// </summary>
        FileInput,

        /// <summary>
        /// Renders the port as a font input port. Instead of a string widget, allows the user to select a font.
        /// </summary>
        FontInput
    }
}