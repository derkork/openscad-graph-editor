using System.Linq;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Refactorings
{
    public class AddInvokableParametersRefactoring : Refactoring
    {
        private readonly InvokableDescription _invokableDescription;
        private readonly ParameterDescription[] _newParameters;

        public AddInvokableParametersRefactoring(InvokableDescription invokableDescription,
            ParameterDescription[] newParameters)
        {
            _invokableDescription = invokableDescription;
            _newParameters = newParameters;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            // find all nodes which are affected by this and make them refactorable
            var affectedNodes = context.Project.FindAllReferencingNodes(_invokableDescription)
                .ToList(); // avoid concurrent modification

            // add the new parameters to the invokable description. Only do this AFTER
            // we have made everything refactorable, otherwise the internal state of the graphs is outdated.
            _newParameters.ForAll(it => _invokableDescription.Parameters.Add(it));

            // walk over all references to the description
            foreach (var referencingNode in affectedNodes)
            {
                // make the owning graph refactorable
                var node = referencingNode.Node;
                var nodeAsReference = referencingNode.NodeAsReference;

                // setup ports for the new parameters
                nodeAsReference.SetupPorts(_invokableDescription);

                // prepare literal structures for the new parameters
                var parameterIndex = _invokableDescription.Parameters.Count - _newParameters.Length;
                for (var i = 0; i < _newParameters.Length; i++)
                {
                    var inputPortIndex = nodeAsReference.GetParameterInputPort(parameterIndex + i);
                    if (inputPortIndex != -1)
                    {
                        node.BuildPortLiteral(PortId.Input(inputPortIndex));
                    }

                    var outputPortIndex = nodeAsReference.GetParameterOutputPort(parameterIndex + i);
                    if (outputPortIndex != -1)
                    {
                        node.BuildPortLiteral(PortId.Output(outputPortIndex));
                    }
                }
            }
        }
    }
}