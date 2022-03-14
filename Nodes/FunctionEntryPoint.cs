using System.Linq;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    public class FunctionEntryPoint : EntryPoint, IMultiExpressionOutputNode
    {
        private FunctionDescription _description;
        
        public override string NodeTitle => _description.NodeNameOrFallback;
        public override string NodeDescription => _description.Description;


        public override void SaveInto(SavedNode node)
        {
            node.SetData("function_description_id", _description.Id);
            base.SaveInto(node);
        }

        public override void LoadFrom(SavedNode node, IReferenceResolver referenceResolver)
        {
            var functionDescriptionId = node.GetData("function_description_id");
            Setup(referenceResolver.ResolveFunctionReference(functionDescriptionId)); 
            base.LoadFrom(node, referenceResolver);
        }

        public void Setup(FunctionDescription description)
        {
            _description = description;
            OutputPorts
                .Flow();

            foreach (var parameter in description.Parameters)
            {
                InputPorts
                    .OfType(parameter.TypeHint, parameter.Name);
                OutputPorts
                    .OfType(parameter.TypeHint, parameter.Name, false);
            }


        }
        
        public override string Render(IScadGraph context)
        {
            var arguments = _description.Parameters.Indices()
                .Select(it => _description.Parameters[it].Name + RenderInput(context, it).PrefixUnlessEmpty(" = "));

            return $"{_description.Name} ({string.Join(", ", arguments)}) = {RenderOutput(context, 0)};";
        }

        public string RenderExpressionOutput(IScadGraph context, int port)
        {
            // return simply the parameter name.
            return _description.Parameters[port - 1].Name;
        }
    }
}