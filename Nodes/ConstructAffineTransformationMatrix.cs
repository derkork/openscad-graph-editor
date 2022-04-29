using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class ConstructAffineTransformationMatrix : ScadNode, IAmAnExpression
    {
        public override string NodeTitle => "Construct Matrix";
        public override string NodeDescription => "Constructs an affine transformation matrix for use with 'multmatrix'.";


        public ConstructAffineTransformationMatrix()
        {
            InputPorts
                .Vector3("Scale")
                .Vector3("Translate")
                .Vector2("Shear X along Y/Z")
                .Vector2("Shear Y along X/Z")
                .Vector2("Shear Z along X/Y");

            OutputPorts
                .Array();
        }

        public override string Render(IScadGraph context)
        {
            var scale = RenderInput(context, 0).OrDefault("[1,1,1]");
            var translate = RenderInput(context, 1).OrDefault("[0,0,0]");
            var shearX = RenderInput(context, 2).OrDefault("[0,0]");
            var shearY = RenderInput(context, 3).OrDefault("[0,0]");
            var shearZ = RenderInput(context, 4).OrDefault("[0,0]");

            var scaleVar = Id.UniqueStableVariableName(0);
            var translateVar = Id.UniqueStableVariableName(1);
            var shearXVar = Id.UniqueStableVariableName(2);
            var shearYVar = Id.UniqueStableVariableName(3);
            var shearZVar = Id.UniqueStableVariableName(4);

            return $"let({scaleVar} = {scale}, \n    {translateVar} = {translate},\n    {shearXVar} = {shearX},\n    {shearYVar} = {shearY},\n    {shearZVar} = {shearZ}\n) [ [{scaleVar}.x, {shearXVar}.x, {shearXVar}.y, {translateVar}.x],\n    [{shearYVar}.x, {scaleVar}.y, {shearYVar}.y, {translateVar}.y],\n    [{shearZVar}.x, {shearZVar}.y, {scaleVar}.z, {translateVar}.z],\n    [0, 0, 0, 1] ]\n";
        }
    }
}