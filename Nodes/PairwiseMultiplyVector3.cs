using Godot;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Nodes
{
    /// <summary>
    /// Utility node that does a pairwise multiplication of two vectors.
    /// </summary>
    [UsedImplicitly]
    public class PairwiseMultiplyVector3 : ScadNode, IAmAnExpression, IHaveCustomWidget, IHaveNodeBackground
    {
        public override string NodeTitle => "Pairwise multiply (Vector3)";
        public override string NodeQuickLookup => "V3Pm";

        public override string NodeDescription => "Multiplies the given two vectors pairwise (each element of the first vector is multiplied with the corresponding element of the second vector).";

        

        public PairwiseMultiplyVector3()
        {
            InputPorts
                .Vector3()
                .Vector3();

            OutputPorts
                .Vector3(allowLiteral:false);
        }

        public override string GetPortDocumentation(PortId portId)
        {
            switch (portId.Port)
            {
                case 0 when portId.IsInput:
                    return "The first vector.";
                case 1 when portId.IsInput:
                    return "The second vector.";
                case 0 when portId.IsOutput:
                    return "The result of the pairwise multiplication.";
                default:
                    return "";
            }
        }
        
        public override string Render(ScadGraph context, int portIndex)
        {
            var first = RenderInput(context, 0);
            var second = RenderInput(context, 1);

            if (first.Empty() || second.Empty())
            {
                return "";
            }

            // because first and second could be expressions, we wrap all of this in a let
            // block otherwise we go crazy with the parentheses.
            
            var var1 = Id.UniqueStableVariableName(0);
            var var2 = Id.UniqueStableVariableName(1);
            
            // we can use a simplified version of the code for the vector3 case
            return $"let({var1} = {first}, {var2} = {second}) [{var1}.x*{var2}.x,{var1}.y*{var2}.y,{var1}.z*{var2}.z]";
        }

        public ScadNodeWidget InstantiateCustomWidget()
        {
            return Prefabs.New<SmallNodeWidget>();
        }

        public Texture NodeBackground => Resources.PairwiseMultiplyIcon;
    }
}