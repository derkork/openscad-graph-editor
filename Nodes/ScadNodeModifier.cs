using System;
using Godot;

namespace OpenScadGraphEditor.Nodes
{
    [Flags]
    public enum ScadNodeModifier
    {
        None = 0,
        Debug = 1,
        Root = 2,
        Background = 4,
        Disable = 8,
        Color = 16
    }


    public static class ScadNodeModifierExt
    {
        public static ScadNodeModifier GetModifiers(this ScadNode node)
        {
            if (node.TryGetCustomAttribute("applied_modifiers", out var appliedModifiers) )
            {
                return (ScadNodeModifier) int.Parse(appliedModifiers);
            }

            return ScadNodeModifier.None;
        }
        
        public static  void SetModifiers(this ScadNode node, ScadNodeModifier modifiers, Color color = default)
        {
            if (modifiers == ScadNodeModifier.None)
            {
                node.UnsetCustomAttribute("applied_modifiers");
                node.UnsetCustomAttribute("applied_color");
                return;
            }
            
            node.SetCustomAttribute("applied_modifiers", ((int)modifiers).ToString());
            if (modifiers.HasFlag(ScadNodeModifier.Color))
            {
                node.SetCustomAttribute("applied_color", color.ToHtml());
            }
            else
            {
                node.UnsetCustomAttribute("applied_color");
            }
        }
        
        public static bool TryGetColorModifier(this ScadNode node, out Color color)
        {
            if (node.GetModifiers().HasFlag(ScadNodeModifier.Color) && node.TryGetCustomAttribute("applied_color", out var appliedColor))
            {
                color = new Color(appliedColor);
                return true;
            }

            color = default;
            return false;
        }
        
    }
}