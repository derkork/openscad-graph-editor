using Godot;

namespace OpenScadGraphEditor.Utils
{
    public static class Resources
    {
        public static Texture FunctionIcon => GD.Load<Texture>("res://Icons/function0000.png");
        public static Texture VariableIcon => GD.Load<Texture>("res://Icons/variable0000.png");
        public static Texture ModuleIcon => GD.Load<Texture>("res://Icons/module0000.png");
        public static Texture ScadBuiltinIcon => GD.Load<Texture>("res://Icons/scad_builtin0000.png");
        public static Texture ImportIcon => GD.Load<Texture>("res://Icons/import0000.png");
        public static Texture TransitiveImportIcon => GD.Load<Texture>("res://Icons/transitive_import0000.png");
        
    }
}