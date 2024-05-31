using Rendering.GI.SphericalHarmonics;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Examples.Rendering.SH
{
    public enum ESphericalHarmonicsExport
    {
        Ambient,
        Gradient,
        Cubemap,
        Directional,
    }
    public class SphericalHarmonicsL2 : MonoBehaviour
    {
        public SHL2Data m_Data;
        public SHL2Output m_Output;
        
        [Header("Bake")]
        public ESphericalHarmonicsExport m_SHMode = ESphericalHarmonicsExport.Gradient;

        [MFoldout(nameof(m_SHMode),ESphericalHarmonicsExport.Cubemap)] public int m_SampleCount = 8192;
        [MFoldout(nameof(m_SHMode),ESphericalHarmonicsExport.Cubemap)] public Cubemap m_Cubemap;
        [MFoldout(nameof(m_SHMode),ESphericalHarmonicsExport.Cubemap)] [Range(0.1f,3f)]public float m_Intensity = 1f;
        [MFoldout(nameof(m_SHMode),ESphericalHarmonicsExport.Gradient)] [ColorUsage(false, true)] public Color m_GradientTop = Color.red;
        [MFoldout(nameof(m_SHMode),ESphericalHarmonicsExport.Gradient)] [ColorUsage(false, true)] public Color m_GradientEquator = Color.green;
        [MFoldout(nameof(m_SHMode),ESphericalHarmonicsExport.Gradient)] [ColorUsage(false, true)] public Color m_GradientBottom = Color.blue;

        [MFoldout(nameof(m_SHMode),ESphericalHarmonicsExport.Ambient,ESphericalHarmonicsExport.Directional)][ColorUsage(false, true)] public Color m_LightColor = Color.white;
        private void OnValidate()
        {
            switch (m_SHMode)
            {
                case ESphericalHarmonicsExport.Ambient:
                    m_Data = SHL2Data.Ambient(UColorTransform.GammaToLinear(m_LightColor.to3()));
                    break;
                case ESphericalHarmonicsExport.Gradient:            //???????????????????????????????????
                    m_Data = SphericalHarmonicsExport.ExportL2Gradient(UColorTransform.GammaToLinear(m_GradientTop.to3()) * .6f,
                                                                                    UColorTransform.GammaToLinear(m_GradientEquator.to3()) * 1.5f,
                                                                                    UColorTransform.GammaToLinear(m_GradientBottom.to3()) * .6f);
                    break;
                case ESphericalHarmonicsExport.Cubemap:
                    m_Data = SphericalHarmonicsExport.ExportL2Cubemap(m_SampleCount, m_Cubemap,m_Intensity,ESHSampleMode.Fibonacci);
                    break;
                case ESphericalHarmonicsExport.Directional:
                    m_Data = SHL2Data.Direction(kfloat3.up, m_LightColor.to3());
                    break;
            }
            
            m_Output = m_Data.Output();
            Ctor();
        }

        [Button]
        void SyncWithUnity()
        {
            m_Output = SHL2ShaderProperties.kUnity.FetchGlobal();
            Ctor();
        }

        void Ctor()
        {
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            SHL2ShaderProperties.kDefault.Apply(block,m_Output);
            GetComponent<MeshRenderer>().SetPropertyBlock(block);
        }
    }
}
