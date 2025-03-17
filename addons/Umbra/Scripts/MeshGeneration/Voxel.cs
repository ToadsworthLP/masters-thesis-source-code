namespace Umbra.MeshGeneration;

public struct Voxel
{
    public bool IsEmpty => !Filled;

    public bool Filled { get; set; } = false;

    public VoxelCell? TopCell { get; set; } = null;
    public VoxelCell? FrontCell { get; set; } = null;

    public UmbraTileModel TopModel { get; set; } = null;
    public UmbraTileModel FrontModel { get; set; } = null;

    public Voxel() {}
}