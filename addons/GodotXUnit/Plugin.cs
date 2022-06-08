using System;
using Godot;
using Godot.Collections;
using GodotXUnitApi.Internal;

namespace GodotXUnit
{
    [Tool]
    public class Plugin : EditorPlugin
    {
        private static Plugin _instance;

        public static Plugin Instance => _instance ?? throw new Exception("Plugin not set");

        private XUnitDock dock;

        public override string GetPluginName()
        {
            return nameof(GodotXUnit);
        }

        public override void _EnterTree()
        {
            _instance = this;
            EnsureProjectSetting(Consts.SETTING_RESULT_SUMMARY_PROP);
            EnsureProjectSetting(Consts.SETTING_TARGET_ASSEMBLY_PROP);
            EnsureProjectSetting(Consts.SETTING_TARGET_ASSEMBLY_CUSTOM_PROP);
            EnsureProjectSetting(Consts.SETTING_TARGET_CLASS_PROP);
            EnsureProjectSetting(Consts.SETTING_TARGET_METHOD_PROP);
            dock = (XUnitDock) GD.Load<PackedScene>(Consts.DOCK_SCENE_PATH).Instance();
            AddControlToBottomPanel(dock, GetPluginName());
        }

        public override void _ExitTree()
        {
            _instance = null;
            if (dock != null)
            {
                RemoveControlFromBottomPanel(dock);
                dock.Free();
                dock = null;
            }
        }

        private void EnsureProjectSetting(Dictionary prop)
        {
            var name = prop["name"]?.ToString() ?? throw new Exception("no name in prop");
            if (!ProjectSettings.HasSetting(name))
            {
                ProjectSettings.SetSetting(name, prop["default"]);
                ProjectSettings.SetInitialValue(name, prop["default"]);
                ProjectSettings.AddPropertyInfo(prop);
                ProjectSettings.Save();
            }
        }
    }
}