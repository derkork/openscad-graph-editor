using System.Threading.Tasks;
using Godot;
using GodotXUnitApi;
using OpenScadGraphEditor.Tests.Drivers;
using Xunit;

namespace OpenScadGraphEditor.Tests
{
    public class MainWindowTest : BaseTest
    {
        protected MainWindowDriver MainWindow { get; private set; }

        protected override async Task Setup()
        {
            await GDU.OnProcessAwaiter;
            
            GD.Print("Setup starts");
            var editor = await Fixture.LoadAndAddScene<GraphEditor>("res://GraphEditor.tscn");
            MainWindow = new MainWindowDriver(() => editor);
            
            GD.Print("Waiting for main window");
            // Wait until we can see a graph editor
            await WithinSeconds(3, () =>
            {
                Assert.True(MainWindow.GraphEditor.IsVisible);
            });
            GD.Print("Setup done.");
        }
    }
}