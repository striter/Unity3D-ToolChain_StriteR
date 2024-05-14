using System;
using System.Linq.Extensions;
using Examples.Rendering.GI.CustomGI;
using Rendering.GI.SphericalHarmonics;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

#if BAKERY_INCLUDED
namespace UnityEditor.Extensions.ScriptableObjectBundle.Process.Lightmap.Bakery
{
    [Serializable]
    public class CustomGIIrradianceInput
    {
        public Color color;
        public float intensity;
        public float3 rotation;
        public Cubemap cubemap;
        public float shIntensity = 1f;

        public CustomGIIrradiance Output() =>new CustomGIIrradiance(){
            
            color = color,
            intensity = intensity,
            rotation = rotation,
            shData = cubemap == null? SHL2Data.kZero : SphericalHarmonicsExport.ExportL2Cubemap(128, cubemap, shIntensity,ESHSampleMode.Fibonacci)
        };
    }

    public struct CustomGIReferences
    {
        public CustomGI customGI;
        public Light mainLight;
        public BakeryDirectLight directLight;
        public BakerySkyLight skyLight;
    }
    
    public class BakeryCustomGIPreparation : EAssetPipelineProcess
    {
        public enum EGICustomLightmapProcess
        {
            ESkyLight = -1,
            EDirectionLight0 = 0,
            EDirectionLight1 = 1,
            EDirectionLight2 = 2,
            EDirectionLight3 = 3,
        }

        public EGICustomLightmapProcess m_Process = EGICustomLightmapProcess.ESkyLight;
        [MFold(nameof(m_Process),EGICustomLightmapProcess.ESkyLight)]public CustomGIIrradianceInput m_Irradiance;

        public static void Finish()
        {
            if (!SafeCheck(out var reference))
                return;
            
            reference.skyLight.enabled = false;
            reference.directLight.enabled = false;
            reference.mainLight.enabled = true;
            reference.customGI.Apply(reference.customGI.m_CurrentLightIndex);
        }
        public static bool SafeCheck(out CustomGIReferences _references)
        {
            _references = default;
            var gi = FindObjectOfType<CustomGI>();
            if (!gi) return false;
            _references.customGI = gi;
            _references.mainLight = _references.customGI.GetComponentsInChildren<Light>().Find(p=>p.type == LightType.Directional);
            _references.directLight = _references.customGI.GetComponentInChildren<BakeryDirectLight>();
            _references.skyLight = _references.customGI.GetComponentInChildren<BakerySkyLight>();
            return _references.mainLight && _references.directLight && _references.skyLight;
        }
        public override bool Execute()
        {
            if (!SafeCheck(out var reference))
                return false;
            
            var skyLightProcess = m_Process == EGICustomLightmapProcess.ESkyLight;
            reference.skyLight.enabled = skyLightProcess;
            reference.mainLight.enabled = !skyLightProcess;
            reference.directLight.enabled = !skyLightProcess;
            if (skyLightProcess)
            {
                reference.skyLight.intensity = 1f;
                reference.skyLight.color = Color.white;
                reference.skyLight.cubemap = null;
                reference.customGI.ClearGI();
                return true;
            }

            var lightData = m_Irradiance.Output();
            
            reference.mainLight.transform.rotation = quaternion.Euler(lightData.rotation * kmath.kDeg2Rad);
            reference.mainLight.color = lightData.color;
            reference.mainLight.intensity = lightData.intensity * lightData.intensity;

            reference.directLight.color = Color.white;     //Bake using the default color
            reference.directLight.transform.rotation = quaternion.Euler(lightData.rotation * kmath.kDeg2Rad);
            reference.directLight.intensity = 1f;

            var gradientIndex = (int)m_Process;
            reference.customGI.m_Irradiances =  reference.customGI.m_Irradiances.Resize(4);
            reference.customGI.m_Irradiances[gradientIndex] = lightData;
            reference.customGI.ClearGI();
            reference.customGI.ApplyIrradiance(lightData);
            return true;
        }

        protected void OnValidate()
        {
            if (!SafeCheck(out var reference))
                return;
            
            var lightData = m_Irradiance.Output();
            reference.customGI.ClearGI();
            reference.customGI.ApplyIrradiance(lightData);
        }
    }
}
#endif