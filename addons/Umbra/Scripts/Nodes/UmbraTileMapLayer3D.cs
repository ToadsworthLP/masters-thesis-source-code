using System;
using Godot;
using Godot.Collections;
using Umbra.MeshGeneration;

namespace Umbra.Nodes;

[GlobalClass]
[Tool]
public partial class UmbraTileMapLayer3D : Node3D, IUmbraCompanion
{
    private const string VisibleMeshInstanceName = "UmbraVisibleMesh";
    private const string ShadowMeshInstanceName = "UmbraShadowMesh";
    
    [Export] public TileMapLayer Target
    {
        get => target;
        set
        {
            target = value;
            CallDeferred(nameof(TargetNodeChanged));
        }
    }
    
    [Export] public bool Update;
    [Export] public bool PixelSnap = true;
    [Export] public bool UseSeparateShadowMesh = true;
    [Export] public int BaseHeight;
    [Export] public Vector3 MeshOffset;
    
    private TileMapLayer target;
    private UmbraRoot root;
    
    public UmbraTileMapLayer3D()
    {
#if TOOLS
        root = UmbraNodeUtils.FindUmbraRootInParents(this);
#endif
    }
    
    public override void _EnterTree()
    {
        root = UmbraNodeUtils.FindUmbraRootInParents(this);
    }
    
    public override void _Process(double delta)
    {
        if (target == null) return;
        
        if (Update || Engine.IsEditorHint())
        {
            UpdatePosition();
            Visible = Target.Visible;
        }
    }

    private void UpdatePosition()
    {
        Vector2 targetPosition = target.GlobalPosition;
        int pixelsPerMeter = root.PixelsPerMeter;

        if (PixelSnap)
        {
            targetPosition.X = MathF.Round(targetPosition.X);
            targetPosition.Y = MathF.Round(targetPosition.Y);
        }

        float height = target.GetHeight();
        Vector3 position = MeshOffset + new Vector3(targetPosition.X / pixelsPerMeter, height, targetPosition.Y / pixelsPerMeter + height);

        Position = position;
    }
    
    public void UpdateMesh()
    {
        EnsureMeshInstanceChildrenExist();

        if (Target == null)
        {
            throw new Exception("Failed to generate Umbra mesh: No target tile map set.");
        }
        
        if (Target.TileSet.GetScript().As<Script>().GetGlobalName() != nameof(UmbraTileSet))
        {
            throw new Exception("Failed to generate Umbra mesh: The tile set associated with the target tile map is not of type UmbraTileSet.");
        }

        if (!Target.GetUsedRect().HasArea())
        {
            throw new Exception("Failed to generate Umbra mesh: The target tile map is empty.");
        }

        ITileMapVoxelShapeGenerator tileMapVoxelShapeGenerator = new AdvancedTileMapVoxelShapeGenerator(Target, BaseHeight);
        VoxelShape voxelShape = tileMapVoxelShapeGenerator.Generate();

        Position = MeshOffset = voxelShape.Offset;
        
        ArrayMesh visibleMesh = new ArrayMesh();
        ITileMapMeshGenerator visibleMeshGenerator = new VisibleMeshGenerator(Target, voxelShape);
        visibleMeshGenerator.Generate(visibleMesh);

        Material material = ((UmbraTileSet)Target.TileSet).UmbraMaterial;
        visibleMesh.SurfaceSetMaterial(0, material);

        ArrayMesh shadowMesh = new ArrayMesh();
        if (UseSeparateShadowMesh)
        {
            ITileMapMeshGenerator shadowMeshGenerator = new ShadowMeshGenerator(Target, voxelShape);
            shadowMeshGenerator.Generate(shadowMesh);
        }
        else
        {
            ITileMapMeshGenerator shadowMeshGenerator = new VisibleMeshGenerator(Target, voxelShape, true);
            shadowMeshGenerator.Generate(shadowMesh);
            shadowMesh.SurfaceSetMaterial(0, material);
        }

        GetNode<MeshInstance3D>(VisibleMeshInstanceName).Mesh = visibleMesh;
        GetNode<MeshInstance3D>(ShadowMeshInstanceName).Mesh = shadowMesh;
    }

    public void ClearMesh()
    {
        GetNode<MeshInstance3D>(VisibleMeshInstanceName).Mesh = null;
        GetNode<MeshInstance3D>(ShadowMeshInstanceName).Mesh = null;
    }

    private void EnsureMeshInstanceChildrenExist()
    {
        if (!HasNode(VisibleMeshInstanceName))
        {
            MeshInstance3D visibleMesh = new MeshInstance3D();
            visibleMesh.Name = VisibleMeshInstanceName;
            visibleMesh.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
            AddChild(visibleMesh);
            
            #if TOOLS
            if(Engine.IsEditorHint()) visibleMesh.Owner = GetTree().EditedSceneRoot;
            #endif
        }

        if (!HasNode(ShadowMeshInstanceName))
        {
            MeshInstance3D shadowMesh = new MeshInstance3D();
            shadowMesh.Name = ShadowMeshInstanceName;
            shadowMesh.CastShadow = GeometryInstance3D.ShadowCastingSetting.ShadowsOnly;
            AddChild(shadowMesh);
            
            #if TOOLS
            if(Engine.IsEditorHint()) shadowMesh.Owner = GetTree().EditedSceneRoot;
            #endif
        }
    }
    
    private void TargetNodeChanged()
    {
        if (target == null)
        {
            ClearMesh();
        }
        else
        {
            UpdateMesh();
            UpdatePosition();
            Visible = Target.Visible;
        }
    }

    public void SetCompanion(Node target)
    {
        if (target is TileMapLayer tileMapLayer)
        {
            Target = tileMapLayer;
        }
    }

    public Node GetCompanion()
    {
        return target;
    }

    public override void _ValidateProperty(Dictionary property)
    {
        if (property["name"].AsStringName() == PropertyName.MeshOffset)
        {
            var usage = property["usage"].As<PropertyUsageFlags>() | PropertyUsageFlags.ReadOnly;
            property["usage"] = (int)usage;
        }
    }
}