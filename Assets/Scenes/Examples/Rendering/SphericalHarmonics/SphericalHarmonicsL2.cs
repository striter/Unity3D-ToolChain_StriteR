using Rendering.GI.SphericalHarmonics;
using Unity.Mathematics;
using UnityEngine;

namespace Examples.Rendering.SH
{
    public enum ESphericalHarmonicsExport
    {
        Gradient,
        Cubemap,
    }

    public class SphericalHarmonicsL2 : MonoBehaviour
    {
        public SHL2Data m_Data;

        // public SHL2Data m_Comparer = new SHL2Output()
        // {
        //     shAr = new float4(0,0.2721655f,0,0.1927083f),
        //     shAg = new float4(0,0,0,0),
        //     shAb = new float4(0,-0.2721655f,0,0.1927083f),
        //     shBr = new float4(0,0,-0.0781250f,0),
        //     shBg = new float4(0,0,-0,0),
        //     shBb = new float4(0,0,-0.0781250f,0),
        //     shC = new float3(-0.0781250f,0,-0.0781250f),
        // }.PackUp();
        
        [Header("Bake")]
        public ESphericalHarmonicsExport m_SHMode = ESphericalHarmonicsExport.Gradient;

        [MFoldout(nameof(m_SHMode),ESphericalHarmonicsExport.Cubemap)]public int m_SampleCount = 8192;
        [MFoldout(nameof(m_SHMode),ESphericalHarmonicsExport.Cubemap)]public Cubemap m_Cubemap;
        [MFoldout(nameof(m_SHMode),ESphericalHarmonicsExport.Cubemap)] [Range(0.1f,3f)]public float m_Intensity = 1f;
        [MFoldout(nameof(m_SHMode),ESphericalHarmonicsExport.Gradient)][ColorUsage(false, true)] public Color m_GradientTop = Color.red;
        [MFoldout(nameof(m_SHMode),ESphericalHarmonicsExport.Gradient)][ColorUsage(false, true)] public Color m_GradientEquator = Color.green;
        [MFoldout(nameof(m_SHMode),ESphericalHarmonicsExport.Gradient)][ColorUsage(false, true)] public Color m_GradientBottom = Color.blue;
        private void OnValidate()
        {
            switch (m_SHMode)
            {
                case ESphericalHarmonicsExport.Gradient:
                    m_Data = SphericalHarmonicsExport.ExportL2Gradient( m_GradientTop, m_GradientEquator, m_GradientBottom);
                    break;
                case ESphericalHarmonicsExport.Cubemap:
                    m_Data = SphericalHarmonicsExport.ExportL2Cubemap(m_SampleCount, m_Cubemap,m_Intensity, "test");
                    break;
            }

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
            // output.Apply(block,SHShaderProperties.kDefault);
            m_Data.Output().Apply(block,SHShaderProperties.kDefault);
            GetComponent<MeshRenderer>().SetPropertyBlock(block);
        }
    }
}
