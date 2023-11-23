using System;
using System.Collections;
using System.Collections.Generic;
using Rendering.GI.SphericalHarmonics;
using UnityEngine;

namespace Examples.Rendering.GI.CustomGI
{
    [Serializable]
    public class GIData
    {
        public Cubemap cubemap;
        public float shIntensity = 1f;
        [HideInInspector] public SHL2Data shData = SHL2Data.kZero;
    }
    
    [ExecuteInEditMode]
    public class CustomGI : MonoBehaviour
    {
        public Texture2DArray kSkyIrradiance;
        public GIData[] kGiData;
        public bool m_AddDirecitonalLight;

        public SHL2ShaderProperties kSHProperties = new SHL2ShaderProperties();
        public float div = 5f;

        private bool isDirty;
        private void OnValidate()
        {
            isDirty = true;
        }

        [Button]
        void ValidateSH()
        {
            foreach (var giData in kGiData)
            {
                if (giData.cubemap == null)
                    return;
                giData.shData = SphericalHarmonicsExport.ExportL2Cubemap(64, giData.cubemap, giData.shIntensity,ESHSampleMode.Fibonacci);
            }
        }
        private void Update()
        {
            if (isDirty)
            {
                isDirty = false;
                ValidateSH();
            }
            
            var (start,end,interp) = kGiData.Gradient(UTime.time / div);
            var light = GetComponent<Light>();
            var sh = SHL2Data.Interpolate(start.shData, end.shData, interp);
            if(m_AddDirecitonalLight)
                sh = SphericalHarmonicsExport.ExportDirectionalLight(light.transform.forward, light.color.ToFloat3() * light.intensity,true);
            kSHProperties.ApplyGlobal(sh.Output());
        }
    }
}
