using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Refactorings
{
    public class ChangeVariableDefaultValueRefactoring : Refactoring
    {
        private readonly VariableDescription _description;
        [CanBeNull] private readonly IScadLiteral _newDefaultValue;

        public ChangeVariableDefaultValueRefactoring(VariableDescription description, [CanBeNull] IScadLiteral newDefaultValue)
        {
            _description = description;
            _newDefaultValue = newDefaultValue;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            // for now this is simple we overwrite the old default value with the new one
            _description.DefaultValue = _newDefaultValue;
        }
    }
}