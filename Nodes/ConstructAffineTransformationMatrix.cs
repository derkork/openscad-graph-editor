using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class ConstructAffineTransformationMatrix : ScadNode, IAmAnExpression
    {
        public override string NodeTitle => "Construct Matrix";

        public override string NodeDescription =>
            "Constructs an affine transformation matrix for use with 'multmatrix'.";


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

        public override string GetPortDocumentation(PortId portId)
        {
            switch (portId.Port)
            {
                case 0 when portId.IsInput:
                    return "How much to scale the object along each axis.";
                case 1 when portId.IsInput:
                    return "How much to translate the object along each axis.";
                case 2 when portId.IsInput:
                    return "How much to shear the object along the Y/Z axis.";
                case 3 when portId.IsInput:
                    return "How much to shear the object along the X/Z axis.";
                case 4 when portId.IsInput:
                    return "How much to shear the object along the X/Y axis.";
                case 0 when portId.IsOutput:
                    return "The constructed matrix.";
                default: 
                    return "";
            }
        }

        public override string Render(ScadGraph context, int portIndex)
        {
            var scale = RenderInput(context, 0).OrDefault("[1,1,1]");
            var translate = RenderInput(context, 1).OrDefault("[0,0,0]");
            var shearX = RenderInput(context, 2).OrDefault("[0,0]");
            var shearY = RenderInput(context, 3).OrDefault("[0,0]");
            var shearZ = RenderInput(context, 4).OrDefault("[0,0]");

            return
                $"[ [{scale}.x, {shearX}.x, {shearX}.y, {translate}.x],\n    [{shearY}.x, {scale}.y, {shearY}.y, {translate}.y],\n    [{shearZ}.x, {shearZ}.y, {scale}.z, {translate}.z],\n    [0, 0, 0, 1] ]\n";
        }
    }
}