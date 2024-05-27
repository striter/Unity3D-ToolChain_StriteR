using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry.Extension;
using Unity.Mathematics;


namespace Runtime.Geometry
{
    public struct GPointSets : IVolume
    {
        public float3[] vertices;
        public GPointSets(IEnumerable<float3> _vertices):this(_vertices.ToArray()){}
        public GPointSets(float3[] _vertices)
        {
            vertices = _vertices;
            Center = default;
            Ctor();
        }

        void Ctor()
        {
            Center = vertices.Average();
        }

        public float3 Center { get; private set; }
        public float3 GetSupportPoint(float3 _direction)=> vertices.MaxElement(_p => math.dot(_direction, _p));
        public GBox GetBoundingBox() => UGeometry.GetBoundingBox(vertices);
        public GSphere GetBoundingSphere() => UGeometry.GetBoundingSphere(vertices);
    }
}