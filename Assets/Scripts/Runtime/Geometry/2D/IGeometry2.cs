using System.Collections.Generic;
using Unity.Mathematics;

namespace Runtime.Geometry
{
    public interface IGeometry2 : IGeometry<float2>
    {
        float2 GetSupportPoint(float2 _direction);
    }

    public interface IArea2
    {
        float GetArea();
    }
    
    public interface IConvex2 : IGeometry2 , IEnumerable<float2> { }
}