#if TOOLS
using Godot;
using Godot.Collections;
using Umbra.MeshGeneration;

namespace Umbra.Editor;

public partial class UmbraTileSetInspectorPlugin : EditorInspectorPlugin
{
    private const string DefaultShaderPath = "res://addons/Umbra/Shaders/Sh_Umbra_TilemapMeshVisibleSurface.gdshader";
    
    public override bool _CanHandle(GodotObject obj)
    {
        return obj.GetScript().Obj != null && obj.GetScript().As<Script>().GetGlobalName() == nameof(UmbraTileSet);
    }

    public override void _ParseBegin(GodotObject obj)
    {
        Button setTexturesButton = new Button();
        setTexturesButton.Text = "Setup Default Umbra Material";
        setTexturesButton.Pressed += () =>
        {
            UmbraTileSet target = (UmbraTileSet)EditorInterface.Singleton.GetInspector().GetEditedObject();
            SetDefaultUmbraMaterial(target);
        };
        AddCustomControl(setTexturesButton);
    }

    private static void SetDefaultUmbraMaterial(UmbraTileSet target)
    {
        ShaderMaterial material;
        if (target.UmbraMaterial is ShaderMaterial shaderMaterial)
        {
            material = shaderMaterial;
        }
        else
        {
            material = new ShaderMaterial();
        }
        
        material.SetShader(GD.Load<Shader>(DefaultShaderPath));

        int sourceCount = target.GetSourceCount();
        Texture2D[] mainTextures = new Texture2D[sourceCount];
        for (int i = 0; i < sourceCount; i++)
        {
            TileSetSource source = target.GetSource(i);
            if (source is TileSetAtlasSource atlasSource)
            {
                mainTextures[i] = atlasSource.Texture;
            }
        }
        
        Array<Texture2D> mainTextureArray = new Array<Texture2D>(mainTextures);
        Array<Texture2D> normalMapTextureArray = new Array<Texture2D>(new Texture2D[sourceCount]);

        material.SetShaderParameter("mainTexture", mainTextureArray);
        material.SetShaderParameter("normalMapTexture", normalMapTextureArray);

        target.UmbraMaterial = material;
    }
}
#endif