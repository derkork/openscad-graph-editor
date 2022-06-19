using System;
using System.Threading.Tasks;
using Godot;
using GodotExt;
using GodotTestDriver.Drivers;
using OpenScadGraphEditor.Widgets;
using OpenScadGraphEditor.Widgets.AddDialog;
using OpenScadGraphEditor.Widgets.IconButton;
using OpenScadGraphEditor.Widgets.InvokableRefactorDialog;
using Serilog;

namespace OpenScadGraphEditor.Tests.Drivers
{
    public class MainWindowDriver : ControlDriver<GraphEditor>
    {
        public GraphEditDriver GraphEditor { get; }
        public AddDialogDriver AddDialog { get; }
        public ButtonDriver AddModuleButton { get; }
        public InvokableRefactorDialogDriver InvokableRefactorDialog { get; }
        
        public TabContainerDriver TabContainer { get; }

        public MainWindowDriver(Func<GraphEditor> producer) : base(producer, "Main Window")
        {
            GraphEditor = new GraphEditDriver(() =>
            {
                // return the currently visible tab.
                var tabContainer = Root?.WithNameOrNull<TabContainer>("TabContainer");
                if (tabContainer == null)
                {
                    return null;
                }

                if (tabContainer.GetChildCount() <= tabContainer.CurrentTab)
                {
                    return null;
                }

                return tabContainer.GetChild<ScadGraphEdit>(tabContainer.CurrentTab);
            }, Description + " -> Graph Editor");

            AddDialog = new AddDialogDriver(
                () => Root?.WithNameOrNull<AddDialog>("AddDialog"),
                Description + " -> Add Dialog"
            );
            AddModuleButton = new ButtonDriver(
                () => Root?.WithNameOrNull<IconButton>("AddModuleButton")?.WithNameOrNull<Button>("Button"),
                Description + " -> Add Module Button"
            );
            InvokableRefactorDialog = new InvokableRefactorDialogDriver(
                () => Root?.WithNameOrNull<InvokableRefactorDialog>("InvokableRefactorDialog"),
                Description + " -> Invokable Refactor Dialog"
            );

            TabContainer = new TabContainerDriver(
                () => Root?.WithNameOrNull<TabContainer>("TabContainer"),
                Description + " -> Tab Container"
            );
        }


        public async Task RequestAddNode()
        {
            await GraphEditor.ClickCenter(ButtonList.Right);
        }

        public async Task AddNode(string nodeTitle)
        {
            await RequestAddNode();
            await AddDialog.SearchField.Enter(nodeTitle);
            // wait for the event to be fully processed
            await Root.NextFrame();
            await Root.NextFrame();
        }
    }
}