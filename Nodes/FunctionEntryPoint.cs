using System.Collections.Generic;
using System.Linq;
using GodotExt;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.IO;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    public class FunctionEntryPoint : EntryPoint,  IReferToAFunction
    {
        private FunctionDescription _description;

        public override string NodeTitle => _description.NodeNameOrFallback;
        public override string NodeQuickLookup => "";
        public override string NodeDescription => _description.Description;


        public InvokableDescription InvokableDescription => _description;


        public override string GetPortDocumentation(PortId portId)
        {
            if (portId.IsOutput)
            {
                return _description.Parameters[portId.Port].Description;
            }

            return "";
        }

        public int GetParameterInputPort(int parameterIndex)
        {
            // entry point has no input ports that refer to parameters
            return -1;
        }

        public int GetParameterOutputPort(int parameterIndex)
        {
            // the n-th parameter is the n-th output port
            return parameterIndex;
        }

        public IEnumerable<PortId> GetPortsReferringToReturnValue()
        {
            // entry point has no return value
            return Enumerable.Empty<PortId>();
        }

        public override void SaveInto(SavedNode node)
        {
            node.SetData("function_description_id", _description.Id);
            base.SaveInto(node);
        }

        public override void RestorePortDefinitions(SavedNode node, IReferenceResolver referenceResolver)
        {
            var functionDescriptionId = node.GetDataString("function_description_id");
            SetupPorts(referenceResolver.ResolveFunctionReference(functionDescriptionId));
            base.RestorePortDefinitions(node, referenceResolver);
        }

        public void SetupPorts(InvokableDescription description)
        {
            GdAssert.That(description is FunctionDescription, "needs a function description");

            InputPorts.Clear();
            OutputPorts.Clear();

            _description = (FunctionDescription) description;

            foreach (var parameter in description.Parameters)
            {
                OutputPorts
                    .PortType(parameter.TypeHint, parameter.Name,  autoSetLiteralWhenPortIsDisconnected: false, literalType: parameter.TypeHint.GetMatchingLiteralType());
            }
        }

        public override string RenderEntryPoint(string content)
        {
            var arguments = _description.Parameters.Indices()
                .Select(it =>
                    _description.Parameters[it].Name + RenderLiteral(PortId.Output(it)).PrefixUnlessEmpty(" = "));

            var comment = RenderDocumentationComment(_description);
            return $"{comment}\nfunction {_description.Name}({string.Join(", ", arguments)}) = {content};";
        }

        public override string Render(ScadGraph context, int portIndex)
        {
            if (portIndex < 0 || portIndex >= _description.Parameters.Count)
            {
                return "";
            }
            
            // return simply the parameter name.
            return _description.Parameters[portIndex].Name;
        }

    }
}