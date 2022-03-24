namespace OpenScadGraphEditor.Library
{
    public interface IReferenceResolver
    {
        FunctionDescription ResolveFunctionReference(string id);
        ModuleDescription ResolveModuleReference(string id);
        VariableDescription ResolveVariableReference(string id);

    }
}