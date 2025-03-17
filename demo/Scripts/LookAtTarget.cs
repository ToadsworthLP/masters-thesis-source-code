using Godot;

[Tool]
public partial class LookAtTarget : Node3D
{
    [Export] public Node3D Target;
    
    public override void _Process(double delta)
    {
        if(Target != null) LookAt(Target.GlobalPosition);
    }
}
