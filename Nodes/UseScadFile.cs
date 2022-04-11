using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.External;

namespace OpenScadGraphEditor.Nodes
{
    public class UseScadFile : ImportScadFile
    {
        public override string NodeTitle => "Use";
        public override string NodeDescription => "Uses a scad file";

        public override string Render(IScadGraph context)
        {
            return $"use <{PathResolver.Decode(ExternalReference.SourceFile, out _)}>;";
        }
    }
}