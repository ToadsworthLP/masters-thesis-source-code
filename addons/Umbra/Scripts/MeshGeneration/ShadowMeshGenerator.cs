using System;
using Godot;

namespace Umbra.MeshGeneration;

public class ShadowMeshGenerator : ITileMapMeshGenerator
{
    private TileMapLayer source;
    private VoxelShape shape;

    public ShadowMeshGenerator(TileMapLayer source, VoxelShape shape)
    {
        this.source = source;
        this.shape = shape;
    }

    public void Generate(ArrayMesh destination)
    {
        SurfaceTool surfaceTool = new SurfaceTool();
        surfaceTool.Begin(Mesh.PrimitiveType.Triangles);
        surfaceTool.SetSmoothGroup(UInt32.MaxValue);

        for (int x = 0; x < shape.Voxels.GetLength(0); x++)
        {
            for (int y = 0; y < shape.Voxels.GetLength(1); y++)
            {
                for (int z = 0; z < shape.Voxels.GetLength(2); z++)
                {
                    if(shape.Voxels[x, y, z].IsEmpty) continue;
                    
                    Vector3 basePosition = new Vector3(x, y, z);
                    UmbraTileModel model = shape.Voxels[x, y, z].FrontModel == null ? shape.Voxels[x, y, z].TopModel : shape.Voxels[x, y, z].FrontModel;

                    if (model == null || model.ShadowMesh == null)
                    {
                        // Top
                        surfaceTool.AddVertex(basePosition + new Vector3(0, 1, 0));
                        surfaceTool.AddVertex(basePosition + new Vector3(1, 1, 0));
                        surfaceTool.AddVertex(basePosition + new Vector3(0, 1, 1));
                
                        surfaceTool.AddVertex(basePosition + new Vector3(1, 1, 0));
                        surfaceTool.AddVertex(basePosition + new Vector3(1, 1, 1));
                        surfaceTool.AddVertex(basePosition + new Vector3(0, 1, 1));
                        
                        // Bottom
                        surfaceTool.AddVertex(basePosition + new Vector3(0, 0, 0));
                        surfaceTool.AddVertex(basePosition + new Vector3(0, 0, 1));
                        surfaceTool.AddVertex(basePosition + new Vector3(1, 0, 0));
                
                        surfaceTool.AddVertex(basePosition + new Vector3(1, 0, 0));
                        surfaceTool.AddVertex(basePosition + new Vector3(0, 0, 1));
                        surfaceTool.AddVertex(basePosition + new Vector3(1, 0, 1));
                        
                        // Front
                        surfaceTool.AddVertex(basePosition + new Vector3(0, 0, 1));
                        surfaceTool.AddVertex(basePosition + new Vector3(1, 1, 1));
                        surfaceTool.AddVertex(basePosition + new Vector3(1, 0, 1));
                
                        surfaceTool.AddVertex(basePosition + new Vector3(1, 1, 1));
                        surfaceTool.AddVertex(basePosition + new Vector3(0, 0, 1));
                        surfaceTool.AddVertex(basePosition + new Vector3(0, 1, 1));
                        
                        // Back
                        surfaceTool.AddVertex(basePosition + new Vector3(0, 0, 0));
                        surfaceTool.AddVertex(basePosition + new Vector3(1, 0, 0));
                        surfaceTool.AddVertex(basePosition + new Vector3(1, 1, 0));
                
                        surfaceTool.AddVertex(basePosition + new Vector3(1, 1, 0));
                        surfaceTool.AddVertex(basePosition + new Vector3(0, 1, 0));
                        surfaceTool.AddVertex(basePosition + new Vector3(0, 0, 0));
                        
                        // Left
                        surfaceTool.AddVertex(basePosition + new Vector3(0, 0, 0));
                        surfaceTool.AddVertex(basePosition + new Vector3(0, 1, 0));
                        surfaceTool.AddVertex(basePosition + new Vector3(0, 0, 1));
                
                        surfaceTool.AddVertex(basePosition + new Vector3(0, 1, 0));
                        surfaceTool.AddVertex(basePosition + new Vector3(0, 1, 1));
                        surfaceTool.AddVertex(basePosition + new Vector3(0, 0, 1));
                        
                        // Right
                        surfaceTool.AddVertex(basePosition + new Vector3(1, 0, 0));
                        surfaceTool.AddVertex(basePosition + new Vector3(1, 0, 1));
                        surfaceTool.AddVertex(basePosition + new Vector3(1, 1, 0));
                
                        surfaceTool.AddVertex(basePosition + new Vector3(1, 0, 1));
                        surfaceTool.AddVertex(basePosition + new Vector3(1, 1, 1));
                        surfaceTool.AddVertex(basePosition + new Vector3(1, 1, 0));
                    }
                    else
                    {
                        Vector3[] vertices = model.ShadowMesh.GetFaces();
                        foreach (Vector3 vertex in vertices)
                        {
                            surfaceTool.AddVertex(basePosition + vertex);
                        }
                    }
                }
            }
        }
        
        surfaceTool.GenerateNormals();
        surfaceTool.Commit(destination);
    }
}