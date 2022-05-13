using Godot;
using OpenScadGraphEditor.Library.IO;
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
        public PortType ReturnTypeHint { get; set; } = PortType.Any;

        /// <summary>
        /// Description of the return value.
        /// </summary>
        public string ReturnValueDescription { get; set; } = "";
        
        public override bool CanUse(ScadNode node)
        {
            return node is IAmAnExpression;
        }
        
        
        public void LoadFrom(SavedFunctionDescription savedInvokableDescription)
        {
            ReturnTypeHint = savedInvokableDescription.ReturnTypeHint;
            ReturnValueDescription = savedInvokableDescription.ReturnValueDescription;
            base.LoadFrom(savedInvokableDescription);
        }

        public void SaveInto(SavedFunctionDescription savedInvokableDescription)
        {
            savedInvokableDescription.ReturnTypeHint = ReturnTypeHint;
            savedInvokableDescription.ReturnValueDescription = ReturnValueDescription;
            base.SaveInto(savedInvokableDescription);
        }
    }
}