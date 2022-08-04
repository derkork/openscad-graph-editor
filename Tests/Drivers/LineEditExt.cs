using System.Threading.Tasks;
using GodotTestDriver.Drivers;

namespace OpenScadGraphEditor.Tests.Drivers
{
    public static class LineEditDriverExt 
    {
        public static async Task SelectAndType(this LineEditDriver driver, string text)
        {
            await driver.ClickCenter();
            await driver.Type(text);
            await driver.ReleaseFocus();
        }
    }
}