using System;
using System.ComponentModel;
using System.Linq;
using Rendering.GI.SphericalHarmonics;
using Unity.Mathematics;
using UnityEngine;

namespace Examples.Rendering.Lighting
{
    public enum ELightType
    {
        Directional = 0,
        Point = 1,
        Spot = 2,
        Line = 3,
    }
    
    [Serializable]
    public struct Light
    {
        public ELightType type;
        [ColorUsage(false,false)] public Color color;
        [Clamp(0.01f)]public float intensity;
        [MFold(nameof(type),ELightType.Directional)] [Clamp(0)] public float constant,linear,quadric;
        [MFoldout(nameof(type),ELightType.Spot)] public float spotPower;
        [MFold(nameof(type),ELightType.Directional)] public float3 position;
        [MFold(nameof(type),ELightType.Point,ELightType.Line)] public float3 euler;

        [MFoldout(nameof(type), ELightType.Line)] public float3 endPosition;
        
        public static readonly Light kDefaultPoint = new Light()
            {
                type = ELightType.Point,
                intensity = 1,
                position = float3.zero,
                euler = kfloat3.forward,
                color = Color.white,
                endPosition = kfloat3.forward,
                constant = 1f, linear = 0f, quadric = 0f
            };

        public LightBufferElement Convert() => new LightBufferElement()
        {
            type = (uint)type,
            position = position,
            direction = type== ELightType.Line ? endPosition: umath.EulerToQuaternion(euler).mul(kfloat3.forward),
            color = color.to3() * intensity,
            lightParameters = new float4( constant, linear, quadric,spotPower),
        };
    }

    public struct LightBufferElement
    {
        public uint type;
        public float3 position;
        public float3 direction;
        public float3 color;
        public float4 lightParameters;
        public static readonly int kSize = sizeof(uint) + sizeof(float) * 13;
    }
    
    [ExecuteInEditMode]
    public class Lighting : MonoBehaviour
    {
        public SHGradient m_SH;
        public Light[] m_Lights = new Light[]{Light.kDefaultPoint,};
        
        private ComputeBuffer m_LightBuffer;

        internal static class ShaderProperties
        {
            public static readonly int kLights = Shader.PropertyToID("_LightArray");
            public static readonly int kLightCount = Shader.PropertyToID("_LightCount");
        }

        void Init()
        {
            if (m_LightBuffer != null)
                return;
            m_LightBuffer = new ComputeBuffer(m_Lights.Length,LightBufferElement.kSize);
        }

        void Release()
        {
            if (m_LightBuffer == null)
                return;
            m_LightBuffer.Release();
            m_LightBuffer = null;
        }

        private void Awake() => Init();
        private void OnDestroy() => Release();

        private void OnValidate()
        {
            Release();
            Init();
        }

        private void Update()
        {
            var lightBuffers = m_Lights.Select(p => p.Convert()).ToArray();
            m_LightBuffer.SetData(lightBuffers);
            Shader.SetGlobalBuffer(ShaderProperties.kLights,m_LightBuffer);
            Shader.SetGlobalInt(ShaderProperties.kLightCount,m_Lights.Length);
            SHL2ShaderProperties.kDefault.ApplyGlobal(m_SH.Ctor().shData);
        }

        private void OnDrawGizmos()
        {
            foreach (var light in m_Lights)
            {
                Gizmos.color = light.color;
                float3 position = light.position;
                var rotation = umath.EulerToQuaternion(light.euler);
                switch (light.type)
                {
                    default:throw new InvalidEnumArgumentException();
                    case ELightType.Directional: UGizmos.DrawArrow(Vector3.zero,rotation.mul(kfloat3.forward),1f,.1f); break;
                    case ELightType.Point: Gizmos.DrawWireSphere(position,.1f); break;
                    case ELightType.Spot: UGizmos.DrawArrow(position,rotation.mul(kfloat3.forward),1f,.1f); break;
                    case ELightType.Line: Gizmos.DrawLine(position,light.endPosition); break;
                }
            }
        }
    }   
}
