using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry.Extension.BoundingSphere;
using Runtime.Pool;
using Unity.Mathematics;

namespace Runtime.Geometry.Extension
{
    public static partial class UGeometry
    {
        static void Minmax(IEnumerable<float2> _positions,out float2 _min,out float2 _max)
        {
            _min = float.MaxValue;
            _max = float.MinValue;
            foreach (var position in _positions)
            {
                _min = math.min(position, _min);
                _max = math.max(position, _max);
            }
        }
        public static G2Box GetBoundingBox(IEnumerable<float2> _positions)
        {
            Minmax(_positions,out var min,out var max);
            return G2Box.Minmax(min,max);
        }
        
        public static G2Box GetBoundingBox<T>(IList<T> _elements, Func<T, float2> _convert)
        {
            var min = kfloat2.max;
            var max = kfloat2.min;
            for(var i = _elements.Count - 1; i>=0;i--)
            {
                var position = _convert( _elements[i]);
                min = math.min(position, min);
                max = math.max(position, max);
            }

            return G2Box.Minmax(min,max);
        }
        
        public static G2Triangle GetSuperTriangle(params float2[] _positions)     //always includes,but not minimum
        {
            Minmax(_positions,out var min,out var max);
            var delta = (max - min);
            return new G2Triangle(
                new float2(min.x - delta.x,min.y - delta.y * 3f),
                new float2(min.x - delta.x,max.y + delta.y),
                new float2(max.x + delta.x*3f,max.y + delta.y)
            );
        }
        
        public static G2Circle GetBoundingCircle(IList<float2> _positions) => EPOS._2D.Evaluate(_positions, EPOS._2D.EMode.EPOS8,Welzl<G2Circle, float2>.Evaluate);
    }
}