using Godot;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Library.IO
{
    /// <summary>
    /// Saved variant of a <see cref="FunctionDescription"/>
    /// </summary>
    public sealed class SavedFunctionDescription : SavedInvokableDescription
    {
        /// <summary>
        /// The hinted return type of the function.
        /// </summary>
        [Export]
        public PortType ReturnTypeHint { get; set; } = PortType.Any;

        /// <summary>
        /// The description of the return value.
        /// </summary>
        [Export]
        public string ReturnValueDescription { get; set; } = "";
    }
}