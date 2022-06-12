using System.Threading.Tasks;
using OpenScadGraphEditor.Tests.Drivers;
using Xunit;

namespace OpenScadGraphEditor.Tests
{
    public class MainWindowTest : BaseTest
    {
        protected MainWindowDriver MainWindow { get; private set; }

        protected override async Task Setup()
        {
            var editor = await Fixture.LoadAndAddScene<GraphEditor>("res://GraphEditor.tscn");
            MainWindow = new MainWindowDriver(() => editor);
            
            // Wait until we can see a graph editor
            await WithinSeconds(3, () =>
            {
                Assert.True(MainWindow.GraphEditor.IsVisible);
            });
        }
    }
}