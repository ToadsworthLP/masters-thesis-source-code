using Godot;

namespace Umbra.demo.Scripts;

public partial class AnimationPlayerSeek : AnimationPlayer
{
    [Export] private string animation;
    [Export] private float time;

    public override void _Ready()
    {
        Play(animation, customSpeed: 0f);
        Seek(time, true);
    }
}