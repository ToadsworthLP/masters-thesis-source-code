using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace Umbra.MeshGeneration;

public class AdvancedTileMapVoxelShapeGenerator : ITileMapVoxelShapeGenerator
{
    public enum ExplorationDirection { Up, Down, Left, Right }
    
    private TileMapLayer source;
    private int baseHeight;

    public AdvancedTileMapVoxelShapeGenerator(TileMapLayer source, int baseHeight)
    {
        this.source = source;
        this.baseHeight = baseHeight;
    }

    public VoxelShape Generate()
    {
        Rect2I usedRect = source.GetUsedRect();
        VoxelShape shape = new VoxelShape(
            new Vector3I(usedRect.Size.X, usedRect.Size.Y + 1, usedRect.Size.Y + 1),
            new Vector3I(usedRect.Position.X, baseHeight - 1, usedRect.Position.Y)
        );
        
        Array<Vector2I> usedCells = source.GetUsedCells();
        
        ISet<Vector2I> unexploredCells = new HashSet<Vector2I>(usedRect.Size.X * usedRect.Size.Y);
        unexploredCells.UnionWith(usedCells);

        while (true)
        {
            Vector2I? nextBaseCell = FindAnyCellTouchingFloor(unexploredCells);
            if(!nextBaseCell.HasValue) break;
            
            Vector2I localCurrentCell = ToLocal(nextBaseCell.Value, usedRect);
            Vector3I localVoxelPosition = new Vector3I(localCurrentCell.X, 0, localCurrentCell.Y);
            
            TileData tileData = source. GetCellTileData(nextBaseCell.Value);
            Vector3I tileNormal = tileData.GetCustomData(UmbraTileSetDataLayerNames.TileNormal).AsVector3I();
            if (IsFacingForwards(tileNormal))
            {
                localVoxelPosition += new Vector3I(0, 1, 0);
            }
            
            ExploreTile(unexploredCells, shape, nextBaseCell.Value, localVoxelPosition, ExplorationDirection.Up, IsForwardsSlope(tileNormal));
        }

        if (unexploredCells.Count > 0)
        {
            GD.PushWarning($"{source.Name}: {unexploredCells.Count} tiles remain unexplored after voxel shape generation.");
        }
        
        shape.Crop();
        return shape;
    }

    private void ExploreTile(ISet<Vector2I> unexploredCells, VoxelShape outputVoxelShape, Vector2I currentCellCoordinates, Vector3I currentVoxelCoordinates, ExplorationDirection explorationDirection, bool forwardsSlopeHalfStep)
    {
        if(!unexploredCells.Contains(currentCellCoordinates)) return;
        unexploredCells.Remove(currentCellCoordinates);
        
        TileData currentTile = source.GetCellTileData(currentCellCoordinates);
        if(currentTile == null) return;

        Vector3I tileNormal = currentTile.GetCustomData(UmbraTileSetDataLayerNames.TileNormal).AsVector3I();
        UmbraTileModel tileModel = currentTile.GetCustomData(UmbraTileSetDataLayerNames.TileModel).As<UmbraTileModel>();
        UmbraTileModel frontTileModel = currentTile.GetCustomData(UmbraTileSetDataLayerNames.TileModelFront).As<UmbraTileModel>();
        UmbraTileModel topTileModel = currentTile.GetCustomData(UmbraTileSetDataLayerNames.TileModelTop).As<UmbraTileModel>();
        
        ref Voxel targetVoxel = ref outputVoxelShape.Voxels[currentVoxelCoordinates.X, currentVoxelCoordinates.Y, currentVoxelCoordinates.Z];
        targetVoxel.Filled = true;
        
        if (IsForwardsSlope(tileNormal))
        {
            bool targetTop = !forwardsSlopeHalfStep;
            
            if (targetTop)
            {
                targetVoxel.TopCell = new VoxelCell(currentCellCoordinates);
                
                if (topTileModel != null)
                    targetVoxel.TopModel = topTileModel;
                else if (tileModel != null) 
                    targetVoxel.TopModel = tileModel;
            }
            else
            {
                targetVoxel.FrontCell = new VoxelCell(currentCellCoordinates);
                
                if (frontTileModel != null)
                    targetVoxel.FrontModel = frontTileModel;
                else if (tileModel != null) 
                    targetVoxel.FrontModel = tileModel;
            }
        }
        else
        {
            if (IsFacingForwards(tileNormal))
            {
                targetVoxel.FrontCell = new VoxelCell(currentCellCoordinates);
                
                if (frontTileModel != null)
                    targetVoxel.FrontModel = frontTileModel;
                else if (tileModel != null) 
                    targetVoxel.FrontModel = tileModel;
            }
            else
            {
                targetVoxel.TopCell = new VoxelCell(currentCellCoordinates);
                
                if (topTileModel != null)
                    targetVoxel.TopModel = topTileModel;
                else if (tileModel != null) 
                    targetVoxel.TopModel = tileModel;
            }
        }

        bool skipFillBelow = currentTile.GetCustomData(UmbraTileSetDataLayerNames.TileSkipFillBelow).AsBool();
        if(!skipFillBelow) 
            FillUnderneathRecursive(outputVoxelShape, currentVoxelCoordinates, tileModel);
        
        ExploreInDirection(unexploredCells, outputVoxelShape, currentCellCoordinates, currentVoxelCoordinates, ExplorationDirection.Up, forwardsSlopeHalfStep);
        ExploreInDirection(unexploredCells, outputVoxelShape, currentCellCoordinates, currentVoxelCoordinates, ExplorationDirection.Down, forwardsSlopeHalfStep);
        ExploreInDirection(unexploredCells, outputVoxelShape, currentCellCoordinates, currentVoxelCoordinates, ExplorationDirection.Left, forwardsSlopeHalfStep);
        ExploreInDirection(unexploredCells, outputVoxelShape, currentCellCoordinates, currentVoxelCoordinates, ExplorationDirection.Right, forwardsSlopeHalfStep);
    }

