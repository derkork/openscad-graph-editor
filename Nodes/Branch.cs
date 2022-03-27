using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class Branch : ScadNode
    {
        public override string NodeTitle => "Branch";

        public override string NodeDescription =>
            "Performs a test to determine\nif the actions in a sub scope\nshould be performed or not.";

        public Branch()
        {
            InputPorts
                .Flow("In")
                .Boolean("Condition");

            OutputPorts
                .Flow("True")
                .Flow("False")
                .Flow("After");
        }

        public override string Render(IScadGraph context)
        {
            var condition = RenderInput(context, 1);
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