using System;
using Godot;
using JetBrains.Annotations;
using Object = Godot.Object;

namespace OpenScadGraphEditor.Utils
{
    public static class Prefabs 
    {
        public static T InstantiateFromScene<T>() where T : Node
        {
            // ReSharper disable once StringLiteralTypo
            var path = $"res://{typeof(T).Namespace?.Replace("OpenScadGraphEditor.", "").Replace(".", "/")}/{typeof(T).Name}.tscn";
            return GD.Load<PackedScene>(path).Instance<T>();
        }

        /// Returns a new instance of the script.
        /// workaround for https://github.com/godotengine/godot/issues/38191
        public static T New<[MeansImplicitUse] T>() where T : Object
        {
            return (T) New(typeof(T));
        }

        private static string ScriptPath(Type type)
        {
            return $"res://{type.Namespace?.Replace("OpenScadGraphEditor.", "").Replace(".", "/")}/{type.Name}.cs";
        }
        
        /// Returns a new instance of the script.
        /// workaround for https://github.com/godotengine/godot/issues/38191
        public static Object New(this Type type)
        {
            var path = ScriptPath(type);
            return (Object) GD.Load<CSharpScript>(path)?.New();

        }
    }
}