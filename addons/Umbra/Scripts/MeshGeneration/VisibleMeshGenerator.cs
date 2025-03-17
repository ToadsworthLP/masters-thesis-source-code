using Godot;
using Godot.Collections;

namespace Umbra.MeshGeneration;

public class VisibleMeshGenerator : ITileMapMeshGenerator
{
    private TileMapLayer source;
    private VoxelShape shape;
    private bool generateBackFaces;

    public VisibleMeshGenerator(TileMapLayer source, VoxelShape shape, bool generateBackFaces = false)
    {
        this.source = source;
        this.shape = shape;
        this.generateBackFaces = generateBackFaces;
    }

    public void Generate(ArrayMesh destination)
    {
        SurfaceTool surfaceTool = new SurfaceTool();
        surfaceTool.Begin(Mesh.PrimitiveType.Triangles);
        surfaceTool.SetCustomFormat(0, SurfaceTool.CustomFormat.RFloat);

        for (int x = 0; x < shape.Voxels.GetLength(0); x++)
        {
            for (int y = 0; y < shape.Voxels.GetLength(1); y++)
            {
                for (int z = 0; z < shape.Voxels.GetLength(2); z++)
                {
                    ref Voxel voxel = ref shape.Voxels[x, y, z];
                    Vector3 basePosition = new Vector3(x, y, z);
                    
                    if(voxel.IsEmpty) continue;

                    if ((voxel.FrontModel != null && voxel.FrontModel.VisibleMesh != null) || (voxel.TopModel != null && voxel.TopModel.VisibleMesh != null))
                    {
                        AddCustomModel(voxel, surfaceTool, basePosition);
                    }
                    else
                    {
                        AddDefaultModel(voxel, surfaceTool, basePosition);
                    }
                }
            }
        }
        
        surfaceTool.GenerateTangents();
        surfaceTool.Commit(destination);
    }

    private void AddCustomModel(Voxel voxel, SurfaceTool surfaceTool, Vector3 basePosition)
    {
        if (!voxel.FrontCell.HasValue && !voxel.TopCell.HasValue) return;

        if (voxel.FrontModel != null)
        {
            CellData? cellTexture = GetCustomModelCellTextureUv(voxel.FrontCell.Value.Coordinates);
            if(!cellTexture.HasValue) return;
            
            Color modulateColor = cellTexture.Value.Modulate;
            Color customDataColor = new Color(cellTexture.Value.AtlasIndex, 0, 0, 0);
            
            UmbraTileModel model = voxel.FrontModel;
            Mesh mesh = model.VisibleMesh;
            Array surfaceArrays = mesh.SurfaceGetArrays(0);

            Vector3[] vertices = surfaceArrays[0].AsVector3Array();
            Vector3[] normals = surfaceArrays[1].AsVector3Array();
            float[] tangents = surfaceArrays[2].AsFloat32Array();
            Vector2[] uv = surfaceArrays[4].AsVector2Array();
            int[] indices = surfaceArrays[12].AsInt32Array();

            foreach (int index in indices)
            {
                Vector2 cellUv = new Vector2(
                    Mathf.Lerp(cellTexture.Value.TextureTopLeftUv.X, cellTexture.Value.TextureBottomRightUv.X, uv[index].X),
                    Mathf.Lerp(cellTexture.Value.TextureTopLeftUv.Y, cellTexture.Value.TextureBottomRightUv.Y, uv[index].Y)
                );
                
                surfaceTool.SetUV(cellUv);
                surfaceTool.SetNormal(normals[index]);
                surfaceTool.SetColor(modulateColor);
                surfaceTool.SetCustom(0, customDataColor);
                surfaceTool.AddVertex(basePosition + vertices[index]);
            }
        }

        if (voxel.TopModel != null)
        {
            CellData? cellTexture = GetCustomModelCellTextureUv(voxel.TopCell.Value.Coordinates);
            if(!cellTexture.HasValue) return;
            
            Color modulateColor = cellTexture.Value.Modulate;
            Color customDataColor = new Color(cellTexture.Value.AtlasIndex, 0, 0, 0);
            
            basePosition += new Vector3(0, 1, 0);
            
            UmbraTileModel model = voxel.TopModel;
            Mesh mesh = model.VisibleMesh;
            Array surfaceArrays = mesh.SurfaceGetArrays(0);

            Vector3[] vertices = surfaceArrays[0].AsVector3Array();
            Vector3[] normals = surfaceArrays[1].AsVector3Array();
            float[] tangents = surfaceArrays[2].AsFloat32Array();
            Vector2[] uv = surfaceArrays[4].AsVector2Array();
            int[] indices = surfaceArrays[12].AsInt32Array();

            foreach (int index in indices)
            {
                Vector2 cellUv = new Vector2(
                    Mathf.Lerp(cellTexture.Value.TextureTopLeftUv.X, cellTexture.Value.TextureBottomRightUv.X, uv[index].X),
                    Mathf.Lerp(cellTexture.Value.TextureTopLeftUv.Y, cellTexture.Value.TextureBottomRightUv.Y, uv[index].Y)
                );
                
                surfaceTool.SetUV(cellUv);
                surfaceTool.SetNormal(normals[index]);
                surfaceTool.SetColor(modulateColor);
                surfaceTool.SetCustom(0, customDataColor);
                surfaceTool.AddVertex(basePosition + vertices[index]);
            }
        }
    }

