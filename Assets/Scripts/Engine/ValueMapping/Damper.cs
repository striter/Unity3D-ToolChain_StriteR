using System;
using System.ComponentModel;
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
    Constant,
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
    [Foldout(nameof(mode), EDamperMode.Constant)] public float constant;
    [Foldout(nameof(mode),EDamperMode.Lerp,EDamperMode.SpringCritical)][Clamp(0.01f)] public float halfLife;
    [Foldout(nameof(mode),EDamperMode.SpringSimple,EDamperMode.SpringImplicit)][Range(0,200)] public float stiffness;
    [Foldout(nameof(mode),EDamperMode.SpringSimple,EDamperMode.SpringImplicit)][Range(0,30)] public float damping;
    [Foldout(nameof(mode), EDamperMode.SecondOrderDynamics)] public SecondOrderDynamics secondOrderDynamics;
    
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
        
        var val = value;
        var vel = velocity;
        switch (mode)
        {
            case EDamperMode.Never: break;
            case EDamperMode.Instant:
            {
                velocity = 0;
                value = xd;
            } break;
            case EDamperMode.Constant:
            {
                var sign = math.sign(xd - value);
                velocity = sign * constant;
                value += velocity * dt;
            }
                break;
            case EDamperMode.Lerp: {
                var nextValue = lerp(value,xd, 1.0f - negExp_Fast( dt*0.69314718056f /(halfLife+float.Epsilon)));
                velocity = nextValue - value;
                value = nextValue;
            } break;
            case EDamperMode.SpringSimple: {
                Damping.SpringSimple(dt,s,d,ref val,ref vel,xd,vd,eps);
            } break;
            case EDamperMode.SpringCritical: {
                Damping.SpringCritical(dt,HalfLife2Damping(halfLife),ref val,ref vel,xd,vd,eps);
            } break;
            case EDamperMode.SpringImplicit: {
                Damping.SpringImplicit(_deltaTime,s,d,ref val,ref vel,xd,vd,eps);
            } break;
            case EDamperMode.SecondOrderDynamics: {
                secondOrderDynamics.Evaluate(ref val,ref vel,_deltaTime,_desire,_desireVelocity);
            } break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        value = val;
        velocity = vel;
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
                case EDamperMode.Never:
                    return -1;
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
