using System;
using GodotExt;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.External;
using OpenScadGraphEditor.Refactorings;

namespace OpenScadGraphEditor.Nodes.ImportScadFile
{
    /// <summary>
    /// Class for nodes importing data from *.scad files.
    /// </summary>
    public sealed class ImportScadFile : ScadNode, IReferToAnExternalReference, IHaveSpecialDestruction
    {

        public override string NodeTitle
        {
            get
            {
                if (ExternalReference != null)
                {
                    switch (ExternalReference.Mode)
                    {
                        case IncludeMode.Use:
                            return "use " + ExternalReference.IncludePath;
                        case IncludeMode.Include:
                            return "include " + ExternalReference.IncludePath;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else
                {
                    return "use/include";
                }
            }
        }

        public override string NodeDescription => "Imports a SCAD file. You can use this to import external libraries.";
        
        public ExternalReference ExternalReference { get; private set; }

        public ImportScadFile()
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
            GdAssert.That(ExternalReference != null, "External reference not found.");
            base.RestorePortDefinitions(node, resolver);
        }

        public override string Render(IScadGraph context)
        {
            var next = RenderOutput(context, 0);
            switch (ExternalReference.Mode)
            {
                case IncludeMode.Use:
                    return "use <" + ExternalReference.IncludePath + ">;\n" + next;
                case IncludeMode.Include:
                    return "include <" + ExternalReference.IncludePath + ">;\n" + next;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public Refactoring GetDestructionRefactoring(IScadGraph nodeHolder)
        {
            return new DeleteImportScadFileRefactoring(nodeHolder, this);
        }
    }
}