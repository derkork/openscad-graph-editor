using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
                // TODO: add remaining built-in modules
                ModuleBuilder.NewBuiltInModule("cube", "Cube")
                    .WithDescription("Creates a cube in the first octant.\nWhen center is true, the cube is\ncentered on the origin.")
                    .WithParameter("size", PortType.Vector3, label: "Size", optional: true)
                    .WithParameter("center", PortType.Boolean, label: "Center", optional: true)
                    .Build(),
                ModuleBuilder.NewBuiltInModule("translate", "Translate")
                    .WithDescription("Translates (moves) its child\nelements along the specified offset.")
                    .WithParameter("v", PortType.Vector3, label: "Offset")
                    .WithChildren()
                    .Build(),
                ModuleBuilder.NewBuiltInModule("rotate", "Rotate (Axis/Angle)")
                    .WithDescription("Rotates the next elements\nalong the given axis and angle.")
                    .WithParameter("v", PortType.Vector3, label: "Axis")
                    .WithParameter("a", PortType.Number, label: "Angle")
                    .WithChildren()
                    .Build(),
                ModuleBuilder.NewBuiltInModule("rotate", "Rotate (Euler Angles)")
                    .WithDescription("Rotates the next elements\nalong the given Euler angles.")
                    .WithParameter("a", PortType.Vector3, label: "Euler Angles")
                    .WithChildren()
                    .Build(),
                ModuleBuilder.NewBuiltInModule("linear_extrude", "Linear Extrude")
                    .WithDescription("Linear Extrusion is an operation that takes a 2D object as input and generates a 3D object as a result.")
                    .WithParameter("height", PortType.Number, label: "Height")
                    .WithParameter("center", PortType.Boolean, label: "Center")
                    .WithParameter("convexity", PortType.Number, label: "Convexity")
                    .WithParameter("twist", PortType.Number, label: "Twist")
                    .WithParameter("slices", PortType.Number, label: "Slices")
                    .WithParameter("scale", PortType.Vector3, label: "Scale")
                    .WithParameter("$fn", PortType.Number, label: "$fn")
                    .WithChildren()
                    .Build(),
                ModuleBuilder.NewBuiltInModule("color", "Color")
                    .WithDescription("Sets the color of the next elements.")
                    .WithParameter("c", PortType.Any, label: "Color")
                    .WithChildren()
                    .Build(),
            };
            
            Functions = new List<FunctionDescription>()
            {
              // abs
              FunctionBuilder.NewBuiltInFunction("abs", "Abs", PortType.Number)
                  .WithDescription("Returns the absolute value of a number.")
                  .WithParameter("number", PortType.Number)
                  .Build()
              ,
              // sign
              FunctionBuilder.NewBuiltInFunction("sign", "Sign", PortType.Number)
                  .WithDescription("Returns the sign of a number.")
                  .WithParameter("number", PortType.Number)
                  .Build()
              
              ,
              // sin
              FunctionBuilder.NewBuiltInFunction("sin", "Sin", PortType.Number)
                  .WithDescription("Returns the sine of an angle given in degrees.")
                  .WithParameter("degrees", PortType.Number)
                  .Build()
              
              ,
              // cos
              FunctionBuilder.NewBuiltInFunction("cos", "Cos", PortType.Number)
                  .WithDescription("Returns the cosine of an angle given in degrees.")
                  .WithParameter("degrees", PortType.Number)
                  .Build()
              
              ,
              // tan
              FunctionBuilder.NewBuiltInFunction("tan", "Tan", PortType.Number)
                  .WithDescription("Returns the tangent of an angle given in degrees.")
                  .WithParameter("degrees", PortType.Number)
                  .Build()
              ,
              // acos
              FunctionBuilder.NewBuiltInFunction("acos", "Acos", PortType.Number)
                  .WithDescription("Returns the arccosine of a number.")
                  .WithParameter("number", PortType.Number)
                  .Build()
              
              ,
              // asin
              FunctionBuilder.NewBuiltInFunction("asin", "Asin", PortType.Number)
                  .WithDescription("Returns the arcsine of a number.")
                  .WithParameter("number", PortType.Number)
                  .Build()
              
              ,
              // atan
              FunctionBuilder.NewBuiltInFunction("atan", "Atan", PortType.Number)
                  .WithDescription("Returns the arctangent of a number.")
                  .WithParameter("number", PortType.Number)
                  .Build()
              ,
              // atan2
              FunctionBuilder.NewBuiltInFunction("atan2", "Atan2", PortType.Number)
                  .WithDescription("Returns the arctangent of number. Variant with two parameters that spans the whole 360 degrees.")
                  .WithParameter("y", PortType.Number)
                  .WithParameter("x", PortType.Number)
                  .Build()
              
              ,
              // floor
              FunctionBuilder.NewBuiltInFunction("floor", "Floor", PortType.Number)
                  .WithDescription("Returns the largest integer less than or equal to a number.")
                  .WithParameter("number", PortType.Number)
                  .Build()
              
              ,
              // round
              FunctionBuilder.NewBuiltInFunction("round", "Round", PortType.Number)
                  .WithDescription("Returns the nearest integer to a number.")
                  .WithParameter("number", PortType.Number)
                  .Build()
              
              ,
              // ceil
              FunctionBuilder.NewBuiltInFunction("ceil", "Ceil", PortType.Number)
                  .WithDescription("Returns the smallest integer greater than or equal to a number.")
                  .WithParameter("number", PortType.Number)
                  .Build()
              
              ,
              // ln
              FunctionBuilder.NewBuiltInFunction("ln", "Ln", PortType.Number)
                  .WithDescription("Returns the natural logarithm of a number.")
                  .WithParameter("number", PortType.Number)
                  .Build()

              ,
              // len
              FunctionBuilder.NewBuiltInFunction("len", "Len", PortType.Number)
                  .WithDescription("Returns the length of a string, vector or array.")
                  .WithParameter("value")
                  .Build()
              ,
              // log
              FunctionBuilder.NewBuiltInFunction("log", "Log", PortType.Number)
                  .WithDescription("Returns the base 10 logarithm of a number.")
                  .WithParameter("number", PortType.Number)
                  .Build()
              ,
              // sqrt
              FunctionBuilder.NewBuiltInFunction("sqrt", "Sqrt", PortType.Number)
                  .WithDescription("Returns the square root of a number.")
                  .WithParameter("number", PortType.Number)
                  .Build()
              ,
              
              // rands
              FunctionBuilder.NewBuiltInFunction("rands", "Rands", PortType.Number)
                  .WithDescription("Random number generator. Generates a constant vector of pseudo random numbers, much like an array. The numbers are doubles not integers.")
                  .WithParameter("min_value", PortType.Number)
                  .WithParameter("max_value", PortType.Number)
                  .WithParameter("value_count", PortType.Number)
                  .WithParameter("seed", PortType.Number, optional: true)
                  .Build()              
              
              ,
              // norm
              FunctionBuilder.NewBuiltInFunction("norm", "Norm", PortType.Number)
                  .WithDescription("Returns the norm of a vector.")
                  .WithParameter("vector", PortType.Array)
                  .Build()
              ,
              // cross
              FunctionBuilder.NewBuiltInFunction("cross", "Cross", PortType.Array)
                  .WithDescription("Returns the cross product of two vectors.")
                  .WithParameter("vector1", PortType.Array)
                  .WithParameter("vector2", PortType.Array)
                  .Build()
            };

            Variables = new List<VariableDescription>()
            {
                VariableBuilder.NewVariable("PI", "__builtin__variable__PI")
            };
        }
    }
}