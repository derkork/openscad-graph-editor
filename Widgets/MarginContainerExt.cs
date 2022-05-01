using Godot;

namespace OpenScadGraphEditor.Widgets
{
    // TODO: pump this into GodotExt
    public static class MarginContainerExt
    {
        public static void SetInnerMargins(this MarginContainer container, int left = 0, int top = 0, int right = 0, int bottom = 0)
        {
            container.AddConstantOverride("margin_top", top);
            container.AddConstantOverride("margin_bottom", bottom);
            container.AddConstantOverride("margin_left", left);
            container.AddConstantOverride("margin_right", right);
        }
        
        public static void SetAllInnerMargins(this MarginContainer container, int size = 0)
        {
            container.SetInnerMargins(size, size, size, size);
        }
    }
}