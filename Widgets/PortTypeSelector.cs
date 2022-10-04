using System.Collections.Generic;
using Godot;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Widgets
{
    /// <summary>
    /// A widget for selecting port types.
    /// </summary>
    public class PortTypeSelector : OptionButton
    {
        private readonly Dictionary<PortType,int> _indexByPortTypes = new Dictionary<PortType, int>();


        public PortTypeSelector()
        {
            Clear();
            
            // Any
            AddItem(PortType.Any.HumanReadableName(), (int) PortType.Any);
            _indexByPortTypes[PortType.Any] = GetItemCount() - 1;

            // Number
            AddItem(PortType.Number.HumanReadableName(), (int) PortType.Number);
            _indexByPortTypes[PortType.Number] = GetItemCount() - 1;
            
            // Boolean
            AddItem(PortType.Boolean.HumanReadableName(), (int) PortType.Boolean);
            _indexByPortTypes[PortType.Boolean] = GetItemCount() - 1;
            
            // Vector2
            AddItem(PortType.Vector2.HumanReadableName(), (int) PortType.Vector2);
            _indexByPortTypes[PortType.Vector2] = GetItemCount() - 1;

            // Vector3
            AddItem(PortType.Vector3.HumanReadableName(), (int) PortType.Vector3);
            _indexByPortTypes[PortType.Vector3] = GetItemCount() - 1;
            
            // Vector
            AddItem(PortType.Vector.HumanReadableName(), (int) PortType.Vector);
            _indexByPortTypes[PortType.Vector] = GetItemCount() - 1;

            // String
            AddItem(PortType.String.HumanReadableName(), (int) PortType.String);
            _indexByPortTypes[PortType.String] = GetItemCount() - 1;
        }

        
        public PortType SelectedPortType
        {
            get => (PortType) GetSelectedId();
            set => Select(_indexByPortTypes[value]);
        }
        
    }
}