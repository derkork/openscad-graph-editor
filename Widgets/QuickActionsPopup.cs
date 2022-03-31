using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotExt;
using JetBrains.Annotations;

namespace OpenScadGraphEditor.Widgets
{
    [UsedImplicitly]
    public class QuickActionsPopup : PopupMenu
    {
        private List<QuickAction> _actions;

        public override void _Ready()
        {
            this.Connect("index_pressed")
                .To(this, nameof(OnIndexPressed));
        }

        public void Open(Vector2 position, IEnumerable<QuickAction> actions)
        {
            _actions = actions.ToList();
            if (_actions.Count == 0)
            {
                return;
            }

            Clear();
            foreach (var action in _actions)
            {
                AddItem(action.Title);
            }

            SetGlobalPosition(position);
            SetAsMinsize();
            Popup_();
        }

        private void OnIndexPressed(int index)
        {
            var action = _actions[index];
            action.OnSelect();
        }
    }
}