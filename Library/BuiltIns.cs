using System;
using System.Collections.Generic;
using System.Linq;
using GodotExt;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Library
{
    /// <summary>
    /// This class contains all built-in modules, functions and nodes.
    /// </summary>
    public static class BuiltIns
    {
        public static IReadOnlyCollection<ModuleDescription> Modules { get; }
        public static IReadOnlyCollection<FunctionDescription> Functions { get; }
        public static IReadOnlyCollection<VariableDescription> Variables { get; }

        public static IReadOnlyCollection<Type> LanguageLevelNodes { get; }

        static BuiltIns()
        {
            LanguageLevelNodes = typeof(ScadNode).GetImplementors()
                // only nodes which can be directly created
                // TODO: make a proper marker interface for language level nodes. 
                .Where(it => !typeof(ICannotBeCreated).IsAssignableFrom(it))
                .ToList();

            
            Modules = new List<ModuleDescription>()
            {
                // 2D Modules
                // circle (radius)
                ModuleBuilder.NewBuiltInModule("circle", "Circle (Radius)", "radius")
                    .WithDescription("Creates a circle at the origin.")
                    .WithParameter("r", PortType.Number, label: "Radius", optional: true, description: "The radius of the circle.")
                    .WithFragmentParameters()
                    .Build(),
                // circle (diameter)
                ModuleBuilder.NewBuiltInModule("circle", "Circle (Diameter)", "diameter")
                    .WithDescription("Creates a circle at the origin.")
                    .WithParameter("d", PortType.Number, label: "Diameter", optional: true, description: "The diameter of the circle.")
                    .WithFragmentParameters()
                    .Build(),
                // square
                ModuleBuilder.NewBuiltInModule("square", "Square")
                    .WithDescription("Creates a square at the origin.")
                    .WithParameter("size", PortType.Number, label: "Size", optional: true, description: "The length of the sides of the square.")
                    .WithParameter("center", PortType.Boolean, label: "Center", optional: true, description: "Whether to center the square at the origin.")
                    .Build(),
                // rectangle
                ModuleBuilder.NewBuiltInModule("square", "Rectangle", "rectangle")
                    .WithDescription("Creates a rectangle or square at the origin.")
                    .WithParameter("size", PortType.Vector2, label: "Size", optional: true, description: "The length of the sides of the rectangle.")
                    .WithParameter("center", PortType.Boolean, label: "Center", optional: true, description: "Whether to center the rectangle at the origin.")
                    .Build(),
                // polygon
                ModuleBuilder.NewBuiltInModule("polygon", "Polygon")
                    .WithDescription("Creates a polygon.")
                    .WithParameter("points", PortType.Array, label: "Points", optional: true, description: "The points of the polygon. You can use a 'Construct Vector (Vector2)' node to build these.")
                    .WithParameter("paths", PortType.Array, label: "Paths", optional: true, description: "The paths. Can be empty, a single vector or a vector of vectors.")
                    .WithParameter("convexity", PortType.Number, label: "Convexity", optional: true,  description: "Integer number of 'inward' curves, ie. expected path crossings of an arbitrary line through the polygon.")
                    .Build(),

                // projection
                ModuleBuilder.NewBuiltInModule("projection", "Projection")
                    .WithDescription("Creates a 2D drawing from a 3D model. Projects a 3D model to the (x,y) plane, with z at 0.")
                    .WithParameter("cut", PortType.Boolean, label: "Cut", optional: true, description: "If cut=true, only points with z=0 are considered (effectively cutting the object), with cut=false(the default), points above and below the plane are considered as well (creating a proper projection)")
                    .WithChildren()
                    .Build(),
                
                // text
                ModuleBuilder.NewBuiltInModule("text", "Text")
                    .WithDescription("Creates text as a 2D geometric object, using fonts installed on the local system or provided as separate font file")
                    .WithParameter("text", PortType.String, label: "Text", description: "The text to generate.")
                    .WithParameter("size", PortType.Number, label: "Size", optional: true, description: " The generated text has an ascent (height above the baseline) of approximately the given value. Different fonts can vary somewhat and may not fill the size specified exactly, typically they render slightly smaller.")
                    .WithParameter("font", PortType.String, label: "Font", optional: true, description: "The name of the font that should be used. This is not the name of the font file, but the logical font name (internally handled by the fontconfig library).")
                    .WithParameter("halign", PortType.String, label: "Horizontal Alignment", optional: true, description: "The horizontal alignment of the text. Can be 'left', 'center' or 'right'.")
                    .WithParameter("valign", PortType.String, label: "Vertical Alignment", optional: true, description: "The vertical alignment of the text. Can be 'top', 'center', 'baseline' or 'bottom'.")
                    .WithParameter("spacing", PortType.Number, label: "Spacing", optional: true, description: "The spacing between characters.  The default value of 1 results in the normal spacing for the font, giving a value greater than 1 causes the letters to be spaced further apart.")
                    .WithParameter("direction", PortType.String, label: "Direction", optional: true, description: "Direction of the text flow. Possible values are 'ltr' (left-to-right), 'rtl' (right-to-left), 'ttb' (top-to-bottom) and 'btt' (bottom-to-top).")
                    .WithParameter("language", PortType.String, label: "Language", optional: true, description: "The language of the text. Default is 'en' (English).")
                    .WithParameter("script", PortType.String, label: "Script", optional: true, description: "The script of the text. Default is 'latin'.")
                    .WithParameter(name: "$fn", PortType.Number, label:"Smoothness", optional:true, description: "Used for subdividing the curved path segments. Higher values result in smoother curves.")
                    .Build(),
                
                // 3D Modules
                // cube
                ModuleBuilder.NewBuiltInModule("cube", "Cube")
                    .WithDescription("Creates a cube in the first octant.When center is true, the cube is centered on the origin.")
                    .WithParameter("size", PortType.Vector3, label: "Size", optional: true, description: "Size of the cube.")
                    .WithParameter("center", PortType.Boolean, label: "Center", optional: true, description: "Whether to center the cube on the origin.")
                    .Build(),
                // sphere
                ModuleBuilder.NewBuiltInModule("sphere", "Sphere (Radius)", "radius")
                    .WithDescription("Creates a sphere.")
                    .WithParameter("r", PortType.Number, label: "Radius", optional: true, description: "The radius of the sphere.")
                    .WithFragmentParameters()
                    .Build(),
                // sphere (diameter)
                ModuleBuilder.NewBuiltInModule("sphere", "Sphere (Diameter)", "diameter")
                    .WithDescription("Creates a sphere.")
                    .WithParameter("d", PortType.Number, label: "Diameter", optional: true, description: "The diameter of the sphere.")
                    .WithFragmentParameters()
                    .Build(),
                // cylinder (radius)
                ModuleBuilder.NewBuiltInModule("cylinder", "Cylinder (Radius)", "radius")
                    .WithDescription("Creates a cylinder.")
                    .WithParameter("h", PortType.Number, label: "Height", optional: true, description: "The height of the cylinder.")
                    .WithParameter("r", PortType.Number, label: "Radius", optional: true, description: "The radius of the cylinder.")
                    .WithParameter("center", PortType.Boolean, label: "Center", optional: true, description: "Whether to center the cylinder on the origin.")
                    .WithFragmentParameters()
                    .Build(),
                // cylinder diameter
                ModuleBuilder.NewBuiltInModule("cylinder", "Cylinder (Diameter)", "diameter")
                    .WithDescription("Creates a cylinder.")
                    .WithParameter("h", PortType.Number, label: "Height", optional: true, description: "The height of the cylinder.")
                    .WithParameter("d", PortType.Number, label: "Diameter", optional: true, description: "The diameter of the cylinder.")
                    .WithParameter("center", PortType.Boolean, label: "Center", optional: true,  description: "Whether to center the cylinder on the origin.")
                    .WithFragmentParameters()
                    .Build(),
                // cone (radius)
                ModuleBuilder.NewBuiltInModule("cylinder", "Cone (Radius)", "cone_radius")
                    .WithDescription("Creates a cone (or cylinder).")
                    .WithParameter("h", PortType.Number, label: "Height", optional: true, description: "The height of the cone.")
                    .WithParameter("r1", PortType.Number, label: "Radius 1", optional: true, description: "The radius of the base of the cone.")
                    .WithParameter("r2", PortType.Number, label: "Radius 2", optional: true, description:"The radius of the top of the cone.")
                    .WithParameter("center", PortType.Boolean, label: "Center", optional: true, description: "Whether to center the cone on the origin.")
                    .WithFragmentParameters()
                    .Build(),
                // cone(diameter)
                ModuleBuilder.NewBuiltInModule("cylinder", "Cone (Diameter)", "cone_diameter")
                    .WithDescription("Creates a cone (or cylinder).")
                    .WithParameter("h", PortType.Number, label: "Height", optional: true, description: "The height of the cone.")
                    .WithParameter("d1", PortType.Number, label: "Diameter 1", optional: true, description: "The diameter of the base of the cone.")
                    .WithParameter("d2", PortType.Number, label: "Diameter 2", optional: true, description: "The diameter of the top of the cone.")
                    .WithParameter("center", PortType.Boolean, label: "Center", optional: true, description: "Whether to center the cone on the origin.")
                    .WithFragmentParameters()
                    .Build(),
                // polyhedron
                ModuleBuilder.NewBuiltInModule("polyhedron", "Polyhedron")
                    .WithDescription("Creates a polyhedron.")
                    .WithParameter("points", PortType.Vector3, label: "Points", optional: true, description: "The points of the polyhedron. You can use a 'Construct Vector (Vector3) node to create the points.")
                    .WithParameter("faces", PortType.Vector3, label: "Faces", optional: true, description: "The faces of the polyhedron. Each face is a vector containing the indices (0 based) of 3 or more points from the points vector. Faces may be defined in any order. Define enough faces to fully enclose the solid, with no overlap. All faces must have points ordered in clockwise direction when looking at each face from outside inward.")
                    .WithParameter("convexity", PortType.Number, label: "Convexity", optional: true, description: "The convexity parameter specifies the maximum number of faces a ray intersecting the object might penetrate.  For display problems, setting it to 10 should work fine for most cases.")
                    .Build(),
                // linear extrude
                ModuleBuilder.NewBuiltInModule("linear_extrude", "Linear Extrude")
                    .WithDescription(
                        "Linear Extrusion is an operation that takes a 2D object as input and generates a 3D object as a result.")
                    .WithParameter("height", PortType.Number, label: "Height", optional: true, description: "The height of the extrusion.")
                    .WithParameter("center", PortType.Boolean, label: "Center", optional: true, description: "Whether to vertically center the extruded object.")
                    .WithParameter("convexity", PortType.Number, label: "Convexity", optional: true,  description: "The convexity parameter specifies the maximum number of faces a ray intersecting the object might penetrate.  For display problems, setting it to 10 should work fine for most cases.")
                    .WithParameter("twist", PortType.Number, label: "Twist", optional: true, description: "Twist is the number of degrees of through which the shape is extruded. Setting the parameter twist = 360 extrudes through one revolution")
                    .WithParameter("slices", PortType.Number, label: "Slices", optional: true, description: "Defines the number of intermediate points along the Z axis of the extrusion. Higher values result in smoother results.")
                    .WithParameter("scale", PortType.Any, label: "Scale", optional: true, description: "Scales the 2D shape by this value over the height of the extrusion. Scale can be a scalar or a vector.")
                    .WithParameter("$fn", PortType.Number, label: "Resolution", optional: true, description: "The resolution of the linear_extrude (higher numbers bring more 'smoothness', but more computation time is needed).")
                    .WithChildren()
                    .Build(),
                // rotate_extrude
                ModuleBuilder.NewBuiltInModule("rotate_extrude", "Rotate Extrude")
                    .WithDescription("Rotational extrusion spins a 2D shape around the Z-axis to form a solid which has rotational symmetry.")
                    .WithParameter("angle", PortType.Number, label: "Angle", optional: true, description: "The angle of rotation in degrees.")
                    .WithParameter("convexity", PortType.Number, label: "Convexity", optional: true, description: "The convexity parameter specifies the maximum number of faces a ray intersecting the object might penetrate.  For display problems, setting it to 10 should work fine for most cases.")
                    .WithFragmentParameters()
                    .WithChildren()
                    .Build(),
                // Transformations
                // translate
                ModuleBuilder.NewBuiltInModule("translate", "Translate")
                    .WithDescription("Translates (moves) its child elements along the specified offset.")
                    .WithParameter("v", PortType.Vector3, label: "Offset", description: "How much to translate the object in each direction.")
                    .WithChildren()
                    .Build(),
                // rotate (axis/angle)
                ModuleBuilder.NewBuiltInModule("rotate", "Rotate (Axis/Angle)", "axis_angle")
                    .WithDescription("Rotates the next elements along the given axis and angle.")
                    .WithParameter("v", PortType.Vector3, label: "Axis", description: "The axis around which to rotate.")
                    .WithParameter("a", PortType.Number, label: "Angle", description: "The angle of rotation in degrees.")
                    .WithChildren()
                    .Build(),
                // rotate (euler angles)
                ModuleBuilder.NewBuiltInModule("rotate", "Rotate (Euler Angles)", "euler_angles")
                    .WithDescription("Rotates the next elements along the given Euler angles.")
                    .WithParameter("a", PortType.Vector3, label: "Euler Angles", description: "Rotation angles around the global X,Y and Z axis in degrees.")
                    .WithChildren()
                    .Build(),
                // scale
                ModuleBuilder.NewBuiltInModule("scale", "Scale")
                    .WithDescription("Scales its child elements using specified scale vector.")
                    .WithParameter("v", PortType.Vector3, label: "Scale", description: "How much to scale the object in each direction.")
                    .WithChildren()
                    .Build(),
                // resize
                ModuleBuilder.NewBuiltInModule("resize", "Resize")
                    .WithDescription("Modifies the size of the child object to match the given x,y, and z size.")
                    .WithParameter("newsize", PortType.Vector3, label: "New size", description: "The new size of the object. If x,y, or z is 0 then that dimension is left as-is.")
                    .WithParameter("auto", PortType.Any, label: "Auto", optional: true, description: "If it is set to true, it auto-scales any 0-dimensions to match. This can also be a vector with 3 booleans which allows you to specify the auto behaviour per axis.")
                    .WithChildren()
                    .Build(),
                // mirror
                ModuleBuilder.NewBuiltInModule("mirror", "Mirror")
                    .WithDescription("Mirrors the child element on a plane through the origin.")
                    .WithParameter("v", PortType.Vector3, label: "Normal", description: "The normal vector of the plane at which to mirror.")
                    .WithChildren()
                    .Build(),
                // multmatrix
                ModuleBuilder.NewBuiltInModule("multmatrix", "Multmatrix")
                    .WithDescription("Multiplies the geometry of all child elements with the given affine transformation matrix.")
                    .WithParameter("m", PortType.Array, label: "Matrix", description: "The affine transformation matrix. You can use the 'Construct Matrix' module to create one.")
                    .WithChildren()
                    .Build(),
                // offset (radius)
                ModuleBuilder.NewBuiltInModule("offset", "Offset (Radius)", "radius")
                    .WithDescription("Generates a new 2d interior or exterior outline from an existing outline. This mode produces round corners.")
                    .WithParameter("r", PortType.Number, label: "Radius", description: "The radius of the corners.")
                    .WithChildren()
                    .Build(),
                // offset (delta)
                ModuleBuilder.NewBuiltInModule("offset", "Offset (Delta)", "delta")
                    .WithDescription("Generates a new 2d interior or exterior outline from an existing outline. This mode produces angled corners.")
                    .WithParameter("delta", PortType.Number, label: "Delta", description: "Delta specifies the distance of the new outline from the original outline.")
                    .WithParameter("chamfer", PortType.Boolean, label: "Chamfer", optional: true, description: "This flag defines if edges should be chamfered (cut off with a straight line) or not (extended to their intersection).")
                    .WithChildren()
                    .Build(),
                // hull
                ModuleBuilder.NewBuiltInModule("hull", "Hull")
                    .WithDescription("Displays the convex hull of child nodes.")
                    .WithChildren()
                    .Build(),
                // minkowski
                ModuleBuilder.NewBuiltInModule("minkowski", "Minkowski")
                    .WithDescription("Displays the minkowski sum of child nodes.")
                    .WithChildren()
                    .Build(),
            };

            // find out if we have any duplicate module ids
            var duplicateModuleIds = Modules.GroupBy(m => m.Id).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            GdAssert.That(duplicateModuleIds.Count == 0,
                "Duplicate module ids found: " + string.Join(", ", duplicateModuleIds));

            Functions = new List<FunctionDescription>()
            {
                // abs
                FunctionBuilder.NewBuiltInFunction("abs", "Abs", PortType.Number, returnValueDescription: "The absolute value of the input.")
                    .WithDescription("Returns the absolute value of a number.")
                    .WithParameter("number", PortType.Number, description: "The number to take the absolute value of.")
                    .Build(),
                // sign
                FunctionBuilder.NewBuiltInFunction("sign", "Sign", PortType.Number, returnValueDescription: "-1 if the input is negative, 0 if the input is zero, and 1 if the input is positive.")
                    .WithDescription("Returns the sign of a number.")
                    .WithParameter("number", PortType.Number, description: "The number to take the sign of.")
                    .Build(),
                // sin
                FunctionBuilder.NewBuiltInFunction("sin", "Sin", PortType.Number, returnValueDescription: "The sine of the input.")
                    .WithDescription("Returns the sine of an angle given in degrees.")
                    .WithParameter("degrees", PortType.Number, description: "The angle in degrees.")
                    .Build(),
                // cos
                FunctionBuilder.NewBuiltInFunction("cos", "Cos", PortType.Number, returnValueDescription: "The cosine of the input.")
                    .WithDescription("Returns the cosine of an angle given in degrees.")
                    .WithParameter("degrees", PortType.Number, description: "The angle in degrees.")
                    .Build(),
                // tan
                FunctionBuilder.NewBuiltInFunction("tan", "Tan", PortType.Number, returnValueDescription: "The tangent of the input.")
                    .WithDescription("Returns the tangent of an angle given in degrees.")
                    .WithParameter("degrees", PortType.Number, description: "The angle in degrees.")
                    .Build(),
                // acos
                FunctionBuilder.NewBuiltInFunction("acos", "Acos", PortType.Number, returnValueDescription: "The arccosine of the input expressed in degrees.")
                    .WithDescription("Returns the arccosine of a number (this is the inverse of cosine).")
                    .WithParameter("number", PortType.Number, description: "The number to take the arccosine of.")
                    .Build(),
                // asin
                FunctionBuilder.NewBuiltInFunction("asin", "Asin", PortType.Number, returnValueDescription: "The arcsine of the input expressed in degrees." )
                    .WithDescription("Returns the arcsine of a number (this is the inverse of sine).")
                    .WithParameter("number", PortType.Number, description: "The number to take the arcsine of.")
                    .Build(),
                // atan
                FunctionBuilder.NewBuiltInFunction("atan", "Atan", PortType.Number, returnValueDescription: "The arctangent of the input expressed in degrees.")
                    .WithDescription("Returns the arctangent of a number (this is the inverse of tangent).")
                    .WithParameter("number", PortType.Number, description: "The number to take the arctangent of.")
                    .Build(),
                // atan2
                FunctionBuilder.NewBuiltInFunction("atan2", "Atan2", PortType.Number, returnValueDescription: "The full angle (0-360) made between the x axis and the vector(x,y) expressed in degrees")
                    .WithDescription(
                        "Returns the arctangent of number. Variant with two parameters that spans the whole 360 degrees.")
                    .WithParameter("x", PortType.Number, description: "The x component of the vector.")
                    .WithParameter("y", PortType.Number, description: "The y component of the vector." )
                    .Build(),
                // floor
                FunctionBuilder.NewBuiltInFunction("floor", "Floor", PortType.Number, returnValueDescription: "The largest integer less than or equal to the input.")
                    .WithDescription("Returns the largest integer less than or equal to a number.")
                    .WithParameter("number", PortType.Number, description: "The number to round down.")
                    .Build(),
                // round
                FunctionBuilder.NewBuiltInFunction("round", "Round", PortType.Number, returnValueDescription: "The nearest integer to the input.")
                    .WithDescription("Returns the nearest integer to a number.")
                    .WithParameter("number", PortType.Number, description: "The number to round.")
                    .Build(),
                // ceil
                FunctionBuilder.NewBuiltInFunction("ceil", "Ceil", PortType.Number, returnValueDescription: "The smallest integer greater than or equal to the input.")
                    .WithDescription("Returns the smallest integer greater than or equal to a number.")
                    .WithParameter("number", PortType.Number, description: "The number to round up.")
                    .Build(),
                // ln
                FunctionBuilder.NewBuiltInFunction("ln", "Ln", PortType.Number, returnValueDescription: "The natural logarithm of the input.")
                    .WithDescription("Returns the natural logarithm of a number.")
                    .WithParameter("number", PortType.Number, description: "The number to take the natural logarithm of.")
                    .Build(),
                // len
                FunctionBuilder.NewBuiltInFunction("len", "Len", PortType.Number, returnValueDescription: "The length of the input.")
                    .WithDescription("Returns the length of a string, vector or array.")
                    .WithParameter("value", PortType.Any, description: "The value to get the length of.")
                    .Build(),
                // log
                FunctionBuilder.NewBuiltInFunction("log", "Log", PortType.Number, returnValueDescription: "The base 10 logarithm of the input.")
                    .WithDescription("Returns the base 10 logarithm of a number.")
                    .WithParameter("number", PortType.Number, description: "The number to take the logarithm of.")
                    .Build(),
                // sqrt
                FunctionBuilder.NewBuiltInFunction("sqrt", "Sqrt", PortType.Number, returnValueDescription: "The square root of the input.")
                    .WithDescription("Returns the square root of a number.")
                    .WithParameter("number", PortType.Number, description: "The number to take the square root of.")
                    .Build(),

                // rands
                FunctionBuilder.NewBuiltInFunction("rands", "Rands", PortType.Array, returnValueDescription: "A vector of random numbers.")
                    .WithDescription(
                        "Random number generator. Generates a constant vector of pseudo random numbers, much like an array. The numbers are doubles not integers.")
                    .WithParameter("min_value", PortType.Number, description: "The minimum value of the random numbers.")
                    .WithParameter("max_value", PortType.Number, description: "The maximum value of the random numbers.")
                    .WithParameter("value_count", PortType.Number, description: "The number of random numbers to generate.")
                    .WithParameter("seed", PortType.Number, optional: true, description: "The seed to use for the random number generator.")
                    .Build(),
                // norm
                FunctionBuilder.NewBuiltInFunction("norm", "Norm", PortType.Number, returnValueDescription: "The norm of the input (e.g. the length of the vector).")
                    .WithDescription("Returns the norm of a vector. Note this returns the actual numeric length while len returns the number of elements in the vector or array.")
                    .WithParameter("vector", PortType.Array, description: "The vector to get the norm of.")
                    .Build(),
                
                // cross
                FunctionBuilder.NewBuiltInFunction("cross", "Cross", PortType.Array, returnValueDescription: "The cross product of the two input vectors.")
                    .WithDescription("Returns the cross product of two vectors.")
                    .WithParameter("vector1", PortType.Array, description: "The first vector.")
                    .WithParameter("vector2", PortType.Array, description: "The second vector.")
                    .Build()
            };

            // find out if we have any duplicate function ids
            var duplicateFunctionIds =
                Functions.GroupBy(f => f.Id).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            GdAssert.That(duplicateFunctionIds.Count == 0,
                "Duplicate function ids found: " + string.Join(", ", duplicateFunctionIds));

            Variables = new List<VariableDescription>()
            {
                VariableBuilder.NewVariable("PI", "__builtin__variable__PI")
            };
        }
    }
}