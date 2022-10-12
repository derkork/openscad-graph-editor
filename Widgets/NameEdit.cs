using System.Collections.Generic;
using System.Text.RegularExpressions;
using Godot;
using JetBrains.Annotations;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Widgets
{
    [UsedImplicitly]
    public class NameEdit : LineEditBase<NameLiteral>
    {
        protected override string LiteralValue => Literal.Value;
        
        /// <summary>
        /// Regex for matching an english letter or underscore.
        /// </summary>
        private static readonly Regex LetterOrUnderscoreRegex = new Regex(@"^[a-zA-Z_]$");

        /// <summary>
        /// Regex for matching disallowed characters.
        /// </summary>
        private static readonly  Regex DisallowedCharacterRegex = new Regex(@"[^a-zA-Z_0-9]");
        
        /// <summary>
        /// Disallowed keywords.
        /// </summary>
        private static readonly HashSet<string> DisallowedKeywords = new HashSet<string> {"each", "echo", "if", "else", "for", "let", "function", "module", "true", "false", "undef", "assert"};
        
        protected override void OnFocusExited()
        {
            // we need to sanitize the name so that it can be used as a variable name
            var newValue = Control.Text;
            
            // we will allow an empty name, in this case the node will synthesize a name
            if (!newValue.Empty())
            {
                // check if the first character is not an english letter or an underscore
                if (!LetterOrUnderscoreRegex.IsMatch(newValue[0].ToString()))
                {
                    newValue = "_" + newValue; // if not, add an underscore to the beginning
                }
                // now replace all disallowed characters with an underscore
                newValue = DisallowedCharacterRegex.Replace(newValue, "_");
                
                // check if the name is a disallowed keyword
                if (DisallowedKeywords.Contains(newValue))
                {
                    newValue = "_" + newValue; // if so, add an underscore to the beginning
                }
            }
            
            EmitValueChange(new NameLiteral(newValue));
        }
    }
}