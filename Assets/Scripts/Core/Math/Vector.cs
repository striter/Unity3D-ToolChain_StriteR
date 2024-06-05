using Unity.Mathematics;
public static partial class umath
{
    public static float3 GetXZLookDirection(float3 _startPoint, float3 _endPoint)
    {
        float3 lookDirection = _endPoint - _startPoint;
        lookDirection.y = 0;
        lookDirection.normalize();
        return lookDirection;
    }
    
    // public static float dot(float3 _src, float3 _dst) => _src.x * _dst.x + _src.y * _dst.y + _src.z * _dst.z;
    // public static float3 cross(float3 _src, float3 _dst) => new float3(_src.y * _dst.z - _src.z * _dst.y, _src.z * _dst.x - _src.x * _dst.z, _src.x * _dst.y - _src.y * _dst.x);
    
    // public static float dot(float2 _src, float2 _dst) => _src.x * _dst.x + _src.y * _dst.y;
    public static float cross(float2 _src, float2 _dst) => _src.x * _dst.y - _src.y * _dst.x;
    public static bool isParallel(float3 a, float3 b,float _tolerence = 0.99f) => math.abs(a.x * b.x + a.y * b.y + a.z * b.z) > _tolerence; // Tolerance to consider parallel
    public static float3 calculatePerpendicular(float3 a, float3 b) => a - b * math.dot(a, b) / math.dot(b, b);
}
