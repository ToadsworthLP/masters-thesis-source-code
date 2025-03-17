using System;
using System.Collections.Generic;
using Godot;
using Umbra.Nodes;

namespace Umbra;

public static class CompanionClassMap
{
    private static IDictionary<Type, Type> companionClasses;

    public static bool HasCompanionType(Type type)
    {
        return companionClasses.ContainsKey(type);
    }

    public static Type GetCompanionType(Type type)
    {
        if (companionClasses.TryGetValue(type, out var result))
        {
            return result;
        }

        return null;
    }
    
    private static void Add<TClass, TCompanion>()
    {
        companionClasses.Add(typeof(TClass), typeof(TCompanion));
    }

    static CompanionClassMap()
    {
        companionClasses = new Dictionary<Type, Type>();
        
        Add<Sprite2D, UmbraSprite3D>();
        Add<AnimatedSprite2D, UmbraAnimatedSprite3D>();
        Add<TileMapLayer, UmbraTileMapLayer3D>();
        Add<Camera2D, UmbraCamera3D>();
        Add<Node2D, UmbraNode3D>();
    }
}