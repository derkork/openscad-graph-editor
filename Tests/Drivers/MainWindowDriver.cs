using System;
using System.Threading.Tasks;
using Godot;
using GodotExt;
using GodotTestDriver.Drivers;
using GodotTestDriver.Util;
using OpenScadGraphEditor.Widgets;
using OpenScadGraphEditor.Widgets.AddDialog;

namespace OpenScadGraphEditor.Tests.Drivers
{
    public class MainWindowDriver : ControlDriver<GraphEditor>
    {
        public MainWindowDriver(Func<GraphEditor> producer) : base(producer)
        {
            GraphEditor = new GraphEditDriver(() =>
            {
                // return the currently visible tab.
                var tabContainer = Root?.WithName<TabContainer>("TabContainer");
                if (tabContainer == null)
                {
                    return null;
                }
                if (tabContainer.GetChildCount() <= tabContainer.CurrentTab)
                {
                    return null;
                }
                return tabContainer.GetChild<ScadGraphEdit>(tabContainer.CurrentTab);
            });

            AddDialog = new AddDialogDriver(() => Root?.WithName<AddDialog>("AddDialog"));
        }

        public GraphEditDriver GraphEditor { get; }
        public AddDialogDriver AddDialog { get; }

        
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