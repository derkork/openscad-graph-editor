using System.Linq;
using Godot;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Refactorings
{
    public class ToggleModifierRefactoring : NodeRefactoring
    {
        private readonly ScadNodeModifier _modifier;
        private readonly bool _enable;
        private readonly Color _newColor;

        public ToggleModifierRefactoring(IScadGraph holder, ScadNode node, ScadNodeModifier modifier, bool enable, Color newColor = default) : base(holder, node)
        {
            _modifier = modifier;
            _enable = enable;
            _newColor = newColor;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            // first make the node & graph refactorable
            
            // as a convenience, when we enable the root modifier, we automatically disable it on any other
            // node in the same graph that may currently have it.
            if (_modifier == ScadNodeModifier.Root && _enable)
            {
                Holder.GetAllNodes()
                    .Where(it => it.GetModifiers().HasFlag(ScadNodeModifier.Root))
                    .Select(it => new ToggleModifierRefactoring(Holder, it, ScadNodeModifier.Root, false))
                    .ForAll(context.PerformRefactoring);
            }

            var effectiveModifiers = _modifier;
            var hasCurrentColor = Node.TryGetColorModifier(out var effectiveColor);

            // we have the debugging modifiers and the color modifier. only one of the debug modifiers can be active at a time
            // the color modifier can be active at the same time as the debug modifiers
            
            // if we remove the modifier, this is easy as we don't need to have any constraints in mind, we can just
            // remove the modifier
            var currentModifiers = Node.GetModifiers();

            if (!_enable)
            {
                if (currentModifiers.HasFlag(_modifier))
                {
                    // remove the modifier
                    effectiveModifiers = currentModifiers & ~_modifier;
                }
            }
            else
            {
                // if we add the modifier we first check if it is the color modifier
                if (_modifier == ScadNodeModifier.Color)
                {
                    // add it to the effective modifiers and overwrite the color
                    effectiveColor = _newColor;
                    effectiveModifiers =  currentModifiers | ScadNodeModifier.Color;
                }
                else
                {
                    // in this case we check if the color modifier is active and combine it with the
                    // new modifier
                    if (hasCurrentColor)
                    {
                        effectiveModifiers |= ScadNodeModifier.Color;
                        // we keep the existing color
                    }
                }
            }
            

            // finally we set the modifiers
            Node.SetModifiers(effectiveModifiers, effectiveColor);
        }
    }
}