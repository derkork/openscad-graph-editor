using System.Drawing;
using Godot;
using GodotExt;
using OpenScadGraphEditor.Actions;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Widgets.EditingInterface
{
    public class EditingInterface : Control
    {
        /// <summary>
        /// The actual graph editor.
        /// </summary>
        private ScadGraphEdit _graphEdit;

  
        /// <summary>
        /// The current editor context.
        /// </summary>
        private IEditorContext _editorContext;

        public ScadGraph Graph => _graphEdit.Graph;

        public Vector2 ScrollOffset
        {
            get => _graphEdit.ScrollOffset;
            set => _graphEdit.ScrollOffset = value;
        }

        public override void _Ready()
        {
            _graphEdit = this.WithName<ScadGraphEdit>("GraphEdit");
            this.WithName<IconButton.IconButton>("AddButton").ButtonPressed += _graphEdit.AddNode;
            this.WithName<IconButton.IconButton>("DeleteButton").ButtonPressed += _graphEdit.DeleteSelection;
            
            this.WithName<IconButton.IconButton>("DuplicateButton").ButtonPressed += _graphEdit.DuplicateSelection;
            this.WithName<IconButton.IconButton>("CopyButton").ButtonPressed += _graphEdit.CopySelection;
            this.WithName<IconButton.IconButton>("CutButton").ButtonPressed += _graphEdit.CutSelection;
            this.WithName<IconButton.IconButton>("PasteButton").ButtonPressed += _graphEdit.PasteClipboard;
            
            this.WithName<IconButton.IconButton>("StraightenButton").ButtonPressed += _graphEdit.StraightenSelection;
            this.WithName<IconButton.IconButton>("ExtractButton").ButtonPressed += _graphEdit.ExtractSelection;
            this.WithName<IconButton.IconButton>("CommentButton").ButtonPressed += _graphEdit.CommentSelection;
            this.WithName<IconButton.IconButton>("AlignLeftButton").ButtonPressed += _graphEdit.AlignSelectionLeft;
            this.WithName<IconButton.IconButton>("AlignRightButton").ButtonPressed += _graphEdit.AlignSelectionRight;
            this.WithName<IconButton.IconButton>("AlignTopButton").ButtonPressed += _graphEdit.AlignSelectionTop;
            this.WithName<IconButton.IconButton>("AlignBottomButton").ButtonPressed += _graphEdit.AlignSelectionBottom;
            this.WithName<IconButton.IconButton>("ShowHelpButton").ButtonPressed += _graphEdit.ShowHelpForSelection;
        }

        public void Init(IEditorContext editorContext)
        {
            _editorContext = editorContext;
        }

        public void Render(ScadGraph scadGraph)
        {
            _graphEdit.Render(_editorContext, scadGraph);
        }


        public void FocusNode(ScadNode node)
        {
            _graphEdit.FocusNode(node);
        }

    }
}