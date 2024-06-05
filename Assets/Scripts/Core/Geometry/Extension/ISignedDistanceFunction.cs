using Unity.Mathematics;

namespace Runtime.Geometry.Extension
{
    public interface ISDF : IVolume
    {
        public float SDF(float3 _position);
    }

    public interface ISDF2D : IArea
    {
        public float SDF(float2 _position);
    }
    
    public static class ISDF_Extension
    {
        public static bool Contains(this ISDF _sdf, float3 _position,float _epsilon = 0) => _sdf.SDF(_position) <= _epsilon;
        public static float Distance(this ISDF _sdf,float3 _position) => _sdf.SDF(_position);
        
        public static bool Contains(this ISDF2D _sdf, float2 _position,float _epsilon = 0) => _sdf.SDF(_position) <= _epsilon;
        public static float Distance(this ISDF2D _sdf,float2 _position) => _sdf.SDF(_position);
    }
}