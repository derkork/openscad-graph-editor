using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using OpenScadGraphEditor.Refactorings;
using OpenScadGraphEditor.Utils;
using Serilog;

namespace OpenScadGraphEditor.Nodes
{
    /// <summary>
    /// This class is a collection of all connection rules. New node types can use the static methods to add
    /// new connection rules.
    /// </summary>
    public static class ConnectionRules
    {
        private static readonly List<ConnectionRule> ConnectRules = new List<ConnectionRule>();
        private static readonly List<ConnectionRule> DisconnectRules = new List<ConnectionRule>();

        private delegate OperationResult ConnectionRule(ScadConnection connection);


        /// <summary>
        /// Sets up some global connection rules.
        /// </summary>
        static ConnectionRules()
        {
            // there can only ever be one incoming connection to a port unless it is a geometry port
            AddConnectRule(it => !it.IsToPortType(PortType.Geometry),
                OperationRuleDecision.Undecided, // this is a side-effect-only rule
                it => new DeleteInputConnectionsRefactoring(it.Owner, it.To, it.ToPort));
            
            // the same connection may not exist twice
            AddConnectRule(it => it.Owner.GetAllConnections().Any(c => c.From == it.From && c.To == it.To && c.FromPort == it.FromPort && c.ToPort == it.ToPort),
                OperationRuleDecision.Veto);
            
            // We can never connect nodes as a circle
            AddConnectRule(WouldCreateCircle, OperationRuleDecision.Veto);

            // connections of the same type can always be made
            AddConnectRule(it =>
                    it.TryGetFromPortType(out var fromType) && it.TryGetToPortType(out var toType) &&
                    fromType == toType,
                OperationRuleDecision.Allow
            );

            // a connection from "Any" can be made to all expression types
            AddConnectRule(it =>
                    it.TryGetFromPortType(out var fromType) && fromType == PortType.Any &&
                    it.TryGetToPortType(out var toType) && toType.IsExpressionType(),
                OperationRuleDecision.Allow
            );
            
            // a connection to "Any" can be made from all expression types
            AddConnectRule(it =>
                    it.TryGetToPortType(out var toType) && toType == PortType.Any &&
                    it.TryGetFromPortType(out var fromType) && fromType.IsExpressionType(),
                OperationRuleDecision.Allow
            );
            
            // a connection to "Array" can also be made from "Vector3" types (but not the other way around)
            AddConnectRule(it =>
                    it.TryGetToPortType(out var toType) && toType == PortType.Array &&
                    it.TryGetFromPortType(out var fromType) && fromType == PortType.Vector3,
                OperationRuleDecision.Allow
            );
            
            // same for Vector2
            AddConnectRule(it =>
                    it.TryGetToPortType(out var toType) && toType == PortType.Array &&
                    it.TryGetFromPortType(out var fromType) && fromType == PortType.Vector2,
                OperationRuleDecision.Allow
            );
            
        }

        public static bool WouldCreateCircle(ScadConnection connection)
        {
         
            // starting at the given connection, walk the graph and check if we would create a circle
            var openSet = new HashSet<ScadConnection> {connection};

            while (openSet.Count > 0)
            {
                var current = openSet.First();
                openSet.Remove(current);

                if (current.To == connection.From)
                {
                    Log.Warning("Inserting node would create a circle");
                    return true;
                }
                
                // now check all outgoing connections of the current node.
                connection.Owner.GetAllConnections()
                    .Where(it => it.From == current.To)
                    .ForAll(it => openSet.Add(it));
            }

            return false;
        }
        
        public static void AddConnectRule(Predicate<ScadConnection> predicate, OperationRuleDecision decision,
            params Func<ScadConnection, Refactoring>[] refactorings)
        {
            ConnectRules.Add(MakeRule(predicate, decision, refactorings));
        }

        public static void AddDisconnectRule(Predicate<ScadConnection> predicate, OperationRuleDecision decision,
            params Func<ScadConnection, Refactoring>[] refactorings)
        {
            DisconnectRules.Add(MakeRule(predicate, decision, refactorings));
        }

