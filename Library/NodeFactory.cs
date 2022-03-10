using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Library
{
    public class NodeFactory
    {
        private static NodeFactory _instance;

        private readonly List<Type> _nodeTypes;

        static NodeFactory()
        {
            _instance = new NodeFactory();
        }

        private NodeFactory()
        {
            _instance = this;
            _nodeTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(ScadNode).IsAssignableFrom(t) && !t.IsAbstract)
                .ToList();
        }

        /// <summary>
        /// Returns all language-level nodes.
        /// </summary>
        /// <returns></returns>
        public static List<ScadNode> GetAllNodes()
        {
            return _instance
                ._nodeTypes
                .Where(it => it != typeof(Start) && it != typeof(ModuleInvocation) && it != typeof(FunctionInvocation))
                .Select(Activator.CreateInstance)
                .Cast<ScadNode>()
                .Select(it =>
                {
                    it.PreparePorts();
                    return it;
                })
                .ToList();
        }

        public static ScadNode Duplicate(ScadNode node)
        {
            var duplicate = (ScadNode) Activator.CreateInstance(node.GetType());
            duplicate.PreparePorts();
            return duplicate;
        }

        public static ScadNode FromType(string type)
        {
            return (ScadNode) Activator.CreateInstance(Assembly.GetExecutingAssembly().GetType(type));
        }
        
    }
}