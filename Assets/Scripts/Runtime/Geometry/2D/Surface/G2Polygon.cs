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
    public partial struct G2Polygon
    {
        public List<float2> positions;
        [NonSerialized] public float2 center;
        public G2Polygon(IEnumerable<float2> _positions) : this(_positions.ToList()) { }
        public G2Polygon(params float2[] _positions) : this(_positions.ToList()) { }
        private G2Polygon(List<float2> _positions)
        {
            positions = _positions.ToList();
            center = _positions.Average();
        }
    }

}