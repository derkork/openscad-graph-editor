using System.Linq;
using System.Threading.Tasks;
using Godot;
using GodotTestDriver.Drivers;
using GodotTestDriver.Input;
using GodotXUnitApi;
using Xunit;

namespace OpenScadGraphEditor.Tests
{
    public class DocumentationTests : MainWindowTest
    {
        [GodotFact(Frame = GodotFactFrame.Process)]
        public async Task EditingDocumentationForAModuleWorks()
        {
            // when
            // i create a new module
            await MainWindow.AddModuleButton.ClickCenter();
            await MainWindow.InvokableRefactorDialog.NameEdit.Type("test_module");
            // and i add three parameters
            await MainWindow.InvokableRefactorDialog.AddParameterButton.ClickCenter();
            await MainWindow.InvokableRefactorDialog.AddParameterButton.ClickCenter();
            await MainWindow.InvokableRefactorDialog.AddParameterButton.ClickCenter();
            
            // and i press OK
            await MainWindow.InvokableRefactorDialog.OkButton.ClickCenter();
            var moduleEntryPoint = MainWindow.GraphEditor.Nodes.First();

            // and i right-click the newly created module's entry point
            await moduleEntryPoint.ClickAtSelectionSpot(ButtonList.Right);
            // and i select  the "Edit documentation of test_module" entry in the popup menu
            await MainWindow.PopupMenu.SelectItemWithText("Edit documentation of test_module");
            
            // then 
            // i should see the documentation editor
            Assert.True( MainWindow.DocumentationDialog.IsVisible);

            // when i enter some description
            await MainWindow.DocumentationDialog.DescriptionEdit.Type("This is a test module");
            // and i also enter some documentation for the parameters
            await MainWindow.DocumentationDialog.Parameters.First().Type("This is the first parameter");
            await MainWindow.DocumentationDialog.Parameters.Skip(1).First().Type("This is the second parameter");
            await MainWindow.DocumentationDialog.Parameters.Skip(2).First().Type("This is the third parameter");
            
            // and i press OK
            await MainWindow.DocumentationDialog.OkButton.ClickCenter();
            
            // and I select the module entry point again
            await moduleEntryPoint.Select();
            
            // and i press F1
            await MainWindow.Root.TypeKey(KeyList.F1);
            
            
            // then
            // the help dialog is shown
            Assert.True(MainWindow.HelpDialog.IsVisible);
            // and the title of the help dialog is "test_module"
            Assert.Equal("test_module", MainWindow.HelpDialog.Title);
            // and the description of the help dialog is "This is a test module"
            Assert.Equal("This is a test module", MainWindow.HelpDialog.NodeDescription);
            // and on the right side of the node we have the three parameters and their descriptions
            Assert.Equal("This is the first parameter", MainWindow.HelpDialog.GetPortDescription(Port.Output(0)));
            Assert.Equal("This is the second parameter", MainWindow.HelpDialog.GetPortDescription(Port.Output(1)));
            Assert.Equal("This is the third parameter", MainWindow.HelpDialog.GetPortDescription(Port.Output(2)));
        }
        
    }
}