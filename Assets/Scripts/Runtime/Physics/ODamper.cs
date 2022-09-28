using System;
using OSwizzling;
using UnityEngine;
using static UMath;
using static UDamper;

public enum EDamperMode
{
    SpringSimple,
    SpringCritical,
    SpringImplicit,
    Lerp,
}

[Serializable]
public class Damper
{
    public EDamperMode mode = EDamperMode.SpringSimple;
    [MFoldout(nameof(mode),EDamperMode.Lerp,EDamperMode.SpringCritical)][Clamp(0.05f)] public float halfLife = .5f;
    [MFoldout(nameof(mode),EDamperMode.SpringSimple,EDamperMode.SpringImplicit)][Range(0,100)] public float stiffness = 20f;
    [MFoldout(nameof(mode),EDamperMode.SpringSimple,EDamperMode.SpringImplicit)][Range(0,30)] public float damping = 4f;
    
    public Vector3 position { get; private set; }
    private Vector3 velocity;

    public void Begin(Vector3 _begin)
    {
        position = _begin;
        velocity = Vector3.zero;
    }

    public Vector3 Tick(float _deltaTime,Vector3 _desirePosition,Vector3 _desireVelocity = default)
    {
        var x = position;
        var g = _desirePosition;
        var v = velocity;
        var q = _desireVelocity;
        var d = damping;
        var s = stiffness;
        var dt = _deltaTime;
        
        switch (mode)
        {
            case EDamperMode.Lerp: {
                position = Vector3.Lerp(position,_desirePosition, 1.0f - NegExp_Fast( _deltaTime*0.69314718056f /(halfLife+float.Epsilon)));
            } break;
            case EDamperMode.SpringSimple:
            {
                velocity += _deltaTime * s * ( g - x) + _deltaTime * d * ( q - v);
                position += _deltaTime * v;
            } break;
            case EDamperMode.SpringCritical:
            {
                d = HalfLife2Damping(halfLife);
                var c = g + (d * q) / (d*d/4.0f);
                var y = d / 2.0f;
                
                var j0 = x - c;
                var j1 = v + j0 * y;
                var eydt = NegExp_Fast(y * dt);
                position = eydt * (j0 + j1 * dt) + c;
                velocity = eydt * (v - j1 * y * dt);
            } break;
            case EDamperMode.SpringImplicit:
            {
                var determination = s - d * d / 4.0f;
                var c = g + (d * q) / (s + eps);
                var y = d * .5f;
                if (Mathf.Abs(determination) < eps)     //Critical Damped
                {
                    var j0 = x - c;
                    var j1 = v + j0 * y;
                    float eydt = NegExp_Fast(y * dt);
                    position = eydt * (j0 + j1 * dt) + c;
                    velocity = eydt * (v - j1 * y * dt);
                }
                else if (determination > 0)     //Under Damped
                {
                    var w = Mathf.Sqrt(s - (d * d) / 4.0f);
                    var j =  ((v + y * (x - c)).Convert(Square) / (w * w + eps) + (x - c).Convert(Square)).sqrt();
                    var p =  (v+(x-c)*y).div(-(x - c) * w + eps3).Convert(Atan_Fast);
                    
                    j = j.Convert((index,value)=>(x[index]-value)>0?value:-value);
                    
                    var param = (w * dt).ToVector3() + p;
                    var cosParam = param.Convert(Cos);
                    var sinParam = param.Convert(Sin);
                    var eydtJ = j *  NegExp_Fast(y * dt);
                    
                    position = eydtJ.mul (cosParam) + c;
                    velocity = -y * eydtJ.mul(cosParam) - w *eydtJ.mul(sinParam);
                }
                else    //Over Damped
                {
                    var param = Mathf.Sqrt(d * d - 4 * s);
                    var y0 = (d + param) / 2.0f;
                    var y1 = (d - param) / 2.0f;
                    var j1 = (c * y0 - x * y0 - v) / (y1 - y0);
                    var j0 = x - j1 - c;
                    var ey0dt = NegExp_Fast(y0 * dt);
                    var ey1dt = NegExp_Fast(y1 * dt);

                    position = j0 * ey0dt + j1 * ey1dt + c;
                    velocity = -y0 * j0 * ey0dt - y1 * j1 * ey1dt;
                }
            } break;
        }
        return position;
    }
}

public static class UDamper
{
    public static readonly float eps = 1e-5f;
    public static readonly Vector3 eps3 = new Vector3(eps,eps,eps);
    public static float HalfLife2Damping(float _halfLife)=> (4.0f * 0.69314718056f) / (_halfLife + eps);
    public static float Damping2HalfLife(float _damping) => (4.0f * 0.69314718056f) / (_damping + eps);

    public static float Frequency2Stiffness(float _frequency) => Square(kPIM2*_frequency);
    public static float Stiffness2Frequency(float _stiffness) => Mathf.Sqrt(_stiffness) / kPIM2;
}
