using Godot;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Refactorings
{
    public class ChangeNodePositionRefactoring :Refactoring
    {
        private readonly IScadGraph _graph;
        private readonly ScadNode _node;
        private readonly Vector2 _newPosition;
        
        public ChangeNodePositionRefactoring(IScadGraph graph, ScadNode node, Vector2 newPosition)
        {
            _graph = graph;
            _node = node;
            _newPosition = newPosition;
        }
        
        
        public override void PerformRefactoring(RefactoringContext context)
        {
            var reference = context.MakeRefactorable(_graph, _node);
            reference.Node.Offset = _newPosition;
        }
    }
}