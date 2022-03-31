using System.Linq;
using Godot;
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
            // find all graphs which are affected by this and make them refactorable
            var graphs = context.Project.FindContainingReferencesTo(_invokableDescription)
                .Select(context.MakeRefactorable)
                .ToArray();

            // add the new parameters to the invokable description
            _newParameters.ForAll(it => _invokableDescription.Parameters.Add(it));

            // walk over all references to the description
            foreach (var graph in graphs)
            {
                var affectedNodes = graph.GetAllNodes()
                    .Where(it =>
                        it is IReferToAnInvokable iReferToAnInvokable &&
                        iReferToAnInvokable.InvokableDescription == _invokableDescription)
                    .Cast<IReferToAnInvokable>()
                    .ToArray();

                foreach (var node in affectedNodes)
                {
                    // setup ports for the new parameters
                    node.SetupPorts(_invokableDescription);

                    // prepare literal structures for the new parameters
                    var parameterIndex = _invokableDescription.Parameters.Count - _newParameters.Length;
                    for (var i = 0; i < _newParameters.Length; i++)
                    {
                        var inputPortIndex = node.GetParameterInputPort(parameterIndex + i);
                        if (inputPortIndex != -1)
                        {
                            ((ScadNode) node).BuildInputPortLiteral(inputPortIndex);
                        }

                        var outputPortIndex = node.GetParameterOutputPort(parameterIndex + i);
                        if (outputPortIndex != -1)
                        {
                            ((ScadNode) node).BuildOutputPortLiteral(outputPortIndex);
                        }
                    }
                }
            }
        }
    }
}