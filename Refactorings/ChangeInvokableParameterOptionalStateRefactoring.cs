using System.Linq;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Refactorings
{
    /// <summary>
    /// Refactoring which changes the optional state of an invokable parameter.
    /// </summary>
    public class ChangeInvokableParameterOptionalStateRefactoring : Refactoring
    {
        private readonly InvokableDescription _description;
        private readonly int _parameterIndex;
        private readonly bool _newOptionalState;

        public ChangeInvokableParameterOptionalStateRefactoring(InvokableDescription description, int parameterIndex,
            bool newOptionalState)
        {
            _description = description;
            _parameterIndex = parameterIndex;
            _newOptionalState = newOptionalState;
        }
        
        public override void PerformRefactoring(RefactoringContext context)
        {
            var parameterDescription = _description.Parameters[_parameterIndex];
            if (parameterDescription.IsOptional == _newOptionalState)
            {
                // nothing to do.
                return;
            }
            
                            
            // since we have now modified the invokable's optional parameter we need to update all nodes
            // that reference the invokable. 
            var affectedNodes = context.Project.FindAllReferencingNodes(_description)
                .ToList() // avoid concurrent modification
                .Select(context.MakeRefactorable)
                .ToList();

            //  set the parameter's optional value according to the literal state
            parameterDescription.IsOptional = _newOptionalState;


            // fi the parameter was previously mandatory and is now optional, we need to mark the
            // literal for this parameter as "Set", so that nodes will use their previously used value
            // and not new default value
            if (_newOptionalState)
            {
                foreach (var referencingNode in affectedNodes)
                {
                    var matchingInputPort = referencingNode.NodeAsReference.GetParameterInputPort(_parameterIndex);
                    if (referencingNode.Node.TryGetLiteral(PortId.Input(matchingInputPort), out var matchingLiteral))
                    {
                        matchingLiteral.IsSet = true;
                    }
                }
            }
        }
    }
}