using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.External;

namespace OpenScadGraphEditor.Nodes
{
    /// <summary>
    /// Base class for nodes importing data from *.scad files.
    /// </summary>
    public abstract class ImportScadFile : ScadNode, IReferToAnExternalReference
    {
        public ExternalReference ExternalReference { get; private set; }

        protected ImportScadFile()
        {
            InputPorts
                .Flow();

            OutputPorts
                .Flow();
        }

        public void SetupPorts(ExternalReference externalReference)
        {
            ExternalReference = externalReference;
        }

        public override void SaveInto(SavedNode node)
        {
            node.SetData("external_reference_id", ExternalReference.Id);
            base.SaveInto(node);
        }

        public override void RestorePortDefinitions(SavedNode node, IReferenceResolver resolver)
        {
            ExternalReference = resolver.ResolveExternalReference(node.GetData("external_reference_id"));
            base.RestorePortDefinitions(node, resolver);
        }
    }
}