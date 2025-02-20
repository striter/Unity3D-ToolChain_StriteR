using System.Collections.Generic;
using Unity.Mathematics;

namespace Runtime.Geometry.Curves.Spline
{
    public interface ISpline : ICurve
    {
        public const int kMinDegree = 2;
        IEnumerable<float3> Coordinates { get; }
    }

}