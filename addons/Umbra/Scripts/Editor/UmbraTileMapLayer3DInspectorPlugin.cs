#if TOOLS
using Godot;
using Umbra.Nodes;

namespace Umbra.Editor;

public partial class UmbraTileMapLayer3DInspectorPlugin : EditorInspectorPlugin
{
    public override bool _CanHandle(GodotObject obj)
    {
        return obj.GetScript().Obj != null && obj.GetScript().As<Script>().GetGlobalName() == nameof(UmbraTileMapLayer3D);
    }

    public override void _ParseBegin(GodotObject obj)
    {
        Button updateButton;
        updateButton = new Button();
        updateButton.Text = "Update Mesh";
        updateButton.Pressed += ((UmbraTileMapLayer3D)obj).UpdateMesh;
        AddCustomControl(updateButton);
    }
}
#endif