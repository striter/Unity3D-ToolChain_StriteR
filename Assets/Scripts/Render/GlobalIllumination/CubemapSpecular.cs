using System;
using UnityEngine;

namespace Rendering.Lightmap
{
    [Serializable]
    public class GlobalIllumination_CubemapSpecular
    {
        [Readonly] public Texture reflectionTexture;
        [Range(0.1f,2f)] public float reflectionIntensity;

        public static GlobalIllumination_CubemapSpecular Export() => new GlobalIllumination_CubemapSpecular()
        {
            reflectionTexture = RenderSettings.customReflectionTexture,
            reflectionIntensity = 1,
        };
        private static readonly int kSpecCube0ID = Shader.PropertyToID("_SpecCube"); 
        private static readonly int kSpecCube0IntensityID = Shader.PropertyToID("_SpecCube_Intensity");
        public void Apply(MaterialPropertyBlock _block,GlobalIllumination_CubemapSpecular _collection)
        {
            _block.SetTexture(kSpecCube0ID,_collection.reflectionTexture);
            _block.SetFloat(kSpecCube0IntensityID,_collection.reflectionIntensity);
        }
    }
}