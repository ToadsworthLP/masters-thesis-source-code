using System;
using Godot;

namespace Umbra.Nodes;

[GlobalClass]
[Tool]
public partial class UmbraSprite3D : Node3D, IUmbraCompanion
{
    public enum UmbraSpriteAxis
    {
        Y,
        Z
    };
    
    private const string VisibleSpriteInstanceName = "UmbraVisibleSprite";
    private const string ShadowSpriteInstanceName = "UmbraShadowSprite";
    private const string DefaultMaterialPath = "res://addons/Umbra/Materials/Mat_Umbra_Sprite.tres";

    [Export]
    public Sprite2D Target
    {
        get => target;
        set
        {
            Sprite2D previous = target;
            target = value;
            CallDeferred(nameof(HandleTargetChanged), previous);
        }
    }
    
    [Export] public bool Update;
    [Export] public bool PixelSnap = true;

    [Export]
    public UmbraSpriteAxis SpriteAxis
    {
        get => spriteAxis;
        set
        {
            spriteAxis = value;
            CallDeferred(nameof(HandleAxisChanged));
        }
    }

    [Export]
    public Texture2D NormalMap
    {
        get => normalMap;
        set
        {
            normalMap = value;
            CallDeferred(nameof(HandleNormalMapChanged));
        }
    }

    private UmbraRoot root;
    private Sprite2D target;
    private UmbraSpriteAxis spriteAxis = UmbraSpriteAxis.Z;
    private Texture2D normalMap;
    
    private Sprite3D visibleSprite;
    private Sprite3D shadowSprite;

    private Texture lastTexture;

    public UmbraSprite3D()
    {
#if TOOLS
        if (Engine.IsEditorHint()) HandleEditorReload();
#endif
    }
    
    public override void _Ready()
    {
        EnsureChildrenExist();
        if(HasNode(VisibleSpriteInstanceName)) visibleSprite = GetNode<Sprite3D>(VisibleSpriteInstanceName);
        if(HasNode(ShadowSpriteInstanceName)) shadowSprite = GetNode<Sprite3D>(ShadowSpriteInstanceName);
        root = UmbraNodeUtils.FindUmbraRootInParents(this);
    }

    public override void _Process(double delta)
    {
        if (target == null) return;
        
        if (Update || Engine.IsEditorHint())
        {
            UpdateSpriteProperties(visibleSprite);
            UpdateSpriteProperties(shadowSprite);
            UpdatePosition();
            UpdateTexture(visibleSprite);
        }
    }

    private void HandleTargetChanged(Sprite2D previous)
    {
        if (target != null)
        {
            UpdateSpriteProperties(visibleSprite);
            UpdateSpriteProperties(shadowSprite);
            UpdatePosition();
            UpdateTexture(visibleSprite);
        }
    }

    private void HandleAxisChanged()
    {
        Vector3.Axis gdAxis = spriteAxis == UmbraSpriteAxis.Y ? Vector3.Axis.Y : Vector3.Axis.Z;
        visibleSprite.Axis = gdAxis;
        shadowSprite.Axis = gdAxis;
    }

    private void HandleNormalMapChanged()
    {
        ShaderMaterial material = (ShaderMaterial)visibleSprite.MaterialOverride;
        material.SetShaderParameter("normalMapTexture", normalMap);
    }

    private void HandleEditorReload()
    {
        if(HasNode(VisibleSpriteInstanceName)) visibleSprite = GetNode<Sprite3D>(VisibleSpriteInstanceName);
        if(HasNode(ShadowSpriteInstanceName)) shadowSprite = GetNode<Sprite3D>(ShadowSpriteInstanceName);
        root = UmbraNodeUtils.FindUmbraRootInParents(this);
    }
    
    private void UpdateSpriteProperties(Sprite3D sprite)
    {
        sprite.PixelSize = 1.0f / root.PixelsPerMeter;
        
        sprite.Texture = target.Texture;
        
        sprite.Hframes = target.Hframes;
        sprite.Vframes = target.Vframes;
        sprite.Frame = target.Frame;
    
        sprite.RegionEnabled = target.RegionEnabled;
        sprite.RegionRect = target.RegionRect;

        sprite.Centered = target.Centered;
        sprite.Offset = PixelSnap ? new Vector2(MathF.Round(target.Offset.X), MathF.Round(target.Offset.Y)) : target.Offset;
        sprite.FlipH = target.FlipH;
        sprite.FlipV = target.FlipV;
        sprite.Modulate = target.Modulate;
        
        Visible = Target.Visible;
    
        // This isn't applied anyway since the custom shader doesn't support it
        //sprite.TextureFilter = UmbraNodeUtils.Get3DTextureFilterModeFromCanvasItem(target);
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
        Vector3 position = new Vector3(targetPosition.X / pixelsPerMeter, height, targetPosition.Y / pixelsPerMeter + height);

        Vector2 targetSize = target.GetRect().Size;
        if (target.Centered)
        {
            if (SpriteAxis == UmbraSpriteAxis.Z)
            {
                position.Z += targetSize.Y / pixelsPerMeter / 2;
                position.Y += targetSize.Y / pixelsPerMeter / 2;
            }
        }
        else
        {
            position.Z += targetSize.Y / pixelsPerMeter;
        }

        Position = position;
    }

    private void UpdateTexture(Sprite3D sprite)
    {
        if(sprite.Texture == lastTexture) return;
        
        ShaderMaterial material = (ShaderMaterial)sprite.MaterialOverride;
        material.SetShaderParameter("mainTexture", sprite.Texture);

        lastTexture = sprite.Texture;
    }

    private void EnsureChildrenExist()
    {
        if (!HasNode(VisibleSpriteInstanceName))
        {
            Material material = (Material)GD.Load<Material>(DefaultMaterialPath).Duplicate();
            
            Sprite3D node = new Sprite3D();
            node.Name = VisibleSpriteInstanceName;
            node.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
            node.Shaded = true;
            node.MaterialOverride = material;
            node.AlphaCut = SpriteBase3D.AlphaCutMode.Discard;
            AddChild(node);
            
#if TOOLS
            if(Engine.IsEditorHint()) node.Owner = GetTree().EditedSceneRoot;
#endif

            visibleSprite = node;
        }
        
        if (!HasNode(ShadowSpriteInstanceName))
        {
            Sprite3D node = new Sprite3D();
            node.Name = ShadowSpriteInstanceName;
            node.CastShadow = GeometryInstance3D.ShadowCastingSetting.ShadowsOnly;
            node.AlphaCut = SpriteBase3D.AlphaCutMode.Discard;
            AddChild(node);
            
#if TOOLS
            if(Engine.IsEditorHint()) node.Owner = GetTree().EditedSceneRoot;
#endif

            shadowSprite = node;
        }
    }


    public void SetCompanion(Node target)
    {
        if (target is Sprite2D sprite)
        {
            EnsureChildrenExist();
            Target = sprite;
        }
    }

    public Node GetCompanion()
    {
        return Target;
    }
}