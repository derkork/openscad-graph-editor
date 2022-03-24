using System;
using System.Reflection;
using JetBrains.Annotations;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Library
{
    public static class NodeFactory
    {
        /// <summary>
        /// Builds a node of the given type.
        /// </summary>
        public static ScadNode Build<[MeansImplicitUse] T>() where T : ScadNode
        {
            return Build(typeof(T));
        }

        public static ScadNode Build<[MeansImplicitUse] T>(InvokableDescription description)
            where T : ScadNode, IReferToAnInvokable
        {
            var node = (T) Activator.CreateInstance(typeof(T));
            node.Setup(description);
            node.PreparePortLiterals();
            return node;
        }

        public static ScadNode Build<[MeansImplicitUse] T>(VariableDescription description)
            where T : ScadNode, IReferToAVariable
        {
            var node = (T) Activator.CreateInstance(typeof(T));
            node.Setup(description);
            node.PreparePortLiterals();
            return node;
        }

        /// <summary>
        /// Builds a node of the given type, using the given invokable description if necessary.
        /// </summary>
        public static ScadNode Build(Type nodeType)
        {
            var node = (ScadNode) Activator.CreateInstance(nodeType);
            node.PreparePortLiterals();
            return node;
        }

        /// <summary>
        /// Loads a scad node from a saved node representation.
        /// </summary>
        public static ScadNode FromSavedNode(SavedNode savedNode, IReferenceResolver resolver)
        {
            var node = (ScadNode) Activator.CreateInstance(Assembly.GetExecutingAssembly().GetType(savedNode.Type));
            node.LoadFrom(savedNode, resolver);
            return node;
        }
    }
}