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
                // now replace everything that is not a letter, number or underscore with an underscore
                newValue = newValue.Replace(@"[^a-zA-Z_0-9]", "_");
            }
            
            EmitValueChange(newValue);
        }
    }
}