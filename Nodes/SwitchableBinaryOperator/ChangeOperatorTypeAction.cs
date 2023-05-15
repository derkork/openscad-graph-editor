using System;
using OpenScadGraphEditor.Actions;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Nodes.SwitchableBinaryOperator
{
    public class ChangeOperatorTypeAction : IEditorAction
    {
        private readonly Type _nodeType;
        public int Order => 1;
        public string Group => "Change operator";

        private readonly SwitchableBinaryOperator _example;

        public ChangeOperatorTypeAction(Type nodeType)
        {
            _nodeType = nodeType;
            _example = (SwitchableBinaryOperator) NodeFactory.Build(nodeType);
        }

        public bool TryBuildQuickAction(IEditorContext context, RequestContext item, out QuickAction result)
        {
            if (item.TryGetNode(out var graph, out var node)
                && node is SwitchableBinaryOperator oldOperator
                && node.GetType() != _nodeType
                && (
                    // the new operator type must either support 'any' or it must support
                    // both operand types of the old operator
                    _example.Supports(PortType.Any) ||
                    (
                        _example.Supports(
                            oldOperator.GetPortType(PortId.Input(0))) &&
                        _example.Supports(
                            oldOperator.GetPortType(PortId.Input(1)))
                    )
                )
               )
            {
                var title = $"to {_example.NodeTitle}";
                result = new QuickAction(title,
                    () => context.PerformRefactoring($"Change operator {title}", 
                        new ChangeOperatorTypeRefactoring(graph, oldOperator, _nodeType))
                );
                return true;
            }
            
            result = default;
            return false;
        }
    }
}