using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Refactoring
{
    public class IntroduceVariableRefactoring : Refactoring
    {
        private readonly VariableDescription _description;

        public IntroduceVariableRefactoring(VariableDescription description)
        {
            _description = description;
        }

        public override void PerformRefactoring(ScadProject project)
        {
            project.AddVariable(_description);
        }
    }
}