using System.Runtime.Remoting.Contexts;
using OpenScadGraphEditor.Actions;
using OpenScadGraphEditor.Refactorings;
using OpenScadGraphEditor.Widgets;
using OpenScadGraphEditor.Widgets.AddDialog;

namespace OpenScadGraphEditor.Nodes.Reroute
{
    /// <summary>
    ///  Editor action for toggling the wireless state of a reroute node.
    /// </summary>
    public class ToggleWirelessAction : IEditorAction
    {
        public int Order => 0;
        public string Group => "";

        public bool TryBuildQuickAction(IEditorContext context, RequestContext item, out QuickAction result)
        {
            if (item.TryGetNode(out var graph, out var node, out _) && node is RerouteNode rerouteNode)
            {
                var text = rerouteNode.IsWireless ? "Make wired" : "Make wireless";

                new QuickAction(text,
                    () => context.PerformRefactoring(text, new ToggleWirelessRefactoring(graph, rerouteNode)));
            }

            result = default;
            return false;
        }
    }
}