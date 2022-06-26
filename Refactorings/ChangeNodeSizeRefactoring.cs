using Godot;
using GodotExt;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Refactorings
{
    public class ChangeNodeSizeRefactoring : NodeRefactoring
    {
        private readonly Vector2 _newSize;

        public ChangeNodeSizeRefactoring(ScadGraph holder, ScadNode node, Vector2 newSize) : base(holder, node)
        {
            _newSize = newSize;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            GdAssert.That(Node is Comment, "Resize is only supported for comments");

            // set the new size
            ((Comment) Node).Size = _newSize;
        }
    }
}