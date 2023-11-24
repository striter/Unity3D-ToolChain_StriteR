using Rendering.GI.SphericalHarmonics;
using UnityEngine;
namespace Rendering.Lightmap
{
    //?
    public class GIPersistent : ScriptableObject
    {
        public SHL2Data m_SHL2;
        public GlobalIllumination_LightmapDiffuse m_Lightmap;
        public GlobalIllumination_CubemapSpecular m_Specular;
    }
}