using Unity.Mathematics;

namespace Runtime.Geometry.Extension
{
    public interface ISDF:ISDF<float3>{}
    
    
    public static class ISDF_Extension
    {
        public static bool Contains<T>(this ISDF<T> _sdf, T _position,float _epsilon = 0) where T:struct => _sdf.SDF(_position) <= _epsilon;
        public static float Distance<T>(this ISDF<T> _sdf,T _position) where T:struct => _sdf.SDF(_position);
    }
}