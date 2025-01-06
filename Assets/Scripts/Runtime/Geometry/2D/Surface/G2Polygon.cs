using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry.Extension;
using Unity.Mathematics;

namespace Runtime.Geometry
{
    [Serializable]
    public struct G2Polygon : IGeometry2, IConvex2
    {
        public List<float2> positions;
        [NonSerialized] public float2 center;
        public G2Polygon(IEnumerable<float2> _positions) : this(_positions.ToList()) { }
        public G2Polygon(params float2[] _positions) : this(_positions.ToList()) { }
        public G2Polygon(List<float2> _positions)
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
        public static readonly G2Polygon kZero = new();
        public static readonly G2Polygon kDefault = new(kfloat2.up,kfloat2.right,kfloat2.down,kfloat2.left);
        public static readonly G2Polygon kDefaultUV = new(new float2(0,0),new float2(0,1),new float2(1,1),new float2(1,0));
        public static readonly G2Polygon kBunny = new G2Polygon(
            new (0.098f, 0.062f), new (0.352f, 0.073f), new (0.422f, 0.136f), new (0.371f, 0.085f), new (0.449f, 0.140f),
            new (0.352f, 0.187f), new (0.379f, 0.202f), new (0.398f, 0.202f), new (0.266f, 0.198f), new (0.318f, 0.345f),
            new (0.402f, 0.359f), new (0.361f, 0.425f), new (0.371f, 0.521f), new (0.410f, 0.491f), new (0.410f, 0.357f),
            new (0.502f, 0.482f), new (0.529f, 0.435f), new (0.426f, 0.343f), new (0.449f, 0.343f), new (0.504f, 0.335f),
            new (0.664f, 0.355f), new (0.748f, 0.208f), new (0.738f, 0.277f), new (0.787f, 0.308f), new (0.748f, 0.183f),
            new (0.623f, 0.081f), new (0.557f, 0.099f), new (0.648f, 0.116f), new (0.598f, 0.116f), new (0.566f, 0.195f),
            new (0.584f, 0.228f), new (0.508f, 0.083f), new (0.457f, 0.140f), new (0.508f, 0.130f), new (0.625f, 0.071f),
            new (0.818f, 0.093f), new (0.951f, 0.066f), new (0.547f, 0.081f) ) - .5f;
                    
        public static G2Polygon operator +(G2Polygon _polygon,float2 _offset) => new(_polygon.positions.Select(p=>p + _offset));
        public static G2Polygon operator -(G2Polygon _polygon,float2 _offset) => new(_polygon.positions.Select(p=>p - _offset));
        public static implicit operator G2Polygon(List<float2> _positions) => new(_positions);
        public float2 this[int _value] => positions[_value];
        public int Count => positions.Count;
        public IEnumerator<float2> GetEnumerator() => positions.GetEnumerator();

        public IEnumerable<G2Triangle> GetTriangles(IEnumerable<PTriangle> _triangleIndexexs)
        {
            foreach (var indexes in _triangleIndexexs)
                yield return new G2Triangle(positions[indexes.V0], positions[indexes.V1], positions[indexes.V2]);
        }
        
        IEnumerator IEnumerable.GetEnumerator()=> GetEnumerator();
        public void DrawGizmos()
        {
            if (positions == null || positions.Count == 0)
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

        public static IEnumerable<int> GetIndexes(this G2Polygon _polygon)
        {
            for (var i = 1; i < _polygon.Count-1; i++)
            {
                yield return 0;
                yield return i;
                yield return i+1;
            }
        }
        
    }
}