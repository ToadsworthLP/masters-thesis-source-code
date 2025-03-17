using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace Umbra.MeshGeneration;

public class TileMapVoxelShapeGenerator : ITileMapVoxelShapeGenerator
{
    private TileMapLayer source;
    private int baseHeight;

    private ISet<Vector2I> visitedCells;
    private Rect2I usedRect;
    private VoxelShape shape;

    private IDictionary<Vector2I, Vector3I> voxelMapping;

    public TileMapVoxelShapeGenerator(TileMapLayer source, int baseHeight)
    {
        this.source = source;
        this.baseHeight = baseHeight;
    }

    public VoxelShape Generate()
    {
        usedRect = source.GetUsedRect();
        shape = new VoxelShape(
            new Vector3I(usedRect.Size.X, usedRect.Size.Y, usedRect.Size.Y),
            new Vector3I(usedRect.Position.X, baseHeight - 1, usedRect.Position.Y)
        );
        
        Array<Vector2I> usedCells = source.GetUsedCells();
        ISet<Vector2I> unvisitedCellsAfterUpwardsPhase = new HashSet<Vector2I>(usedCells);
        visitedCells = new HashSet<Vector2I>(usedRect.Size.X * usedRect.Size.Y);
        voxelMapping = new System.Collections.Generic.Dictionary<Vector2I, Vector3I>();
        
        ExploreUpwards(usedCells);

        unvisitedCellsAfterUpwardsPhase.ExceptWith(visitedCells);

        ExploreNeighbors(unvisitedCellsAfterUpwardsPhase);
        
        shape.Crop();
        return shape;
    }

    private void ExploreNeighbors(ICollection<Vector2I> unvisitedCellsAfterUpwardsPhase)
    {
        for (int i = 0; i < unvisitedCellsAfterUpwardsPhase.Count; i++)
        {
            foreach (Vector2I cell in unvisitedCellsAfterUpwardsPhase)
            {
                if(visitedCells.Contains(cell)) continue;
                
                ExploreNeighborVertical(cell, TileSet.CellNeighbor.TopSide);
                ExploreNeighborVertical(cell, TileSet.CellNeighbor.BottomSide);
                
                ExploreNeighborHorizontal(cell, TileSet.CellNeighbor.LeftSide);
                ExploreNeighborHorizontal(cell, TileSet.CellNeighbor.RightSide);
            }
        }
    }

    private void ExploreNeighborHorizontal(Vector2I cell, TileSet.CellNeighbor direction)
    {
        Vector2I neighborCell = source.GetNeighborCell(cell, direction);
        TileData currentTile = source.GetCellTileData(cell);
        TileData tileLeft = source.GetCellTileData(neighborCell);
        int directionalMultiplier = direction == TileSet.CellNeighbor.LeftSide ? 1 : -1;
        
        bool noBarrier = direction == TileSet.CellNeighbor.LeftSide
            ? currentTile.GetCustomData(UmbraTileSetDataLayerNames.TileBarrierLeft).AsBool() == false &&
              tileLeft.GetCustomData(UmbraTileSetDataLayerNames.TileBarrierRight).AsBool() == false
            : currentTile.GetCustomData(UmbraTileSetDataLayerNames.TileBarrierRight).AsBool() == false &&
              tileLeft.GetCustomData(UmbraTileSetDataLayerNames.TileBarrierLeft).AsBool() == false;
        
        if (tileLeft != null && visitedCells.Contains(neighborCell) && noBarrier)
        {
            Vector3I neighborVoxelPosition = voxelMapping[neighborCell];
            Vector3I currentVoxelPosition = new Vector3I(neighborVoxelPosition.X + 1 * directionalMultiplier, neighborVoxelPosition.Y, neighborVoxelPosition.Z);
            
            Vector2I currentCellGradient = currentTile.GetCustomData(UmbraTileSetDataLayerNames.TileGradient).AsVector2I();
            UmbraTileModel model = currentTile.GetCustomData(UmbraTileSetDataLayerNames.TileModel).As<UmbraTileModel>();
                    
            ref Voxel voxel = ref shape.Voxels[currentVoxelPosition.X, currentVoxelPosition.Y, currentVoxelPosition.Z];
            voxel.Filled = true;
                    
            if (currentCellGradient.Y == 0)
            {
                voxel.TopCell = new VoxelCell(cell);
                if(model != null) voxel.TopModel = model;
            }
            else
            {
                shape.Voxels[currentVoxelPosition.X, currentVoxelPosition.Y, currentVoxelPosition.Z].FrontCell = new VoxelCell(cell);
                if(model != null) voxel.FrontModel = model;
            }
                    
            FillUnderneathRecursive(currentVoxelPosition, model);
            visitedCells.Add(cell);
            voxelMapping[cell] = currentVoxelPosition;
        }
    }

