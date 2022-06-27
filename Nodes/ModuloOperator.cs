using Godot;
using JetBrains.Annotations;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class ModuloOperator : BinaryOperator
    {
        public override string NodeTitle => "%";
        public override string NodeQuickLookup => "%";
        public override string NodeDescription => "Calculates the modulus the given inputs.";
        protected override string OperatorSign => "%";

        public override Texture NodeBackground => Resources.ModulusIcon; 

        public ModuloOperator()
        {
            InputPorts
                .Number()
                .Number();

            OutputPorts
                .Number(allowLiteral: false);
        }
    }
}