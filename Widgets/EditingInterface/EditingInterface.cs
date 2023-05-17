using System.Collections.Generic;
using Godot;
using GodotExt;
using OpenScadGraphEditor.Actions;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Widgets;
using OpenScadGraphEditor.Widgets.IconButton;

public class EditingInterface : Control
{
    private ScadGraphEdit _graphEdit;
    private Control _buttonBar;
    private IEditorContext _editorContext;
    /// <summary>
    /// Controls which appear when a selection is active.
    /// </summary>
    private List<Control> _selectionControls;
    
    public override void _Ready()
    {
        _graphEdit = this.WithName<ScadGraphEdit>("GraphEdit");
        _buttonBar = this.WithName<Control>("ButtonBar");
        var alignLeftButton = _buttonBar.WithName<IconButton>("AlignLeftButton");
        
        var alignRightButton = _buttonBar.WithName<IconButton>("AlignRightButton");
        var alignTopButton = _buttonBar.WithName<IconButton>("AlignTopButton");
        var alignBottomButton = _buttonBar.WithName<IconButton>("AlignBottomButton");
        
        _selectionControls.Add(alignLeftButton);
        _selectionControls.Add(alignRightButton);
        _selectionControls.Add(alignTopButton);
        _selectionControls.Add(alignBottomButton);
        
    }

    public void Init(IEditorContext editorContext)
    {
        _editorContext = editorContext;
    }
    
    public void Render(ScadGraph scadGraph)
    {
        _graphEdit.Render(_editorContext, scadGraph);
    }
    
    
    
}
