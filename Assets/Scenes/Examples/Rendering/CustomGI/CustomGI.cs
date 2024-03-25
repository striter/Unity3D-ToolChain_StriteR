using System;
using Rendering.GI.SphericalHarmonics;
using Rendering.Lightmap;
using Unity.Mathematics;
using UnityEngine;
using System.Linq.Extensions;

namespace Examples.Rendering.GI.CustomGI
{
    [Serializable]
    public class GIData
    {
        public Cubemap cubemap;
        public float shIntensity = 1f;
        [HideInInspector] public SHL2Data shData = SHL2Data.kZero;
    }

    [Serializable]
    public class MainLightData
    {
        public quaternion rotation;
        [ColorUsage(false,true)]public Color color;
        public float intensity;
    }
    
    [ExecuteInEditMode]
    public class CustomGI : MonoBehaviour
    {
        public GIData[] kGiData;
        public MainLightData[] kMainLightData;
        [Range(1, 10)] public float m_SkylightIndirectIntensity;
        [Range(1, 10)] public float m_MainLightIndirectIntensity;

        public float div = 5f;
        
        private bool isDirty;
        public SHL2ShaderProperties kSHProperties = new SHL2ShaderProperties();
        public GlobalIllumination_LightmapDiffuse m_Diffuse;
        private readonly PassiveInstance<Light> m_MainLight = new PassiveInstance<Light>(()=>GameObject.FindObjectOfType<Light>(true));
        public float m_CurrentLightIndex;

        public bool m_EnableIndirect = true;
        public float m_CurLightmapIndex;
        
        private void OnValidate()
        {
            isDirty = true;
        }

        private void Awake()
        {
            if (!Application.isPlaying)
                return;
            
            TouchConsole.InitDefaultCommands();
            TouchConsole.Command("NextMainLight",KeyCode.Space).Button(()=>{
                isDirty = true;
                m_CurrentLightIndex += 1f;
            });
            TouchConsole.Command("MainLightIrradiance",KeyCode.Q).Button(()=>{
                isDirty = true;
                m_EnableIndirect = !m_EnableIndirect;
            });

            ApplyGlobalIllumination();
        }

        private void Update()
        {
            
            if (isDirty)
            {
                isDirty = false;
                ValidateSH();

            }
            
            var (start,end,interp) = kGiData.Gradient(UTime.time / div);
            var sh = SHL2Data.Interpolate(start.shData, end.shData, interp);
            kSHProperties.ApplyGlobal(sh.Output());

            if (Application.isPlaying)
            {
                m_CurLightmapIndex = math.lerp(m_CurLightmapIndex,m_CurrentLightIndex,Time.deltaTime);
                UpdateMainLightIndirectContribution(m_CurLightmapIndex);
            }
            Shader.SetGlobalVector("_IrradianceParameters",new Vector4(m_SkylightIndirectIntensity,m_CurLightmapIndex,m_MainLightIndirectIntensity));
        }

        void UpdateMainLightIndirectContribution(float _value)
        {
            URender.EnableGlobalKeyword("_LIGHTMAP_MAIN_INDIRECT", m_EnableIndirect);
            var(lightStart,lightEnd,lightInterp) = kMainLightData.Gradient(_value);
            m_MainLight.Value.transform.rotation = math.slerp(lightStart.rotation, lightEnd.rotation, lightInterp);
            m_MainLight.Value.color = Color.Lerp(lightStart.color, lightEnd.color, lightInterp);
            m_MainLight.Value.intensity = math.lerp(lightStart.intensity, lightEnd.intensity, lightInterp);
        }
        
        void ValidateSH()
        {
            foreach (var giData in kGiData)
            {
                if (giData.cubemap == null)
                    return;
                giData.shData = SphericalHarmonicsExport.ExportL2Cubemap(64, giData.cubemap, giData.shIntensity,ESHSampleMode.Fibonacci);
            }
        }


        public bool developerMode = false;
        [FoldoutButton(nameof(developerMode),true)]
        void RecordMainLightDataToLast()
        {
            kMainLightData[^1] = new MainLightData()
            {
                color = m_MainLight.Value.color,
                rotation = m_MainLight.Value.transform.rotation,
                intensity = m_MainLight.Value.intensity,
            };
        }

        [FoldoutButton(nameof(developerMode),true)]
        void SyncMainLightToDataIndex(int _index)
        {
            var lightData = kMainLightData[_index];
            m_MainLight.Value.transform.rotation = lightData.rotation;
            m_MainLight.Value.intensity = 1f;
            m_MainLight.Value.color = Color.white;
        }
        
        [FoldoutButton(nameof(developerMode),true)]
        public void ApplyGlobalIllumination()
        {
            m_Diffuse?.Apply(transform);
        }
        
        
        [FoldoutButton(nameof(developerMode),true)]
        public void OutputSkyIrradiance()
        {
#if UNITY_EDITOR
            m_Diffuse = GlobalIllumination_LightmapDiffuse.Export(transform);
#endif
        }
        
    }
}
