using System;
using Godot;

namespace Umbra.Nodes;

[GlobalClass]
[Tool]
public partial class UmbraNode3D : Node3D, IUmbraCompanion
{
    [Export]
    public Node2D Target
    {
        get => target;
        set
        {
            target = value;
            CallDeferred(nameof(HandleTargetChanged));
        }
    }
    
    [Export] public bool Update;
    [Export] public bool PixelSnap = true;
    
    private Node2D target;    
    private UmbraRoot root;
    
    public UmbraNode3D()
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
            UpdateCompanion();
        }
    }
    
    public virtual void SetCompanion(Node target)
    {
        Target = (Node2D)target;
    }

    public virtual Node GetCompanion()
    {
        return Target;
    }

    public virtual void UpdateCompanion()
    {
        UpdatePosition();
        Visible = Target.Visible;
    }

    protected virtual void UpdatePosition()
    {
        Vector2 targetPosition = target.GlobalPosition;
        int pixelsPerMeter = root.PixelsPerMeter;

        if (PixelSnap)
        {
            targetPosition.X = MathF.Round(targetPosition.X);
            targetPosition.Y = MathF.Round(targetPosition.Y);
        }

        float height = target.GetHeight();
        Vector3 position = new Vector3(targetPosition.X / pixelsPerMeter, height, targetPosition.Y / pixelsPerMeter + height);

        Position = position;
    }
    
    protected virtual void HandleTargetChanged()
    {
        UpdateCompanion();
    }
}