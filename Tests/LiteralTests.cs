using System.Linq;
using System.Threading.Tasks;
using GodotTestDriver.Drivers;
using GodotXUnitApi;
using OpenScadGraphEditor.Tests.Drivers;
using Xunit;

namespace OpenScadGraphEditor.Tests
{
    public class LiteralTests : MainWindowTest
    {
        [GodotFact(Frame = GodotFactFrame.Process)]
        public async Task EnteringAFloatNumberWorks()
        {
            // when
            // i add a cube node
            await MainWindow.AddNode("Cbe");
            var cube = MainWindow.GraphEditor.Nodes.Last();
            
            // and i enable the literal for the first input port
            await cube.ToggleButton(Port.Input(0)).ClickCenter();
            
            // and i enter a float number into the X field
            await cube.Vector3Literal(Port.Input(0)).X.SelectAndType("1.1");

            // then the value should be 1.1
            Assert.Equal("1.1", cube.Vector3Literal(Port.Input(0)).X.Text);
        }

        [GodotFact(Frame = GodotFactFrame.Process)]
        public async Task AFloatNumberWithACommaIsCorrected()
        {
            // when
            // i add a cube node
            await MainWindow.AddNode("Cbe");
            var cube = MainWindow.GraphEditor.Nodes.Last();
            
            // and i enable the literal for the first input port
            await cube.ToggleButton(Port.Input(0)).ClickCenter();
            
            // and i enter a float number with a comma into the X field
            await cube.Vector3Literal(Port.Input(0)).X.SelectAndType("1,1");

            // then the value should be 11
            Assert.Equal("11", cube.Vector3Literal(Port.Input(0)).X.Text);
        }

        [GodotFact(Frame = GodotFactFrame.Process)]
        public async Task ANonNumberIsConvertedToZero()
        {
            // when
            // i add a cube node
            await MainWindow.AddNode("Cbe");
            var cube = MainWindow.GraphEditor.Nodes.Last();
            
            // and i enable the literal for the first input port
            await cube.ToggleButton(Port.Input(0)).ClickCenter();
            
            // and i enter something that is not a valid number
            await cube.Vector3Literal(Port.Input(0)).X.SelectAndType("1.1.7");
            
            // then the value should be 0
            Assert.Equal("0", cube.Vector3Literal(Port.Input(0)).X.Text);
        }
    }
}