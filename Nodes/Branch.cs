using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class Branch : ScadNode, ICanHaveModifier
    {
        public override string NodeTitle => "Branch";
        public override string NodeQuickLookup => "Brh";

        public override string NodeDescription =>
            "Renders the true input if the condition is true, otherwise renders the false input.";

        public Branch()
        {
            InputPorts
                .Boolean("Condition")
                .Geometry("True")
                .Geometry("False");

            OutputPorts
                .Geometry();
        }

        public override string GetPortDocumentation(PortId portId)
        {
            switch (portId.Port)
            {
                case 0 when portId.IsInput:
                    return "The condition that should be tested.";
                case 1 when portId.IsInput:
                    return "The geometry that will output if the condition is true.";
                case 2 when portId.IsInput:
                    return "The geometry that will output if the condition is false.";
                case 0 when portId.IsOutput:
                    return "Output geometry";
                default:
                    return "";
            }
        }


        public override string Render(ScadGraph context, int portIndex)
        {
            var condition = RenderInput(context, 0).OrUndef();
            var ifBranch = RenderInput(context, 1);
            var elseBranch = RenderInput(context, 2);

            if (ifBranch.Length == 0)
            {
                if (elseBranch.Length == 0)
                {
                    return "";
                }

                return $@"if (!({condition})){elseBranch.AsBlock()}";
            }

            if (elseBranch.Length == 0)
            {
                return $@"if ({condition}){ifBranch.AsBlock()}";
            }
            
            return $@"if ({condition}){ifBranch.AsBlock()}else{elseBranch.AsBlock()}";
        }
    }
}