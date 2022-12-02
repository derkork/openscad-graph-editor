using System.Collections.Generic;
using System.Linq;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Refactorings
{
    /// <summary>
    /// Various helpers for matching references to externally declared code structures.
    /// </summary>
    public static class ExternalReferenceMatchingExt
    {
        /// <summary>
        /// Tries to find a match in the given list of modules that is closest to the given old module.
        /// </summary>
        public static bool TryFindMatch(this IEnumerable<ModuleDescription> modules, ModuleDescription old,
            out ModuleDescription match)
        {
            // for it to match, it must have the same name
            var potentialMatches = modules.Where(m => m.Name == old.Name).ToList();

            // we now give a score to each potential match using the following rules:
            var scores = potentialMatches.Select(m => new {Module = m, Score = ScoreMatch(old, m)}).ToList();
            match = scores.OrderByDescending(it => it.Score).FirstOrDefault()?.Module;
            return match != null;
        }
        
        /// <summary>
        /// Tries to find a match in the given list of functions that is closest to the given old function.
        /// </summary>
        public static bool TryFindMatch(this IEnumerable<FunctionDescription> functions, FunctionDescription old,
            out FunctionDescription match)
        {
            // for it to match, it must have the same name
            var potentialMatches = functions.Where(m => m.Name == old.Name).ToList();

            // we now give a score to each potential match using the following rules:
            var scores = potentialMatches.Select(m => new {Module = m, Score = ScoreMatch(old, m)}).ToList();
            match = scores.OrderByDescending(it => it.Score).FirstOrDefault()?.Module;
            return match != null;
        }
        
        public static bool TryFindMatch(this IEnumerable<VariableDescription> variableDescriptions, VariableDescription old,
            out VariableDescription match)
        {
            // for it to match, it must have the same name
            var potentialMatches = variableDescriptions.Where(m => m.Name == old.Name).ToList();

            // we now give a score to each potential match using the following rules:
            var scores = potentialMatches.Select(m => new {Module = m, Score = ScoreMatch(old, m)}).ToList();
            match = scores.OrderByDescending(it => it.Score).FirstOrDefault()?.Module;
            return match != null;
        }
        
        /// <summary>
        /// Scoring rules:
        /// - same number of parameters: +1
        /// - same support of children: +1
        /// - for each parameter: +1 if we have a parameter with the same name, another +2 if the type is the same or +1 if the type is compatible
        /// - we then take the match with the highest score
        /// </summary>
        private static int ScoreMatch(ModuleDescription old, ModuleDescription match)
        {
            var score = 0;
            if (old.Parameters.Count == match.Parameters.Count)
            {
                score++;
            }

            if (old.SupportsChildren == match.SupportsChildren)
            {
                score++;
            }

            foreach (var oldParameter in old.Parameters)
            {
                var matchParameter = match.Parameters.FirstOrDefault(p => p.Name == oldParameter.Name);
                if (matchParameter == null)
                {
                    continue;
                }

                score++;
                
                if (matchParameter.TypeHint == oldParameter.TypeHint)
                {
                    score += 2;
                }
                else if (oldParameter.TypeHint.CanBeAssignedTo(matchParameter.TypeHint))
                {
                    score++;
                }
            }

            return score;
        }
       
        /// <summary>
        /// Scoring rules:
        /// - same number of parameters: +1
        /// - for the return type: +2 if the type is the same or +1 if the type is compatible
        /// - for each parameter: +1 if we have a parameter with the same name, another +2 if the type is the same or +1 if the type is compatible
        /// - we then take the match with the highest score
        /// </summary>
        private static int ScoreMatch(FunctionDescription old, FunctionDescription match)
        {
            var score = 0;
            if (old.Parameters.Count == match.Parameters.Count)
            {
                score++;
            }
            
            if (old.ReturnTypeHint == match.ReturnTypeHint)
            {
                score += 2;
            }
            else if (old.ReturnTypeHint.CanBeAssignedTo(match.ReturnTypeHint))
            {
                score++;
            }

            foreach (var oldParameter in old.Parameters)
            {
                var matchParameter = match.Parameters.FirstOrDefault(p => p.Name == oldParameter.Name);
                if (matchParameter == null)
                {
                    continue;
                }

                score++;
                
                if (matchParameter.TypeHint == oldParameter.TypeHint)
                {
                    score += 2;
                }
                else if (oldParameter.TypeHint.CanBeAssignedTo(matchParameter.TypeHint))
                {
                    score++;
                }
            }

            return score;
        }
        
        /// <summary>
        /// Scoring rules:
        /// - for the type: +2 if the type is the same or +1 if the type is compatible
        /// </summary>
        public static int ScoreMatch(VariableDescription old, VariableDescription match)
        {
            var score = 0;
            if (old.TypeHint == match.TypeHint)
            {
                score += 2;
            }
            else if (old.TypeHint.CanBeAssignedTo(match.TypeHint))
            {
                score++;
            }

            return score;
        }
    }
}