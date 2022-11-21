using Rendering.GI.SphericalHarmonics;
using UnityEngine;

namespace ExampleScenes.Rendering.SH
{
    public enum ESphericalHarmonicsExport
    {
        Gradient,
        Cubemap,
    }

    public class SphericalHarmonicsL2 : MonoBehaviour
    {
        public SHL2Data m_Data;

        [Header("Bake")] public int m_SampleCount = 8192;

        public ESphericalHarmonicsExport m_SHMode = ESphericalHarmonicsExport.Gradient;

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
                    m_Data = SphericalHarmonicsExport.ExportL2Gradient(m_SampleCount, m_GradientTop, m_GradientEquator,
                        m_GradientBottom, "Test");
                    break;
                case ESphericalHarmonicsExport.Cubemap:
                    m_Data = SphericalHarmonicsExport.ExportL2Cubemap(m_SampleCount, m_Cubemap,m_Intensity, "test");
                    break;
            }

            MaterialPropertyBlock block = new MaterialPropertyBlock();
            block.SetVector("_L00", m_Data.l00);
            block.SetVector("_L10", m_Data.l10);
            block.SetVector("_L11", m_Data.l11);
            block.SetVector("_L12", m_Data.l12);
            block.SetVector("_L20", m_Data.l20);
            block.SetVector("_L21", m_Data.l21);
            block.SetVector("_L22", m_Data.l22);
            block.SetVector("_L23", m_Data.l23);
            block.SetVector("_L24", m_Data.l24);

            m_Data.OutputSH(out var shAr, out var shAg, out var shAb, out var shBr, out var shBg, out var shBb,
                out var shc);
            block.SetVector("_SHAr", shAr);
            block.SetVector("_SHAg", shAg);
            block.SetVector("_SHAb", shAb);
            block.SetVector("_SHBr", shBr);
            block.SetVector("_SHBg", shBg);
            block.SetVector("_SHBb", shBb);
            block.SetVector("_SHC", shc);
            GetComponent<MeshRenderer>().SetPropertyBlock(block);
        }
    }
}
