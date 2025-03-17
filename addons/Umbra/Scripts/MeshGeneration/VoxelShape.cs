using Godot;

namespace Umbra.MeshGeneration;

public class VoxelShape
{
    public Voxel[,,] Voxels { get; private set; }
    public Vector3I Size { get; private set; }
    public Vector3I Offset { get; private set; }

    public VoxelShape(Vector3I size, Vector3I offset)
    {
        Voxels = new Voxel[size.X, size.Y, size.Z];
        Size = size;
        Offset = offset;
    }

    public VoxelShape(Voxel[,,] voxels, Vector3I offset)
    {
        Voxels = voxels;
        Size = new Vector3I(voxels.GetLength(0), voxels.GetLength(1), voxels.GetLength(2));
        Offset = offset;
    }
    
    /// <summary>
    /// Resizes the current voxel shape to the minimum size required to fit all non-empty voxels
    /// </summary>
    public void Crop()
    {
        Voxel[,,] original = Voxels;

        // Find minimal AABB of original voxel shape
        int minX = int.MaxValue, minY = int.MaxValue, minZ = int.MaxValue;
        int maxX = int.MinValue, maxY = int.MinValue, maxZ = int.MinValue;
        for (int x = 0; x < original.GetLength(0); x++)
        {
            for (int y = 0; y < original.GetLength(1); y++)
            {
                for (int z = 0; z < original.GetLength(2); z++)
                {
                    if (!original[x, y, z].IsEmpty)
                    {
                        if (x < minX) minX = x;
                        if (y < minY) minY = y;
                        if (z < minZ) minZ = z;

                        if (x > maxX) maxX = x;
                        if (y > maxY) maxY = y;
                        if (z > maxZ) maxZ = z;
                    }
                }
            }
        }

        // The voxel shape is completely empty
        if (minX == int.MaxValue && minY == int.MaxValue && minZ == int.MaxValue &&
            maxX == int.MinValue && maxY == int.MinValue && maxZ == int.MinValue)
        {
            Size = new Vector3I(0, 0, 0);
            Offset = new Vector3I(0, 0, 0);
            Voxels = new Voxel[0, 0, 0];
            return;
        }
        
        // Allocate new voxel array with the dimensions of the AABB
        int width = maxX - minX + 1;
        int height = maxY - minY + 1;
        int depth = maxZ - minZ + 1;
        Voxel[,,] cropped = new Voxel[width, height, depth];

        // Copy over voxel data from old array to new array
        for (int x = 0; x < cropped.GetLength(0); x++)
        {
            for (int y = 0; y < cropped.GetLength(1); y++)
            {
                for (int z = 0; z < cropped.GetLength(2); z++)
                {
                    cropped[x, y, z] = original[x + minX, y + minY, z + minZ];
                }
            }
        }

        Size = new Vector3I(width, height, depth);
        Offset += new Vector3I(minX, minY, minZ);
        Voxels = cropped;
    }
}