    private void ExploreNeighborVertical(Vector2I cell, TileSet.CellNeighbor direction)
    {
        Vector2I neighborCell = source.GetNeighborCell(cell, direction);
        TileData currentTile = source.GetCellTileData(cell);
        TileData neighborTile = source.GetCellTileData(neighborCell);
        int directionalMultiplier = direction == TileSet.CellNeighbor.TopSide ? 1 : -1;
        
        bool noBarrier = direction == TileSet.CellNeighbor.TopSide
            ? currentTile.GetCustomData(UmbraTileSetDataLayerNames.TileBarrierTop).AsBool() == false &&
              neighborTile.GetCustomData(UmbraTileSetDataLayerNames.TileBarrierBottom).AsBool() == false
            : currentTile.GetCustomData(UmbraTileSetDataLayerNames.TileBarrierBottom).AsBool() == false &&
              neighborTile.GetCustomData(UmbraTileSetDataLayerNames.TileBarrierTop).AsBool() == false;
        
        if (neighborTile != null && visitedCells.Contains(neighborCell) && noBarrier)
        {
            Vector2I currentCellGradient = currentTile.GetCustomData(UmbraTileSetDataLayerNames.TileGradient).AsVector2I();
            Vector2I neighborCellGradient = neighborTile.GetCustomData(UmbraTileSetDataLayerNames.TileGradient).AsVector2I();
            Vector3I neighborVoxelPosition = voxelMapping[neighborCell];
            
            UmbraTileModel model = currentTile.GetCustomData(UmbraTileSetDataLayerNames.TileModel).As<UmbraTileModel>();

            Vector3I currentVoxelPosition = default;
            if (currentCellGradient.Y == 0 && neighborCellGradient.Y == 0)
            {
                currentVoxelPosition = new Vector3I(neighborVoxelPosition.X, neighborVoxelPosition.Y, neighborVoxelPosition.Z + 1 * directionalMultiplier);
                shape.Voxels[currentVoxelPosition.X, currentVoxelPosition.Y, currentVoxelPosition.Z].Filled = true;
                shape.Voxels[currentVoxelPosition.X, currentVoxelPosition.Y, currentVoxelPosition.Z].TopCell = new VoxelCell(cell);
                if(model != null) shape.Voxels[currentVoxelPosition.X, currentVoxelPosition.Y, currentVoxelPosition.Z].TopModel = model;
            } 
            else if (currentCellGradient.Y != 0 && neighborCellGradient.Y == 0)
            {
                if (direction == TileSet.CellNeighbor.TopSide)
                {
                    currentVoxelPosition = new Vector3I(neighborVoxelPosition.X, neighborVoxelPosition.Y, neighborVoxelPosition.Z);
                }
                else
                {
                    currentVoxelPosition = new Vector3I(neighborVoxelPosition.X, neighborVoxelPosition.Y - 1 * directionalMultiplier, neighborVoxelPosition.Z + 1 * directionalMultiplier);
                }
                shape.Voxels[currentVoxelPosition.X, currentVoxelPosition.Y, currentVoxelPosition.Z].Filled = true;
                shape.Voxels[currentVoxelPosition.X, currentVoxelPosition.Y, currentVoxelPosition.Z].FrontCell = new VoxelCell(cell);
                if(model != null) shape.Voxels[currentVoxelPosition.X, currentVoxelPosition.Y, currentVoxelPosition.Z].FrontModel = model;
            } 
            else if (currentCellGradient.Y == 0 && neighborCellGradient.Y != 0)
            {
                if (direction == TileSet.CellNeighbor.TopSide)
                {
                    currentVoxelPosition = new Vector3I(neighborVoxelPosition.X, neighborVoxelPosition.Y - 1 * directionalMultiplier, neighborVoxelPosition.Z + 1 * directionalMultiplier);
                }
                else
                {
                    currentVoxelPosition = new Vector3I(neighborVoxelPosition.X, neighborVoxelPosition.Y, neighborVoxelPosition.Z);
                }
                
                shape.Voxels[currentVoxelPosition.X, currentVoxelPosition.Y, currentVoxelPosition.Z].Filled = true;
                shape.Voxels[currentVoxelPosition.X, currentVoxelPosition.Y, currentVoxelPosition.Z].TopCell = new VoxelCell(cell);
                if(model != null) shape.Voxels[currentVoxelPosition.X, currentVoxelPosition.Y, currentVoxelPosition.Z].TopModel = model;
            }
            else if (currentCellGradient.Y != 0 && neighborCellGradient.Y != 0)
            {
                currentVoxelPosition = new Vector3I(neighborVoxelPosition.X, neighborVoxelPosition.Y - 1 * directionalMultiplier, neighborVoxelPosition.Z);
                shape.Voxels[currentVoxelPosition.X, currentVoxelPosition.Y, currentVoxelPosition.Z].Filled = true;
                shape.Voxels[currentVoxelPosition.X, currentVoxelPosition.Y, currentVoxelPosition.Z].FrontCell = new VoxelCell(cell);
                if(model != null) shape.Voxels[currentVoxelPosition.X, currentVoxelPosition.Y, currentVoxelPosition.Z].FrontModel = model;
            }
                    
            FillUnderneathRecursive(currentVoxelPosition, model);
            visitedCells.Add(cell);
            voxelMapping[cell] = currentVoxelPosition;
        }
    }

