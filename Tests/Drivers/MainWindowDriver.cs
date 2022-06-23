using System;
using System.Threading.Tasks;
using Godot;
using GodotExt;
using GodotTestDriver.Drivers;
using OpenScadGraphEditor.Widgets;
using OpenScadGraphEditor.Widgets.AddDialog;
using OpenScadGraphEditor.Widgets.DocumentationDialog;
using OpenScadGraphEditor.Widgets.IconButton;
using OpenScadGraphEditor.Widgets.InvokableRefactorDialog;

namespace OpenScadGraphEditor.Tests.Drivers
{
    public class MainWindowDriver : ControlDriver<GraphEditor>
    {
        public GraphEditDriver<GraphEdit, ScadNodeWidgetDriver, ScadNodeWidget> GraphEditor { get; }
        public AddDialogDriver AddDialog { get; }
        public ButtonDriver AddModuleButton { get; }
        public InvokableRefactorDialogDriver InvokableRefactorDialog { get; }
        public TabContainerDriver TabContainer { get; }
        public PopupMenuDriver PopupMenu { get; }

        public DocumentationDialogDriver DocumentationDialog { get; }

        public MainWindowDriver(Func<GraphEditor> producer) : base(producer, "Main Window")
        {
            GraphEditor = new GraphEditDriver<GraphEdit, ScadNodeWidgetDriver, ScadNodeWidget>(() =>
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
                },
                (node, description) => new ScadNodeWidgetDriver(node, description)
                , Description + " -> Graph Editor");

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

            PopupMenu = new PopupMenuDriver(
                () => Root?.WithNameOrNull<PopupMenu>("QuickActionsPopup")
            );

            DocumentationDialog = new DocumentationDialogDriver(
                () => Root?.WithNameOrNull<DocumentationDialog>("DocumentationDialog"),
                Description + " -> Documentation Dialog"
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