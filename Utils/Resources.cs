using Godot;

namespace OpenScadGraphEditor.Utils
{
    public static class Resources
    {
        public static readonly Texture FunctionIcon = GD.Load<Texture>("res://Icons/function0000.png");
        public static readonly Texture VariableIcon = GD.Load<Texture>("res://Icons/variable0000.png");
        public static readonly Texture ModuleIcon = GD.Load<Texture>("res://Icons/module0000.png");
        public static readonly Texture ScadBuiltinIcon = GD.Load<Texture>("res://Icons/scad_builtin0000.png");
        public static readonly Texture ImportIcon = GD.Load<Texture>("res://Icons/import0000.png");
        public static readonly Texture TransitiveImportIcon = GD.Load<Texture>("res://Icons/transitive_import0000.png");
        public static readonly Texture EditIcon = GD.Load<Texture>("res://Icons/pencil0000.png");
        
        public static readonly Texture DebugIcon = GD.Load<Texture>("res://Icons/debug0000.png");
        public static readonly Texture BackgroundIcon = GD.Load<Texture>("res://Icons/background0000.png");
        public static readonly Texture RootIcon = GD.Load<Texture>("res://Icons/root0000.png");
        
        
        // icons for node background
        public static readonly Texture PlusIcon = GD.Load<Texture>("res://Icons/plus0000.png");
        public static readonly Texture MinusIcon = GD.Load<Texture>("res://Icons/minus0000.png");
        public static readonly Texture TimesIcon = GD.Load<Texture>("res://Icons/times0000.png");
        public static readonly Texture DivideIcon = GD.Load<Texture>("res://Icons/divide0000.png");
        public static readonly Texture ExpIcon = GD.Load<Texture>("res://Icons/exp0000.png");
        public static readonly Texture GreaterIcon = GD.Load<Texture>("res://Icons/greater0000.png");
        public static readonly Texture GreaterEqualIcon = GD.Load<Texture>("res://Icons/greater_equal0000.png");
        public static readonly Texture LessIcon = GD.Load<Texture>("res://Icons/less0000.png");
        public static readonly Texture LessEqualIcon = GD.Load<Texture>("res://Icons/less_equal0000.png");
        public static readonly Texture EqualIcon = GD.Load<Texture>("res://Icons/equal0000.png");
        public static readonly Texture ModulusIcon = GD.Load<Texture>("res://Icons/modulus0000.png");
        
        public static readonly Texture Vector3SplitIcon = GD.Load<Texture>("res://Icons/vector3split0000.png");
        public static readonly Texture Vector3MergeIcon = GD.Load<Texture>("res://Icons/vector3merge0000.png");
        public static readonly Texture Vector2SplitIcon = GD.Load<Texture>("res://Icons/vector2split0000.png");
        public static readonly Texture Vector2MergeIcon = GD.Load<Texture>("res://Icons/vector2merge0000.png");
        
        public static readonly Texture CastIcon = GD.Load<Texture>("res://Icons/cast0000.png");
        public static readonly Texture RangeIcon = GD.Load<Texture>("res://Icons/range0000.png");
        public static readonly Texture WirelessIcon = GD.Load<Texture>("res://Icons/wireless0000.png"); 
        
        public static readonly Texture AndIcon = GD.Load<Texture>("res://Icons/and0000.png");
        public static readonly Texture OrIcon = GD.Load<Texture>("res://Icons/or0000.png");
        public static readonly Texture NotIcon = GD.Load<Texture>("res://Icons/not0000.png");
        

        
        public static readonly Theme SimpleNodeWidgetTheme = GD.Load<Theme>("res://Widgets/SimpleNodeWidgetTheme.tres");
        public static readonly Theme StandardNodeWidgetTheme = GD.Load<Theme>("res://Widgets/StandardNodeWidgetTheme.tres");

    }
}