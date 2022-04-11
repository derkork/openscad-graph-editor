using System;
using System.Reflection;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library.External;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Library
{
    /// <summary>
    /// This class used to build <see cref="ScadNode"/>s. Don't try to build them manually
    /// as this can get a bit complex. Use the factory and be happy.
    /// </summary>
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
            node.SetupPorts(description);
            node.PrepareLiteralsFromPortDefinitions();
            return node;
        }

        public static ScadNode Build<[MeansImplicitUse] T>(VariableDescription description)
            where T : ScadNode, IReferToAVariable
        {
            var node = (T) Activator.CreateInstance(typeof(T));
            node.SetupPorts(description);
            node.PrepareLiteralsFromPortDefinitions();
            return node;
        }
        
        public static ScadNode Build<[MeansImplicitUse] T>(ExternalReference externalReference)
            where T : ScadNode, IReferToAnExternalReference
        {
            var node = (T) Activator.CreateInstance(typeof(T));
            node.SetupPorts(externalReference);
            node.PrepareLiteralsFromPortDefinitions();
            return node;
        }
        
        

        /// <summary>
        /// Builds a node of the given type.
        /// </summary>
        public static ScadNode Build(Type nodeType)
        {
            var node = (ScadNode) Activator.CreateInstance(nodeType);
            node.PrepareLiteralsFromPortDefinitions();
            return node;
        }

        /// <summary>
        /// Loads a scad node from a saved node representation.
        /// </summary>
        public static ScadNode FromSavedNode(SavedNode savedNode, IReferenceResolver resolver)
        {
            var node = (ScadNode) Activator.CreateInstance(Assembly.GetExecutingAssembly().GetType(savedNode.Type));
            node.RestorePortDefinitions(savedNode, resolver);
            node.RestoreLiteralStructures(savedNode, resolver);
            node.RestoreLiteralValues(savedNode, resolver);
            return node;
        }
    }
}