using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Refactoring
{
    public class IntroduceInvokableRefactoring : Refactoring
    {
        private readonly InvokableDescription _description;
        private IScadGraph _graph;

        public IntroduceInvokableRefactoring(InvokableDescription description)
        {
            _description = description;
        }

        public override void PerformRefactoring(ScadProject project)
        {
            _graph = project.AddInvokable(_description);
        }

        public override void AfterRefactoring(GraphEditor graphEditor)
        {
            graphEditor.Open(_graph);
        }
    }
}