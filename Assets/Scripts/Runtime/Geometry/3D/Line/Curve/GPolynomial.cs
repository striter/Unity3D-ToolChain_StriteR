using System;
using Unity.Mathematics;

namespace Runtime.Geometry.Curves       //Leave it
{
    [Serializable]
    public struct GPolynomialCurve : ICurve
    {
        public float3 constant, linear, quadratic, cubic;
        public GPolynomialCurve(float3 _constant,float3 _linear,float3 _quadratic,float3 _cubic)
        {
            constant = _constant;
            linear = _linear;
            quadratic = _quadratic;
            cubic = _cubic;
        }

        public float3 Evaluate(float _t) => constant + linear * _t + quadratic * umath.pow2(_t) + cubic * umath.pow3(_t);
        public float3 Origin => constant;
        public void DrawGizmos() => this.DrawGizmos(64);
    }
}