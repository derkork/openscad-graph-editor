using System;
using System.Collections.Generic;
using System.Linq;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Library
{
    public class InvokableLibrary
    {
        private static InvokableLibrary _instance;


        static InvokableLibrary()
        {
            _instance = new InvokableLibrary();
        }
        
        /// <summary>
        /// Builds a ScadNode from the given description.
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        public static ScadNode FromDescription(InvokableDescription description)
        {
            switch (description)
            {
                case ModuleDescription moduleDescription:
                {
                    var moduleNode = Prefabs.New<ModuleInvocation>();
                    moduleNode.Setup(moduleDescription);
                    return moduleNode;
                }
                case FunctionDescription functionDescription:
                {
                    var functionNode = Prefabs.New<FunctionInvocation>();
                    functionNode.Setup(functionDescription);
                    return functionNode;
                }
                default:
                    throw new NotImplementedException();
            }
        }

        public static IEnumerable<InvokableDescription> GetDescriptions()
        {
            return BuiltIns.Modules.Union<InvokableDescription>(BuiltIns.Functions);
        }

        public static ModuleDescription ForModuleDescriptionId(string id)
        {
            return BuiltIns.Modules.First(it => it.Id == id);
        }

        public static FunctionDescription ForFunctionDescriptionId(string id)
        {
            return BuiltIns.Functions.First(it => it.Id == id);
        }
    }
}