using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;
using JetBrains.Annotations;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
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
                .Select(it => it.New())
                .Cast<ScadNode>()
                .ToList();
        }

        public static ScadNode Duplicate(ScadNode node)
        {
            return (ScadNode) node.GetType().New();
        }

        public static ScadNode FromScript(string scriptPath)
        {
            return (ScadNode) GD.Load<CSharpScript>(scriptPath)?.New();
        }
        
    }
}