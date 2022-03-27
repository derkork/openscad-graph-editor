using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Refactorings
{
    public class IntroduceInvokableRefactoring : Refactoring
    {
        private readonly InvokableDescription _description;
        private IScadGraph _graph;

        public IntroduceInvokableRefactoring(InvokableDescription description)
        {
            _description = description;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            _graph = context.Project.AddInvokable(_description);
        }

        public override void AfterRefactoring(GraphEditor graphEditor)
        {
            graphEditor.Open(_graph);
        }
    }
}