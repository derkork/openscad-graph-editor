using System.Collections.Generic;
using System.Linq;

namespace OpenScadGraphEditor.Nodes
{
    public readonly struct PortDefinition
    {
        private PortDefinition(PortType portType, string name, bool allowLiteral)
        {
            PortType = portType;
            Name = name;
            AllowLiteral = allowLiteral;
        }

        public PortType PortType { get; }
        public string Name { get; }
        
        public bool AllowLiteral { get; }


        public static PortDefinition Boolean(string name= "", bool allowLiteral = true)
        {
            return new PortDefinition(PortType.Boolean, name, allowLiteral);
        }


        public static PortDefinition Number(string name = "",  bool allowLiteral = true)
        {
            return new PortDefinition(PortType.Number, name, allowLiteral);
        }
        
        public static PortDefinition Vector3(string name = "", bool allowLiteral = true)
        {
            return new PortDefinition(PortType.Vector3, name, allowLiteral);
        }
        
        public static PortDefinition Flow(string name = "")
        {
            return new PortDefinition(PortType.Flow, name, false);
        }

        public static List<PortDefinition> Of(params PortDefinition[] ports)
        {
            return ports.ToList();
        }

        public static List<PortDefinition> None()
        {
            return Of();
        } 
    }

    public static class PortDefinitionExt
    {
        public static List<PortDefinition> Number(this List<PortDefinition> self, string name = "",
            bool allowLiteral = true)
        {
            self.Add(PortDefinition.Number(name, allowLiteral));
            return self;
        }

        public static List<PortDefinition> Vector3(this List<PortDefinition> self, string name = "",
            bool allowLiteral = true)
        {
            self.Add(PortDefinition.Vector3(name, allowLiteral));
            return self;
        }

        public static List<PortDefinition> Boolean(this List<PortDefinition> self, string name = "",
            bool allowLiteral = true)
        {
            self.Add(PortDefinition.Boolean(name, allowLiteral));
            return self;
        }

        public static List<PortDefinition> Flow(this List<PortDefinition> self, string name = "")
        {
            self.Add(PortDefinition.Flow(name));
            return self;
        }
    }
}