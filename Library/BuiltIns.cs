using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GodotExt;
using OpenScadGraphEditor.Nodes;

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
            LanguageLevelNodes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(ScadNode).IsAssignableFrom(t) && !t.IsAbstract)
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
                    .WithParameter("r", PortType.Number, label: "Radius", optional: true)
                    .WithFragmentParameters()
                    .Build(),
                // circle (diameter)
                ModuleBuilder.NewBuiltInModule("circle", "Circle (Diameter)", "diameter")
                    .WithDescription("Creates a circle at the origin.")
                    .WithParameter("d", PortType.Number, label: "Diameter", optional: true)
                    .WithFragmentParameters()
                    .Build(),
                // square
                ModuleBuilder.NewBuiltInModule("square", "Square")
                    .WithDescription("Creates a rectangle or square at the origin.")
                    .WithParameter("size", PortType.Number, label: "Size", optional: true)
                    .WithParameter("center", PortType.Boolean, label: "Center", optional: true)
                    .Build(),
                // rectangle
                ModuleBuilder.NewBuiltInModule("square", "Rectangle", "rectangle")
                    .WithDescription("Creates a rectangle or square at the origin.")
                    .WithParameter("size", PortType.Vector2, label: "Size", optional: true)
                    .WithParameter("center", PortType.Boolean, label: "Center", optional: true)
                    .Build(),
                // polygon
                ModuleBuilder.NewBuiltInModule("polygon", "Polygon")
                    .WithDescription("Creates a polygon.")
                    .WithParameter("points", PortType.Array, label: "Points", optional: true)
                    .WithParameter("paths", PortType.Array, label: "Paths", optional: true)
                    .WithParameter("convexity", PortType.Number, label: "Convexity", optional: true)
                    .Build(),

                // projection
                ModuleBuilder.NewBuiltInModule("projection", "Projection")
                    .WithDescription("Creates a projection.")
                    .WithParameter("cut", PortType.Boolean, label: "Cut", optional: true)
                    .Build(),
                // 3D Modules
                // cube
                ModuleBuilder.NewBuiltInModule("cube", "Cube")
                    .WithDescription("Creates a cube in the first octant.When center is true, the cube is centered on the origin.")
                    .WithParameter("size", PortType.Vector3, label: "Size", optional: true)
                    .WithParameter("center", PortType.Boolean, label: "Center", optional: true)
                    .Build(),
                // sphere
                ModuleBuilder.NewBuiltInModule("sphere", "Sphere (Radius)", "radius")
                    .WithDescription("Creates a sphere.")
                    .WithParameter("r", PortType.Number, label: "Radius", optional: true)
                    .WithFragmentParameters()
                    .Build(),
                // sphere (diameter)
                ModuleBuilder.NewBuiltInModule("sphere", "Sphere (Diameter)", "diameter")
                    .WithDescription("Creates a sphere.")
                    .WithParameter("d", PortType.Number, label: "Diameter", optional: true)
                    .WithFragmentParameters()
                    .Build(),
                // cylinder (radius)
                ModuleBuilder.NewBuiltInModule("cylinder", "Cylinder (Radius)", "radius")
                    .WithDescription("Creates a cylinder.")
                    .WithParameter("h", PortType.Number, label: "Height", optional: true)
                    .WithParameter("r", PortType.Number, label: "Radius", optional: true)
                    .WithParameter("center", PortType.Boolean, label: "Center", optional: true)
                    .WithFragmentParameters()
                    .Build(),
                // cylinder diameter
                ModuleBuilder.NewBuiltInModule("cylinder", "Cylinder (Diameter)", "diameter")
                    .WithDescription("Creates a cylinder.")
                    .WithParameter("h", PortType.Number, label: "Height", optional: true)
                    .WithParameter("d", PortType.Number, label: "Diameter", optional: true)
                    .WithParameter("center", PortType.Boolean, label: "Center", optional: true)
                    .WithFragmentParameters()
                    .Build(),
                // cone (radius)
                ModuleBuilder.NewBuiltInModule("cylinder", "Cone (Radius)", "cone_radius")
                    .WithDescription("Creates a cone (or cylinder).")
                    .WithParameter("h", PortType.Number, label: "Height", optional: true)
                    .WithParameter("r1", PortType.Number, label: "Radius 1", optional: true)
                    .WithParameter("r2", PortType.Number, label: "Radius 2", optional: true)
                    .WithParameter("center", PortType.Boolean, label: "Center", optional: true)
                    .WithFragmentParameters()
                    .Build(),
                // cone(diameter)
                ModuleBuilder.NewBuiltInModule("cylinder", "Cone (Diameter)", "cone_diameter")
                    .WithDescription("Creates a cone (or cylinder).")
                    .WithParameter("h", PortType.Number, label: "Height", optional: true)
                    .WithParameter("d1", PortType.Number, label: "Diameter 1", optional: true)
                    .WithParameter("d2", PortType.Number, label: "Diameter 2", optional: true)
                    .WithParameter("center", PortType.Boolean, label: "Center", optional: true)
                    .WithFragmentParameters()
                    .Build(),
                // polyhedron
                ModuleBuilder.NewBuiltInModule("polyhedron", "Polyhedron")
                    .WithDescription("Creates a polyhedron.")
                    .WithParameter("points", PortType.Vector3, label: "Points", optional: true)
                    .WithParameter("faces", PortType.Vector3, label: "Faces", optional: true)
                    .WithParameter("convexity", PortType.Number, label: "Convexity", optional: true)
                    .Build(),
                // linear extrude
                ModuleBuilder.NewBuiltInModule("linear_extrude", "Linear Extrude")
                    .WithDescription(
                        "Linear Extrusion is an operation that takes a 2D object as input and generates a 3D object as a result.")
                    .WithParameter("height", PortType.Number, label: "Height", optional: true)
                    .WithParameter("center", PortType.Boolean, label: "Center", optional: true)
                    .WithParameter("convexity", PortType.Number, label: "Convexity", optional: true)
                    .WithParameter("twist", PortType.Number, label: "Twist", optional: true)
                    .WithParameter("slices", PortType.Number, label: "Slices", optional: true)
                    .WithParameter("scale", PortType.Vector3, label: "Scale", optional: true)
                    .WithParameter("$fn", PortType.Number, label: "Resolution", optional: true)
                    .WithChildren()
                    .Build(),
                // rotate_extrude
                ModuleBuilder.NewBuiltInModule("rotate_extrude", "Rotate Extrude")
                    .WithDescription("Rotational extrusion spins a 2D shape around the Z-axis to form a solid which has rotational symmetry.")
                    .WithParameter("angle", PortType.Number, label: "Angle", optional: true)
                    .WithParameter("convexity", PortType.Number, label: "Convexity", optional: true)
                    .WithFragmentParameters()
                    .WithChildren()
                    .Build(),
                // Transformations
                // translate
                ModuleBuilder.NewBuiltInModule("translate", "Translate")
                    .WithDescription("Translates (moves) its child elements along the specified offset.")
                    .WithParameter("v", PortType.Vector3, label: "Offset")
                    .WithChildren()
                    .Build(),
                // rotate (axis/angle)
                ModuleBuilder.NewBuiltInModule("rotate", "Rotate (Axis/Angle)", "axis_angle")
                    .WithDescription("Rotates the next elements along the given axis and angle.")
                    .WithParameter("v", PortType.Vector3, label: "Axis")
                    .WithParameter("a", PortType.Number, label: "Angle")
                    .WithChildren()
                    .Build(),
                // rotate (euler angles)
                ModuleBuilder.NewBuiltInModule("rotate", "Rotate (Euler Angles)", "euler_angles")
                    .WithDescription("Rotates the next elements along the given Euler angles.")
                    .WithParameter("a", PortType.Vector3, label: "Euler Angles")
                    .WithChildren()
                    .Build(),
                // scale
                ModuleBuilder.NewBuiltInModule("scale", "Scale")
                    .WithDescription("Scales its child elements using specified scale vector.")
                    .WithParameter("v", PortType.Vector3, label: "Scale")
                    .WithChildren()
                    .Build(),
                // resize
                ModuleBuilder.NewBuiltInModule("resize", "Resize")
                    .WithDescription("Modifies the size of the child object to match the given x,y, and z.")
                    .WithParameter("newsize", PortType.Vector3, label: "New size")
                    .WithParameter("auto", PortType.Boolean, label: "Auto", optional: true)
                    .WithChildren()
                    .Build(),
                // mirror
                ModuleBuilder.NewBuiltInModule("mirror", "Mirror")
                    .WithDescription("Mirrors the child element on a plane through the origin.")
                    .WithParameter("v", PortType.Vector3, label: "Normal")
                    .WithChildren()
                    .Build(),
                // multmatrix
                ModuleBuilder.NewBuiltInModule("multmatrix", "Multmatrix")
                    .WithDescription("Multiplies the geometry of all child elements with the given affine transformation matrix.")
                    .WithParameter("m", PortType.Array, label: "Matrix")
                    .WithChildren()
                    .Build(),
                // offset (radius)
                ModuleBuilder.NewBuiltInModule("offset", "Offset (Radius)", "radius")
                    .WithDescription("Generates a new 2d interior or exterior outline from an existing outline. This mode produces round corners.")
                    .WithParameter("r", PortType.Number, label: "Radius")
                    .WithChildren()
                    .Build(),
                // offset (delta)
                ModuleBuilder.NewBuiltInModule("offset", "Offset (Delta)", "delta")
                    .WithDescription("Generates a new 2d interior or exterior outline from an existing outline. Delta specifies the distance of the new outline from the original outline, and therefore reproduces angled corners.")
                    .WithParameter("delta", PortType.Number, label: "Delta")
                    .WithParameter("chamfer", PortType.Boolean, label: "Chamfer", optional: true)
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
                FunctionBuilder.NewBuiltInFunction("abs", "Abs", PortType.Number)
                    .WithDescription("Returns the absolute value of a number.")
                    .WithParameter("number", PortType.Number)
                    .Build(),
                // sign
                FunctionBuilder.NewBuiltInFunction("sign", "Sign", PortType.Number)
                    .WithDescription("Returns the sign of a number.")
                    .WithParameter("number", PortType.Number)
                    .Build(),
                // sin
                FunctionBuilder.NewBuiltInFunction("sin", "Sin", PortType.Number)
                    .WithDescription("Returns the sine of an angle given in degrees.")
                    .WithParameter("degrees", PortType.Number)
                    .Build(),
                // cos
                FunctionBuilder.NewBuiltInFunction("cos", "Cos", PortType.Number)
                    .WithDescription("Returns the cosine of an angle given in degrees.")
                    .WithParameter("degrees", PortType.Number)
                    .Build(),
                // tan
                FunctionBuilder.NewBuiltInFunction("tan", "Tan", PortType.Number)
                    .WithDescription("Returns the tangent of an angle given in degrees.")
                    .WithParameter("degrees", PortType.Number)
                    .Build(),
                // acos
                FunctionBuilder.NewBuiltInFunction("acos", "Acos", PortType.Number)
                    .WithDescription("Returns the arccosine of a number.")
                    .WithParameter("number", PortType.Number)
                    .Build(),
                // asin
                FunctionBuilder.NewBuiltInFunction("asin", "Asin", PortType.Number)
                    .WithDescription("Returns the arcsine of a number.")
                    .WithParameter("number", PortType.Number)
                    .Build(),
                // atan
                FunctionBuilder.NewBuiltInFunction("atan", "Atan", PortType.Number)
                    .WithDescription("Returns the arctangent of a number.")
                    .WithParameter("number", PortType.Number)
                    .Build(),
                // atan2
                FunctionBuilder.NewBuiltInFunction("atan2", "Atan2", PortType.Number)
                    .WithDescription(
                        "Returns the arctangent of number. Variant with two parameters that spans the whole 360 degrees.")
                    .WithParameter("y", PortType.Number)
                    .WithParameter("x", PortType.Number)
                    .Build(),
                // floor
                FunctionBuilder.NewBuiltInFunction("floor", "Floor", PortType.Number)
                    .WithDescription("Returns the largest integer less than or equal to a number.")
                    .WithParameter("number", PortType.Number)
                    .Build(),
                // round
                FunctionBuilder.NewBuiltInFunction("round", "Round", PortType.Number)
                    .WithDescription("Returns the nearest integer to a number.")
                    .WithParameter("number", PortType.Number)
                    .Build(),
                // ceil
                FunctionBuilder.NewBuiltInFunction("ceil", "Ceil", PortType.Number)
                    .WithDescription("Returns the smallest integer greater than or equal to a number.")
                    .WithParameter("number", PortType.Number)
                    .Build(),
                // ln
                FunctionBuilder.NewBuiltInFunction("ln", "Ln", PortType.Number)
                    .WithDescription("Returns the natural logarithm of a number.")
                    .WithParameter("number", PortType.Number)
                    .Build(),
                // len
                FunctionBuilder.NewBuiltInFunction("len", "Len", PortType.Number)
                    .WithDescription("Returns the length of a string, vector or array.")
                    .WithParameter("value")
                    .Build(),
                // log
                FunctionBuilder.NewBuiltInFunction("log", "Log", PortType.Number)
                    .WithDescription("Returns the base 10 logarithm of a number.")
                    .WithParameter("number", PortType.Number)
                    .Build(),
                // sqrt
                FunctionBuilder.NewBuiltInFunction("sqrt", "Sqrt", PortType.Number)
                    .WithDescription("Returns the square root of a number.")
                    .WithParameter("number", PortType.Number)
                    .Build(),

                // rands
                FunctionBuilder.NewBuiltInFunction("rands", "Rands", PortType.Number)
                    .WithDescription(
                        "Random number generator. Generates a constant vector of pseudo random numbers, much like an array. The numbers are doubles not integers.")
                    .WithParameter("min_value", PortType.Number)
                    .WithParameter("max_value", PortType.Number)
                    .WithParameter("value_count", PortType.Number)
                    .WithParameter("seed", PortType.Number, optional: true)
                    .Build(),
                // norm
                FunctionBuilder.NewBuiltInFunction("norm", "Norm", PortType.Number)
                    .WithDescription("Returns the norm of a vector.")
                    .WithParameter("vector", PortType.Array)
                    .Build(),
                // cross
                FunctionBuilder.NewBuiltInFunction("cross", "Cross", PortType.Array)
                    .WithDescription("Returns the cross product of two vectors.")
                    .WithParameter("vector1", PortType.Array)
                    .WithParameter("vector2", PortType.Array)
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