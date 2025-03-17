using System;
using Godot;
using Umbra.Nodes;

namespace Umbra;

public static class UmbraNodeUtils
{
    public static UmbraRoot FindUmbraRootInParents(Node node)
    {
        Node current = node;
        while (current is not UmbraRoot)
        {
            current = current.GetParent();
            if (current == null) return null;
        }

        return (UmbraRoot)current;
    }
    
    public static UmbraRoot FindUmbraRootInChildren(Node node)
    {
        if (node is UmbraRoot root) return root;

        UmbraRoot childResult = null;
        foreach (var child in node.GetChildren())
        {
            var result = FindUmbraRootInChildren(child);
            if (result != null) childResult = result;
        }

        return childResult;
    }
    
    public static bool HasHeight(this Node node)
    {
        return node.HasMeta(UmbraMetadataNames.HeightName);
    }

    public static float GetHeight(this Node node)
    {
        if (node.HasMeta(UmbraMetadataNames.HeightName))
        {
            return node.GetMeta(UmbraMetadataNames.HeightName).As<float>();
        }

        return 0f;
    }
    
    public static void SetHeight(this Node node, float value)
    {
        node.SetMeta(UmbraMetadataNames.HeightName, value);

        if (node is CanvasItem canvasItem) canvasItem.EmitSignal(CanvasItem.SignalName.ItemRectChanged);
    }

    public static void RemoveHeight(this Node node)
    {
        node.RemoveMeta(UmbraMetadataNames.HeightName);
    }
    
    public static bool HasCompanion(this Node node)
    {
        return node.HasMeta(UmbraMetadataNames.CompanionName);
    }
    
    public static NodePath GetCompanion(this Node node)
    {
        if (node.HasMeta(UmbraMetadataNames.CompanionName))
        {
            return node.GetMeta(UmbraMetadataNames.CompanionName).AsNodePath();
        }

        return null;
    }

    public static void SetCompanion(this Node node, Node companion)
    {
        node.SetMeta(UmbraMetadataNames.CompanionName, node.GetPathTo(companion));
    }

    public static void RemoveCompanion(this Node node)
    {
        node.RemoveMeta(UmbraMetadataNames.CompanionName);
    }

    public static BaseMaterial3D.TextureFilterEnum Get3DTextureFilterModeFromCanvasItem(CanvasItem canvasItem)
    {
        CanvasItem.TextureFilterEnum canvasItemMode = canvasItem.TextureFilter;
        if (canvasItemMode == CanvasItem.TextureFilterEnum.ParentNode)
        {
            canvasItemMode = FindTextureFilterModeInParents(canvasItem);
        }

        return Convert2DTo3DTextureFilterEnum(canvasItemMode);
    }

    public static CanvasItem.TextureFilterEnum FindTextureFilterModeInParents(Node node)
    {
        Node current = node;
        while (current != null)
        {
            if (current is CanvasItem canvasItem)
            {
                if (canvasItem.TextureFilter != CanvasItem.TextureFilterEnum.ParentNode)
                {
                    return canvasItem.TextureFilter;
                }
            }
            
            current = current.GetParent();
        }

        return GetDefaultTextureFilterMode();
    }

    public static CanvasItem.TextureFilterEnum GetDefaultTextureFilterMode()
    {
        Viewport.DefaultCanvasItemTextureFilter defaultMode = ProjectSettings.GetSettingWithOverride("rendering/textures/canvas_textures/default_texture_filter")
            .As<Viewport.DefaultCanvasItemTextureFilter>();

        switch (defaultMode)
        {
            case Viewport.DefaultCanvasItemTextureFilter.Nearest:
                return CanvasItem.TextureFilterEnum.Nearest;
            case Viewport.DefaultCanvasItemTextureFilter.Linear:
                return CanvasItem.TextureFilterEnum.Linear;
            case Viewport.DefaultCanvasItemTextureFilter.LinearWithMipmaps:
                return CanvasItem.TextureFilterEnum.LinearWithMipmaps;
            case Viewport.DefaultCanvasItemTextureFilter.NearestWithMipmaps:
                return CanvasItem.TextureFilterEnum.NearestWithMipmaps;
            case Viewport.DefaultCanvasItemTextureFilter.Max:
                break;
        }

        return CanvasItem.TextureFilterEnum.Linear;
    }

    public static BaseMaterial3D.TextureFilterEnum Convert2DTo3DTextureFilterEnum(CanvasItem.TextureFilterEnum input)
    {
        switch (input)
        {
            case CanvasItem.TextureFilterEnum.ParentNode:
                return BaseMaterial3D.TextureFilterEnum.Linear;
            case CanvasItem.TextureFilterEnum.Nearest:
                return BaseMaterial3D.TextureFilterEnum.Nearest;
            case CanvasItem.TextureFilterEnum.Linear:
                return BaseMaterial3D.TextureFilterEnum.Linear;
            case CanvasItem.TextureFilterEnum.NearestWithMipmaps:
                return BaseMaterial3D.TextureFilterEnum.NearestWithMipmaps;
            case CanvasItem.TextureFilterEnum.LinearWithMipmaps:
                return BaseMaterial3D.TextureFilterEnum.LinearWithMipmaps;
            case CanvasItem.TextureFilterEnum.NearestWithMipmapsAnisotropic:
                return BaseMaterial3D.TextureFilterEnum.NearestWithMipmapsAnisotropic;
            case CanvasItem.TextureFilterEnum.LinearWithMipmapsAnisotropic:
                return BaseMaterial3D.TextureFilterEnum.LinearWithMipmapsAnisotropic;
            case CanvasItem.TextureFilterEnum.Max:
                return BaseMaterial3D.TextureFilterEnum.Max;
            default:
                throw new ArgumentOutOfRangeException(nameof(input), input, null);
        }
    }
}