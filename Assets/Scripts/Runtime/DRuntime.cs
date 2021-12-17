using UnityEngine;

public static class DRuntime
{
    public static readonly string kDataPersistentPath = Application.persistentDataPath + "/Save/";
}

public static class KRect
{
    public static readonly Rect kRect01 = new Rect(Vector2.zero, Vector2.one);
}

namespace Geometry
{
    public partial class KQuad
    {
        public static readonly Quad<Vector3> V3_Square55 = new Quad<Vector3>( Vector3.right+Vector3.back,Vector3.back+Vector3.left, Vector3.left+Vector3.forward ,Vector3.forward+Vector3.right).Resize(.5f);
        public static readonly Quad<Vector3> V3_Square00 = new Quad<Vector3>(Vector3.zero,Vector3.forward,Vector3.forward+Vector3.right,Vector3.right);
        public static readonly Quad<Vector3> V3_Square55_45Deg = new Quad<Vector3>(Vector3.back,Vector3.left,Vector3.forward,Vector3.right);
        
        public static readonly Quad<Vector2> V2_Square55 = V3_Square55.Convert(p=>new Vector2(p.x,p.z));
    }
}

namespace Geometry.Voxel
{
    public partial class KQube
    {
        public static readonly Qube<Vector3> unitQube = KQuad.V3_Square55.ExpandToQube(Vector3.up,0f);
        public static readonly Qube<Vector3> halfUnitQube = unitQube.Resize(.5f);
    }

    public partial class KCubeFacing
    {
        public static readonly CubeFacing<Vector3> V3_UnitFacing = new CubeFacing<Vector3>(Vector3.back*.5f,Vector3.left*.5f,Vector3.forward*.5f,Vector3.right*.5f,Vector3.up*.5f,Vector3.down*.5f);
    }
}
