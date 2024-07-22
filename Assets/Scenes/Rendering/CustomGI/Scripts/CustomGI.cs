using System;
using Rendering.GI.SphericalHarmonics;
using Rendering.Lightmap;
using Unity.Mathematics;
using UnityEngine;
using System.Linq.Extensions;

namespace Examples.Rendering.GI.CustomGI
{
    [Serializable]
    public struct CustomGIIrradiance
    {
        [ColorUsage(false,false)]public Color color;
        public float intensity;
        public float3 rotation;
        public SHL2Data shData;

        public CustomGIIrradiance Interpolate(CustomGIIrradiance _a, CustomGIIrradiance _b, float _t) => new()
            {
                color = Color.Lerp(_a.color, _b.color, _t),
                intensity = Mathf.Lerp(_a.intensity, _b.intensity, _t),
                rotation = math.lerp(_a.rotation, _b.rotation, _t),
                shData = SHL2Data.Interpolate(_a.shData, _b.shData, _t),
            };
    }

    public class CustomGI : MonoBehaviour
    {
        [Min(0)]public float m_CurrentLightIndex;
        [Readonly] public CustomGIIrradiance[] m_Irradiances;
        
        public bool m_GIEnable = true;
        [MFoldout(nameof(m_GIEnable),true)]public GlobalIllumination_LightmapDiffuse m_Diffuse;
        [MFoldout(nameof(m_GIEnable),true)][Range(0, 10)] public float m_SkylightIndirectIntensity = 1f;
        [MFoldout(nameof(m_GIEnable),true)][Range(0, 10)] public float m_MainLightIndirectIntensity = 1f;

        public Damper m_TimeDamper = Damper.kDefault;

        private PassiveInstance<Light> m_MainLight = new(FindObjectOfType<Light>);

        private void Awake()
        {
            TouchConsole.InitDefaultCommands();
            TouchConsole.Command("NextMainLight", KeyCode.Space).Button(() => { m_CurrentLightIndex += 1f; });
            TouchConsole.Command("MainLightIrradiance", KeyCode.Q).Button(() => {m_MainLightIndirectIntensity = m_MainLightIndirectIntensity > 0 ? 0 : 1; });
            TouchConsole.Command("NextSkylightIrradiance", KeyCode.E).Button(() => { m_SkylightIndirectIntensity = m_SkylightIndirectIntensity > 0 ? 0 : 1; });
            
            m_Diffuse?.Apply(transform);
            m_TimeDamper.Initialize(m_CurrentLightIndex);
            Apply(m_CurrentLightIndex);
        }

        private void OnValidate()
        {
            m_Diffuse?.Apply(transform);
            Apply(m_CurrentLightIndex);
        }

        private void Update() => Apply(m_TimeDamper.Tick(Time.deltaTime, m_CurrentLightIndex));

        private SHL2ShaderProperties kSHProperties = new SHL2ShaderProperties();
        private static readonly int IrradianceParameters = Shader.PropertyToID("_IrradianceParameters");
        private static string kCustomLMEnabled = "LIGHTMAP_CUSTOM";
        public void Apply(float _gradient)
        {
            var giAvailable =  m_GIEnable && m_Irradiances is { Length: 4 } && m_Diffuse != null && m_Diffuse.lightmaps.Length > 0;
            URender.EnableGlobalKeyword(kCustomLMEnabled,giAvailable);

            var irradianceAvailable = m_Irradiances is { Length: >0 };
            if (!irradianceAvailable)
                return;
            
            var (start, end, interp,repeat) =  m_Irradiances.Gradient(_gradient);
            ApplyIrradiance( start, end, interp,repeat);
        }
        

        void ApplyIrradiance(CustomGIIrradiance start, CustomGIIrradiance end, float interp,float repeat)
        {
            kSHProperties.ApplyGlobal(SHL2Data.Interpolate(start.shData, end.shData, interp));

            m_MainLight.Value.transform.rotation = math.slerp(quaternion.Euler(start.rotation * kmath.kDeg2Rad), quaternion.Euler(end.rotation * kmath.kDeg2Rad), interp);
            m_MainLight.Value.color = Color.Lerp(start.color, end.color, interp);
            m_MainLight.Value.intensity = math.lerp(start.intensity, end.intensity, interp);
            Shader.SetGlobalVector(IrradianceParameters, new Vector4(m_SkylightIndirectIntensity, repeat , m_MainLightIndirectIntensity));
        }
        
        public void ApplyIrradiance(CustomGIIrradiance _irradiance) =>  ApplyIrradiance(_irradiance, _irradiance, 1, 1);
        public void ClearGI()
        {
            URender.EnableGlobalKeyword(kCustomLMEnabled,false);
            kSHProperties.ApplyGlobal(SHL2Data.kZero);
        }
    }
}
