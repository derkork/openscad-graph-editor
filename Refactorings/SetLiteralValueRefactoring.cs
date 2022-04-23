using System;
using GodotExt;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Refactorings
{
    public class SetLiteralValueRefactoring : NodeRefactoring
    {
        private readonly PortId _port;
        private readonly object _value;

        public SetLiteralValueRefactoring(IScadGraph graph, ScadNode node, PortId port, object value) : base(graph, node)
        {
            _port = port;
            _value = value;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            var reference = context.MakeRefactorable(Holder, Node);

            var hasLiteral = reference.Node.TryGetLiteral(_port, out var literal);
            GdAssert.That(hasLiteral, "Tried to change a literal that doesn't exist");

            switch (literal)
            {
                case BooleanLiteral booleanLiteral:
                    booleanLiteral.Value = (bool) _value;
                    break;
                case NumberLiteral numberLiteral:
                    numberLiteral.Value = (double) _value;
                    break;
                case StringLiteral stringLiteral:
                    stringLiteral.Value = (string) _value;
                    break;
                case Vector3Literal vector3Literal:
                    var array = (double[]) _value;
                    vector3Literal.X = array[0];
                    vector3Literal.Y = array[1];
                    vector3Literal.Z = array[2];
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(literal));
            }
        }
    }
}