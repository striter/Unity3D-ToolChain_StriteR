using System;
using System.ComponentModel;
using Runtime.Physics;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using static umath;
using static kmath;
using static UDamper;
using quaternion = Unity.Mathematics.quaternion;

public enum EDamperMode
{
    Never = -1,
    Instant,
    SpringSimple,
    SpringCritical,
    SpringImplicit,
    Lerp,
    SecondOrderDynamics,
}

[Serializable]
public struct Damper
{
    [Header("Config")]
    public EDamperMode mode;
    [MFoldout(nameof(mode),EDamperMode.Lerp,EDamperMode.SpringCritical)][Clamp(0.01f)] public float halfLife;
    [MFoldout(nameof(mode),EDamperMode.SpringSimple,EDamperMode.SpringImplicit)][Range(0,200)] public float stiffness;
    [MFoldout(nameof(mode),EDamperMode.SpringSimple,EDamperMode.SpringImplicit)][Range(0,30)] public float damping;
    [MFoldout(nameof(mode), EDamperMode.SecondOrderDynamics)] public SecondOrderDynamics secondOrderDynamics;
    
    public static Damper kDefault => new() {mode = EDamperMode.SpringCritical, halfLife = .1f, stiffness = 20f, damping = 4f, secondOrderDynamics = SecondOrderDynamics.kDefault};
    
    public float lifeTime { get; private set; }
    public float4 value { get; private set; }
    public float4 velocity { get; private set; }
    public void Initialize(float _begin) => Initialize((float4)_begin);
    public void Initialize(float2 _begin) => Initialize(_begin.to4());
    public void Initialize(float3 _begin) => Initialize(_begin.to4());
    public void Initialize(float4 _begin)
    {
        secondOrderDynamics.Initialize(_begin);
        value = _begin;
        velocity = 0;
        lifeTime = 0f;
    }

    public void InitializeAngle(float3 _begin) => Initialize(quaternion.Euler(_begin * kDeg2Rad));
    public float3 TickAngle(float _deltaTime, float3 _desire) => Tick(_deltaTime,quaternion.Euler(_desire * kDeg2Rad)).toEulerAngles();

