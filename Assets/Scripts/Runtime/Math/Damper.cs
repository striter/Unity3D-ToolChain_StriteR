using System;
using UnityEngine;
using static UMath;
using static KMath;
using static UDamper;

public enum EDamperMode
{
    None,
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
    [MFoldout(nameof(mode),EDamperMode.Lerp,EDamperMode.SpringCritical)][Clamp(0.01f)] public float halfLife = .5f;
    [MFoldout(nameof(mode),EDamperMode.SpringSimple,EDamperMode.SpringImplicit)][Range(0,100)] public float stiffness = 20f;
    [MFoldout(nameof(mode),EDamperMode.SpringSimple,EDamperMode.SpringImplicit)][Range(0,30)] public float damping = 4f;
    
    [MFoldout(nameof(mode), EDamperMode.SecondOrderDynamics)] [Range(0.01f, 20)] public float f = 5;
    [MFoldout(nameof(mode), EDamperMode.SecondOrderDynamics)] [Range(0, 1.5f)] public float z = 0.35f;
    [MFoldout(nameof(mode), EDamperMode.SecondOrderDynamics)] [Range(-5, 5)] public float r = -.5f;
    [MFoldout(nameof(mode), EDamperMode.SecondOrderDynamics)] public bool poleMatching;
    
    public Vector3 x { get; private set; }
    private Vector3 v;
    
    //Used by second order dynamics
    private Vector3 _xp;
    private float _w,_d,_k1, _k2, _k3;
    
    public void Initialize(Vector3 _begin)
    {
        _xp = _begin;
        x = _begin;
        v = Vector3.zero;
        Ctor();
    }

    void Ctor()
    {
        _w = 2 * kPI * f;
        _d = _w * Mathf.Sqrt(Mathf.Abs(z*z-1));
        _k1 = z / (kPI * f);
        _k2 = 1 / Square(_w);
        _k3 = r * z / (_w);
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
            case EDamperMode.None:
            {
                x = xd;
            } break;
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
                    v = eydt * (-y*j0 - y * j1 * dt+ j1);
                }
                else if (determination > 0)     //Under Damped
                {
                    var w = Mathf.Sqrt(determination);
                    var j = ((v + y*(x - c)).square() / (w*w + eps) + (x - c).square()).sqrt();
                    var p = (v + (x - c) * y).div(-(x - c) * w + eps3).Convert(Atan_Fast);
                    
                    j = j.Convert((index,value)=>(x[index]-c[index])>0?value:-value);
                    
                    var jeydt = j*NegExp_Fast(y * dt );
                    
                    var param = (w * dt).ToVector3() + p;
                    var cosParam = param.Convert(Cos);
                    var sinParam = param.Convert(Sin);
                    
                    x = jeydt.mul(cosParam) + c;
                    v = -y * jeydt.mul(cosParam) - w * jeydt.mul(sinParam);
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
                    vd = (xd - _xp) / dt;
                    _xp = xd;
                }

                float k1Stable, k2Stable;
                if (!poleMatching || _w * dt < z)
                {
                    k1Stable = _k1;
                    k2Stable = Mathf.Max(_k2,dt*dt/2 + dt*_k1/2,dt*_k1);
                }
                else
                {
                    float t1 = Mathf.Exp(-z * _w * dt);
                    float alpha = 2 * t1 * (z <= 1 ? Mathf.Cos(dt * this._d) : UMath.CosH(dt * this._d));
                    float beta = Square(t1);
                    float t2 = dt / (1 + beta - alpha);
                    k1Stable = (1 - beta) * t2;
                    k2Stable = dt * t2;
                }
                
                x += v * dt;
                v += dt * (xd + _k3 * vd - x - k1Stable * v) / k2Stable;
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

    public static float Frequency2Stiffness(float _frequency) => Square(kPI2*_frequency);
    public static float Stiffness2Frequency(float _stiffness) => Mathf.Sqrt(_stiffness) / kPI2;
}
