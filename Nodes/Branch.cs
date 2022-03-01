using JetBrains.Annotations;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class Branch : ScadNode
    {
        public override string NodeTitle => "Branch";

        public override string NodeDescription =>
            "Performs a test to determine\nif the actions in a sub scope\nshould be performed or not.";

        public override void _Ready()
        {
            InputPorts
                .Flow("In")
                .Boolean("Condition");

            OutputPorts
                .Flow("True")
                .Flow("False");
                
            base._Ready();
        }

        public override string Render(ScadContext scadContext)
        {
            var condition = RenderInput(scadContext, 1);
            var ifBranch = RenderOutput(scadContext, 0);
            var elseBranch = RenderOutput(scadContext, 1);

            if (ifBranch.Length == 0)
            {
                if (elseBranch.Length == 0)
                {
                    return "";
                }

                return $@"if (!({condition})) {{"
                    .AppendLines(
                        elseBranch.Indent(),
                        "}"
                    );
            }

            if (elseBranch.Length == 0)
            {
                return $@"if ({condition}) {{"
                    .AppendLines(
                        ifBranch.Indent(),
                        "}"
                    );
            }

            return $@"if ({condition}) {{"
                .AppendLines(
                    ifBranch.Indent(),
                    "} else {",
                    elseBranch.Indent(),
                    "}"
                );
        }
    }
}