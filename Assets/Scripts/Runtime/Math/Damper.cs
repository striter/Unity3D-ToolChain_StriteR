using System;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using static umath;
using static kmath;
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
    [MFoldout(nameof(mode),EDamperMode.SpringSimple,EDamperMode.SpringImplicit)][Range(0,200)] public float stiffness = 20f;
    [MFoldout(nameof(mode),EDamperMode.SpringSimple,EDamperMode.SpringImplicit)][Range(0,30)] public float damping = 4f;
    
    [MFoldout(nameof(mode), EDamperMode.SecondOrderDynamics)] [Range(0.01f, 20)] public float f = 5;
    [MFoldout(nameof(mode), EDamperMode.SecondOrderDynamics)] [Range(0, 1.5f)] public float z = 0.35f;
    [MFoldout(nameof(mode), EDamperMode.SecondOrderDynamics)] [Range(-5, 5)] public float r = -.5f;
    [MFoldout(nameof(mode), EDamperMode.SecondOrderDynamics)] public bool poleMatching;

    public float4 value { get; private set; }
    private float4 v;
    
    //Used by second order dynamics
    private float4 _xp;
    private float _w,_d,_k1, _k2, _k3;

    public void Initialize(quaternion _begin) => Initialize(_begin.value);
    public void Initialize(float _begin) => Initialize((float4)_begin);
    public void Initialize(float2 _begin) => Initialize(_begin.to4());
    public void Initialize(float3 _begin) => Initialize(_begin.to4());
    public void Initialize(float4 _begin)
    {
        _xp = _begin;
        value = _begin;
        v = 0;
        Ctor();
    }

    void Ctor()
    {
        _w = 2 * kPI * f;
        _d = _w * sqrt(abs(z*z-1));
        _k1 = z / (kPI * f);
        _k2 = 1 / sqr(_w);
        _k3 = r * z / (_w);
    }


    public float TickAngle(float _deltaTime, float _desire)
    {
        _desire = value.x + deltaAngle(value.x,_desire);
        return Tick(_deltaTime,(float4)_desire).x;
    }
    public float2 TickAngle(float _deltaTime, float2 _desire)
    {
        _desire = value.xy + deltaAngle(value.xy,_desire);
        return Tick(_deltaTime,_desire.to4()).xy;
    }

    public quaternion Tick(float _deltaTime, quaternion _target)
    {
        var dot = math.dot(value, _target.value);
        var tq = dot < 0 ? -_target.value: _target.value;
        return new quaternion(Tick(_deltaTime, tq));
    }
    public float2 Tick(float _deltaTime, float _desire, float _desireVelocity = default) => Tick(_deltaTime,(float4)_desire,(_desireVelocity)).x;
    public float2 Tick(float _deltaTime, float2 _desire, float2 _desireVelocity = default) => Tick(_deltaTime, _desire.to4(),_desireVelocity.to4()).xy;
    public float3 Tick(float _deltaTime, float3 _desire, float3 _desireVelocity = default) => Tick(_deltaTime, _desire.to4(),_desireVelocity.to4()).xyz;

    public float4 Tick(float _deltaTime,float4 _desire,float4 _desireVelocity = default)
    {
        if (_deltaTime == 0) return value;
        
        var xd = _desire;
        var vd = _desireVelocity;

        var d = damping;
        var s = stiffness;
        var dt = _deltaTime;
        
        switch (mode)
        {
            case EDamperMode.None:
            {
                value = xd;
            } break;
            case EDamperMode.Lerp: {
                value = lerp(value,xd, 1.0f - negExp_Fast( dt*0.69314718056f /(halfLife+float.Epsilon)));
            } break;
            case EDamperMode.SpringSimple:
            {
                v += _deltaTime * s * ( xd - value) + _deltaTime * d * ( vd - v);
                value += _deltaTime * v;
            } break;
            case EDamperMode.SpringCritical:
            {
                d = HalfLife2Damping(halfLife);
                var c = xd + (d * vd) / (d*d/4.0f);
                var y = d / 2.0f;
                
                var j0 = value - c;
                var j1 = v + j0 * y;
                var eydt = negExp_Fast(y * dt);
                value = eydt * (j0 + j1 * dt) + c;
                v = eydt * (v - j1 * y * dt);
            } break;
            case EDamperMode.SpringImplicit:
            {
                var determination = s - d * d / 4.0f;
                var c = xd + (d * vd) / (s + eps);
                var y = d * .5f;
                if (abs(determination) < eps)     //Critical Damped
                {
                    var j0 = value - c;
                    var j1 = v + j0 * y;
                    var eydt = negExp_Fast(y * dt);
                    value = eydt * (j0 + j1 * dt) + c;
                    v = eydt * (-y*j0 - y * j1 * dt+ j1);
                }
                else if (determination > 0)     //Under Damped
                {
                    var w = sqrt(determination);
                    var j = sqr(v + y*(value - c)) / (w*w + eps) + sqr(value - c);
                    j = sqrt(j);
                    
                    var p = ((v + (value - c) * y)/(-(value - c) * w + eps4)).convert(atan_Fast);
                    
                    j = j.convert((_i,_value)=>(value[_i]-c[_i])>0?_value:-_value);
                    
                    var jeydt = j*negExp_Fast(y * dt );
                    
                    var param = (w * dt) + p;
                    var cosParam = cos(param);
                    var sinParam = sin(param);
                    
                    value = jeydt*(cosParam) + c;
                    v = -y * jeydt*cosParam - w * jeydt*sinParam;
                }
                else    //Over Damped
                {
                    var param = sqrt(d * d - 4 * s);
                    var y0 = (d + param) / 2.0f;
                    var y1 = (d - param) / 2.0f;
                    var j1 = (c * y0 - value * y0 - v) / (y1 - y0);
                    var j0 = value - j1 - c;
                    var ey0dt = negExp_Fast(y0 * dt);
                    var ey1dt = negExp_Fast(y1 * dt);

                    value = j0 * ey0dt + j1 * ey1dt + c;
                    v = -y0 * j0 * ey0dt - y1 * j1 * ey1dt;
                }
            } break;
            case EDamperMode.SecondOrderDynamics:
            {
                if (vd.sqrmagnitude() < float.Epsilon)
                {
                    vd = (xd - _xp) / dt;
                    _xp = xd;
                }

                float k1Stable, k2Stable;
                if (!poleMatching || _w * dt < z)
                {
                    k1Stable = _k1;
                    k2Stable = max(_k2,max(dt*dt/2 + dt*_k1/2,dt*_k1));
                }
                else
                {
                    float t1 = exp(-z * _w * dt);
                    float alpha = 2 * t1 * (z <= 1 ? cos(dt * _d) : cosH(dt * _d));
                    float beta = sqr(t1);
                    float t2 = dt / (1 + beta - alpha);
                    k1Stable = (1 - beta) * t2;
                    k2Stable = dt * t2;
                }
                
                value += v * dt;
                v += dt * (xd + _k3 * vd - value - k1Stable * v) / k2Stable;
            } break;
        }
        return value;
    }

    public void OnBeforeSerialize(){}
    public void OnAfterDeserialize() => Ctor();
}

public static class UDamper
{
    public static readonly float eps = 1e-5f;
    public static readonly float4 eps4 = eps;
    public static float HalfLife2Damping(float _halfLife)=> (4.0f * 0.69314718056f) / (_halfLife + eps);
    public static float Damping2HalfLife(float _damping) => (4.0f * 0.69314718056f) / (_damping + eps);

    public static float Frequency2Stiffness(float _frequency) => sqr(kPIMul2*_frequency);
    public static float Stiffness2Frequency(float _stiffness) => sqrt(_stiffness) / kPIMul2;
}
