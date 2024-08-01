using System.Collections.Generic;
using Unity.Mathematics;

namespace Runtime.Geometry
{

    public interface IGeometry2 : IGeometry<float2>
    {
        float2 GetSupportPoint(float2 _direction);
    }

    public interface IArea : IGeometry2
    {
        
    }
    
    public interface IConvex2D : IGeometry2, IEnumerable<float2>
    {
        public IEnumerable<G2Line> GetEdges();
    }

}