    private void AddDefaultModel(Voxel voxel, SurfaceTool surfaceTool, Vector3 basePosition)
    {
        if (voxel.TopCell.HasValue)
        {
            CellData? cellTexture = GetDefaultModelCellTextureUv(voxel.TopCell.Value.Coordinates);

            if(!cellTexture.HasValue) return;
                        
            Color modulateColor = cellTexture.Value.Modulate;
            Color customDataColor = new Color(cellTexture.Value.AtlasIndex, 0, 0, 0);
                        
            surfaceTool.SetUV(cellTexture.Value.TextureTopLeftUv);
            surfaceTool.SetNormal(new Vector3(0, 1, 0));
            surfaceTool.SetColor(modulateColor);
            surfaceTool.SetCustom(0, customDataColor);
            surfaceTool.AddVertex(basePosition + new Vector3(0, 1, 0));
            
            surfaceTool.SetUV(cellTexture.Value.TextureTopRightUv);
            surfaceTool.SetNormal(new Vector3(0, 1, 0));
            surfaceTool.SetColor(modulateColor);
            surfaceTool.SetCustom(0, customDataColor);
            surfaceTool.AddVertex(basePosition + new Vector3(1, 1, 0));
            
            surfaceTool.SetUV(cellTexture.Value.TextureBottomLeftUv);
            surfaceTool.SetNormal(new Vector3(0, 1, 0));
            surfaceTool.SetColor(modulateColor);
            surfaceTool.SetCustom(0, customDataColor);
            surfaceTool.AddVertex(basePosition + new Vector3(0, 1, 1));

            
            surfaceTool.SetUV(cellTexture.Value.TextureTopRightUv);
            surfaceTool.SetNormal(new Vector3(0, 1, 0));
            surfaceTool.SetColor(modulateColor);
            surfaceTool.SetCustom(0, customDataColor);
            surfaceTool.AddVertex(basePosition + new Vector3(1, 1, 0));
            
            surfaceTool.SetUV(cellTexture.Value.TextureBottomRightUv);
            surfaceTool.SetNormal(new Vector3(0, 1, 0));
            surfaceTool.SetColor(modulateColor);
            surfaceTool.SetCustom(0, customDataColor);
            surfaceTool.AddVertex(basePosition + new Vector3(1, 1, 1));
            
            surfaceTool.SetUV(cellTexture.Value.TextureBottomLeftUv);
            surfaceTool.SetNormal(new Vector3(0, 1, 0));
            surfaceTool.SetColor(modulateColor);
            surfaceTool.SetCustom(0, customDataColor);
            surfaceTool.AddVertex(basePosition + new Vector3(0, 1, 1));

            
            if (generateBackFaces)
            {
                surfaceTool.SetUV(cellTexture.Value.TextureBottomLeftUv);
                surfaceTool.SetNormal(new Vector3(0, 1, 0));
                surfaceTool.SetColor(modulateColor);
                surfaceTool.SetCustom(0, customDataColor);
                surfaceTool.AddVertex(basePosition + new Vector3(0, 1, 1));
                
                surfaceTool.SetUV(cellTexture.Value.TextureTopRightUv);
                surfaceTool.SetNormal(new Vector3(0, 1, 0));
                surfaceTool.SetColor(modulateColor);
                surfaceTool.SetCustom(0, customDataColor);
                surfaceTool.AddVertex(basePosition + new Vector3(1, 1, 0));
                
                surfaceTool.SetUV(cellTexture.Value.TextureTopLeftUv);
                surfaceTool.SetNormal(new Vector3(0, 1, 0));
                surfaceTool.SetColor(modulateColor);
                surfaceTool.SetCustom(0, customDataColor);
                surfaceTool.AddVertex(basePosition + new Vector3(0, 1, 0));
                
                
                surfaceTool.SetUV(cellTexture.Value.TextureBottomLeftUv);
                surfaceTool.SetNormal(new Vector3(0, 1, 0));
                surfaceTool.SetColor(modulateColor);
                surfaceTool.SetCustom(0, customDataColor);
                surfaceTool.AddVertex(basePosition + new Vector3(0, 1, 1));
                
                surfaceTool.SetUV(cellTexture.Value.TextureBottomRightUv);
                surfaceTool.SetNormal(new Vector3(0, 1, 0));
                surfaceTool.SetColor(modulateColor);
                surfaceTool.SetCustom(0, customDataColor);
                surfaceTool.AddVertex(basePosition + new Vector3(1, 1, 1));
                
                surfaceTool.SetUV(cellTexture.Value.TextureTopRightUv);
                surfaceTool.SetNormal(new Vector3(0, 1, 0));
                surfaceTool.SetColor(modulateColor);
                surfaceTool.SetCustom(0, customDataColor);
                surfaceTool.AddVertex(basePosition + new Vector3(1, 1, 0));
            }
        }

        if (voxel.FrontCell.HasValue)
        {
            CellData? cellTexture = GetDefaultModelCellTextureUv(voxel.FrontCell.Value.Coordinates);

            if(!cellTexture.HasValue) return;
                        
            Color modulateColor = cellTexture.Value.Modulate;
            Color customDataColor = new Color(cellTexture.Value.AtlasIndex, 0, 0, 0);
                        
            surfaceTool.SetUV(cellTexture.Value.TextureBottomLeftUv);
            surfaceTool.SetNormal(new Vector3(0, 0, 1));
            surfaceTool.SetColor(modulateColor);
            surfaceTool.SetCustom(0, customDataColor);
            surfaceTool.AddVertex(basePosition + new Vector3(0, 0, 1));
                        
            surfaceTool.SetUV(cellTexture.Value.TextureTopLeftUv);
            surfaceTool.SetNormal(new Vector3(0, 0, 1));
            surfaceTool.SetColor(modulateColor);
            surfaceTool.SetCustom(0, customDataColor);
            surfaceTool.AddVertex(basePosition + new Vector3(0, 1, 1));
                        
            surfaceTool.SetUV(cellTexture.Value.TextureBottomRightUv);
            surfaceTool.SetNormal(new Vector3(0, 0, 1));
            surfaceTool.SetColor(modulateColor);
            surfaceTool.SetCustom(0, customDataColor);
            surfaceTool.AddVertex(basePosition + new Vector3(1, 0, 1));
                        
                        
            surfaceTool.SetUV(cellTexture.Value.TextureTopLeftUv);
            surfaceTool.SetNormal(new Vector3(0, 0, 1));
            surfaceTool.SetColor(modulateColor);
            surfaceTool.SetCustom(0, customDataColor);
            surfaceTool.AddVertex(basePosition + new Vector3(0, 1, 1));
                        
            surfaceTool.SetUV(cellTexture.Value.TextureTopRightUv);
            surfaceTool.SetNormal(new Vector3(0, 0, 1));
            surfaceTool.SetColor(modulateColor);
            surfaceTool.SetCustom(0, customDataColor);
            surfaceTool.AddVertex(basePosition + new Vector3(1, 1, 1));
                        
            surfaceTool.SetUV(cellTexture.Value.TextureBottomRightUv);
            surfaceTool.SetNormal(new Vector3(0, 0, 1));
            surfaceTool.SetColor(modulateColor);
            surfaceTool.SetCustom(0, customDataColor);
            surfaceTool.AddVertex(basePosition + new Vector3(1, 0, 1));
            

            if (generateBackFaces)
            {
                surfaceTool.SetUV(cellTexture.Value.TextureBottomRightUv);
                surfaceTool.SetNormal(new Vector3(0, 0, 1));
                surfaceTool.SetColor(modulateColor);
                surfaceTool.SetCustom(0, customDataColor);
                surfaceTool.AddVertex(basePosition + new Vector3(1, 0, 1));
                
                surfaceTool.SetUV(cellTexture.Value.TextureTopLeftUv);
                surfaceTool.SetNormal(new Vector3(0, 0, 1));
                surfaceTool.SetColor(modulateColor);
                surfaceTool.SetCustom(0, customDataColor);
                surfaceTool.AddVertex(basePosition + new Vector3(0, 1, 1));
                
                surfaceTool.SetUV(cellTexture.Value.TextureBottomLeftUv);
                surfaceTool.SetNormal(new Vector3(0, 0, 1));
                surfaceTool.SetColor(modulateColor);
                surfaceTool.SetCustom(0, customDataColor);
                surfaceTool.AddVertex(basePosition + new Vector3(0, 0, 1));
                
                
                surfaceTool.SetUV(cellTexture.Value.TextureBottomRightUv);
                surfaceTool.SetNormal(new Vector3(0, 0, 1));
                surfaceTool.SetColor(modulateColor);
                surfaceTool.SetCustom(0, customDataColor);
                surfaceTool.AddVertex(basePosition + new Vector3(1, 0, 1));
                
                surfaceTool.SetUV(cellTexture.Value.TextureTopRightUv);
                surfaceTool.SetNormal(new Vector3(0, 0, 1));
                surfaceTool.SetColor(modulateColor);
                surfaceTool.SetCustom(0, customDataColor);
                surfaceTool.AddVertex(basePosition + new Vector3(1, 1, 1));
                
                surfaceTool.SetUV(cellTexture.Value.TextureTopLeftUv);
                surfaceTool.SetNormal(new Vector3(0, 0, 1));
                surfaceTool.SetColor(modulateColor);
                surfaceTool.SetCustom(0, customDataColor);
                surfaceTool.AddVertex(basePosition + new Vector3(0, 1, 1));
            }
        }
    }

