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


        // TODO: merge this somehow with NodeFactory
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
                    var moduleNode = new ModuleInvocation();
                    moduleNode.Setup(moduleDescription);
                    moduleNode.PreparePorts();
                    return moduleNode;
                }
                case FunctionDescription functionDescription:
                {
                    var functionNode = new FunctionInvocation();
                    functionNode.Setup(functionDescription);
                    functionNode.PreparePorts();
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