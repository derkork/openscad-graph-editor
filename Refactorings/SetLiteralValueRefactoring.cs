using System;
using GodotExt;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Refactorings
{
    public class SetLiteralValueRefactoring : NodeRefactoring
    {
        private readonly PortId _port;
        private readonly IScadLiteral _value;

        public SetLiteralValueRefactoring(ScadGraph graph, ScadNode node, PortId port, IScadLiteral value) : base(graph, node)
        {
            _port = port;
            _value = value;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            var hasLiteral = Node.TryGetLiteral(_port, out var literal);
            GdAssert.That(hasLiteral, "Tried to change a literal that doesn't exist");
            GdAssert.That(literal.GetType() == _value.GetType(), "Tried to change a literal to a different type");

            // simply copy over the serialized value
            literal.SerializedValue = _value.SerializedValue;
        }
    }
}