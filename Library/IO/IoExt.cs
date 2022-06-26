using System;
using OpenScadGraphEditor.Library.External;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Library.IO
{
    public static class IoExt
    {
        public static InvokableDescription FromSavedState(this SavedInvokableDescription saved)
        {
            switch (saved)
            {
                case SavedFunctionDescription savedFunctionDescription:
                    var result = new FunctionDescription();
                    result.LoadFrom(savedFunctionDescription);
                    return result;
                case SavedModuleDescription savedModuleDescription:
                    var result2 = new ModuleDescription();
                    result2.LoadFrom(savedModuleDescription);
                    return result2;
                case SavedMainModuleDescription savedMainModuleDescription:
                    var result3 = new MainModuleDescription();
                    result3.LoadFrom(savedMainModuleDescription);
                    return result3;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static VariableDescription FromSavedState(this SavedVariableDescription saved)
        {
            var result = new VariableDescription();
            result.LoadFrom(saved);
            return result;
        }
        
        public static ParameterDescription FromSavedState(this SavedParameterDescription saved)
        {
            var result = new ParameterDescription();
            result.LoadFrom(saved);
            return result;
        }

        public static ExternalReference FromSavedState(this SavedExternalReference saved)
        {
            var result = new ExternalReference();
            result.LoadFrom(saved);
            return result;
        }
        

        public static SavedInvokableDescription ToSavedState(this InvokableDescription invokableDescription)
        {
            switch (invokableDescription)
            {
                case FunctionDescription functionDescription:
                    var result = Prefabs.New<SavedFunctionDescription>();
                    functionDescription.SaveInto(result);
                    return result;
                case ModuleDescription moduleDescription:
                    var result2 = Prefabs.New<SavedModuleDescription>();
                    moduleDescription.SaveInto(result2);
                    return result2;
                case MainModuleDescription mainModuleDescription:
                    var result3 = Prefabs.New<SavedMainModuleDescription>();
                    mainModuleDescription.SaveInto(result3);
                    return result3;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static SavedVariableDescription ToSavedState(this VariableDescription variableDescription)
        {
            var result = Prefabs.New<SavedVariableDescription>();
            variableDescription.SaveInto(result);
            return result;
        }
        
        public static SavedParameterDescription ToSavedState(this ParameterDescription parameterDescription)
        {
            var result = Prefabs.New<SavedParameterDescription>();
            parameterDescription.SaveInto(result);
            return result;
        }

        public static SavedNode ToSavedState(this ScadNode scadNode)
        {
            var result = Prefabs.New<SavedNode>();
            scadNode.SaveInto(result);
            return result;
        }
        
        public static SavedGraph ToSavedState(this ScadGraph graph)
        {
            var result = Prefabs.New<SavedGraph>();
            graph.SaveInto(result);
            return result;
        }
        
        public static SavedExternalReference ToSavedState(this ExternalReference externalReference)
        {
            var result = Prefabs.New<SavedExternalReference>();
            externalReference.SaveInto(result);
            return result;
        }
        
    }
}