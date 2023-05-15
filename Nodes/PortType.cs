using System;

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
                case PortType.Reroute:
                case PortType.None:
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
                case PortType.None:
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
    }
}