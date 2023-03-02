using System.Collections.Generic;

namespace OpenScadGraphEditor.Nodes
{
    public class PortDefinition
    {
        public PortDefinition(PortType portType, LiteralType literalType, string name,
            bool literalIsAutoSet = true, object defaultValue = default, RenderHint renderHint = RenderHint.None)
        {
            PortType = portType;
            Name = name;
            LiteralType = literalType;
            LiteralIsAutoSet = literalIsAutoSet;
            DefaultValue = defaultValue;
            RenderHint = renderHint;
        }

        public PortType PortType { get; }

        public LiteralType LiteralType { get; }
        
        public string Name { get; }

        /// <summary>
        /// Whether a literal should be automatically set when the port is disconnected and unset when the port is connected.
        /// </summary>
        public bool LiteralIsAutoSet { get; }
        
        /// <summary>
        /// The default value that should be used for this port.
        /// </summary>
        public object DefaultValue { get; }
        
        /// <summary>
        /// A render hint for the widget rendering the port.
        /// </summary>
        public RenderHint RenderHint { get; }

        public double DefaultValueAsDouble => (DefaultValue is double value ? value : 0);

        public string DefaultValueAsString => DefaultValue as string ?? "";

        public bool DefaultValueAsBoolean => DefaultValue is bool value && value;
    }

    public static class PortDefinitionExt
    {
        public static List<PortDefinition> Number(this List<PortDefinition> self, string name = "",
            bool allowLiteral = true, bool autoSetLiteralWhenPortIsDisconnected = true, double defaultValue = 0)
        {
            return self.PortType(Nodes.PortType.Number, name, allowLiteral ? LiteralType.Number : LiteralType.None, 
                autoSetLiteralWhenPortIsDisconnected, defaultValue);
        }

        public static List<PortDefinition> Vector3(this List<PortDefinition> self, string name = "",
            bool allowLiteral = true, bool autoSetLiteralWhenPortIsDisconnected = true)
        {
            return self.PortType(Nodes.PortType.Vector3, name, allowLiteral ? LiteralType.Vector3 : LiteralType.None,
                autoSetLiteralWhenPortIsDisconnected);
        }
        
        public static List<PortDefinition> Vector2(this List<PortDefinition> self, string name = "",
            bool allowLiteral = true, bool autoSetLiteralWhenPortIsDisconnected = true)
        {
            return self.PortType(Nodes.PortType.Vector2, name, allowLiteral ? LiteralType.Vector2 : LiteralType.None,
                    autoSetLiteralWhenPortIsDisconnected);
        }

        public static List<PortDefinition> Boolean(this List<PortDefinition> self, string name = "",
            bool allowLiteral = true,bool autoSetLiteralWhenPortIsDisconnected = true, bool defaultValue = false)
        {
            return self.PortType(Nodes.PortType.Boolean, name, allowLiteral ? LiteralType.Boolean : LiteralType.None,
                autoSetLiteralWhenPortIsDisconnected, defaultValue);
        }

        public static List<PortDefinition> Any(this List<PortDefinition> self, string name = "")
        {
            return self.PortType(Nodes.PortType.Any, name);
        }

        public static List<PortDefinition> String(this List<PortDefinition> self, string name = "",
            bool allowLiteral = true, bool autoSetLiteralWhenPortIsDisconnected = true, string defaultValue = "", 
            RenderHint renderHint = RenderHint.None)
        {
            return self.PortType(Nodes.PortType.String, name,allowLiteral ? LiteralType.String : LiteralType.None, autoSetLiteralWhenPortIsDisconnected, defaultValue, renderHint);
        }

        public static List<PortDefinition> Geometry(this List<PortDefinition> self, string name = "")
        {
            return self.PortType(Nodes.PortType.Geometry, name);
        }

        public static List<PortDefinition> Array(this List<PortDefinition> self, string name = "")
        {
            return self.PortType(Nodes.PortType.Vector, name);
        }

        public static List<PortDefinition> PortType(this List<PortDefinition> self, PortType portType, string name = "",
           LiteralType literalType = LiteralType.None, bool autoSetLiteralWhenPortIsDisconnected = true, object defaultValue = default,
           RenderHint renderHint = RenderHint.None)
        {
            self.Add(new PortDefinition(portType, literalType, name, autoSetLiteralWhenPortIsDisconnected, defaultValue, renderHint));
            return self;
        }
    }
    
 
}