    private void ExploreInDirection(ISet<Vector2I> unexploredCells, VoxelShape outputVoxelShape, Vector2I previousCellCoordinates, Vector3I previousVoxelCoordinates, ExplorationDirection explorationDirection, bool forwardsSlopeHalfStep)
    {
        Vector2I currentCellCoordinates = GetUpdatedCellCoordinates(previousCellCoordinates, explorationDirection);
        if(!unexploredCells.Contains(currentCellCoordinates)) return;
        
        TileData previousTileData = source.GetCellTileData(previousCellCoordinates);
        TileData currentTileData = source.GetCellTileData(currentCellCoordinates);
        
        if(currentTileData == null) return;
        if(HasTileBarrierBetween(previousCellCoordinates, currentCellCoordinates, explorationDirection)) return;
        
        Vector3I previousTileNormal = previousTileData.GetCustomData(UmbraTileSetDataLayerNames.TileNormal).AsVector3I();
        Vector3I currentTileNormal = currentTileData.GetCustomData(UmbraTileSetDataLayerNames.TileNormal).AsVector3I();
        
        Vector3I? currentVoxelCoordinates  = GetUpdatedVoxelCoordinates(previousVoxelCoordinates, previousTileNormal, currentTileNormal, explorationDirection, forwardsSlopeHalfStep);
        if (!currentVoxelCoordinates.HasValue)
        {
            return;
        }
        
        bool nextForwardsSlopeHalfStep = forwardsSlopeHalfStep;
        switch (explorationDirection)
        {
            case ExplorationDirection.Up when IsForwardsSlope(currentTileNormal):
            case ExplorationDirection.Down when IsForwardsSlope(previousTileNormal):
                nextForwardsSlopeHalfStep = !nextForwardsSlopeHalfStep;
                break;
        }
        
        ExploreTile(unexploredCells, outputVoxelShape, currentCellCoordinates, currentVoxelCoordinates.Value, explorationDirection, nextForwardsSlopeHalfStep);
    }

    private bool HasTileBarrierBetween(Vector2I firstCellCoordinates, Vector2I secondCellCoordinates, ExplorationDirection direction)
    {
        return HasTileBarrier(firstCellCoordinates, direction) || HasTileBarrier(secondCellCoordinates, GetOppositeDirection(direction));
    }

