using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Refactorings;

namespace OpenScadGraphEditor.Nodes.Reroute
{
    /// <summary>
    /// Refactoring which toggles whether or not a reroute node is wireless.
    /// </summary>
    public class ToggleWirelessRefactoring : NodeRefactoring
    {
        public ToggleWirelessRefactoring(ScadGraph holder, RerouteNode node) : base(holder, node)
        {
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            var node = (RerouteNode)Node;
            node.IsWireless = !node.IsWireless;
        }
    }
}