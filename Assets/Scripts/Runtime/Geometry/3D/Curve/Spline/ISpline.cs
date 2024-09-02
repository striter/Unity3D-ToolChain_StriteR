using System.Collections.Generic;
using Unity.Mathematics;

namespace Runtime.Geometry.Curves.Spline
{
    public interface ISpline : ICurve
    {
        IEnumerable<float3> Coordinates { get; }
    }

}