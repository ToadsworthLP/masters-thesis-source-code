using Godot;
using Godot.Collections;

namespace Umbra.MeshGeneration;

public class FlatVisibleMeshGenerator : ITileMapMeshGenerator
{
    private TileMapLayer source;
    private VoxelShape shape;
    
    public FlatVisibleMeshGenerator(TileMapLayer source, VoxelShape shape)
    {
        this.source = source;
        this.shape = shape;
    }
    
    public void Generate(ArrayMesh destination)
    {
        SurfaceTool surfaceTool = new SurfaceTool();
        surfaceTool.Begin(Mesh.PrimitiveType.Triangles);
        
        Array<Vector2I> usedCells = source.GetUsedCells();
        foreach (Vector2I cell in usedCells)
        {
            Vector3 basePosition = new Vector3(cell.X, 0, cell.Y);
            int sourceId = source.GetCellSourceId(cell);
            TileSetSource tileSetSource = source.TileSet.GetSource(sourceId);
            
            if(tileSetSource.GetType() != typeof(TileSetAtlasSource)) continue;

            TileSetAtlasSource tileSetAtlasSource = (TileSetAtlasSource)tileSetSource;
            Vector2I atlasCoords = source.GetCellAtlasCoords(cell);
            Rect2I textureRegion = tileSetAtlasSource.GetTileTextureRegion(atlasCoords, 0);
            Vector2 textureSize = tileSetAtlasSource.Texture.GetSize();

            Vector2 textureOriginUv = new Vector2(textureRegion.Position.X / textureSize.X, textureRegion.Position.Y / textureSize.Y);
            Vector2 textureSizeUv = new Vector2(textureRegion.Size.X / textureSize.X, textureRegion.Size.Y / textureSize.Y);
            
            // This assumes only one atlas source with a texture that is also set as the texture in the material used to
            // render the mesh, so this will need to be improved later.
            
            surfaceTool.SetUV(textureOriginUv);
            surfaceTool.AddVertex(basePosition + new Vector3(0, 0, 0));
            
            surfaceTool.SetUV(textureOriginUv + new Vector2(textureSizeUv.X, 0));
            surfaceTool.AddVertex(basePosition + new Vector3(1, 0, 0));
            
            surfaceTool.SetUV(textureOriginUv + new Vector2(0, textureSizeUv.Y));
            surfaceTool.AddVertex(basePosition + new Vector3(0, 0, 1));
            
            surfaceTool.SetUV(textureOriginUv + new Vector2(textureSizeUv.X, 0));
            surfaceTool.AddVertex(basePosition + new Vector3(1, 0, 0));
            
            surfaceTool.SetUV(textureOriginUv + textureSizeUv);
            surfaceTool.AddVertex(basePosition + new Vector3(1, 0, 1));
            
            surfaceTool.SetUV(textureOriginUv + new Vector2(0, textureSizeUv.Y));
            surfaceTool.AddVertex(basePosition + new Vector3(0, 0, 1));
        }
        
        surfaceTool.GenerateNormals();
        surfaceTool.GenerateTangents();
        surfaceTool.Commit(destination);
    }
}