using System.Linq;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Refactorings
{
    public class DropInvokableParametersRefactoring : Refactoring
    {
        private readonly InvokableDescription _description;
        private readonly int[] _parameterIndicesToDrop;

        public DropInvokableParametersRefactoring(InvokableDescription description, int[] parameterIndicesToDrop)
        {
            _description = description;
            _parameterIndicesToDrop = parameterIndicesToDrop;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            // first find all graphs that are affected by this and make them refactorable.
            var graphs =
                context.Project.FindContainingReferencesTo(_description)
                    .Select(context.MakeRefactorable)
                    .ToList();

            // now drop the parameters from the description
            // sort the indices descending, so that the indices are not changed when we remove items from the list
            // see https://stackoverflow.com/questions/9908564/remove-list-elements-at-given-indices
            foreach (var index in _parameterIndicesToDrop.OrderByDescending(it => it))
            {
                _description.Parameters.RemoveAt(index);
            }

            // then for each graph find all nodes that are affected by this.
            foreach (var graph in graphs)
            {
                var affectedNodes = graph.GetAllNodes()
                    .Where(it =>
                        it is IReferToAnInvokable iReferToAnInvokable &&
                        iReferToAnInvokable.InvokableDescription == _description)
                    .Cast<IReferToAnInvokable>()
                    .ToList();

                // now for each affected node find out which input and output ports refer to the parameters that are to be dropped
                // and drop all connections from or to these ports.
                foreach (var node in affectedNodes)
                {
                    foreach (var parameterIndex in _parameterIndicesToDrop)
                    {
                        var inputPort = node.GetParameterInputPort(parameterIndex);
                        if (inputPort != -1)
                        {
                            graph.GetAllConnections().Where(it => it.IsTo((ScadNode) node, inputPort))
                                .ToList() // because we modify it.
                                .ForAll(it => graph.RemoveConnection(it));

                            // also we need to fix up all connections that are on ports that are after the dropped ones.
                            // temporarily the connections will actually not be correct, but it will be fixed once we drop
                            // the ports.
                            var connectionsToFollowingInputPorts = graph.GetAllConnections()
                                .Where(it => it.To == ((ScadNode) node) && it.ToPort > inputPort)
                                .ToList();

                            // create connections with a fixed input port
                            connectionsToFollowingInputPorts
                                .ForAll(it => graph.AddConnection(it.From.Id, it.FromPort, it.To.Id, it.ToPort - 1));

                            // and remove the other ones
                            connectionsToFollowingInputPorts
                                .ForAll(it => graph.RemoveConnection(it));
                        }

                        var outputPort = node.GetParameterOutputPort(parameterIndex);
                        if (outputPort != -1)
                        {
                            graph.GetAllConnections()
                                .Where(it => it.IsFrom((ScadNode) node, outputPort))
                                .ToList() // because we modify it.
                                .ForAll(it => graph.RemoveConnection(it));
                            
                            // again we need to fix up connections that are on ports after the dropped one
                            var connectionsToFollowingOutputPorts = graph.GetAllConnections()
                                .Where(it => it.From == ((ScadNode) node) && it.FromPort > outputPort)
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
                    node.SetupPorts(_description);
                }

                // finally drop the corresponding literals from all the nodes. Again we sort the indices descending,
                // so that the indices are not changed when we remove items from the list
                foreach (var node in affectedNodes)
                {
                    // start with the input literals
                    var inputLiteralsToDrop = _parameterIndicesToDrop
                        .Select(it => node.GetParameterInputPort(it))
                        .Where(it => it != -1)
                        .OrderByDescending(it => it);

                    foreach (var literalIndex in inputLiteralsToDrop)
                    {
                        ((ScadNode) node).DropInputPortLiteral(literalIndex);
                    }

                    // continue with the output literals
                    var outputLiteralsToDrop = _parameterIndicesToDrop
                        .Select(it => node.GetParameterOutputPort(it))
                        .Where(it => it != -1)
                        .OrderByDescending(it => it);

                    foreach (var literalIndex in outputLiteralsToDrop)
                    {
                        ((ScadNode) node).DropOutputPortLiteral(literalIndex);
                    }
                }
            }
        }
    }
}