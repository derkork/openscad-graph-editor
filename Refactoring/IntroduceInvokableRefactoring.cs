using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Refactoring
{
    public class IntroduceInvokableRefactoring : Refactoring
    {
        public InvokableDescription Description { get; }

        public IntroduceInvokableRefactoring(InvokableDescription description)
        {
            Description = description;
        }

    }
}