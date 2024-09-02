using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Unity.Mathematics;
namespace Runtime.Geometry.Extension
{
    public class PCA2
    {
        public static void Evaluate(IList<float2> _points,out float2 _centre, out float2 _R, out float2 _S)
        {
            _R = kfloat2.up;
            _S = kfloat2.right;

            _centre = _points.Average();
            var m = _centre;
            var a00 = _points.Average(p => umath.pow2(p.x - m.x));
            var a11 = _points.Average(p => umath.pow2(p.y - m.y));
            var a01mirror = _points.Average(p => (p.x - m.x)*(p.y-m.y));
            new float2x2(a00,a01mirror,a01mirror,a11).GetEigenVectors(out _R,out _S);
        }
    }
}
