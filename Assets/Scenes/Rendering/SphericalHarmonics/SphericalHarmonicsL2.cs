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
        CubemapAccurate,
        Directional,
    }
    public class SphericalHarmonicsL2 : MonoBehaviour
    {
        public SHL2Data m_Data;
        public SHL2ShaderConstants m_Output;
        
        [Header("Bake")]
        public ESphericalHarmonicsExport m_SHMode = ESphericalHarmonicsExport.Gradient;

        [MFoldout(nameof(m_SHMode),ESphericalHarmonicsExport.Cubemap)] public int m_SampleCount = 8192;
        [MFoldout(nameof(m_SHMode),ESphericalHarmonicsExport.Cubemap)] public Cubemap m_Cubemap;
        [MFoldout(nameof(m_SHMode),ESphericalHarmonicsExport.Cubemap)] [Range(0.1f,3f)]public float m_Intensity = 1f;
        [MFoldout(nameof(m_SHMode),ESphericalHarmonicsExport.Gradient)] [ColorUsage(false, true)] public Color m_GradientTop = Color.red;
        [MFoldout(nameof(m_SHMode),ESphericalHarmonicsExport.Gradient)] [ColorUsage(false, true)] public Color m_GradientEquator = Color.green;
        [MFoldout(nameof(m_SHMode),ESphericalHarmonicsExport.Gradient)] [ColorUsage(false, true)] public Color m_GradientBottom = Color.blue;

        [MFoldout(nameof(m_SHMode), ESphericalHarmonicsExport.Directional)] [PostNormalize]
        public float3 m_Direction = kfloat3.up;

        [MFoldout(nameof(m_SHMode),ESphericalHarmonicsExport.Ambient,ESphericalHarmonicsExport.Directional)][ColorUsage(false, true)] public Color m_LightColor = Color.white;
        private void OnValidate()
        {
            switch (m_SHMode)
            {
                case ESphericalHarmonicsExport.Ambient:
                    m_Data = SHL2Data.Ambient(m_LightColor.linear.to3());
                    break;
                case ESphericalHarmonicsExport.Gradient:            //???????????????????????????????????
                    m_Data = SphericalHarmonicsExport.ExportGradient(m_GradientTop.linear.to3(), m_GradientEquator.linear.to3(), m_GradientBottom.linear.to3());
                    break;
                case ESphericalHarmonicsExport.Cubemap:
                    m_Data = SphericalHarmonicsExport.ExportL2Cubemap(m_SampleCount, m_Cubemap,m_Intensity,ESHSampleMode.Fibonacci);
                    break;
                case ESphericalHarmonicsExport.CubemapAccurate:
                    m_Data = SphericalHarmonicsExport.ExportCubemap(m_Cubemap,m_Intensity);
                    break;
                case ESphericalHarmonicsExport.Directional:
                    m_Data = SHL2Data.Direction(m_Direction, m_LightColor.linear.to3());
                    break;
            }
            
            m_Output = m_Data;
            Ctor();
        }

        [InspectorButton]
        void SyncWithUnity()
        {
            m_Data = RenderSettings.ambientProbe;
            m_Output = m_Data;
            Ctor();
        }

        void Ctor()
        {
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            block.SetVector("_L00", m_Data.l00.to4());
            block.SetVector("_L10", m_Data.l10.to4());
            block.SetVector("_L11", m_Data.l11.to4());
            block.SetVector("_L12", m_Data.l12.to4());
            block.SetVector("_L20", m_Data.l20.to4());
            block.SetVector("_L21", m_Data.l21.to4());
            block.SetVector("_L22", m_Data.l22.to4());
            block.SetVector("_L23", m_Data.l23.to4());
            block.SetVector("_L24", m_Data.l24.to4());
            SHL2ShaderProperties.kDefault.Apply(block,m_Output);
            GetComponent<MeshRenderer>().SetPropertyBlock(block);
        }
    }
}
