using System;
using Godot;

namespace Umbra.Nodes;

[GlobalClass]
[Tool]
public partial class UmbraRoot : Node3D
{
    [Export] public int PixelsPerMeter;

    private static readonly float Sqrt2 = MathF.Sqrt(2);
    
    public UmbraRoot()
    {
        Scale = new Vector3(1, Sqrt2, Sqrt2);
    }
}