using System.Collections.Generic;

namespace OpenScadGraphEditor.Nodes
{
    public class PortDefinition
    {
        private PortDefinition(PortType portType, LiteralType literalType, string name,
            bool literalIsAutoSet = true, object defaultValue = default)
        {
            PortType = portType;
            Name = name;
            LiteralType = literalType;
            LiteralIsAutoSet = literalIsAutoSet;
            DefaultValue = defaultValue;
        }

        public PortType PortType { get; }

        public LiteralType LiteralType { get; }
        
        public string Name { get; }

        /// <summary>
        /// Whether a literal should be automatically set when the port is disconnected and unset when the port is connected.
        /// </summary>
        public bool LiteralIsAutoSet { get; }
        
        public object DefaultValue { get; }

        public double DefaultValueAsDouble => (DefaultValue is double value ? value : 0);

        public string DefaultValueAsString => DefaultValue as string ?? "";

        public bool DefaultValueAsBoolean => DefaultValue is bool value && value;


        public static PortDefinition Boolean(string name = "", bool allowLiteral = true, bool autoSetLiteralWhenPortIsDisconnected = true,  bool defaultValue = false)
        {
            return new PortDefinition(PortType.Boolean,
                allowLiteral
                    ? LiteralType.Boolean
                    : LiteralType.None,
                name,
                autoSetLiteralWhenPortIsDisconnected,
                defaultValue);
        }


        public static PortDefinition Number(string name = "", bool allowLiteral = true, bool autoSetLiteralWhenPortIsDisconnected = true, double defaultValue = 0)
        {
            return new PortDefinition(PortType.Number,
                allowLiteral
                    ? LiteralType.Number
                    : LiteralType.None,
                name,
                autoSetLiteralWhenPortIsDisconnected,
                defaultValue);
        }

        public static PortDefinition String(string name = "", bool allowLiteral = true, bool autoSetLiteralWhenPortIsDisconnected = true, string defaultValue = "")
        {
            return new PortDefinition(PortType.String,
                allowLiteral
                    ? LiteralType.String
                    : LiteralType.None,
                name,
                autoSetLiteralWhenPortIsDisconnected,
                defaultValue);
        }

        public static PortDefinition Any(string name = "")
        {
            return new PortDefinition(PortType.Any, LiteralType.None, name);
        }

        public static PortDefinition Vector3(string name = "", bool allowLiteral = true, bool autoSetLiteralWhenPortIsDisconnected = true)
        {
            return new PortDefinition(PortType.Vector3,
                allowLiteral
                    ? LiteralType.Vector3
                    : LiteralType.None,
                name,
                autoSetLiteralWhenPortIsDisconnected);
        }
        
        public static PortDefinition Vector2(string name, bool allowLiteral, bool autoSetLiteralWhenPortIsDisconnected = true)
        {
            return new PortDefinition(PortType.Vector2,
                allowLiteral
                    ? LiteralType.Vector2
                    : LiteralType.None,
                name,
                autoSetLiteralWhenPortIsDisconnected);
        }


        public static PortDefinition Flow(string name = "")
        {
            return new PortDefinition(PortType.Flow, LiteralType.None, name);
        }

        public static PortDefinition Array(string name = "")
        {
            return new PortDefinition(PortType.Array, LiteralType.None, name);
        }

        public static PortDefinition OfType(PortType type,
            string name = "",
            LiteralType literalType = LiteralType.None,
            bool autoSetLiteralWhenPortIsDisconnected = true,
            object defaultValue = default)
        {
            return new PortDefinition(type, literalType, name, autoSetLiteralWhenPortIsDisconnected, defaultValue);
        }
    }

    public static class PortDefinitionExt
    {
        public static List<PortDefinition> Number(this List<PortDefinition> self, string name = "",
            bool allowLiteral = true, bool autoSetLiteralWhenPortIsDisconnected = true, double defaultValue = 0)
        {
            self.Add(PortDefinition.Number(name, allowLiteral, autoSetLiteralWhenPortIsDisconnected, defaultValue));
            return self;
        }

        public static List<PortDefinition> Vector3(this List<PortDefinition> self, string name = "",
            bool allowLiteral = true, bool autoSetLiteralWhenPortIsDisconnected = true)
        {
            self.Add(PortDefinition.Vector3(name, allowLiteral,autoSetLiteralWhenPortIsDisconnected));
            return self;
        }
        
        public static List<PortDefinition> Vector2(this List<PortDefinition> self, string name = "",
            bool allowLiteral = true, bool autoSetLiteralWhenPortIsDisconnected = true)
        {
            self.Add(PortDefinition.Vector2(name, allowLiteral, autoSetLiteralWhenPortIsDisconnected));
            return self;
        }

        public static List<PortDefinition> Boolean(this List<PortDefinition> self, string name = "",
            bool allowLiteral = true,bool autoSetLiteralWhenPortIsDisconnected = true, bool defaultValue = false)
        {
            self.Add(PortDefinition.Boolean(name, allowLiteral, autoSetLiteralWhenPortIsDisconnected, defaultValue));
            return self;
        }

        public static List<PortDefinition> Any(this List<PortDefinition> self, string name = "")
        {
            self.Add(PortDefinition.Any(name));
            return self;
        }

        public static List<PortDefinition> String(this List<PortDefinition> self, string name = "",
            bool allowLiteral = true, bool autoSetLiteralWhenPortIsDisconnected = true, string defaultValue = "")
        {
            self.Add(PortDefinition.String(name, allowLiteral, autoSetLiteralWhenPortIsDisconnected, defaultValue));
            return self;
        }

        public static List<PortDefinition> Flow(this List<PortDefinition> self, string name = "")
        {
            self.Add(PortDefinition.Flow(name));
            return self;
        }

        public static List<PortDefinition> Array(this List<PortDefinition> self, string name = "")
        {
            self.Add(PortDefinition.Array(name));
            return self;
        }

        public static List<PortDefinition> OfType(this List<PortDefinition> self, PortType portType, string name = "",
           LiteralType literalType = LiteralType.None, bool autoSetLiteralWhenPortIsDisconnected = true, object defaultValue = default)
        {
            self.Add(PortDefinition.OfType(portType, name, literalType, autoSetLiteralWhenPortIsDisconnected, defaultValue));
            return self;
        }
    }
}