using System;
using Godot;
using GodotTestDriver.Drivers;

namespace OpenScadGraphEditor.Nodes
{
    public enum PortType
    {
        // indicates the absence of a port type.
        None = 0,
        // do not change the numbers, otherwise saved graphs will break!
        Geometry = 1,
        Boolean = 2,
        Number = 3,
        Vector3 = 4,
        Vector = 5,
        String = 6,
        Any = 8,
        Reroute = 9,
        Vector2 = 10,
        Many = 11,  // multiple any
    }

    public static class PortTypeExt
    {
        public static string HumanReadableName(this PortType self)
        {
            switch (self)
            {
                case PortType.Geometry:
                    return "geometry";
                case PortType.Boolean:
                    return "boolean";   
                case PortType.Number:
                    return "number";
                case PortType.Vector3:
                    return "vector3";
                case PortType.Vector2:
                    return "vector2";
                case PortType.Vector:
                    return "vector";
                case PortType.String:
                    return "string";
                case PortType.Any:
                    return "any";
                case PortType.Many:
                    return "many";
                case PortType.Reroute:
                    return "";
                default:
                    throw new ArgumentOutOfRangeException(nameof(self), self, null);
            }
        }
        
        public static bool IsExpressionType(this PortType self)
        {
            switch (self)
            {
                case PortType.Boolean:
                case PortType.Number:
                case PortType.Vector3:
                case PortType.Vector2:
                case PortType.Vector:
                case PortType.String:
                case PortType.Any:
                    return true;
                case PortType.Geometry:
                case PortType.Reroute:
                case PortType.Many:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException(nameof(self), self, null);
            } 
        }

        public static bool IsNumericInCustomizer(this PortType self)
        {
            switch (self)
            {
                case PortType.None:
                case PortType.Geometry:
                case PortType.Boolean:
                case PortType.String:
                case PortType.Any:
                case PortType.Many:
                case PortType.Reroute:
                    return false;
                case PortType.Number:
                case PortType.Vector3:
                case PortType.Vector:
                case PortType.Vector2:
                    return true;
                default:
                    throw new ArgumentOutOfRangeException(nameof(self), self, null);
            }
        }

        public static bool SupportsMultipleInputs(this PortType self)
        {
            switch (self)
            {
                // these port types support multiple inputs
                case PortType.Geometry:
                case PortType.Number:
                case PortType.Vector2:
                case PortType.Vector3:
                case PortType.Vector:
                case PortType.String:
                case PortType.Many:
                    return true;
                // anything else does not
                case PortType.Any:
                case PortType.Boolean:
                case PortType.None:
                case PortType.Reroute:
                default:
                    return false;
            }
        }

        public static bool CanBeAssignedTo(this PortType self, PortType other)
        {
            // if port types are the same, it's always possible
            if (self == other)
            {
                return true;
            }
            
            // vector2 and 3 can be assigned to Vector
            if (self == PortType.Vector2 && other == PortType.Vector)
            {
                return true;
            }
            if (self == PortType.Vector3 && other == PortType.Vector)
            {
                return true;
            }
            
            // any can be assigned to any expression type
            if (self == PortType.Any && other.IsExpressionType())
            {
                return true;
            }
            
            // any expression type can be assigned to any
            if (self.IsExpressionType() && other == PortType.Any)
            {
                return true;
            }
            
            // anything else - nope
            return false;
        }

        public static Color Color(this PortType portType)
        {
            switch (portType)
            {
                case PortType.Geometry:
                    return new Color(1, 1, 1); // white
                case PortType.Boolean:
                    return new Color(1, 0.4f, 0.4f); // red
                case PortType.Number:
                    return new Color(1, 0.8f, 0.4f); // orange
                case PortType.Vector3:
                    return new Color(.8f, 0.8f, 1f); // light blue
                case PortType.Vector2:
                    return new Color(.9f, 0.9f, 1f); // lighter blue
                case PortType.Vector:
                    return new Color(.5f, 0.5f, 1f); // dark blue
                case PortType.String:
                    return new Color(1f, 1f, 0f); // yellow
                case PortType.Any:
                    return new Color(1, 0f, 1f); // magenta
                case PortType.Many:
                    return new Color(0.7f, 0, 0.7f); // dark magenta
                case PortType.Reroute:
                    return new Color(0, 0.8f, 0); // green
                default:
                    throw new ArgumentOutOfRangeException(nameof(portType), portType, null);
            }
        } 
            
    }
}