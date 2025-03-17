using System;
using Godot;

namespace Umbra.Nodes;

[GlobalClass]
[Tool]
public partial class UmbraCamera3D : Camera3D, IUmbraCompanion
{
    [Export] public Camera2D Target;
    [Export] public bool Update = true;
    [Export] public bool PixelSnap = true;

    private UmbraRoot root;
    
    public UmbraCamera3D()
    {
#if TOOLS
        root = UmbraNodeUtils.FindUmbraRootInParents(this);
#endif
    }

    public override void _EnterTree()
    {
        root = UmbraNodeUtils.FindUmbraRootInParents(this);
        Projection = ProjectionType.Orthogonal;
    }

    public override void _Process(double delta)
    {
        // Stop if there's no target or Umbra root
        if (Target == null || root == null) return;

        if (!Update && !Engine.IsEditorHint()) return;

        // Current camera management
        bool targetCurrent = Target.IsCurrent();
        if (Engine.IsEditorHint())
        {
            Current = true;
        }
        else
        {
            Current = targetCurrent;
            if(!targetCurrent) return;
        }
        
        // Position
        Vector3 position;
        Vector2 targetPosition = Target.GetScreenCenterPosition();
        Vector2 targetOffset = Target.Offset;
        int pixelsPerMeter = root.PixelsPerMeter;

        if (PixelSnap)
        {
            targetPosition.X = MathF.Round(targetPosition.X);
            targetPosition.Y = MathF.Round(targetPosition.Y);

            targetOffset.X = MathF.Round(targetOffset.X);
            targetOffset.Y = MathF.Round(targetOffset.Y);
        }

        float height = Target.GetHeight();
        position = new Vector3((targetPosition.X - targetOffset.X) / pixelsPerMeter, height, (targetPosition.Y - targetOffset.Y) / pixelsPerMeter + height);

        // Size
        Vector2 visibleRectSize = GetViewport().GetVisibleRect().Size;
        float size;
        if (KeepAspect == KeepAspectEnum.Height)
        {
            size = visibleRectSize.Y / pixelsPerMeter;
        }
        else
        {
            size = visibleRectSize.X / pixelsPerMeter;
        }

        // Setting final properties
        Size = size;
        Position = position;
        RotationDegrees = new Vector3(315, 0 ,0);
    }

    public void SetCompanion(Node target)
    {
        if (target is Camera2D camera)
        {
            Target = camera;
        }
    }

    public Node GetCompanion()
    {
        return Target;
    }
}