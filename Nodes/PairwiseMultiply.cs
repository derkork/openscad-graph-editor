using Godot;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    /// <summary>
    /// Utility node that does a pairwise multiplication of two arrays.
    /// </summary>
    [UsedImplicitly]
    public class PairwiseMultiply : ScadNode, IAmAnExpression
    {
        public override string NodeTitle => "Pairwise multiply";
        public override string NodeDescription => "Multiplies the given two vectors pairwise (each element of the first vector is multiplied with the corresponding element of the second vector).";

        public PairwiseMultiply()
        {
            InputPorts
                .Array()
                .Array();

            OutputPorts
                .Array();
        }

        public override string Render(IScadGraph context)
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
            
            return $"let({var1} = ({first}), {var2} = ({second})) [for(i =[0:max(len({var1}), len({var2}))-1]) (i < len({var1}) ? {var1}[i] : 0) * (i < len({var2}) ? {var2}[i] : 0)]";
        }
    }
}