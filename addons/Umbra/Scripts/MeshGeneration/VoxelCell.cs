using Godot;

namespace Umbra.MeshGeneration;

public struct VoxelCell
{
    public Vector2I Coordinates;

    public VoxelCell(Vector2I coordinates)
    {
        Coordinates = coordinates;
    }
}