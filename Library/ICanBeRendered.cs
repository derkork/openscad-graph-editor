namespace OpenScadGraphEditor.Library
{
    public interface ICanBeRendered
    {
        /// <summary>
        /// Renders this context into an OpenScad file.
        /// </summary>
        string Render();
    }
}