    public void Initialize(quaternion _begin) => Initialize(_begin.value);
    public quaternion Tick(float _deltaTime, quaternion _target)
    {
        var dot = math.dot(value, _target.value);
        var tq = dot < 0 ? -_target.value: _target.value;
        return new quaternion(Tick(_deltaTime, tq));
    }
    public float Tick(float _deltaTime, float _desire, float _desireVelocity = default) => Tick(_deltaTime,(float4)_desire,_desireVelocity).x;
    public float2 Tick(float _deltaTime, float2 _desire, float2 _desireVelocity = default) => Tick(_deltaTime, _desire.to4(),_desireVelocity.to4()).xy;
    public float3 Tick(float _deltaTime, float3 _desire, float3 _desireVelocity = default) => Tick(_deltaTime, _desire.to4(),_desireVelocity.to4()).xyz;
    public float4 Tick(float _deltaTime,float4 _desire,float4 _desireVelocity = default)
    {
        if (_deltaTime == 0) return value;
        lifeTime += _deltaTime;
        
        var xd = _desire;
        var vd = _desireVelocity;

        var d = damping;
        var s = stiffness;
        var dt = _deltaTime;
        
        switch (mode)
        {
            case EDamperMode.Never: break;
            case EDamperMode.Instant:
            {
                velocity = 0;
                value = xd;
            } break;
            case EDamperMode.Lerp: {
                var nextValue = lerp(value,xd, 1.0f - negExp_Fast( dt*0.69314718056f /(halfLife+float.Epsilon)));
                velocity = nextValue - value;
                value = nextValue;
            } break;
            case EDamperMode.SpringSimple:
            {
                velocity += _deltaTime * s * ( xd - value) + _deltaTime * d * ( vd - velocity);
                value += _deltaTime * velocity;
            } break;
            case EDamperMode.SpringCritical:
            {
                d = HalfLife2Damping(halfLife);
                var c = xd + (d * vd) / (d*d/4.0f);
                var y = d / 2.0f;
                
                var j0 = value - c;
                var j1 = velocity + j0 * y;
                var eydt = negExp_Fast(y * dt);
                value = eydt * (j0 + j1 * dt) + c;
                velocity = eydt * (velocity - j1 * y * dt);
            } break;
            case EDamperMode.SpringImplicit:
            {
                var determination = s - d * d / 4.0f;
                var c = xd + (d * vd) / (s + eps);
                var y = d * .5f;
                if (abs(determination) < eps)     //Critical Damped
                {
                    var j0 = value - c;
                    var j1 = velocity + j0 * y;
                    var eydt = negExp_Fast(y * dt);
                    value = eydt * (j0 + j1 * dt) + c;
                    velocity = eydt * (-y*j0 - y * j1 * dt+ j1);
                }
                else if (determination > 0)     //Under Damped
                {
                    var w = sqrt(determination);
                    var j = sqr(velocity + y*(value - c)) / (w*w + eps) + sqr(value - c);
                    j = sqrt(j);
                    
                    var p = ((velocity + (value - c) * y)/(-(value - c) * w + eps4)).convert(atan_Fast);

                    var srcValue = value;
                    j = j.convert((_i,_value)=>(srcValue[_i]-c[_i])>0?_value:-_value);
                    
                    var jeydt = j*negExp_Fast(y * dt );
                    
                    var param = (w * dt) + p;
                    var cosParam = cos(param);
                    var sinParam = sin(param);
                    
                    value = jeydt*(cosParam) + c;
                    velocity = -y * jeydt*cosParam - w * jeydt*sinParam;
                }
                else    //Over Damped
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
            } break;
            case EDamperMode.SecondOrderDynamics:
            {
                var val = value;
                var vel = velocity;
                secondOrderDynamics.Evaluate(ref val,ref vel,_deltaTime,_desire,_desireVelocity);
                value = val;
                velocity = vel;
            } break;
        }
        return value;
    }

    public float duration
    {
        get
        {
            switch (mode)
            {
                default:
                    throw new InvalidEnumArgumentException();
                case EDamperMode.Instant:
                    return 0;
                case EDamperMode.Lerp:
                case EDamperMode.SpringCritical:
                    return halfLife;
                case EDamperMode.SpringImplicit:
                case EDamperMode.SpringSimple:
                    return Damping2HalfLife(damping);
                case EDamperMode.SecondOrderDynamics:
                     return secondOrderDynamics.duration;
            }
        }
    }

    public bool Working(float _desire,float _sqrTolerance = float.Epsilon) => pow2(value.x - _desire) < _sqrTolerance && velocity.sqrmagnitude() < _sqrTolerance;
    public bool Working(float2 _desire,float _sqrTolerance = float.Epsilon) => (value.xy - _desire).sqrmagnitude() < _sqrTolerance && velocity.sqrmagnitude() < _sqrTolerance;
    public bool Working(float3 _desire,float _sqrTolerance = float.Epsilon) => (value.xyz - _desire).sqrmagnitude() < _sqrTolerance && velocity.sqrmagnitude() < _sqrTolerance;
    public bool Working(float4 _desire,float _sqrTolerance = float.Epsilon) => (value - _desire).sqrmagnitude() < _sqrTolerance && velocity.sqrmagnitude() < _sqrTolerance;
}


public static class UDamper
{
    public static readonly float eps = 1e-5f;
    public static readonly float4 eps4 = eps;
    public static float HalfLife2Damping(float _halfLife)=> (4.0f * 0.69314718056f) / (_halfLife + eps);
    public static float Damping2HalfLife(float _damping) => (4.0f * 0.69314718056f) / (_damping + eps);
    public static float Frequency2Stiffness(float _frequency) => sqr(kPI2*_frequency);
    public static float Stiffness2Frequency(float _stiffness) => sqrt(_stiffness) / kPI2;
}
