using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Unity.Mathematics;

namespace Runtime.Geometry
{
    [Serializable]
    public struct G2Polygon : IShape2D, IConvex2D
    {
        public float2[] positions;
        [NonSerialized] public float2 center;
        public G2Polygon(IEnumerable<float2> _positions) : this(_positions.ToArray()) { }
        public G2Polygon(params float2[] _positions)
        {
            positions = _positions;
            center = _positions.Average();
        }


        public float2 GetSupportPoint(float2 _direction)
        {
            var center = this.center;
            return positions.MaxElement(_p => math.dot(_direction, _p - center));
        }
        public float2 Origin => center;
        public static readonly G2Polygon kZero = new G2Polygon();
        public static readonly G2Polygon kDefault = new G2Polygon(kfloat2.up,kfloat2.right,kfloat2.down,kfloat2.left);
        public static G2Polygon operator +(G2Polygon _polygon,float2 _offset) => new G2Polygon(_polygon.positions.Select(p=>p + _offset));
        public float2 this[int _value] => positions[_value];
        public int Count => positions.Length;
        public IEnumerator<float2> GetEnumerator()
        {
            foreach (var point in positions)
                yield return point;
        }

        public IEnumerable<G2Line> GetEdges()
        {
            for (int i = 0; i < positions.Length-1; i++)
                yield return new G2Line(positions[i], positions[i + 1]);
        }

        public IEnumerable<G2Triangle> GetTriangles(IEnumerable<PTriangle> _triangleIndexexs)
        {
            foreach (var indexes in _triangleIndexexs)
                yield return new G2Triangle(positions[indexes.V0], positions[indexes.V1], positions[indexes.V2]);
        }
        
        IEnumerator IEnumerable.GetEnumerator()=> GetEnumerator();
        public void DrawGizmos()
        {
            if (positions == null || positions.Length == 0)
                return;
            UGizmos.DrawLinesConcat(positions.Select(p=>p.to3xz()));
        } 
    }

    public static class G2Polygon_Extension
    {
        private static readonly List<float2> kTempPool = new List<float2>();
        public static bool DoorClip(this G2Polygon _polygon,G2Plane _plane,out G2Polygon _clippedPolygon)    //Convex and Counter Clockwise is needed
        {
            var cliped = false;
            kTempPool.Clear();
            kTempPool.AddRange(_polygon);
            for (int i = _polygon.Count - 1; i >=0; i--)
            {
                var curPoint = _polygon[i];
                var nextPoint = _polygon[(i + 1) % _polygon.Count];
                var curDot = _plane.dot(curPoint);
                var nextDot = _plane.dot(nextPoint);
                var curForward = curDot > 0;
                var nextForward = nextDot > 0;

                if (curForward && !nextForward)
                {
                    var t = curDot / math.dot(_plane.normal,(curPoint-nextPoint));
                    var insertPoint = math.lerp(curPoint, nextPoint, t);
                    kTempPool.Insert(i + 1, insertPoint);
                    cliped = true;
                }
                else if (!curForward && !nextForward)
                {
                    kTempPool.RemoveAt(i);
                    cliped = true;
                }
            }
            _clippedPolygon = new G2Polygon(kTempPool);
            return cliped;
        }

    }
}