using System.Linq;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Refactorings
{
    public class DeleteInvokableParametersRefactoring : Refactoring
    {
        private readonly InvokableDescription _description;
        private readonly int[] _parameterIndicesToDrop;

        public DeleteInvokableParametersRefactoring(InvokableDescription description, int[] parameterIndicesToDrop)
        {
            _description = description;
            _parameterIndicesToDrop = parameterIndicesToDrop;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            // first find all nodes that are affected by this and make their graphs refactorable.
            var affectedNodes =
                context.Project.FindAllReferencingNodes(_description)
                    .ToList(); // ensure it is evaluated before we start modifying the graph.

            // now drop the parameters from the description
            // sort the indices descending, so that the indices are not changed when we remove items from the list
            // see https://stackoverflow.com/questions/9908564/remove-list-elements-at-given-indices
            foreach (var index in _parameterIndicesToDrop.OrderByDescending(it => it))
            {
                _description.Parameters.RemoveAt(index);
            }

            // now for each affected node find out which input and output ports refer to the parameters that are to be dropped
            // and drop all connections from or to these ports.
            foreach (var node in affectedNodes)
            {
                foreach (var parameterIndex in _parameterIndicesToDrop)
                {
                    var nodeNodeAsReference = node.NodeAsReference;
                    var inputPort = nodeNodeAsReference.GetParameterInputPort(parameterIndex);
                    var graph = node.Graph;

                    if (inputPort != -1)
                    {
                        graph.GetAllConnections().Where(it => it.IsTo(node.Node, inputPort))
                            .ToList() // because we modify it.
                            .ForAll(it => graph.RemoveConnection(it));

                        // also we need to fix up all connections that are on ports that are after the dropped ones.
                        // temporarily the connections will actually not be correct, but it will be fixed once we drop
                        // the ports.
                        var connectionsToFollowingInputPorts = graph.GetAllConnections()
                            .Where(it => it.To == node.Node && it.ToPort > inputPort)
                            .ToList();

                        // create connections with a fixed input port
                        connectionsToFollowingInputPorts
                            .ForAll(it => graph.AddConnection(it.From.Id, it.FromPort, it.To.Id, it.ToPort - 1));

                        // and remove the other ones
                        connectionsToFollowingInputPorts
                            .ForAll(it => graph.RemoveConnection(it));
                    }

                    var outputPort = nodeNodeAsReference.GetParameterOutputPort(parameterIndex);
                    if (outputPort != -1)
                    {
                        graph.GetAllConnections()
                            .Where(it => it.IsFrom(node.Node, outputPort))
                            .ToList() // because we modify it.
                            .ForAll(it => graph.RemoveConnection(it));

                        // again we need to fix up connections that are on ports after the dropped one
                        var connectionsToFollowingOutputPorts = graph.GetAllConnections()
                            .Where(it => it.From == (node.Node) && it.FromPort > outputPort)
                            .ToList();

                        // create connections with a fixed output port
                        connectionsToFollowingOutputPorts
                            .ForAll(it => graph.AddConnection(it.From.Id, it.FromPort - 1, it.To.Id, it.ToPort));

                        // and remove the other ones
                        connectionsToFollowingOutputPorts
                            .ForAll(it => graph.RemoveConnection(it));
                    }
                }
            }

            // let the node refresh the ports
            foreach (var node in affectedNodes)
            {
                node.NodeAsReference.SetupPorts(_description);
            }

            // finally drop the corresponding literals from all the nodes. Again we sort the indices descending,
            // so that the indices are not changed when we remove items from the list
            foreach (var node in affectedNodes)
            {
                // start with the input literals
                var inputLiteralsToDrop = _parameterIndicesToDrop
                    .Select(it => node.NodeAsReference.GetParameterInputPort(it))
                    .Where(it => it != -1)
                    .OrderByDescending(it => it);

                foreach (var literalIndex in inputLiteralsToDrop)
                {
                    node.Node.DropPortLiteral(PortId.Input(literalIndex));
                    
                    // now swap all input port literals that are after the dropped one with the one before it
                    // so they move up
                    for (var i = literalIndex + 1; i < node.Node.InputPortCount + _parameterIndicesToDrop.Length; i++)
                    {
                        node.Node.SwapInputLiterals(i-1, i);
                    }
                }

                // continue with the output literals
                var outputLiteralsToDrop = _parameterIndicesToDrop
                    .Select(it => node.NodeAsReference.GetParameterOutputPort(it))
                    .Where(it => it != -1)
                    .OrderByDescending(it => it);

                foreach (var literalIndex in outputLiteralsToDrop)
                {
                    node.Node.DropPortLiteral(PortId.Output(literalIndex));
                    // now swap all output port literals that are after the dropped one with the one before it
                    // so they move up
                    for (var i = literalIndex + 1; i < node.Node.OutputPortCount + _parameterIndicesToDrop.Length; i++)
                    {
                        node.Node.SwapOutputLiterals(i-1, i);
                    }
                }
            }
        }
    }
}