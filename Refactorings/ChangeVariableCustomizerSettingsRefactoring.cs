using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Refactorings
{
    public class ChangeVariableCustomizerSettingsRefactoring : Refactoring
    {
        private readonly VariableDescription _description;
        private readonly bool _showInCustomizer;
        private readonly VariableCustomizerDescription _newSettings;

        public ChangeVariableCustomizerSettingsRefactoring(VariableDescription description, bool showInCustomizer, VariableCustomizerDescription newSettings)
        {
            _description = description;
            _showInCustomizer = showInCustomizer;
            _newSettings = newSettings;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            // for now this is simple we overwrite the old settings with the new ones
            // as it has no effect on the node graph
            _description.ShowInCustomizer = _showInCustomizer;
            _description.CustomizerDescription = _newSettings;

        }
    }
}