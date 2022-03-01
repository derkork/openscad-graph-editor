using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using GodotExt;
using JetBrains.Annotations;

namespace OpenScadGraphEditor.Utils
{
    [PublicAPI]
    public static class GraphEditExt
    {
        /// <summary>
        /// Same as <see cref="GraphEdit.ConnectNode"/> but can be invoked with the actual node instances.
        /// </summary>
        public static void ConnectNode(this GraphEdit self, GraphNode from, int fromPort, GraphNode to, int toPort)
        {
            GdAssert.That(from.GetParent() == self, "From node is not a child of graph edit!");
            GdAssert.That(to.GetParent() == self, "To node is not a child of graph edit!");

            self.ConnectNode(from.Name, fromPort, to.Name, toPort);
        }


        /// <summary>
        /// Tries to get the first connection that is matching the given criteria, if there is any.
        /// This function is useful if you know that there can be only one or no connection that
        /// matches the given predicate. Otherwise you can just use <see cref="GetConnections"/>
        /// and filter the connections down with a "where".
        /// </summary>
        public static bool TryGetFirst(this GraphEdit self, Predicate<GraphConnection> predicate,
            out GraphConnection result)
        {
            var matches = self.GetConnections().Where(it => predicate(it));
            foreach (var match in matches)
            {
                result = match;
                return true;
            }

            result = default;
            return false;
        }

        /// <summary>
        /// Returns all connections currently known to this <see cref="GraphEdit"/>. The returned
        /// enumeration contains a type-safe representation of the connections rather than
        /// a dictionary which is returned by the vanilla implementation.
        /// </summary>
        public static IEnumerable<GraphConnection> GetConnections(this GraphEdit self)
        {
            return self.GetConnectionList()
                .Cast<Dictionary>()
                .Select(item => new GraphConnection(
                    self,
                    (string) item["from"],
                    (int) item["from_port"],
                    (string) item["to"],
                    (int) item["to_port"]
                ));
        }

        [PublicAPI]
        public readonly struct GraphConnection
        {
            private readonly GraphEdit _source;
            public string From { get; }
            public int FromPort { get; }
            public string To { get; }
            public int ToPort { get; }

            public bool IsFrom(string node)
            {
                return From == node;
            }

            public bool IsFrom(GraphNode node, int port)
            {
                return IsFrom(node.Name, port);
            }

            public bool IsFrom(string node, int port)
            {
                return From == node && FromPort == port;
            }

            public bool IsTo(string node)
            {
                return To == node;
            }

            public bool IsTo(string node, int port)
            {
                return To == node && ToPort == port;
            }

            public bool IsTo(GraphNode node, int port)
            {
                return IsTo(node.Name, port);
            }

            public bool InvolvesNode(GraphNode node)
            {
                return InvolvesNode(node.Name);
            }

            public bool InvolvesNode(string node)
            {
                return From == node || To == node;
            }

            public T GetFromNode<T>() where T : GraphNode
            {
                return _source.AtPath<T>(From);
            }

            public T GetToNode<T>() where T : GraphNode
            {
                return _source.AtPath<T>(To);
            }

            public void Disconnect()
            {
                _source.DisconnectNode(From, FromPort, To, ToPort);
            }

            public GraphConnection(GraphEdit source, string @from, int fromPort, string to, int toPort)
            {
                _source = source;
                From = @from;
                FromPort = fromPort;
                To = to;
                ToPort = toPort;
            }
        }
    }
}