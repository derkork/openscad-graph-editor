using System.Linq;
using GodotExt;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Refactorings
{
    public class ChangeParameterOrderRefactoring : Refactoring
    {
        private readonly InvokableDescription _description;
        private readonly string[] _newParameterNames;

        public ChangeParameterOrderRefactoring(InvokableDescription description, string[] newParameterNames)
        {
            _description = description;
            _newParameterNames = newParameterNames;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            // ensure that parameter counts and names match the description
            GdAssert.That(_description.Parameters.Count == _newParameterNames.Length, "Parameter count mismatch");
            GdAssert.That(_description.Parameters
                .Select(it => it.Name)
                .All(it => _newParameterNames.Contains(it)), "Parameter names mismatch");


            // first find all the graphs that may be affected by this refactoring and make them refactorable
            var graphs = context.Project.FindContainingReferencesTo(_description)
                .Select(context.MakeRefactorable)
                .ToList();

            // iterate over the parameter names and find one position that is not in the correct order
            var currentPosition = 0;
            var numberOfParameters = _description.Parameters.Count;
            while (currentPosition < numberOfParameters - 1)
            {
                var requiredParameterName = _newParameterNames[currentPosition];
                var actualParameterName = _description.Parameters[currentPosition].Name;

                // if the parameter names are equal we can move on to the next position
                if (requiredParameterName == actualParameterName)
                {
                    currentPosition++;
                    continue;
                }

                // find the actual parameter index of the required parameter name
                var actualParameterIndex = _description.Parameters
                    .Indices()
                    .First(it => _description.Parameters[it].Name == requiredParameterName);
                GdAssert.That(actualParameterIndex > currentPosition,
                    "Parameter index should be greater than current position.");

                // swap the parameters in the description
                (_description.Parameters[currentPosition], _description.Parameters[actualParameterIndex]) =
                    (_description.Parameters[actualParameterIndex], _description.Parameters[currentPosition]);


                // now that we have found the actual parameter index, we can swap the parameters
                foreach (var graph in graphs)
                {
                    // find affected nodes
                    var affectedNodes = graph.GetAllNodes()
                        .Where(it =>
                            it is IReferToAnInvokable iReferToAnInvokable &&
                            iReferToAnInvokable.InvokableDescription == _description)
                        .Cast<IReferToAnInvokable>()
                        .ToList();

                    foreach (var node in affectedNodes)
                    {
                        var scadNode = (ScadNode) node;
                        // rebuild the ports
                        node.SetupPorts(_description);

                        // swap out the literals and connections for the input ports
                        var firstInputPort = node.GetParameterInputPort(currentPosition);
                        if (firstInputPort != -1)
                        {
                            var secondInputPort = node.GetParameterInputPort(actualParameterIndex);
                            GdAssert.That(secondInputPort != -1, "Could not find second input port");

                            // swap the literals
                            scadNode.SwapInputLiterals(firstInputPort, secondInputPort);

                            // and swap the connections
                            var connectionsToFirstInput = graph.GetAllConnections()
                                .Where(it => it.IsTo(scadNode, firstInputPort))
                                .ToList();

                            var connectionsToSecondInput = graph.GetAllConnections()
                                .Where(it => it.IsTo(scadNode, secondInputPort))
                                .ToList();

                            // drop all these connections
                            connectionsToFirstInput.ForAll(graph.RemoveConnection);
                            connectionsToSecondInput.ForAll(graph.RemoveConnection);

                            // now build them again with swapped ports
                            connectionsToFirstInput
                                .ForAll(it => graph.AddConnection(it.From.Id, it.FromPort, it.To.Id, secondInputPort));
                            connectionsToSecondInput
                                .ForAll(it => graph.AddConnection(it.From.Id, it.FromPort, it.To.Id, firstInputPort));
                        }

                        // and the same for the output ports
                        var firstOutputPort = node.GetParameterOutputPort(currentPosition);
                        if (firstOutputPort != -1)
                        {
                            var secondOutputPort = node.GetParameterOutputPort(actualParameterIndex);
                            GdAssert.That(secondOutputPort != -1, "Could not find second output port");

                            // swap the literals
                            scadNode.SwapOutputLiterals(firstOutputPort, secondOutputPort);

                            // and swap the connections
                            var connectionsFromFirstOutput = graph.GetAllConnections()
                                .Where(it => it.IsFrom(scadNode, firstOutputPort))
                                .ToList();

                            var connectionsFromSecondOutput = graph.GetAllConnections()
                                .Where(it => it.IsFrom(scadNode, secondOutputPort))
                                .ToList();

                            // drop all these connections
                            connectionsFromFirstOutput.ForAll(graph.RemoveConnection);
                            connectionsFromSecondOutput.ForAll(graph.RemoveConnection);

                            // now build them again with swapped ports
                            connectionsFromFirstOutput
                                .ForAll(it => graph.AddConnection(it.From.Id, secondOutputPort, it.To.Id, it.ToPort));
                            connectionsFromSecondOutput
                                .ForAll(it => graph.AddConnection(it.From.Id, firstOutputPort, it.To.Id, it.ToPort));
                        }
                    }
                }

                currentPosition++;
            }
        }
    }
}