using System.Linq;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    public class ModuleInvocation : ScadNode
    {
        private ModuleDescription _description;
        public override string NodeTitle => _description.NodeNameOrFallback;
        public override string NodeDescription => _description.Description;


        public override void SaveInto(SavedNode node)
        {
            base.SaveInto(node);
            node.SetData("module_description_id", _description.Id);
        }

        public override void PrepareForLoad(SavedNode node)
        {
            var moduleDescriptionId = node.GetData("module_description_id");
            Setup(InvokableLibrary.ForModuleDescriptionId(moduleDescriptionId));
        }

        public void Setup(ModuleDescription description)
        {
            _description = description;

            InputPorts
                .Flow();

            foreach (var parameter in description.Parameters)
            {
                var type = parameter.TypeHint;
                InputPorts
                    .OfType(type, parameter.LabelOrFallback, type != PortType.Any && type != PortType.Flow,
                        parameter.IsAutoCoerced);
            }

            if (description.SupportsChildren)
            {
                OutputPorts
                    .Flow("Children")
                    .Flow("After");
            }
            else
            {
                OutputPorts
                    .Flow();
            }
        }

        public override string Render(ScadContext scadContext)
        {
            var parameters = string.Join(", ",
                InputPorts
                    .Indices()
                    .Skip(1)
                    .Select(it => $"{_description.Parameters[it - 1].Name} = {RenderInput(scadContext, it)}")
            );
            var result = $"{_description.Name}({parameters})";
            var childNodes = _description.SupportsChildren ? RenderOutput(scadContext, 0) : "";
            var nextNodes = RenderOutput(scadContext, _description.SupportsChildren ? 1 : 0);
            if (childNodes.Length > 0)
            {
                return result + childNodes.AsBlock() + ";" + nextNodes;
            }

            return result + ";\n" + nextNodes;
        }
    }
}