        private static ConnectionRule MakeRule(Predicate<ScadConnection> predicate, OperationRuleDecision decision,
            Func<ScadConnection, Refactoring>[] refactorings)
        {
            return connection =>
            {
                if (predicate(connection))
                {
                    switch (decision)
                    {
                        case OperationRuleDecision.Undecided:
                            return OperationResult.Undecided(refactorings.Select(it => it(connection)));
                        case OperationRuleDecision.Allow:
                            return OperationResult.Allow(refactorings.Select(it => it(connection)));
                        case OperationRuleDecision.Veto:
                            return OperationResult.Veto();
                        default:
                            throw new ArgumentOutOfRangeException(nameof(decision), decision, null);
                    }
                }

                return OperationResult.Undecided();
            };
        }

        /// <summary>
        /// Checks if a connection is allowed. Checks all the connection rules and return the effective
        /// result of these rules. If no rule can decide, the connection will be vetoed. 
        /// </summary>
        public static OperationResult CanConnect(ScadConnection connection)
        {
            var result = ConnectRules
                .Select(rule => rule(connection))
                .Aggregate(OperationResult.Undecided(), CalculateEffectiveResult);

            return result.Decision == OperationRuleDecision.Undecided ? OperationResult.Veto() : result;
        }

        /// <summary>
        /// Checks if disconnecting a connection is allowed. Checks all the disconnection rules and return the effective
        /// result of these rules. If no rule can decide, the disconnection is allowed. 
        /// </summary>
        public static OperationResult CanDisconnect(ScadConnection connection)
        {
            var result = DisconnectRules
                .Select(rule => rule(connection))
                // on undecided, we allow the disconnect, since nobody vetoed it.
                .Aggregate(OperationResult.Undecided(), CalculateEffectiveResult);

            return result.Decision == OperationRuleDecision.Undecided ? OperationResult.Allow() : result;
        }

        private static OperationResult CalculateEffectiveResult(OperationResult first, OperationResult second)
        {
            // if any of the two is Veto, the result is Veto
            if (first.Decision == OperationRuleDecision.Veto || second.Decision == OperationRuleDecision.Veto)
            {
                return OperationResult.Veto();
            }

            // if one of the two is Undecided, take the other one
            if (first.Decision == OperationRuleDecision.Undecided)
            {
                return OperationResult.Of(second.Decision, first.Refactorings.Concat(second.Refactorings));
            }

            if (second.Decision == OperationRuleDecision.Undecided)
            {
                return OperationResult.Of(first.Decision, first.Refactorings.Concat(second.Refactorings));
            }

            return OperationResult.Allow(first.Refactorings.Concat(second.Refactorings));
        }


        public enum OperationRuleDecision
        {
            /// <summary>
            /// It is not decided if the operation is allowed or not.
            /// </summary>
            Undecided,

            /// <summary>
            /// The operation is allowed.
            /// </summary>
            Allow,

            /// <summary>
            /// The operation is vetoed.
            /// </summary>
            Veto
        }

        public readonly struct OperationResult
        {
            public OperationRuleDecision Decision { get; }

            public IEnumerable<Refactoring> Refactorings { get; }

            private OperationResult(OperationRuleDecision decision,
                [CanBeNull] IEnumerable<Refactoring> refactorings = null)
            {
                Decision = decision;
                Refactorings = refactorings ?? Enumerable.Empty<Refactoring>();
            }


            public static OperationResult Of(OperationRuleDecision decision,
                [CanBeNull] IEnumerable<Refactoring> refactorings = null)
            {
                return new OperationResult(decision, refactorings);
            }

            public static OperationResult Allow(IEnumerable<Refactoring> refactorings = null)
            {
                return new OperationResult(OperationRuleDecision.Allow, refactorings);
            }

            public static OperationResult Veto()
            {
                return new OperationResult(OperationRuleDecision.Veto);
            }

            public static OperationResult Undecided(IEnumerable<Refactoring> refactorings = null)
            {
                return new OperationResult(OperationRuleDecision.Undecided, refactorings);
            }
        }
        
    }
}