    private bool HasTileBarrier(Vector2I cellCoordinates, ExplorationDirection direction)
    {
        TileData tileData = source.GetCellTileData(cellCoordinates);
        if(tileData == null) return false;
        
        return direction switch
        {
            ExplorationDirection.Up => tileData.GetCustomData(UmbraTileSetDataLayerNames.TileBarrierTop).AsBool(),
            ExplorationDirection.Down => tileData.GetCustomData(UmbraTileSetDataLayerNames.TileBarrierBottom).AsBool(),
            ExplorationDirection.Left => tileData.GetCustomData(UmbraTileSetDataLayerNames.TileBarrierLeft).AsBool(),
            ExplorationDirection.Right => tileData.GetCustomData(UmbraTileSetDataLayerNames.TileBarrierRight).AsBool(),
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
    }

    private ExplorationDirection GetOppositeDirection(ExplorationDirection explorationDirection)
    {
        return explorationDirection switch
        {
            ExplorationDirection.Up => ExplorationDirection.Down,
            ExplorationDirection.Down => ExplorationDirection.Up,
            ExplorationDirection.Left => ExplorationDirection.Right,
            ExplorationDirection.Right => ExplorationDirection.Left,
            _ => throw new ArgumentOutOfRangeException(nameof(explorationDirection), explorationDirection, null)
        };
    }

    private Vector2I GetUpdatedCellCoordinates(Vector2I coordinates, ExplorationDirection direction)
    {
        return source.GetNeighborCell(coordinates, GetNeighborEnumFromDirection(direction));
    }

    private Vector3I? GetUpdatedVoxelCoordinates(Vector3I previousCoordinates, Vector3I previousTileNormal, Vector3I currentTileNormal, ExplorationDirection direction, bool forwardsSlopeHalfStep)
    {
        Vector3I? result = Vector3I.Zero;
        if (direction == ExplorationDirection.Up)
        {
            if (IsForwardsSlope(currentTileNormal))
            {
                if (currentTileNormal != previousTileNormal)
                {
                    result = IsFacingForwards(previousTileNormal) ? new Vector3I(0, 1, 0) : new Vector3I(0, 1, -1);
                }
                else
                {
                    result = forwardsSlopeHalfStep ? new Vector3I(0, 0, 0) : new Vector3I(0, 1, -1);
                }
            }
            else if (IsFacingUpwards(currentTileNormal) && (IsFacingForwards(previousTileNormal) && !IsForwardsSlope(previousTileNormal))) result = new Vector3I(0, 0, 0);
            else if (IsFacingUpwards(currentTileNormal)) result = new Vector3I(0, 0, -1);
            else if (IsFacingForwards(currentTileNormal) && IsFacingUpwards(previousTileNormal)) result = new Vector3I(0, 1, -1);
            else if (IsFacingForwards(currentTileNormal)) result = new Vector3I(0, 1, 0);
            else result = null;
        } 
        else if (direction == ExplorationDirection.Down)
        {
            if (IsForwardsSlope(currentTileNormal))
            {
                if (currentTileNormal != previousTileNormal)
                {
                    result = IsFacingForwards(previousTileNormal) ? new Vector3I(0, -1, 1) : new Vector3I(0, 0, 1);
                }
                else
                {
                    result = forwardsSlopeHalfStep ? new Vector3I(0, -1, 1) : new Vector3I(0, 0, 0);
                }
            }
            else if (IsFacingUpwards(currentTileNormal) && IsFacingForwards(previousTileNormal)) result = new Vector3I(0, -1, 1);
            else if (IsFacingUpwards(currentTileNormal)) result = new Vector3I(0, 0, 1);
            else if (IsFacingForwards(currentTileNormal) && (IsFacingUpwards(previousTileNormal) && !IsForwardsSlope(previousTileNormal))) result = new Vector3I(0, 0, 0);
            else if (IsFacingForwards(currentTileNormal)) result = new Vector3I(0, -1, 0);
            else result = null;
        }
        else if (direction == ExplorationDirection.Left)
        {
            if (currentTileNormal == previousTileNormal)
            {
                if (IsSidewaysSlope(currentTileNormal)) result = IsFacingLeft(currentTileNormal) ? new Vector3I(-1, -1, -1) : new Vector3I(-1, 1, 1);
                else if (IsDiagonal(currentTileNormal)) result = IsFacingLeft(currentTileNormal) ? new Vector3I(-1, -1, -1) : new Vector3I(-1, 1, 1);
                else if (IsFacingForwards(currentTileNormal) && IsFacingForwards(previousTileNormal)) result = new Vector3I(-1, 0, 0);
                else if (IsFacingUpwards(currentTileNormal) && IsSidewaysSlope(previousTileNormal)) result = IsFacingLeft(previousTileNormal) ? new Vector3I(-1, -1, -1) : new Vector3I(-1, 0, 0);
                else if (IsFacingUpwards(currentTileNormal) && IsFacingUpwards(previousTileNormal)) result = new Vector3I(-1, 0, 0);
                else result = null;
            }
            else
            {
                result = null;
            }
        }
        else if (direction == ExplorationDirection.Right)
        {
            if (currentTileNormal == previousTileNormal)
            {
                if (IsSidewaysSlope(currentTileNormal)) result = IsFacingLeft(currentTileNormal) ? new Vector3I(1, 1, 1) : new Vector3I(1, -1, -1);
                else if (IsDiagonal(currentTileNormal)) result = IsFacingLeft(currentTileNormal) ? new Vector3I(1, 1, 1) : new Vector3I(1, -1, -1);
                else if (IsFacingForwards(currentTileNormal) && IsFacingForwards(previousTileNormal)) result = new Vector3I(1, 0, 0);
                else if (IsFacingUpwards(currentTileNormal) && IsSidewaysSlope(previousTileNormal)) result = IsFacingLeft(previousTileNormal) ? new Vector3I(1, 0, 0) : new Vector3I(1, -1, -1);
                else if (IsFacingUpwards(currentTileNormal) && IsFacingUpwards(previousTileNormal)) result = new Vector3I(1, 0, 0);
                else result = null;
            }
            else
            { 
                result = null;
            }
        }

        return previousCoordinates + result;
    }

    private void FillUnderneathRecursive(VoxelShape outputVoxelShape, Vector3I voxelPosition, UmbraTileModel model = null)
    {
        voxelPosition += new Vector3I(0, -1, 0);
        if(voxelPosition.Y <= 0) return;

        ref Voxel voxel = ref outputVoxelShape.Voxels[voxelPosition.X, voxelPosition.Y, voxelPosition.Z];
        voxel.Filled = true;
        if (model != null && voxel.FrontModel == null && model.Fill) voxel.FrontModel = model;
        FillUnderneathRecursive(outputVoxelShape, voxelPosition, model);
    }

    private Vector2I? FindAnyCellTouchingFloor(ISet<Vector2I> cells)
    {
        foreach (Vector2I currentCell in cells)
        {
            TileData tileBelow = source.GetCellTileData(new Vector2I(currentCell.X, currentCell.Y + 1));
            if (tileBelow != null) continue;
            if (HasTileBarrier(currentCell, ExplorationDirection.Down)) continue;
            
            return currentCell;
        }
        
        return null;
    }
    
    private Vector2I ToLocal(Vector2I global, Rect2I usedRect)
    {
        return global - usedRect.Position;
    }

    private bool IsFacingForwards(Vector3I normal)
    {
        return normal.Z > float.Epsilon;
    }
    
    private bool IsFacingUpwards(Vector3I normal)
    {
        return normal.Y > float.Epsilon;
    }
    
    private bool IsFacingSideways(Vector3I normal)
    {
        return Math.Abs(normal.X) > float.Epsilon;
    }
    
    private bool IsFacingLeft(Vector3I normal)
    {
        return normal.X < float.Epsilon;
    }
    
    private bool IsFacingRight(Vector3I normal)
    {
        return normal.X > float.Epsilon;
    }

    private bool IsForwardsSlope(Vector3I normal)
    {
        return IsFacingForwards(normal) && IsFacingUpwards(normal);
    }

    private bool IsSidewaysSlope(Vector3I normal)
    {
        return IsFacingSideways(normal) && IsFacingUpwards(normal);
    }

    private bool IsDiagonal(Vector3I normal)
    {
        return IsFacingForwards(normal) && IsFacingSideways(normal);
    }
    
    private TileSet.CellNeighbor GetNeighborEnumFromDirection(ExplorationDirection explorationDirection)
    {
        return explorationDirection switch
        {
            ExplorationDirection.Up => TileSet.CellNeighbor.TopSide,
            ExplorationDirection.Down => TileSet.CellNeighbor.BottomSide,
            ExplorationDirection.Left => TileSet.CellNeighbor.LeftSide,
            ExplorationDirection.Right => TileSet.CellNeighbor.RightSide,
            _ => throw new ArgumentOutOfRangeException(nameof(explorationDirection), explorationDirection, null)
        };
    }
}