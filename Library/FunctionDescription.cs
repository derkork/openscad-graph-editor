using Godot;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Library
{
    /// <summary>
    /// A function that can be invoked.
    /// </summary>
    public class FunctionDescription : InvokableDescription
    {
        /// <summary>
        /// The hinted return type of the function.
        /// </summary>
        [Export]
        public PortType ReturnTypeHint { get; } = PortType.Any;
    }
}