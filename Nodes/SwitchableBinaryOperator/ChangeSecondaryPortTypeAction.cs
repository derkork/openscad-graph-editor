using System;
using System.Linq;
using OpenScadGraphEditor.Actions;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Nodes.SwitchableBinaryOperator
{
    /// <summary>
    /// An action which allows to change the port type of the currently unconnected port of a binary operator
    /// to something else.
    /// </summary>
    public class ChangeSecondaryPortTypeAction : IEditorAction
    {
        private readonly PortType _portType;
        private readonly Type _nodeType;
        public int Order => 1;
        public string Group => "Change unconnected port";


        public ChangeSecondaryPortTypeAction(PortType portType)
        {
            _portType = portType;
        }

        public bool TryBuildQuickAction(IEditorContext context, RequestContext item, out QuickAction result)
        {
            result = default;

            if (item.TryGetNode(out var graph, out var node)
                && node is SwitchableBinaryOperator switchableBinaryOperator)
            {
                // check if only one port is connected
                var connectedPorts = graph
                    .GetAllConnections()
                    .Where(it => it.To == node)
                    .ToList();

                // the refactoring only applies if there is exactly one connection
                if (connectedPorts.Count != 1)
                {
                    return false;
                }

                // get the port type of the currently connected port
                if (!connectedPorts[0].TryGetFromPortType(out var type))
                {
                    return false;
                }

          
                // make a pair of port types based on the index of the currently connected port
                var freePortIndex = connectedPorts[0].FromPort == 0 ? 1 : 0;
                (PortType First, PortType Second) pair = connectedPorts[0].ToPort == 0 ? (type, _portType) : (_portType, type);


                // get the port type of the free port
                if (node.GetPortType(PortId.Input(freePortIndex)) == _portType)
                {
                    // the free port already has the desired type, nothing to do
                    return false;
                }
                
                // now ask the node if it supports a combination of the currently connected and 
                // the switchable port type
                if (!switchableBinaryOperator.Supports(pair.First, pair.Second, out _))
                {
                    return false;
                }

                // now we can build the quick action
                var title = $"to {_portType.HumanReadableName()}";
                result = new QuickAction(title,
                    () => context.PerformRefactoring($"Change port {title}",
                        new SwitchBinaryOperatorInputPortTypesRefactoring(graph, switchableBinaryOperator, pair.First, pair.Second))
                );
                return true;
            }

            result = default;
            return false;
        }
    }
}