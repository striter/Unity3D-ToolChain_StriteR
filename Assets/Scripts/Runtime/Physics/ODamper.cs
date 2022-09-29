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
    SecondOrderDynamics,
}

[Serializable]
public class Damper : ISerializationCallbackReceiver
{
    public EDamperMode mode = EDamperMode.SpringSimple;
    [MFoldout(nameof(mode),EDamperMode.Lerp,EDamperMode.SpringCritical)][Clamp(0.05f)] public float halfLife = .5f;
    [MFoldout(nameof(mode),EDamperMode.SpringSimple,EDamperMode.SpringImplicit)][Range(0,100)] public float stiffness = 20f;
    [MFoldout(nameof(mode),EDamperMode.SpringSimple,EDamperMode.SpringImplicit)][Range(0,30)] public float damping = 4f;

    [MFoldout(nameof(mode), EDamperMode.SecondOrderDynamics)] [Range(0.01f, 20)] public float f = 5;
    [MFoldout(nameof(mode), EDamperMode.SecondOrderDynamics)] [Range(0, 1.5f)] public float z = 0.35f;
    [MFoldout(nameof(mode), EDamperMode.SecondOrderDynamics)] [Range(-5, 5)] public float r = -.5f;
    public Vector3 x { get; private set; }
    private Vector3 v;
    
    private Vector3 xp;
    private float w,_d,k1, k2, k3;
    
    public void Initialize(Vector3 _begin)
    {
        xp = _begin;
        x = _begin;
        v = Vector3.zero;
        Ctor();
    }

    void Ctor()
    {
        w = 2 * kPI * f;
        _d = w * Mathf.Sqrt(Mathf.Abs(z*z-1));
        k1 = z / (kPI * f);
        k2 = 1 / Square(w);
        k3 = r * z / (w);
    }

    public Vector3 Tick(float _deltaTime,Vector3 _desirePosition,Vector3 _desireVelocity = default)
    {
        if (_deltaTime == 0)
            return x;
        
        var xd = _desirePosition;
        var vd = _desireVelocity;
        
        var d = damping;
        var s = stiffness;
        var dt = _deltaTime;
        
        switch (mode)
        {
            case EDamperMode.Lerp: {
                x = Vector3.Lerp(x,xd, 1.0f - NegExp_Fast( dt*0.69314718056f /(halfLife+float.Epsilon)));
            } break;
            case EDamperMode.SpringSimple:
            {
                v += _deltaTime * s * ( xd - x) + _deltaTime * d * ( vd - v);
                x += _deltaTime * v;
            } break;
            case EDamperMode.SpringCritical:
            {
                d = HalfLife2Damping(halfLife);
                var c = xd + (d * vd) / (d*d/4.0f);
                var y = d / 2.0f;
                
                var j0 = x - c;
                var j1 = v + j0 * y;
                var eydt = NegExp_Fast(y * dt);
                x = eydt * (j0 + j1 * dt) + c;
                v = eydt * (v - j1 * y * dt);
            } break;
            case EDamperMode.SpringImplicit:
            {
                var determination = s - d * d / 4.0f;
                var c = xd + (d * vd) / (s + eps);
                var y = d * .5f;
                if (Mathf.Abs(determination) < eps)     //Critical Damped
                {
                    var j0 = x - c;
                    var j1 = v + j0 * y;
                    float eydt = NegExp_Fast(y * dt);
                    x = eydt * (j0 + j1 * dt) + c;
                    v = eydt * (v - j1 * y * dt);
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
                    
                    x = eydtJ.mul (cosParam) + c;
                    v = -y * eydtJ.mul(cosParam) - w *eydtJ.mul(sinParam);
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

                    x = j0 * ey0dt + j1 * ey1dt + c;
                    v = -y0 * j0 * ey0dt - y1 * j1 * ey1dt;
                }
            } break;
            case EDamperMode.SecondOrderDynamics:
            {
                if (vd == Vector3.zero)
                {
                    vd = (xd - xp) / dt;
                    xp = xd;
                }

                float k1Stable, k2Stable;
                if (w * dt < z)
                {
                    k1Stable = k1;
                    k2Stable = Mathf.Max(k2,dt*dt/2 + dt*k1/2,dt*k1);
                }
                else
                {
                    float t1 = Mathf.Exp(-z * w * dt);
                    float alpha = 2 * t1 * (z <= 1 ? Mathf.Cos(dt * _d) : UMath.CosH(dt * _d));
                    float beta = Square(t1);
                    float t2 = dt / (1 + beta - alpha);
                    k1Stable = (1 - beta) * t2;
                    k2Stable = dt * t2;
                }
                
                x += v * dt;
                v += dt * (xd + k3 * vd - x - k1Stable * v) / k2Stable;
            } break;
        }

        return x;
    }

    public void OnBeforeSerialize(){}
    public void OnAfterDeserialize() => Ctor();
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
