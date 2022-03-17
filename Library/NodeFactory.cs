using System;
using System.Reflection;
using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Library
{
    public static class NodeFactory
    {
        /// <summary>
        /// Builds a node of the given type, using the given invokable description if necessary.
        /// </summary>
        public static ScadNode Build<[MeansImplicitUse] T>(InvokableDescription description = null) where T : ScadNode
        {
            return Build(typeof(T), description);
        }

        /// <summary>
        /// Builds a node of the given type, using the given invokable description if necessary.
        /// </summary>
        public static ScadNode Build(Type nodeType, InvokableDescription description = null)
        {
            var node = (ScadNode) Activator.CreateInstance(nodeType);
            if (node is IReferToAnInvokable invokableReference)
            {
                GdAssert.That(description != null, "Need an invokable description to create an instance.");
                invokableReference.Setup(description);
            }

            node.PreparePorts();
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