    private void ExploreUpwards(IEnumerable<Vector2I> usedCells)
    {
        foreach (Vector2I currentCell in usedCells)
        {
            TileData tileBelow = source.GetCellTileData(new Vector2I(currentCell.X, currentCell.Y + 1));
            if (tileBelow != null) continue;
            
            // Found a baseline tile!
            Vector2I localCurrentCell = ToLocal(currentCell);
            Vector3I localVoxelPosition = new Vector3I(localCurrentCell.X, 0, localCurrentCell.Y);

            // Explore upwards
            ExploreUpwardsRecursive(currentCell, localVoxelPosition);
        }
    }

    private void ExploreUpwardsRecursive(Vector2I currentCell, Vector3I voxelPosition)
    {
        TileData currentTile = source.GetCellTileData(currentCell);
        if(currentTile == null) return;
        
        shape.Voxels[voxelPosition.X, voxelPosition.Y, voxelPosition.Z].Filled = true;
        visitedCells.Add(currentCell);
        
        Vector2I currentCellGradient = currentTile.GetCustomData(UmbraTileSetDataLayerNames.TileGradient).AsVector2I();
        UmbraTileModel model = currentTile.GetCustomData(UmbraTileSetDataLayerNames.TileModel).As<UmbraTileModel>();
        
        FillUnderneathRecursive(voxelPosition, model);
        
        if (currentCellGradient.Y < 0)
        {
            ref Voxel voxel = ref shape.Voxels[voxelPosition.X, voxelPosition.Y + 1, voxelPosition.Z];
            voxel.FrontCell = new VoxelCell(currentCell);
            if(model != null) voxel.FrontModel = model;
            voxelMapping[currentCell] = new Vector3I(voxelPosition.X, voxelPosition.Y + 1, voxelPosition.Z);
            voxelPosition += new Vector3I(0, 1, 0);
        }
        else
        {
            ref Voxel voxel = ref shape.Voxels[voxelPosition.X, voxelPosition.Y, voxelPosition.Z];
            voxel.TopCell = new VoxelCell(currentCell);
            if(model != null) voxel.TopModel = model;
            voxelMapping[currentCell] = new Vector3I(voxelPosition.X, voxelPosition.Y, voxelPosition.Z);
            voxelPosition += new Vector3I(0, 0, -1);
        }

        bool upwardsBarrier = currentTile.GetCustomData(UmbraTileSetDataLayerNames.TileBarrierTop).AsBool();
        if(upwardsBarrier) return;
        
        Vector2I cellAbove = source.GetNeighborCell(currentCell, TileSet.CellNeighbor.TopSide);
        ExploreUpwardsRecursive(cellAbove, voxelPosition);
    }

    private void FillUnderneathRecursive(Vector3I voxelPosition, UmbraTileModel model = null)
    {
        voxelPosition += new Vector3I(0, -1, 0);
        if(voxelPosition.Y < 0) return;

        ref Voxel voxel = ref shape.Voxels[voxelPosition.X, voxelPosition.Y, voxelPosition.Z];
        voxel.Filled = true;
        if (model != null && voxel.FrontModel == null && model.Fill) voxel.FrontModel = model;
        FillUnderneathRecursive(voxelPosition, model);
    }

    private Vector2I ToLocal(Vector2I global)
    {
        return global - usedRect.Position;
    }
}