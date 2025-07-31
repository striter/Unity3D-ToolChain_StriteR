using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;


namespace Runtime.Geometry
{
    public struct GPointSets : IVolume
    {
        public List<float3> vertices;
        public GPointSets(IEnumerable<float3> _vertices):this(_vertices.ToList()){}
        public GPointSets(List<float3> _vertices)
        {
            vertices = _vertices;
            Origin = default;
            Ctor();
        }

        void Ctor()
        {
            Origin = vertices.Average();
        }

        public float3 Origin { get; private set; }
        public float3 GetSupportPoint(float3 _direction)=> vertices.MaxElement(_p => math.dot(_direction, _p));
        public GBox GetBoundingBox() => GBox.GetBoundingBox(vertices);
        public GSphere GetBoundingSphere() => GSphere.GetBoundingSphere(vertices);
        
        public void DrawGizmos()
        {
            foreach (var vertex in vertices)
                Gizmos.DrawSphere(vertex,.01f);
            // DrawLinesConcat(_points.vertices);
        }
    }
}