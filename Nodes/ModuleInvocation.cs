using System;
using System.Linq;
using Godot;
using GodotExt;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.IO;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    public class ModuleInvocation : ScadNode, IReferToAnInvokable, ICanHaveModifier
    {
        private ModuleDescription _description;
        public override string NodeTitle => _description.NodeNameOrFallback;
        public override string NodeQuickLookup => _description.QuickNodeLookup;
        public override string NodeDescription => _description.Description;

        public InvokableDescription InvokableDescription => _description;

        public override string GetPortDocumentation(PortId portId)
        {
            if (portId.IsInput)
            {
                if (_description.SupportsChildren && portId.Port == 0)
                {
                    return "Input geometry for the module";
                }
                
                return _description.Parameters[portId.Port - (_description.SupportsChildren ? 1 : 0)].Description;
            }

            if (portId.IsOutput)
            {
                return "Output geometry";
            }

            return "";
        }

        public override void SaveInto(SavedNode node)
        {
            base.SaveInto(node);
            node.SetData("module_description_id", _description.Id);
        }

        public int GetParameterInputPort(int parameterIndex)
        {
            // the n-th parameter corresponds to the n-th input port
            return parameterIndex;
        }

        public int GetParameterOutputPort(int parameterIndex)
        {
            // no output ports correspond to parameters
            return -1;
        }

        public override void RestorePortDefinitions(SavedNode node, IReferenceResolver referenceResolver)
        {
            var moduleDescriptionId = node.GetDataString("module_description_id");
            SetupPorts(referenceResolver.ResolveModuleReference(moduleDescriptionId));
            base.RestorePortDefinitions(node, referenceResolver);
        }

        public void SetupPorts(InvokableDescription description)
        {
            GdAssert.That(description is ModuleDescription, "needs a module description");
            _description = (ModuleDescription) description;
            InputPorts.Clear();
            OutputPorts.Clear();

            if (_description.SupportsChildren)
            {
                InputPorts
                    .Geometry();
            }

            foreach (var parameter in description.Parameters)
            {
                var type = parameter.TypeHint;
                InputPorts
                    .OfType(type, parameter.LabelOrFallback, type.GetMatchingLiteralType(), !parameter.IsOptional);
            }


            OutputPorts
                .Geometry();
        }


        public override string Render(ScadGraph context, int portIndex)
        {
            var parameters = _description.Parameters.Count.Range()
                .Select(it =>
                {
                    var parameterDescription = _description.Parameters[it];
                    var value = RenderInput(context, it+ ( _description.SupportsChildren ?  1 : 0));
                    return value.Empty() && parameterDescription.IsOptional
                        ? ""
                        : $"{parameterDescription.Name} = {value.OrUndef()}";
                })
                .Where(it => !it.Empty())
                .JoinToString(", ");
       
            var result = $"{_description.Name}({parameters})";
            var childNodes = _description.SupportsChildren ? RenderInput(context, 0) : "";
            if (!childNodes.Empty())
            {
                return result + childNodes.AsBlock();
            }

            return result + ";";
        }
    }
}