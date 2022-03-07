using System.Collections.Generic;

namespace OpenScadGraphEditor.Nodes
{
    public readonly struct PortDefinition
    {
        private PortDefinition(PortType portType, string name, bool allowLiteral, bool autoCoerce)
        {
            PortType = portType;
            Name = name;
            AllowLiteral = allowLiteral;
            AutoCoerce = autoCoerce;
        }

        public PortType PortType { get; }
        public string Name { get; }

        public bool AllowLiteral { get; }
        public bool AutoCoerce { get; }


        public static PortDefinition Boolean(string name = "", bool allowLiteral = true, bool autoCoerce = false)
        {
            return new PortDefinition(PortType.Boolean, name, allowLiteral, autoCoerce);
        }


        public static PortDefinition Number(string name = "", bool allowLiteral = true, bool autoCoerce = false)
        {
            return new PortDefinition(PortType.Number, name, allowLiteral, autoCoerce);
        }

        public static PortDefinition String(string name = "", bool allowLiteral = true, bool autoCoerce = false)
        {
            return new PortDefinition(PortType.String, name, allowLiteral, autoCoerce);
        }

        public static PortDefinition Any(string name = "")
        {
            return new PortDefinition(PortType.Any, name, false, false);
        }

        public static PortDefinition Vector3(string name = "", bool allowLiteral = true, bool autoCoerce = false)
        {
            return new PortDefinition(PortType.Vector3, name, allowLiteral, autoCoerce);
        }

        public static PortDefinition Flow(string name = "")
        {
            return new PortDefinition(PortType.Flow, name, false, false);
        }

        public static PortDefinition OfType(PortType type, string name = "", bool allowLiteral = true,
            bool autoCoerce = false)
        {
            return new PortDefinition(type, name, allowLiteral, autoCoerce);
        }
    }

    public static class PortDefinitionExt
    {
        public static List<PortDefinition> Number(this List<PortDefinition> self, string name = "",
            bool allowLiteral = true, bool autoCoerce = false)
        {
            self.Add(PortDefinition.Number(name, allowLiteral, autoCoerce));
            return self;
        }

        public static List<PortDefinition> Vector3(this List<PortDefinition> self, string name = "",
            bool allowLiteral = true, bool autoCoerce = false)
        {
            self.Add(PortDefinition.Vector3(name, allowLiteral, autoCoerce));
            return self;
        }

        public static List<PortDefinition> Boolean(this List<PortDefinition> self, string name = "",
            bool allowLiteral = true, bool autoCoerce = false)
        {
            self.Add(PortDefinition.Boolean(name, allowLiteral, autoCoerce));
            return self;
        }

        public static List<PortDefinition> Any(this List<PortDefinition> self, string name = "")
        {
            self.Add(PortDefinition.Any(name));
            return self;
        }

        public static List<PortDefinition> String(this List<PortDefinition> self, string name = "",
            bool allowLiteral = true, bool autoCoerce = false)
        {
            self.Add(PortDefinition.String(name, allowLiteral, autoCoerce));
            return self;
        }

        public static List<PortDefinition> Flow(this List<PortDefinition> self, string name = "")
        {
            self.Add(PortDefinition.Flow(name));
            return self;
        }

        public static List<PortDefinition> OfType(this List<PortDefinition> self, PortType portType, string name = "",
            bool allowLiteral = true, bool autoCoerce = false)
        {
            self.Add(PortDefinition.OfType(portType, name, allowLiteral, autoCoerce));
            return self;
        }
    }
}