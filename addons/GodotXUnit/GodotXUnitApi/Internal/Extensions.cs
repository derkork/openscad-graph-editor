using System;
using Godot;

namespace GodotXUnitApi.Internal
{
    public static class Extensions
    {
        public static void ThrowIfNotOk(this Error check)
        {
            if (check == Error.Ok) return;
            throw new Exception($"godot error returned: {check}");
        }
    }
}