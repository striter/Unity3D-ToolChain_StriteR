using System.Collections.Generic;
using Unity.Mathematics;

namespace Runtime.Geometry
{

    public interface IShape2D : IShapeDimension<float2>
    {
        float2 GetSupportPoint(float2 _direction);
    }
    
}