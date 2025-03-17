using System;
using Godot;
using Array = Godot.Collections.Array;

namespace Umbra.MeshGeneration;

public static class MeshTransformer
{
    private static readonly Transform3D FlipXAroundVoxelCenter = Transform3D.Identity
        .Translated(new Vector3(-0.5f, -0.5f, -0.5f))
        .Scaled(new Vector3(-1, 1, 1))
        .Translated(new Vector3(0.5f, 0.5f, 0.5f));

    private static readonly Transform3D FlipZAroundVoxelCenter = Transform3D.Identity
        .Translated(new Vector3(-0.5f, -0.5f, -0.5f))
        .Scaled(new Vector3(1, 1, -1))
        .Translated(new Vector3(0.5f, 0.5f, 0.5f));

    private static readonly Transform3D FlipXZAroundVoxelCenter = Transform3D.Identity
        .Translated(new Vector3(-0.5f, -0.5f, -0.5f))
        .Scaled(new Vector3(-1, 1, -1))
        .Translated(new Vector3(0.5f, 0.5f, 0.5f));

    private static readonly Transform3D FlipXZMatrix = Transform3D.FlipX * Transform3D.FlipZ;
    
    public static Mesh FlipX(Mesh source)
    {
        return Transform(source, vertex =>
        {
            return vertex * FlipXAroundVoxelCenter;
        }, normal =>
        {
            return normal * Transform3D.FlipX;
        }, uv =>
        {
            return new Vector2(((uv.X - 0.5f) * -1) + 0.5f, uv.Y);
        }, true);
    }
    
    public static Mesh FlipZ(Mesh source)
    {
        return Transform(source, vertex =>
        {
            return vertex * FlipZAroundVoxelCenter;
        }, normal =>
        {
            return normal * Transform3D.FlipZ;
        }, uv =>
        {
            return new Vector2(uv.X, ((uv.Y - 0.5f) * -1) + 0.5f);
        }, true);
    }
    
    public static Mesh FlipXZ(Mesh source)
    {
        return Transform(source, vertex =>
        {
            return vertex * FlipXZAroundVoxelCenter;
        }, normal =>
        {
            return normal * FlipXZMatrix;
        }, uv =>
        {
            return new Vector2(((uv.X - 0.5f) * -1) + 0.5f, ((uv.Y - 0.5f) * -1) + 0.5f);
        }, false);
    }
    
    public static Mesh Transform(Mesh source, Func<Vector3, Vector3> vertexTransformation, Func<Vector3, Vector3> normalTransformation, Func<Vector2, Vector2> uvTransformation, bool flipVertexOrder = false)
    {
        Array sourceSurfaceArrays = source.SurfaceGetArrays(0);
        
        Vector3[] vertices = sourceSurfaceArrays[0].AsVector3Array();
        Vector3[] normals = sourceSurfaceArrays[1].AsVector3Array();
        Vector2[] uv = sourceSurfaceArrays[4].AsVector2Array();
        int[] indices = sourceSurfaceArrays[12].AsInt32Array();

        SurfaceTool surfaceTool = new SurfaceTool();
        surfaceTool.Begin(Mesh.PrimitiveType.Triangles);

        for (int triangleIndex = 0; triangleIndex < indices.Length / 3; triangleIndex++)
        {
            if (flipVertexOrder)
            {
                for (int triangleVertexIndex = 2; triangleVertexIndex >= 0; triangleVertexIndex--)
                {
                    int index = indices[triangleIndex * 3 + triangleVertexIndex];
            
                    surfaceTool.SetUV(uvTransformation(uv[index]));
                    surfaceTool.SetNormal(normalTransformation(normals[index]));
                    surfaceTool.AddVertex(vertexTransformation(vertices[index]));
                }
            }
            else
            {
                for (int triangleVertexIndex = 0; triangleVertexIndex < 3; triangleVertexIndex++)
                {
                    int index = indices[triangleIndex * 3 + triangleVertexIndex];
            
                    surfaceTool.SetUV(uvTransformation(uv[index]));
                    surfaceTool.SetNormal(normalTransformation(normals[index]));
                    surfaceTool.AddVertex(vertexTransformation(vertices[index]));
                }
            }
        }

        surfaceTool.GenerateTangents();
        surfaceTool.Index();

        ArrayMesh result = surfaceTool.Commit();
        return result;
    }
}