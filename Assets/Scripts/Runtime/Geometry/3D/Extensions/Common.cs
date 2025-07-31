using System.Collections.Generic;
using Unity.Mathematics;

namespace Runtime.Geometry.Extension
{
    public static partial class UGeometry
    {
        public static void Minmax(IEnumerable<float3> _positions,out float3 _min,out float3 _max)
        {
            _min = float.MaxValue;
            _max = float.MinValue;
            foreach (var position in _positions)
            {
                _min = math.min(position, _min);
                _max = math.max(position, _max);
            }
        }
    }
}