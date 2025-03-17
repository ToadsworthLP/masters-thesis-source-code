#if TOOLS
using Godot;

namespace Umbra.Editor;

[Tool]
public partial class UmbraPlugin : EditorPlugin
{
    private UmbraTileMapLayer3DInspectorPlugin umbraTileMapLayer3DInspectorPlugin;
    private UmbraTileSetInspectorPlugin umbraTileSetInspectorPlugin;
    private UmbraCompanionInspectorPlugin companionInspectorPlugin;
    
    public override void _EnterTree()
    {
        umbraTileMapLayer3DInspectorPlugin = new Editor.UmbraTileMapLayer3DInspectorPlugin();
        AddInspectorPlugin(umbraTileMapLayer3DInspectorPlugin);

        umbraTileSetInspectorPlugin = new Editor.UmbraTileSetInspectorPlugin();
        AddInspectorPlugin(umbraTileSetInspectorPlugin);

        companionInspectorPlugin = new UmbraCompanionInspectorPlugin();
        AddInspectorPlugin(companionInspectorPlugin);
    }

    public override void _ExitTree()
    {
        RemoveInspectorPlugin(umbraTileMapLayer3DInspectorPlugin);
        RemoveInspectorPlugin(umbraTileSetInspectorPlugin);
        RemoveInspectorPlugin(companionInspectorPlugin);
    }
}
#endif