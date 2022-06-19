using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotExt;
using GodotTestDriver.Drivers;
using OpenScadGraphEditor.Widgets.InvokableRefactorDialog;

namespace OpenScadGraphEditor.Tests.Drivers
{
    public class InvokableRefactorDialogDriver : ControlDriver<InvokableRefactorDialog>
    {
        public LineEditDriver NameEdit { get; }
        public ButtonDriver OkButton { get; }
        public ButtonDriver CancelButton { get; }
        
        public ButtonDriver AddParameterButton { get; }
        

        public InvokableRefactorDialogDriver(Func<InvokableRefactorDialog> producer, string description = "") : base(producer, description)
        {
            NameEdit = new LineEditDriver(() => Root?.WithNameOrNull<LineEdit>("NameEdit"), Description + "-> NameEdit");
            OkButton = new ButtonDriver(() => Root?.WithNameOrNull<Button>("OkButton"), Description + "-> OkButton");
            CancelButton = new ButtonDriver(() => Root?.WithNameOrNull<Button>("CancelButton"), Description + "-> CancelButton");
            AddParameterButton = new ButtonDriver(() => Root?.WithNameOrNull<Button>("AddParameterButton"), Description + "-> AddParameterButton");
            
        }
        
        
        public IEnumerable<InvokableRefactorDialogParameterLineDriver> ParameterLines  {
            get
            {
                var root = Root;
                if (root == null)
                {
                    yield break;
                }

                var container = Root.WithName<GridContainer>("ParameterGrid");
                var uuids = container.GetChildNodes<LineEdit>()
                    .Where(it => it.Name.StartsWith("name-"))
                    .Select(it => it.Name.Substring("name-".Length))
                    .ToList();

                foreach (var uuid in uuids)
                {
                    yield return new InvokableRefactorDialogParameterLineDriver(uuid, () => Root?.WithName<GridContainer>("ParameterGrid"), Description + "-> Parameter Line");
                }
            }
        }
    }
}