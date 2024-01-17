using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TObjectPool;
using Unity.Mathematics;

namespace Runtime.Geometry
{
    [Serializable]
    public struct G2Polygon : IShape2D , IEnumerable<float2>
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
        public float2 Center => center;
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
        IEnumerator IEnumerable.GetEnumerator()=> GetEnumerator();
    }

    public static class G2Polygon_Extension
    {
        public static bool DoorClip(this G2Polygon _polygon,G2Plane _plane,out G2Polygon _clippedPolygon)    //Convex and Counter Clockwise is needed
        {
            bool cliped = false;
            TSPoolList<float2>.Spawn(out var clippedPolygon);
            clippedPolygon.AddRange(_polygon);
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
                    clippedPolygon.Insert(i + 1, insertPoint);
                    cliped = true;
                }
                else if (!curForward && !nextForward)
                {
                    clippedPolygon.RemoveAt(i);
                    cliped = true;
                }
            }
            _clippedPolygon = new G2Polygon(clippedPolygon);
            TSPoolList<float2>.Recycle(clippedPolygon);
            return cliped;
        }

    }
}