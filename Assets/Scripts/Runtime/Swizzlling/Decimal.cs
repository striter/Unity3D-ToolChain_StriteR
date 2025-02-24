using System;
using Unity.Mathematics;

namespace Runtime.Swizzlling
{
    public static class FDecimal
    {
        private static readonly floatDecimal kFloatHelper = new floatDecimal();
        private static readonly float2Decimal kFloat2Helper = new float2Decimal();
        private static readonly float3Decimal kFloat3Helper = new float3Decimal();
        private static readonly float4Decimal kFloat4Helper = new float4Decimal();
        private static IDecimal GetDecimalHelper_Internal<T>()
        {
            var type = typeof(T);
            return type switch
            {
                not null when type == floatDecimal.kType => kFloatHelper,
                not null when type == float2Decimal.kType => kFloat2Helper,
                not null when type == float3Decimal.kType => kFloat3Helper,
                not null when type == float4Decimal.kType => kFloat4Helper,
                _ => null
            };
        }
        public static IDecimal<T> Helper<T>() => GetDecimalHelper_Internal<T>() as IDecimal<T>;
    }

    public interface IDecimal { }
    public interface IDecimal<T> : IDecimal
    {
        public T add(T _a, T _b);
        public T sub(T _a, T _b);
        public T mul(T _a, T _b);
        public T mul(T _a, float _b);
        public T div(T _a, T _b);
        public T div(T _a, float _b);
        public T pow(T _a, float _value);
        public T sqrt(T _a);
        public float distance(T _a, T _b);
        public float length(T _a);
        public T kOne { get; }
        public T kZero { get; }
    }

    public struct floatDecimal : IDecimal<float>
    {
        public static Type kType => typeof(float);
        public float add(float _a, float _b) => _a + _b;
        public float sub(float _a, float _b) => _a - _b;
        public float mul(float _a, float _b) => _a * _b;
        public float div(float _a, float _b) => _a / _b;
        public float distance(float _a, float _b) => math.distance(_a, _b);
        public float length(float _a) => math.length(_a);
        public float pow(float _a, float _value) => math.pow(_a, _value);
        public float sqrt(float _a) => math.sqrt(_a);
        public float kOne => 1f;
        public float kZero => 0f;
    }

    public struct float2Decimal : IDecimal<float2>
    {
        public static Type kType => typeof(float2);
        public float2 add(float2 _a, float2 _b) => _a + _b;
        public float2 sub(float2 _a, float2 _b) => _a - _b;
        public float2 mul(float2 _a, float2 _b) => _a * _b;
        public float2 mul(float2 _a, float _b) => _a * _b;
        public float2 div(float2 _a, float2 _b) => _a / _b;
        public float2 div(float2 _a, float _b) => _a / _b;
        public float distance(float2 _a, float2 _b) => math.distance(_a, _b);
        public float length(float2 _a) => math.length(_a);
        public float2 pow(float2 _a, float _value) => math.pow(_a, _value);
        public float2 sqrt(float2 _a) => math.sqrt(_a);
        public float2 kOne => kfloat2.one;
        public float2 kZero => kfloat2.zero;
    }

    public struct float3Decimal : IDecimal<float3>
    {
        public static Type kType => typeof(float3);
        public float3 add(float3 _a, float3 _b) => _a + _b;
        public float3 sub(float3 _a, float3 _b) => _a - _b;
        public float3 mul(float3 _a, float3 _b) => _a * _b;
        public float3 mul(float3 _a, float _b) => _a * _b;
        public float3 div(float3 _a, float3 _b) => _a / _b;
        public float3 div(float3 _a, float _b) => _a / _b;
        public float distance(float3 _a, float3 _b) => math.distance(_a, _b);
        public float length(float3 _a) => math.length(_a);
        public float3 pow(float3 _a, float _value) => math.pow(_a, _value);
        public float3 sqrt(float3 _a) => math.sqrt(_a);
        public float3 kOne => kfloat3.one;
        public float3 kZero => kfloat3.zero;
    }

    public struct float4Decimal : IDecimal<float4>
    {
        public static Type kType => typeof(float4);
        public float4 add(float4 _a, float4 _b) => _a + _b;
        public float4 sub(float4 _a, float4 _b) => _a - _b;
        public float4 mul(float4 _a, float4 _b) => _a * _b;
        public float4 mul(float4 _a, float _b) => _a * _b;
        public float4 div(float4 _a, float4 _b) => _a / _b;
        public float4 div(float4 _a, float _b) => _a / _b;
        public float distance(float4 _a, float4 _b) => math.distance(_a, _b);
        public float length(float4 _a) => math.length(_a);
        public float4 pow(float4 _a, float _value) => math.pow(_a, _value);
        public float4 sqrt(float4 _a) => math.sqrt(_a);
        public float4 kOne => kfloat4.one;
        public float4 kZero => kfloat4.zero;
    }
}