    private CellData? GetCustomModelCellTextureUv(Vector2I cell)
    {
        int sourceId = source.GetCellSourceId(cell);
        TileSetSource tileSetSource = source.TileSet.GetSource(sourceId);
            
        if(tileSetSource.GetType() != typeof(TileSetAtlasSource)) return null;

        TileSetAtlasSource tileSetAtlasSource = (TileSetAtlasSource)tileSetSource;
        TileData tileData = source.GetCellTileData(cell);

        Vector2I atlasCoords = source.GetCellAtlasCoords(cell);
        Rect2I textureRegion = tileSetAtlasSource.GetTileTextureRegion(atlasCoords, 0);
        Vector2 textureSize = tileSetAtlasSource.Texture.GetSize();

        return new CellData(textureRegion, textureSize, sourceId, tileData.Modulate, false, false, false);
    }
    
    private CellData? GetDefaultModelCellTextureUv(Vector2I cell)
    {
        int sourceId = source.GetCellSourceId(cell);
        TileSetSource tileSetSource = source.TileSet.GetSource(sourceId);
            
        if(tileSetSource.GetType() != typeof(TileSetAtlasSource)) return null;

        TileSetAtlasSource tileSetAtlasSource = (TileSetAtlasSource)tileSetSource;
        TileData tileData = source.GetCellTileData(cell);

        Vector2I atlasCoords = source.GetCellAtlasCoords(cell);
        Rect2I textureRegion = tileSetAtlasSource.GetTileTextureRegion(atlasCoords, 0);
        Vector2 textureSize = tileSetAtlasSource.Texture.GetSize();

        int alternativeId = source.GetCellAlternativeTile(cell);
        bool flipH = (alternativeId & TileSetAtlasSource.TransformFlipH) != 0;
        bool flipV = (alternativeId & TileSetAtlasSource.TransformFlipV) != 0;
        bool transpose = (alternativeId & TileSetAtlasSource.TransformTranspose) != 0;

        return new CellData(textureRegion, textureSize, sourceId, tileData.Modulate, flipH, flipV, transpose);
    }

