using System.Collections.Generic;
using Unity.Mathematics;

namespace Runtime.Geometry
{
    public interface IShape3D : IShapeDimension<float3> 
    {
        float3 GetSupportPoint(float3 _direction);
    }

    public interface IBoundingBox3D : IShape3D
    {
        public GBox GetBoundingBox();
    }

    public interface IBoundingSphere3D : IShape3D
    {
        public GSphere GetBoundingSphere();
    }

    public interface IConvex3D : IShape3D, IEnumerable<float3>
    {
        public IEnumerable<GLine> GetEdges();
        public IEnumerable<float3> GetAxes();
    }
}