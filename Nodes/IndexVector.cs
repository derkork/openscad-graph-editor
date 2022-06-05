using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.IO;

namespace OpenScadGraphEditor.Nodes
{
    /// <summary>
    /// Node which allows to access vector/string indices.
    /// </summary>
    [UsedImplicitly]
    public class IndexVector : ScadNode, IAmAnExpression, IHaveVariableInputSize
    {
        public override string NodeTitle => "Index Vector/String";
        public override string NodeDescription => "Returns the value of the vector/string at the given index";

        public int CurrentInputSize { get; private set; } = 1;

        // TODO: make outputs and inputs line up
        public int InputPortOffset => 1;
        public int OutputPortOffset => 0;
        public string AddRefactoringTitle => "Add index";
        public string RemoveRefactoringTitle => "Remove index";
        public bool OutputPortsMatchVariableInputs => true;


        public IndexVector()
        {
            RebuildPorts();
        }

        public override void SaveInto(SavedNode node)
        {
            node.SetData("ports", CurrentInputSize);
            base.SaveInto(node);
        }

        public override void RestorePortDefinitions(SavedNode node, IReferenceResolver referenceResolver)
        {
            CurrentInputSize = node.GetDataInt("ports", 1);
            RebuildPorts();
            base.RestorePortDefinitions(node, referenceResolver);
        }

        public void AddVariableInputPort()
        {
            CurrentInputSize++;
            RebuildPorts();
            
            // build an input port literal
            BuildPortLiteral(PortId.Input(CurrentInputSize));
            // build an output port literal
            BuildPortLiteral(PortId.Output(CurrentInputSize-1));
        }

        public void RemoveVariableInputPort()
        {
            GdAssert.That(CurrentInputSize > 1, "Cannot decrease ports below 1.");
            DropPortLiteral(PortId.Input(CurrentInputSize));
            var idx = CurrentInputSize - 1;
            DropPortLiteral(PortId.Output(idx));

            CurrentInputSize--;
            RebuildPorts();
        }

        private void RebuildPorts()
        {
            InputPorts.Clear();
            OutputPorts.Clear();

            InputPorts
                .Any("Vector/String");
            
            for (var i = 0; i < CurrentInputSize; i++)
            {
                InputPorts
                    .Number($"Index {i + 1}");
                OutputPorts
                    .Any($"Value {i + 1}");
            }
        }

        public override string GetPortDocumentation(PortId portId)
        {
            if (portId.IsInput)
            {
                if (portId.Port == 0)
                {
                    return "The vector from which a value should be extracted.";
                }
                return "The index of the value to extract.";
            }

            if (portId.IsOutput)
            {
                return "The value at the given index.";
            }

            return "";
        }
        
        
        public override string Render(ScadGraph context, int portIndex)
        {
            if (portIndex < 0 || portIndex >= CurrentInputSize)
            {
                return "";
            }
            var vector = RenderInput(context, 0);
            var index = RenderInput(context, portIndex+1);
            return $"{vector}[{index}]";
        }

        
    }
}