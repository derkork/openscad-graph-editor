using System.Linq;

namespace OpenScadGraphEditor.Library
{
    public class GlobalLibrary : IReferenceResolver
    {
        public ModuleDescription ResolveModuleReference(string id)
        {
            return BuiltIns.Modules.First(it => it.Id == id);
        }

        public FunctionDescription ResolveFunctionReference(string id)
        {
            return BuiltIns.Functions.First(it => it.Id == id);
        }
    }
}