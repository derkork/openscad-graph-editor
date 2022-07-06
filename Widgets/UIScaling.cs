using Godot;
using Serilog;

namespace OpenScadGraphEditor.Widgets
{
    public static class UiScaling
    {
        /// <summary>
        /// Scales all items in the given theme to the given scale factor.
        /// </summary>
        public static void Scale(this Theme theme, int percent)
        {
            var currentScale = 100; // we check if the theme is already scaled, so we don't scale it twice if this is called multiple times.
            if (theme.HasConstant("ui_scale_percent", "UIScaling"))
            {
                currentScale = theme.GetConstant("ui_scale_percent", "UIScaling");
            }
            
            if (currentScale == percent)
            {
                // nothing to do.
                return; 
            }
            
            theme.SetConstant("ui_scale_percent", "UIScaling", percent);
            var scale =  percent / 100f;

            theme.ScaleConstant("arrow_margin", "OptionButton", scale);
            theme.ScaleConstant("hseparation", "OptionButton", scale);

            // 	theme->set_constant("hseparation", "CheckButton", 4 * EDSCALE);
            // theme->set_constant("check_vadjust", "CheckButton", 0 * EDSCALE);
            theme.ScaleConstant("hseparation", "CheckButton", scale);
            theme.ScaleConstant("check_vadjust", "CheckButton", scale);

            // 	theme->set_constant("hseparation", "CheckBox", 4 * EDSCALE);
            // theme->set_constant("check_vadjust", "CheckBox", 0 * EDSCALE);
            theme.ScaleConstant("hseparation", "CheckBox", scale);
            theme.ScaleConstant("check_vadjust", "CheckBox", scale);

            // 	theme->set_constant("vseparation", "PopupMenu", (extra_spacing + default_margin_size + 1) * EDSCALE);
            theme.ScaleConstant("vseparation", "PopupMenu", scale);

            // 	theme->set_constant("vseparation", "Tree", (extra_spacing + default_margin_size) * EDSCALE);
            // theme->set_constant("hseparation", "Tree", (extra_spacing + default_margin_size) * EDSCALE);
            // theme->set_constant("item_margin", "Tree", 3 * default_margin_size * EDSCALE);
            // theme->set_constant("button_margin", "Tree", default_margin_size * EDSCALE);
            // theme->set_constant("scroll_border", "Tree", 40 * EDSCALE);
            theme.ScaleConstant("vseparation", "Tree", scale);
            theme.ScaleConstant("hseparation", "Tree", scale);
            theme.ScaleConstant("item_margin", "Tree", scale);
            theme.ScaleConstant("button_margin", "Tree", scale);
            theme.ScaleConstant("scroll_border", "Tree", scale);

            // theme->set_constant("vseparation", "ItemList", 3 * EDSCALE);
            // theme->set_constant("hseparation", "ItemList", 3 * EDSCALE);
            // theme->set_constant("icon_margin", "ItemList", default_margin_size * EDSCALE);
            // theme->set_constant("line_separation", "ItemList", 3 * EDSCALE);
            theme.ScaleConstant("vseparation", "ItemList", scale);
            theme.ScaleConstant("hseparation", "ItemList", scale);
            theme.ScaleConstant("icon_margin", "ItemList", scale);
            theme.ScaleConstant("line_separation", "ItemList", scale);

            // 	theme->set_constant("hseparation", "Tabs", 4 * EDSCALE);
            theme.ScaleConstant("hseparation", "Tabs", scale);

            // 	theme->set_constant("separation", "HSplitContainer", default_margin_size * 2 * EDSCALE);
            //  theme->set_constant("separation", "VSplitContainer", default_margin_size * 2 * EDSCALE);
            theme.ScaleConstant("separation", "HSplitContainer", scale);
            theme.ScaleConstant("separation", "VSplitContainer", scale);


            // theme->set_constant("separation", "BoxContainer", default_margin_size * EDSCALE);
            // theme->set_constant("separation", "HBoxContainer", default_margin_size * EDSCALE);
            // theme->set_constant("separation", "VBoxContainer", default_margin_size * EDSCALE);
            // theme->set_constant("hseparation", "GridContainer", default_margin_size * EDSCALE);
            // theme->set_constant("vseparation", "GridContainer", default_margin_size * EDSCALE);
            theme.ScaleConstant("separation", "BoxContainer", scale);
            theme.ScaleConstant("separation", "HBoxContainer", scale);
            theme.ScaleConstant("separation", "VBoxContainer", scale);
            theme.ScaleConstant("hseparation", "GridContainer", scale);
            theme.ScaleConstant("vseparation", "GridContainer", scale);

            // theme->set_constant("close_h_ofs", "WindowDialog", 22 * EDSCALE);
            // theme->set_constant("close_v_ofs", "WindowDialog", 20 * EDSCALE);
            // theme->set_constant("title_height", "WindowDialog", 24 * EDSCALE);
            theme.ScaleConstant("close_h_ofs", "WindowDialog", scale);
            theme.ScaleConstant("close_v_ofs", "WindowDialog", scale);
            theme.ScaleConstant("title_height", "WindowDialog", scale);

            // theme->set_constant("shadow_offset_x", "RichTextLabel", 1 * EDSCALE);
            // theme->set_constant("shadow_offset_y", "RichTextLabel", 1 * EDSCALE);
            // theme->set_constant("shadow_as_outline", "RichTextLabel", 0 * EDSCALE);            
            theme.ScaleConstant("shadow_offset_x", "RichTextLabel", scale);
            theme.ScaleConstant("shadow_offset_y", "RichTextLabel", scale);
            theme.ScaleConstant("shadow_as_outline", "RichTextLabel", scale);

            // theme->set_constant("shadow_offset_x", "Label", 1 * EDSCALE);
            // theme->set_constant("shadow_offset_y", "Label", 1 * EDSCALE);
            // theme->set_constant("shadow_as_outline", "Label", 0 * EDSCALE);
            // theme->set_constant("line_spacing", "Label", 3 * EDSCALE);
            theme.ScaleConstant("shadow_offset_x", "Label", scale);
            theme.ScaleConstant("shadow_offset_y", "Label", scale);
            theme.ScaleConstant("shadow_as_outline", "Label", scale);
            theme.ScaleConstant("line_spacing", "Label", scale);


            // theme->set_constant("bezier_len_pos", "GraphEdit", 80 * EDSCALE);
            // theme->set_constant("bezier_len_neg", "GraphEdit", 160 * EDSCALE);            
            theme.ScaleConstant("bezier_len_pos", "GraphEdit", scale);
            theme.ScaleConstant("bezier_len_neg", "GraphEdit", scale);

            // theme->set_constant("port_offset", "GraphNode", 14 * EDSCALE);
            // theme->set_constant("title_h_offset", "GraphNode", -16 * EDSCALE);
            // theme->set_constant("title_offset", "GraphNode", 20 * EDSCALE);
            // theme->set_constant("close_h_offset", "GraphNode", 20 * EDSCALE);
            // theme->set_constant("close_offset", "GraphNode", 20 * EDSCALE);
            // theme->set_constant("separation", "GraphNode", 1 * EDSCALE);

            theme.ScaleConstant("port_offset", "GraphNode", scale);
            theme.ScaleConstant("title_h_offset", "GraphNode", scale);
            theme.ScaleConstant("title_offset", "GraphNode", scale);
            theme.ScaleConstant("close_h_offset", "GraphNode", scale);
            theme.ScaleConstant("close_offset", "GraphNode", scale);
            theme.ScaleConstant("separation", "GraphNode", scale);

            // theme->set_constant("vseparation", "GridContainer", (extra_spacing + default_margin_size) * EDSCALE);
            theme.ScaleConstant("vseparation", "GridContainer", scale);

            // theme->set_constant("sv_width", "ColorPicker", 256 * EDSCALE);
            // theme->set_constant("sv_height", "ColorPicker", 256 * EDSCALE);
            // theme->set_constant("h_width", "ColorPicker", 30 * EDSCALE);
            // theme->set_constant("label_width", "ColorPicker", 10 * EDSCALE);
            theme.ScaleConstant("sv_width", "ColorPicker", scale);
            theme.ScaleConstant("sv_height", "ColorPicker", scale);
            theme.ScaleConstant("h_width", "ColorPicker", scale);
            theme.ScaleConstant("label_width", "ColorPicker", scale);


            foreach (var nodeType in theme.GetFontTypes())
            {
                foreach (var fontName in theme.GetFontList(nodeType))
                {
                    var font = theme.GetFont( fontName, nodeType);
                    if (font is DynamicFont dynamicFont)
                    {
                        dynamicFont.Size = dynamicFont.Size.ScaleInt(scale);
                        Log.Information("Font: {FontName} {NodeType} => {Size}",fontName, nodeType ,dynamicFont.Size);
                    }
                }
            }

            var defaultFont = theme.DefaultFont;
            if (defaultFont is DynamicFont dynamicDefaultFont)
            {
                dynamicDefaultFont.Size = dynamicDefaultFont.Size.ScaleInt(scale);
            }

            foreach (var nodeType in theme.GetStyleboxTypes())
            {
                foreach (var styleBoxName in theme.GetStyleboxList(nodeType))
                {
                    var styleBox = theme.GetStylebox(styleBoxName, nodeType);

                    switch (styleBox)
                    {
                        case StyleBoxTexture styleBoxTexture:
                            styleBoxTexture.ScaleStyleBoxTexture(scale);
                            break;
                        case StyleBoxFlat styleBoxFlat:
                            styleBoxFlat.ScaleStyleBoxFlat(scale);
                            break;
                        case StyleBoxLine styleBoxLine:
                            styleBoxLine.ScaleStyleBoxLine(scale);
                            break;
                        default:
                            styleBox.ScaleStyleBox(scale);
                            break;
                    }
                }
            }
        }

