using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.External;

namespace OpenScadGraphEditor.Nodes
{
    public class IncludeScadFile : ImportScadFile
    {
        public override string NodeTitle => "Include";
        public override string NodeDescription => "Includes a scad file";

        public override string Render(IScadGraph context)
        {
            return $"include <{PathResolver.Decode(ExternalReference.SourceFile, out _)}>;";
        }
    }
}