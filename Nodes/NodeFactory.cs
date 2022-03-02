using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;
using JetBrains.Annotations;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class NodeFactory : Node
    {
        private static NodeFactory _instance;

        private readonly List<Type> _nodeTypes;


        private NodeFactory()
        {
            _instance = this;
            _nodeTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(ScadNode).IsAssignableFrom(t) && !t.IsAbstract)
                .ToList();
        }

        public static List<ScadNode> GetAllNodes()
        {
            return _instance
                ._nodeTypes
                .Where(it => it != typeof(Start))
                .Select(it => it.New())
                .Cast<ScadNode>()
                .ToList();
        }

        public static ScadNode MakeOne(ScadNode node)
        {
            return (ScadNode) node.GetType().New();
        }

        public static ScadNode FromScript(string scriptPath)
        {
            return (ScadNode) GD.Load<CSharpScript>(scriptPath)?.New();
        }
        
    }
}