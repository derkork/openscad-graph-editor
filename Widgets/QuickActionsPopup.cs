using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Utils;

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
            if (FillActions(actions))
            {
                return;
            }

            SetGlobalPosition(position);
            SetAsMinsize();
            Popup_();
        }

        private bool FillActions(IEnumerable<QuickAction> actions)
        {
            _actions = actions.ToList();
            if (_actions.Count == 0)
            {
                return true;
            }

            // remove all items
            Clear();
            // clear any submenus we may have created
            this.GetChildNodes<QuickActionsPopup>().ForAll(it => it.RemoveAndFree());

            foreach (var action in _actions)
            {
                if (action.IsCheckbox)
                {
                    AddCheckItem(action.Title);
                }
                else if (action.IsSeparator)
                {
                    AddSeparator(action.Title);
                }
                else if (action.IsSubGroup)
                {
                    var submenu = new QuickActionsPopup();
                    var id = Guid.NewGuid().ToString();
                    submenu.Name = id;
                    submenu.MoveToNewParent(this);
                    submenu.FillActions(action.Children);
                    AddSubmenuItem(action.Title, id);
                }
                else
                {
                    AddItem(action.Title);
                }

                var itemCount = GetItemCount() - 1;
                SetItemChecked(itemCount, action.IsChecked);
            }

            return false;
        }

        private void OnIndexPressed(int index)
        {
            var action = _actions[index];
            action.OnSelect();
        }
    }
}