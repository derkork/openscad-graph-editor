using System;
using System.IO;
using System.Text;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace GodotXUnitApi.Internal
{
    /// <summary>
    /// this class will handle outputs in tests.
    ///
    /// i would also like to figure out how to captures GD.Print
    /// to here, but i havent figured out that part yet.
    ///
    /// the main issue here is that passing around ITestOutputHelper
    /// for internal code is bad practice as well, and GD.Print
    /// is not available if you're running unit tests outside of godot.
    ///
    /// also note. we cannot just replace this ITestOutputHelper instance
    /// handed in with our own because it breaks compatibility with
    /// IDE runners.
    /// </summary>
    public class GodotTestOutputHelper : TextWriter
    {
        private TestOutputHelper wrapping;
        private TextWriter oldOutput;
        private StringBuilder builder = new StringBuilder();

        public override Encoding Encoding { get; } = Console.OutputEncoding;

        public GodotTestOutputHelper(TestOutputHelper wrapping = null)
        {
            this.wrapping = wrapping ?? new TestOutputHelper();
        }

        public void Initialize(IMessageBus messageBus, ITest test)
        {
            wrapping.Initialize(messageBus, test);
            oldOutput = Console.Out;
            Console.SetOut(this);
        }

        public string UnInitAndGetOutput()
        {
            if (oldOutput != null)
                Console.SetOut(oldOutput);
            oldOutput = null;
            if (builder.Length != 0)
                WriteLine();
            return wrapping.Output;
        }
        
        public override void Write(char value)
        {
            builder.Append(value);
        }
            
        public override void Write(String value)
        {
            builder.Append(value);
        }
        
        public override void WriteLine()
        {
            wrapping.WriteLine(builder.ToString());
            builder.Clear();
        }

        public override void WriteLine(String value)
        {
            builder.Append(value);
            WriteLine();
        }
    }
}