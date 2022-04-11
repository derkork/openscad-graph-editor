using System.Linq;
using OpenScadGraphEditor.Library.External;

namespace OpenScadGraphEditor.Library
{
    public class GlobalLibrary : IReferenceResolver
    {
        public ModuleDescription ResolveModuleReference(string id)
        {
            return BuiltIns.Modules.First(it => it.Id == id);
        }

        public VariableDescription ResolveVariableReference(string id)
        {
           return BuiltIns.Variables.First(it => it.Id == id);   
        }

        public FunctionDescription ResolveFunctionReference(string id)
        {
            return BuiltIns.Functions.First(it => it.Id == id);
        }

        public ExternalReference ResolveExternalReference(string id)
        {
            return null;
        }
    }
}