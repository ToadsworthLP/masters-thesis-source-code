using Godot;

namespace Umbra.MeshGeneration;

public interface ITileMapMeshGenerator
{
    void Generate(ArrayMesh destination);
}