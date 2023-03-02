using System.Collections.Generic;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.IO;

namespace OpenScadGraphEditor.Nodes
{
    public class GetVariable : ScadNode, IReferToAVariable, IAmAnExpression
    {
        public VariableDescription VariableDescription { get; private set; }
        // no point in having a quick lookup as this is repeated for each variable
        public override string NodeQuickLookup => "";
        public override string NodeTitle => $"Get {VariableDescription?.Name ?? "Variable"}";
        public override string NodeDescription => "Gets a variable's value.";

        public override string GetPortDocumentation(PortId portId)
        {
            return "The value of the variable.";
        }

        public void SetupPorts(VariableDescription description)
        {
            VariableDescription = description;
            OutputPorts.Clear();
            OutputPorts.PortType(description.TypeHint);
        }

        public IEnumerable<PortId> GetPortsReferringToVariable()
        {
            yield return PortId.Output(0);
        }


        public override void SaveInto(SavedNode node)
        {
            node.SetData("variable_description_id", VariableDescription.Id);
            base.SaveInto(node);
        }

        public override void RestorePortDefinitions(SavedNode node, IReferenceResolver referenceResolver)
        {
            SetupPorts(referenceResolver.ResolveVariableReference(node.GetDataString("variable_description_id"))); 
            base.RestorePortDefinitions(node, referenceResolver);
        }

        public override string Render(ScadGraph context, int portIndex)
        {
            return $"{VariableDescription.Name}";
        }
    }
}