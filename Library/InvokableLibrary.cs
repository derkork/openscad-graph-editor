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
            if (description is ModuleDescription moduleDescription)
            {
                var moduleNode = Prefabs.New<ModuleInvocation>();
                moduleNode.Setup(moduleDescription);
                return moduleNode;
            }

            throw new NotImplementedException();
        }

        public static IReadOnlyCollection<ModuleDescription> GetDescriptions()
        {
            return BuiltIns.Modules;
        }

        public static ModuleDescription ForModuleDescriptionId(string id)
        {
            return BuiltIns.Modules.First(it => it.Id == id);
        }
    }
}