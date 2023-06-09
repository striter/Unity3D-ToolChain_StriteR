using Geometry;
using Unity.Mathematics;


public static partial class umath
{
    public static float dot(this G2Plane _plane, float2 _point) => math.dot(_point.to3xy(-1), _plane);
        
}
