using Godot;

namespace OpenScadGraphEditor.Utils
{
    public static class Resources
    {
        public static Texture FunctionIcon => GD.Load<Texture>("res://Icons/function.png");
        public static Texture VariableIcon => GD.Load<Texture>("res://Icons/variable.png");
        public static Texture ModuleIcon => GD.Load<Texture>("res://Icons/module.png");
        public static Texture ScadBuiltinIcon => GD.Load<Texture>("res://Icons/scad_builtin.png");
    }
}