        private static void ScaleConstant(this Theme theme, string name, string nodeType, float scale)
        {
            if (!theme.HasConstant(name, nodeType))
            {
                return; // do not set constant if it does not exist
            }
            
            var before = theme.GetConstant(name, nodeType);
            theme.SetConstant(name, nodeType, theme.GetConstant(name, nodeType).ScaleInt(scale));
            Log.Information("Constant: {Name} {NodeType} => {OldValue} -> {NewValue}", name, nodeType, before, theme.GetConstant(name, nodeType));
        }

        private static int ScaleInt(this int someInt, float scale) => Mathf.CeilToInt(someInt * scale);


        private static void ScaleStyleBox(this StyleBox styleBox, float scale)
        {
            styleBox.ContentMarginLeft *= scale;
            styleBox.ContentMarginRight *= scale;
            styleBox.ContentMarginBottom *= scale;
            styleBox.ContentMarginTop *= scale;
        }


        private static void ScaleStyleBoxTexture(this StyleBoxTexture styleBox, float scale)
        {
            styleBox.MarginLeft *= scale;
            styleBox.MarginRight *= scale;
            styleBox.MarginBottom *= scale;
            styleBox.MarginTop *= scale;

            styleBox.ScaleStyleBox(scale);
        }

        private static void ScaleStyleBoxFlat(this StyleBoxFlat styleBox, float scale)
        {
            styleBox.CornerDetail = Mathf.CeilToInt(styleBox.CornerDetail * scale);
            styleBox.CornerRadiusTopLeft = styleBox.CornerRadiusTopLeft.ScaleInt(scale);
            styleBox.CornerRadiusTopRight = styleBox.CornerRadiusTopRight.ScaleInt(scale);
            styleBox.CornerRadiusBottomLeft = styleBox.CornerRadiusBottomLeft.ScaleInt(scale);
            styleBox.CornerRadiusBottomRight = styleBox.CornerRadiusBottomRight.ScaleInt(scale);

            // Ensure borders are visible when using an editor scale below 100%.
            var compensatedScale = Mathf.Max(1, scale);
            styleBox.BorderWidthLeft = styleBox.BorderWidthLeft.ScaleInt(compensatedScale);
            styleBox.BorderWidthRight = styleBox.BorderWidthRight.ScaleInt(compensatedScale);
            styleBox.BorderWidthBottom = styleBox.BorderWidthBottom.ScaleInt(compensatedScale);
            styleBox.BorderWidthTop = styleBox.BorderWidthTop.ScaleInt(compensatedScale);

            styleBox.ShadowSize = styleBox.ShadowSize.ScaleInt(scale);

            styleBox.ScaleStyleBox(scale);
        }

        private static void ScaleStyleBoxLine(this StyleBoxLine styleBox, float scale)
        {
            var compensatedScale = Mathf.Max(1, scale);
            styleBox.GrowBegin *= compensatedScale;
            styleBox.GrowEnd *= compensatedScale;
            styleBox.Thickness = styleBox.Thickness.ScaleInt(compensatedScale);
            styleBox.ScaleStyleBox(scale);
        }
    }
}