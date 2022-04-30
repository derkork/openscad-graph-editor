using Godot;
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

            // if the literal is for a parameter of a function or module entry point
            // toggling the literal implicitly sets the "optional" status of the respective parameter.
            if (!_port.IsOutput || !(reference.Node is EntryPoint) ||
                !(reference.Node is IReferToAnInvokable iReferToAnInvokable))
            {
                // this is not the case so we're done here.
                return;
            }

            // try to find out which parameter is being referenced
            var invokableDescription = iReferToAnInvokable.InvokableDescription;
            for (var parameterIndex = 0; parameterIndex < invokableDescription.Parameters.Count; parameterIndex++)
            {
                if (iReferToAnInvokable.GetParameterOutputPort(parameterIndex) == _port.Port)
                {
                    context.PerformRefactoring(new ChangeInvokableParameterOptionalStateRefactoring(
                        invokableDescription, parameterIndex,
                        _enabled));
                    // only one parameter is changed, no need to iterate the others
                    break;
                }
            }
        }
    }
}