using System.Collections.Generic;
using Unity.Mathematics;

namespace Runtime.Geometry
{

    public interface IShape2D : IShape<float2>
    {
        float2 GetSupportPoint(float2 _direction);
    }

    public interface IArea : IShape2D
    {
        
    }
    
    public interface IConvex2D : IShape2D, IEnumerable<float2>
    {
        public IEnumerable<G2Line> GetEdges();
    }
    
}