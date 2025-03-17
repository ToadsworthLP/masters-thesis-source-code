using Godot;

namespace Umbra.MeshGeneration;

[GlobalClass]
[Tool]
public partial class UmbraTileSet : TileSet
{
    [Export] public Material UmbraMaterial;

    public UmbraTileSet()
    {
        #if TOOLS
        SetupCustomDataLayers();
        #endif
    }

    private void SetupCustomDataLayers()
    {
        SetupCustomDataLayer(UmbraTileSetDataLayerNames.TileNormal, Variant.Type.Vector3I);
        SetupCustomDataLayer(UmbraTileSetDataLayerNames.TileBarrierTop, Variant.Type.Bool);
        SetupCustomDataLayer(UmbraTileSetDataLayerNames.TileBarrierBottom, Variant.Type.Bool);
        SetupCustomDataLayer(UmbraTileSetDataLayerNames.TileBarrierLeft, Variant.Type.Bool);
        SetupCustomDataLayer(UmbraTileSetDataLayerNames.TileBarrierRight, Variant.Type.Bool);
        SetupCustomDataLayer(UmbraTileSetDataLayerNames.TileModel, Variant.Type.Object);
        SetupCustomDataLayer(UmbraTileSetDataLayerNames.TileModelFront, Variant.Type.Object);
        SetupCustomDataLayer(UmbraTileSetDataLayerNames.TileModelTop, Variant.Type.Object);
        SetupCustomDataLayer(UmbraTileSetDataLayerNames.TileSkipFillBelow, Variant.Type.Bool);
    }

    private void SetupCustomDataLayer(string name, Variant.Type type)
    {
        if (GetCustomDataLayerByName(name) < 0)
        {
            int nextIndex = GetCustomDataLayersCount();
            AddCustomDataLayer(nextIndex);
            SetCustomDataLayerName(nextIndex, name);
            SetCustomDataLayerType(nextIndex, type);
        }
    }
}