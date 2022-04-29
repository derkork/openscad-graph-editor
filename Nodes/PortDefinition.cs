using System.Collections.Generic;

namespace OpenScadGraphEditor.Nodes
{
    public class PortDefinition
    {
        private PortDefinition(PortType portType, string name, bool allowLiteral, object defaultValue = default)
        {
            PortType = portType;
            Name = name;
            AllowLiteral = allowLiteral && portType != PortType.Any && portType != PortType.Flow &&
                           PortType != PortType.Array;
            DefaultValue = defaultValue;
        }

        public PortType PortType { get; }
        public string Name { get; }

        public bool AllowLiteral { get; }
        
        public object DefaultValue { get; }

        public double DefaultValueAsDouble => (DefaultValue is double value ? value : 0);

        public string DefaultValueAsString => DefaultValue is string value ? value : "";

        public bool DefaultValueAsBoolean => DefaultValue is bool value && value;


        public static PortDefinition Boolean(string name = "", bool allowLiteral = true, bool defaultValue = false)
        {
            return new PortDefinition(PortType.Boolean, name, allowLiteral, defaultValue);
        }


        public static PortDefinition Number(string name = "", bool allowLiteral = true, double defaultValue = 0)
        {
            return new PortDefinition(PortType.Number, name, allowLiteral, defaultValue);
        }

        public static PortDefinition String(string name = "", bool allowLiteral = true, string defaultValue = "")
        {
            return new PortDefinition(PortType.String, name, allowLiteral, defaultValue);
        }

        public static PortDefinition Any(string name = "")
        {
            return new PortDefinition(PortType.Any, name, false);
        }

        public static PortDefinition Vector3(string name = "", bool allowLiteral = true)
        {
            return new PortDefinition(PortType.Vector3, name, allowLiteral);
        }
        
        public static PortDefinition Vector2(string name, bool allowLiteral)
        {
            return new PortDefinition(PortType.Vector2, name, allowLiteral);
        }


        public static PortDefinition Flow(string name = "")
        {
            return new PortDefinition(PortType.Flow, name, false);
        }

        public static PortDefinition Array(string name = "")
        {
            return new PortDefinition(PortType.Array, name, false);
        }

        public static PortDefinition OfType(PortType type, string name = "", bool allowLiteral = true, object defaultValue = default)
        {
            return new PortDefinition(type, name, allowLiteral, defaultValue);
        }
    }

    public static class PortDefinitionExt
    {
        public static List<PortDefinition> Number(this List<PortDefinition> self, string name = "",
            bool allowLiteral = true, double defaultValue = 0)
        {
            self.Add(PortDefinition.Number(name, allowLiteral, defaultValue));
            return self;
        }

        public static List<PortDefinition> Vector3(this List<PortDefinition> self, string name = "",
            bool allowLiteral = true)
        {
            self.Add(PortDefinition.Vector3(name, allowLiteral));
            return self;
        }
        
        public static List<PortDefinition> Vector2(this List<PortDefinition> self, string name = "",
            bool allowLiteral = true)
        {
            self.Add(PortDefinition.Vector2(name, allowLiteral));
            return self;
        }

        public static List<PortDefinition> Boolean(this List<PortDefinition> self, string name = "",
            bool allowLiteral = true, bool defaultValue = false)
        {
            self.Add(PortDefinition.Boolean(name, allowLiteral, defaultValue));
            return self;
        }

        public static List<PortDefinition> Any(this List<PortDefinition> self, string name = "")
        {
            self.Add(PortDefinition.Any(name));
            return self;
        }

        public static List<PortDefinition> String(this List<PortDefinition> self, string name = "",
            bool allowLiteral = true, string defaultValue = "")
        {
            self.Add(PortDefinition.String(name, allowLiteral, defaultValue));
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
            bool allowLiteral = true, object defaultValue = default)
        {
            self.Add(PortDefinition.OfType(portType, name, allowLiteral, defaultValue));
            return self;
        }
    }
}