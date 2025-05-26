using Unity.Mathematics;
using static umath;
using static Unity.Mathematics.math;

public static class Damping
{
    public static void SpringSimple(float dt,float s, float d,ref float4 value,ref float4 velocity, float4 xd, float4 vd, float eps = float.Epsilon)
    {
        velocity += dt * s * ( xd - value) + dt * d * ( vd - velocity);
        value += dt * velocity;
    }
    public static void SpringCritical(float dt,float d,ref float4 value,ref float4 velocity, float4 xd, float4 vd, float eps = float.Epsilon)
    {
        var c = xd + (d * vd) / (d*d/4.0f);
        var y = d / 2.0f;
            
        var j0 = value - c;
        var j1 = velocity + j0 * y;
        var eydt = negExp_Fast(y * dt);
        value = eydt * (j0 + j1 * dt) + c;
        velocity = eydt * (velocity - j1 * y * dt);
    }
    public static void SpringImplicit(float dt,float s, float d,ref float4 value,ref float4 velocity, float4 xd, float4 vd, float eps = float.Epsilon)
    {
        var determination = s - d * d / 4.0f;
        var c = xd + (d * vd) / (s + eps);
        var y = d * .5f;
        if (abs(determination) < eps) //Critical Damped
        {
            var j0 = value - c;
            var j1 = velocity + j0 * y;
            var eydt = negExp_Fast(y * dt);
            value = eydt * (j0 + j1 * dt) + c;
            velocity = eydt * (-y * j0 - y * j1 * dt + j1);
        }
        else if (determination > 0) //Under Damped
        {
            var w = sqrt(determination);
            var j = sqr(velocity + y * (value - c)) / (w * w + eps) + sqr(value - c);
            j = sqrt(j);

            var p = ((velocity + (value - c) * y) / (-(value - c) * w + eps)).convert(atan_Fast);

            var srcValue = value;
            j = j.convert((_i, _value) => (srcValue[_i] - c[_i]) > 0 ? _value : -_value);

            var jeydt = j * negExp_Fast(y * dt);

            var param = (w * dt) + p;
            var cosParam = cos(param);
            var sinParam = sin(param);

            value = jeydt * (cosParam) + c;
            velocity = -y * jeydt * cosParam - w * jeydt * sinParam;
        }
        else //Over Damped
        {
            var param = sqrt(d * d - 4 * s);
            var y0 = (d + param) / 2.0f;
            var y1 = (d - param) / 2.0f;
            var j1 = (c * y0 - value * y0 - velocity) / (y1 - y0);
            var j0 = value - j1 - c;
            var ey0dt = negExp_Fast(y0 * dt);
            var ey1dt = negExp_Fast(y1 * dt);

            value = j0 * ey0dt + j1 * ey1dt + c;
            velocity = -y0 * j0 * ey0dt - y1 * j1 * ey1dt;
        }
    }
}
