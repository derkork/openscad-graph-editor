using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class Branch : ScadNode, ICanHaveModifier
    {
        public override string NodeTitle => "Branch";

        public override string NodeDescription =>
            "Performs a test to determine if the actions in a sub scope should be performed or not.";

        public Branch()
        {
            InputPorts
                .Flow()
                .Boolean("Condition");

            OutputPorts
                .Flow("True")
                .Flow("False")
                .Flow("After");
        }

        public override string GetPortDocumentation(PortId portId)
        {
            switch (portId.Port)
            {
                case 0 when portId.IsInput:
                    return "Input flow";
                case 1 when portId.IsInput:
                    return "The condition that should be tested.";
                case 0 when portId.IsOutput:
                    return "The flow that is executed if the condition is true.";
                case 1 when portId.IsOutput:
                    return "The flow that is executed if the condition is false.";
                case 2 when portId.IsOutput:
                    return "The flow that is executed after the condition is tested.";
                default:
                    return "";
            }
        }


        public override string Render(IScadGraph context)
        {
            var condition = RenderInput(context, 1).OrUndef();
            var ifBranch = RenderOutput(context, 0);
            var elseBranch = RenderOutput(context, 1);
            var after = RenderOutput(context, 2);

            if (ifBranch.Length == 0)
            {
                if (elseBranch.Length == 0)
                {
                    return after;
                }

                return $@"if (!({condition})){elseBranch.AsBlock()}{after}";
            }

            if (elseBranch.Length == 0)
            {
                return $@"if ({condition}){ifBranch.AsBlock()}{after}";
            }
            
            return $@"if ({condition}){ifBranch.AsBlock()}else{elseBranch.AsBlock()}{after}";
        }
    }
}