using GodotExt;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
namespace OpenScadGraphEditor.Refactorings
{
    public class ToggleLiteralRefactoring : NodeRefactoring
    {
        private readonly PortId _port;
        private readonly bool _enabled;

        public ToggleLiteralRefactoring(IScadGraph graph, ScadNode node, PortId port, bool enabled) : base(graph, node)
        {
            _port = port;
            _enabled = enabled;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            var reference = context.MakeRefactorable(Holder, Node);

            var hasLiteral = reference.Node.TryGetLiteral(_port, out var literal);
            GdAssert.That(hasLiteral, "Tried to toggle a literal that doesn't exist");
            
            literal.IsSet = _enabled;
        }
    }
}