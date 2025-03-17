using Godot;

namespace Umbra.Nodes;

public interface IUmbraCompanion
{
    public void SetCompanion(Node target);
    public Node GetCompanion();
}