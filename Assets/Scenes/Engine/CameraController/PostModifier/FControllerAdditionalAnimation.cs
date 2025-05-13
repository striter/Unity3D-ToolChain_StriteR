using System;
using System.Collections.Generic;
using System.Linq;
using CameraController.Inputs;
using Runtime.Geometry.Extension.Sphere;
using Unity.Mathematics;
using UnityEngine;

namespace CameraController.Animation
{
    [CreateAssetMenu(fileName = "Animation", menuName = "Camera/PostModifier/AdditionalAnimation")]
    public class FControllerAdditionalAnimation : AControllerPostModifer
    {
        public FControllerAdditionalAnimationData data = FControllerAdditionalAnimationData.kShake;
        public override bool Disposable(bool _reset) => data.Disposable(_reset);
        public override EControllerPostModiferQueue Queue => data.Queue;
        public override void Tick(float _deltaTime, AControllerInput _input, ref FCameraControllerOutput _output) => data.Tick(_deltaTime, _input, ref _output);
        public override void OnBegin(FCameraControllerCore _input) => data.OnBegin(_input);
        public override void OnFinished() => data.OnFinished();
        public override void DrawGizmos(AControllerInput _input) => data.DrawGizmos(_input);
        public override float timeExists {
            get => data.timeExists;
            set => data.timeExists = value;
        }
    }

    [Serializable]
    public struct FControllerAdditionalAnimationData : IControllerPostModifer
    {
        public FCameraAdditionalAnimationData data;
        public AnimationCurve curve;
        public bool random;
        
        [Foldout(nameof(random),true)] public FLDSNoise noise;
        public static readonly FControllerAdditionalAnimationData kEmpty = new(){data = FCameraAdditionalAnimationData.kEmpty, curve = AnimationCurve.Linear(0f,1f,1f,1f),noise = FLDSNoise.kDefault};
        public static readonly FControllerAdditionalAnimationData kShake = new(){data = new(){viewPortDeltaX = 0.05f,viewPortDeltaY = 0.05f}, curve = new AnimationCurve(
            new (0, 0, 0.0f, 0.333f), new (0.25f, 1, 0.333f, 0.333f), new (0.75f, 1, 0.333f, 0.333f), new (1, 0, 0.333f, 0f)),
            noise = FLDSNoise.kDefault,random = true};
        public EControllerPostModiferQueue Queue => EControllerPostModiferQueue.Additional;
        public bool Disposable(bool _reset) => _reset || timeExists > curve.length;
        public float timeExists { get; set; }
        public void OnBegin(FCameraControllerCore _input){}
        public void Tick(float _deltaTime,AControllerInput _input, ref FCameraControllerOutput _output)
        {
            var finalData = data * curve.Evaluate(timeExists);

            if (random)
                finalData = noise.Evaluate(finalData,timeExists);

            _output.distance += finalData.distance;
            _output.anchor += finalData.offset;
            _output.euler += finalData.euler;
            _output.fov += finalData.fov;
            _output.viewPort += new float2(finalData.viewPortDeltaX, finalData.viewPortDeltaY);
        }
        public void OnFinished() { }
        public void DrawGizmos(AControllerInput _input) { }
    }
    
    [Serializable]
    public struct FCameraAdditionalAnimationData
    {
        public float3 offset;
        public float3 euler;
        public float distance;
        public float fov;
        [Range(-.5f,.5f)] public float viewPortDeltaX;
        [Range(-.5f,.5f)] public float viewPortDeltaY;
        public static FCameraAdditionalAnimationData kEmpty = new FCameraAdditionalAnimationData();

        public static FCameraAdditionalAnimationData operator +(FCameraAdditionalAnimationData a, FCameraAdditionalAnimationData b) => new()
        {
            offset = a.offset + b.offset,
            euler = a.euler + b.euler,
            distance = a.distance + b.distance,
            fov = a.fov + b.fov,
            viewPortDeltaX = a.viewPortDeltaX + b.viewPortDeltaX,
            viewPortDeltaY = a.viewPortDeltaY + b.viewPortDeltaY
        };

        public static FCameraAdditionalAnimationData operator -(FCameraAdditionalAnimationData a, FCameraAdditionalAnimationData b) => new()
        {
            offset = a.offset - b.offset,
            euler = a.euler - b.euler,
            distance = a.distance - b.distance,
            fov = a.fov - b.fov,
            viewPortDeltaX = a.viewPortDeltaX - b.viewPortDeltaX,
            viewPortDeltaY = a.viewPortDeltaY - b.viewPortDeltaY
        };

        public static FCameraAdditionalAnimationData operator *(FCameraAdditionalAnimationData a, float b) => new()
        {
            offset = a.offset * b,
            euler = a.euler * b,
            distance = a.distance * b,
            fov = a.fov * b,
            viewPortDeltaX = a.viewPortDeltaX * b,
            viewPortDeltaY = a.viewPortDeltaY * b
        };
        public static FCameraAdditionalAnimationData Interpolate(FCameraAdditionalAnimationData a, FCameraAdditionalAnimationData b, float t) => a * (1 - t) + b * t;
    }
    
    internal struct FrequencyData
    {
        public float[] sobel; 
        public float2[] sobel2;
        public float[] direction1;
        public float2[] direction2;
        public float3[] direction3;
        public static float2 GetCircle(float _value)
        {
            var angle = _value * math.PI * 2;
            return new float2(math.cos(angle), math.sin(angle));
        }

        public FrequencyData(uint frequency)
        {
            sobel = ULowDiscrepancySequences.Sobel(frequency);
            sobel2 = ULowDiscrepancySequences.Sobol2D(frequency);
            direction1 = sobel.Select(p => math.sign(p - .5f)).ToArray();
            direction2 = sobel.Select(GetCircle).ToArray();
            direction3 = sobel2.Select(ConcentricOctahedral.kDefault.ToPosition).ToArray();
        }
    }

    [Serializable]
    public struct FLDSNoise
    {
        private const int kMaxFrequency = 60; //60 shake per second
        private static Dictionary<uint, FrequencyData> kFrequencies = new();

        private static FrequencyData GetFrequencies(uint _frequency)
        {
            if (kFrequencies.TryGetValue(_frequency, out var value))
                return value;
            value = new FrequencyData(_frequency);
            kFrequencies.Add(_frequency, value);
            return value;
        }

        [Range(0f, kMaxFrequency)] public int sineFrequency;
        public static FLDSNoise kDefault = new() { sineFrequency = kMaxFrequency };

        public FCameraAdditionalAnimationData Evaluate(FCameraAdditionalAnimationData _control, float _time)
        {
            if (sineFrequency <= 0)
                return _control;

            var frequencyData = GetFrequencies((uint)sineFrequency);
            var waveIndex = (int)((_time % 1f) * sineFrequency);
            Func<int, float> random1D = (offset) =>
                frequencyData.direction1[(offset + waveIndex) % frequencyData.direction1.Length];
            var random2D = frequencyData.direction2[waveIndex % frequencyData.direction2.Length];
            var random3D = frequencyData.direction3[waveIndex % frequencyData.direction3.Length];
            _control = new FCameraAdditionalAnimationData()
            {
                distance = _control.distance * random1D(0),
                fov = _control.fov * random1D(40),
                euler = _control.euler * random3D,
                offset = _control.offset * random3D,
                viewPortDeltaX = _control.viewPortDeltaX * random2D.x,
                viewPortDeltaY = _control.viewPortDeltaY * random2D.y,
            };

            var sineValue = math.sin(math.PI * _time * sineFrequency * 2);
            return _control * sineValue;
        }
    }

}