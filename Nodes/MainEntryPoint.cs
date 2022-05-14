using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Nodes
{
    public class MainEntryPoint : EntryPoint
    {
        public override string NodeTitle => "<main>";
        public override string NodeDescription => "The main entry point.";

        public MainEntryPoint()
        {
            OutputPorts
                .Flow();
        }

        public override string GetPortDocumentation(PortId portId)
        {
            return "Output flow";
        }

        public override string Render(IScadGraph context)
        {
            return RenderOutput(context, 0);
        }
    }
}