using Godot;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using GodotExt;
using OpenScadGraphEditor.Refactorings;

namespace OpenScadGraphEditor.Widgets
{
    public class RefactoringPopup : PopupMenu
    {

        [Signal]
        public delegate void RefactoringSelected(Refactoring refactoring);

        public override void _Ready()
        {
            this.Connect("index_pressed")
                .To(this, nameof(OnIndexPressed));
        }

        public void Open(Vector2 position, IScadGraph graph, ScadNode node)
        {
            var applicableRefactorings = UserSelectableNodeRefactoring.GetApplicable(graph, node);
            if (applicableRefactorings.Count == 0)
            {
                return; // nothing to show
            }
            Clear();
            var idx = 0;
            foreach (var refactoring in applicableRefactorings)
            {
                AddItem(refactoring.Title);
                SetItemMetadata(idx++, refactoring);
            }
            
            SetGlobalPosition(position);
            SetAsMinsize();
            Popup_();
        }

        private void OnIndexPressed(int index)
        {
            var refactoring = (Refactoring) GetItemMetadata(index);
            EmitSignal(nameof(RefactoringSelected), refactoring);
        }
    }
}