    private record struct CellData(Rect2I TextureRegion, Vector2 TextureSize, int AtlasIndex, Color Modulate, bool FlipH, bool FlipV, bool Transposed)
    {
        public Rect2I TextureRegion { get; } = TextureRegion;
        public Vector2 TextureSize { get; } = TextureSize;
        public int AtlasIndex { get; } = AtlasIndex;
        public Color Modulate { get; } = Modulate;
        public bool FlipH { get; } = FlipH;
        public bool FlipV { get; } = FlipV;
        public bool Transposed { get; } = Transposed;
        
        public Vector2 TextureOriginUv => new Vector2(TextureRegion.Position.X / TextureSize.X, TextureRegion.Position.Y / TextureSize.Y);
        public Vector2 TextureSizeUv => new Vector2(TextureRegion.Size.X / TextureSize.X, TextureRegion.Size.Y / TextureSize.Y);
        public Vector2 TextureSizeUvTransposed => new Vector2(TextureRegion.Size.Y / TextureSize.X, TextureRegion.Size.X / TextureSize.Y);

        public Vector2 TextureTopLeftUv => FlipH ? (FlipV ? _TextureBottomRightUv : _TextureTopRightUv) : (FlipV ? _TextureBottomLeftUv : _TextureTopLeftUv);
        public Vector2 TextureTopRightUv => FlipH ? (FlipV ? _TextureBottomLeftUv : _TextureTopLeftUv) : (FlipV ? _TextureBottomRightUv : _TextureTopRightUv);
        public Vector2 TextureBottomLeftUv => FlipH ? (FlipV ? _TextureTopRightUv : _TextureBottomRightUv) : (FlipV ? _TextureTopLeftUv : _TextureBottomLeftUv);
        public Vector2 TextureBottomRightUv => FlipH ? (FlipV ? _TextureTopLeftUv : _TextureBottomLeftUv) : (FlipV ? _TextureTopRightUv : _TextureBottomRightUv);
        
        private Vector2 _TextureTopLeftUv => TextureOriginUv;
        private Vector2 _TextureTopRightUv => TextureOriginUv + (Transposed ? new Vector2(0, TextureSizeUvTransposed.Y) : new Vector2(TextureSizeUv.X, 0));
        private Vector2 _TextureBottomLeftUv => TextureOriginUv + (Transposed ? new Vector2(TextureSizeUvTransposed.X, 0) : new Vector2(0, TextureSizeUv.Y));
        private Vector2 _TextureBottomRightUv => TextureOriginUv + (Transposed ? new Vector2(TextureSizeUvTransposed.X, TextureSizeUvTransposed.Y) : new Vector2(TextureSizeUv.X, TextureSizeUv.Y));
    }
}