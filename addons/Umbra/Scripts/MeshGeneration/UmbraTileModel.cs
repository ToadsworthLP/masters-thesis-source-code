using Godot;

namespace Umbra.MeshGeneration;

[GlobalClass]
[Tool]
public partial class UmbraTileModel : Resource
{
    [Export] public Mesh SourceShadowMesh
    {
        get => sourceShadowMesh;
        set
        {
            sourceShadowMesh = value;
            xFlippedVisibleMesh = null;
            zFlippedVisibleMesh = null;
            xzFlippedVisibleMesh = null;
        }
    }

    [Export] public Mesh SourceVisibleMesh
    {
        get => sourceVisibleMesh;
        set
        {
            sourceVisibleMesh = value;
            xFlippedShadowMesh = null;
            zFlippedShadowMesh = null;
            xzFlippedShadowMesh = null;
        }
    }
    
    [Export] public bool FlipH;
    [Export] public bool FlipV;
    [Export] public bool Fill = true;

    private Mesh sourceVisibleMesh;
    private Mesh sourceShadowMesh;
    private Mesh xFlippedVisibleMesh;
    private Mesh xFlippedShadowMesh;
    private Mesh zFlippedVisibleMesh;
    private Mesh zFlippedShadowMesh;
    private Mesh xzFlippedVisibleMesh;
    private Mesh xzFlippedShadowMesh;
    
    public Mesh VisibleMesh
    {
        get
        {
            if (SourceVisibleMesh == null) return null;
            
            if (FlipH && !FlipV)
            {
                if (xFlippedVisibleMesh == null)
                    xFlippedVisibleMesh = MeshTransformer.FlipX(SourceVisibleMesh);

                return xFlippedVisibleMesh;
            }
            
            if (!FlipH && FlipV)
            {
                if (zFlippedVisibleMesh == null)
                    zFlippedVisibleMesh = MeshTransformer.FlipZ(SourceVisibleMesh);

                return zFlippedVisibleMesh;
            }
            
            if (FlipH && FlipV)
            {
                if (xzFlippedVisibleMesh == null)
                    xzFlippedVisibleMesh = MeshTransformer.FlipXZ(SourceVisibleMesh);

                return xzFlippedVisibleMesh;
            }

            return SourceVisibleMesh;
        }
    }
    
    public Mesh ShadowMesh
    {
        get
        {
            if (SourceShadowMesh == null) return null;
            
            if (FlipH && !FlipV)
            {
                if (xFlippedShadowMesh == null)
                    xFlippedShadowMesh = MeshTransformer.FlipX(SourceShadowMesh);

                return xFlippedShadowMesh;
            }
            
            if (!FlipH && FlipV)
            {
                if (zFlippedShadowMesh == null)
                    zFlippedShadowMesh = MeshTransformer.FlipZ(SourceShadowMesh);

                return zFlippedShadowMesh;
            }
            
            if (FlipH && FlipV)
            {
                if (xzFlippedShadowMesh == null)
                    xzFlippedShadowMesh = MeshTransformer.FlipXZ(SourceShadowMesh);

                return xzFlippedShadowMesh;
            }

            return SourceShadowMesh;
        }
    }
}