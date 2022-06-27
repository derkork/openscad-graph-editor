using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Godot;
using GodotXUnitApi;
using Xunit;

namespace OpenScadGraphEditor.Tests
{
    public class AddDialogTest : MainWindowTest
    {
        [GodotFact(Frame = GodotFactFrame.Process)]
        public async Task AddDialogOpens()
        {
            // when
            // i request to add a node
            await MainWindow.RequestAddNode();

            // then
            // i can see the add node dialog
            Assert.True(MainWindow.AddDialog.IsVisible, "Dialog did not appear");
        }


        [GodotFact(Frame = GodotFactFrame.Process)]
        public async Task TypingInDialogFiltersTheList()
        {
            // setup
            await MainWindow.RequestAddNode();

            // when 
            // i type in the dialog
            await MainWindow.AddDialog.SearchField.Type("circle");

            // then
            var selectableItems = MainWindow.AddDialog.ItemList.SelectableItems;
            // i see the two circle items in the list
            Assert.Contains("Circle (Radius) [CirR]", selectableItems);
            Assert.Contains("Circle (Diameter) [CirD]", selectableItems);
            Assert.Equal(2, selectableItems.Count);
        }

        [GodotFact(Frame = GodotFactFrame.Process)]
        public async Task AddingACubeWorks()
        {
            // when
            // i request to add a specific node
            await MainWindow.AddNode("cbe");

            // then
            // the add dialog closes
            Assert.False(MainWindow.AddDialog.IsVisible, "Dialog did not close");

            // i can see the cube node
            var cube = MainWindow.GraphEditor.Nodes.FirstOrDefault();
            Assert.NotNull(cube);
            Assert.Equal("Cube", cube.Title);
        }

        [GodotFact(Frame = GodotFactFrame.Process)]
        public async Task QuickLookupsYieldUniqueResults()
        {
            // setup
            // open dialog
            await MainWindow.RequestAddNode();
            
            // make a regex to filter out the quick node lookups. They look like this: "Cube (Radius) [cbe]". We want the "[cbe]" part.
            // however, the "[cbe]" part may not be present in which case we want an empty string.
            var regex = new Regex(@"\[(?<lkup>[^\]]+)\]");
            
            
            // get all entries in the list
            var allQuickLookups = MainWindow.AddDialog.ItemList.SelectableItems
                .Select(it =>  regex.Match(it).Groups["lkup"].Value)
                .Where(it  => !it.Empty());
            
            // when
            // i type a quick lookup in the dialog
            foreach (var quickLookup in allQuickLookups)
            {
                await MainWindow.AddDialog.SearchField.Type(quickLookup);
                // then
                // there is only one result in the list
                Assert.True(MainWindow.AddDialog.ItemList.SelectableItems.Count == 1, "Quick lookup " + quickLookup + " did not yield a unique result.");
            }
        }
    }
}