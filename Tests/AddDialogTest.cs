using System.Linq;
using System.Threading.Tasks;
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
            Assert.Contains("Circle (Radius)", selectableItems);
            Assert.Contains("Circle (Diameter)", selectableItems);
            Assert.Equal(2, selectableItems.Count);
        }

        [GodotFact(Frame = GodotFactFrame.Process)]
        public async Task AddingACubeWorks()
        {
            // when
            // i request to add a specific node
            await MainWindow.AddNode("cube");

            // then
            // the add dialog closes
            Assert.False(MainWindow.AddDialog.IsVisible, "Dialog did not close");

            // i can see the cube node
            var cube = MainWindow.GraphEditor.Nodes.FirstOrDefault();
            Assert.NotNull(cube);
            Assert.Equal("Cube", cube.Title);
        }
    }
}