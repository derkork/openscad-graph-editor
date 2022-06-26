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

        public SetLiteralValueRefactoring(ScadGraph graph, ScadNode node, PortId port, object value) : base(graph, node)
        {
            _port = port;
            _value = value;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            var hasLiteral = Node.TryGetLiteral(_port, out var literal);
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
                case NameLiteral nameLiteral:
                    nameLiteral.Value = (string) _value;
                    break;
                case Vector3Literal vector3Literal:
                    var arrayVector3 = (double[]) _value;
                    vector3Literal.X = arrayVector3[0];
                    vector3Literal.Y = arrayVector3[1];
                    vector3Literal.Z = arrayVector3[2];
                    break;
                case Vector2Literal vector3Literal:
                    var arrayVector2 = (double[]) _value;
                    vector3Literal.X = arrayVector2[0];
                    vector3Literal.Y = arrayVector2[1];
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(literal));
            }
        }
    }
}