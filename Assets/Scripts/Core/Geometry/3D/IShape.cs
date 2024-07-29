using System.Collections.Generic;
using Unity.Mathematics;

namespace Runtime.Geometry
{
    public interface IShape  : IShape<float3> { }

    public interface ILine : IShape
    {
        
    }

    public interface ISurface : IShape
    {
        public float3 Normal { get; }
    }
    
    public interface IVolume : IShape
    {
        float3 GetSupportPoint(float3 _direction);
        public GBox GetBoundingBox();
        public GSphere GetBoundingSphere();
    }
    
    public interface IConvex : IShape , IEnumerable<float3>
    {
        public IEnumerable<GLine> GetEdges();
        public IEnumerable<float3> GetAxes();
    }
}