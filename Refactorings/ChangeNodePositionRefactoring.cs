using Godot;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Refactorings
{
    public class ChangeNodePositionRefactoring :NodeRefactoring
    {
        private readonly Vector2 _newPosition;
        
        public ChangeNodePositionRefactoring(IScadGraph graph, ScadNode node, Vector2 newPosition) : base(graph, node)
        {
            _newPosition = newPosition;
        }
        
        
        public override void PerformRefactoring(RefactoringContext context)
        {
            Node.Offset = _newPosition;
        }
    }
}