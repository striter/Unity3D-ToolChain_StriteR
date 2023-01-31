using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Rendering.GI.SphericalHarmonics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Rendering
{
    [Serializable]
    public struct SHGradient
    {
        [ColorUsage(true,true)]public Color gradientSky;
        [ColorUsage(true,true)]public Color gradientEquator;
        [ColorUsage(true,true)]public Color gradientGround;
        [HideInInspector] public SHL2Data shData;

        public SHGradient Ctor()
        {
            shData = SphericalHarmonicsExport.ExportL2Gradient(4096, gradientSky, gradientEquator,gradientGround,"Test");
            return this;
        }

        public static readonly SHGradient kDefault = new SHGradient()
        {
            gradientSky = Color.cyan,
            gradientGround = Color.black,
            gradientEquator = Color.white.SetAlpha(.5f),
        }.Ctor();
        
        public static SHGradient Interpolate(SHGradient _a, SHGradient _b,
            float _interpolate)
        {
            return new SHGradient()
            {
                gradientSky = Color.Lerp(_a.gradientSky,_b.gradientSky,_interpolate),
                gradientEquator = Color.Lerp(_a.gradientEquator,_b.gradientEquator,_interpolate),
                gradientGround = Color.Lerp(_a.gradientGround,_b.gradientGround,_interpolate),
                shData = SHL2Data.Interpolate(_a.shData,_b.shData,_interpolate),
            };
        }
    }

    [Serializable]
    public struct LightmapParameter
    {
        public int index;
        public Vector4 scaleOffset;
    }
    
    [Serializable]
    public class GlobalIlluminationOverrideData:ISerializationCallbackReceiver
    {    
        public enum EGIOverride
        {
            None=0,
            GI_OVERRIDE,
            GI_INTERPOLATE,
        }
        public SHGradient gradient=SHGradient.kDefault;
        
        public Texture2D[] lightmapColors;
        [HideInInspector] public LightmapParameter[] lightmapParameters;

        public Texture reflectionTexture;
        [Range(0.1f,2f)] public float reflectionIntensity;
        
        public void OnBeforeSerialize(){}
        public void OnAfterDeserialize() => gradient.Ctor();
        
        public static GlobalIlluminationOverrideData Export(MeshRenderer[] _renderers)
        {
            Debug.Assert(LightmapSettings.lightmapsMode== LightmapsMode.NonDirectional,"Only none-directional mode supported");
            return new GlobalIlluminationOverrideData()
            {
                gradient = new SHGradient()
                {
                    gradientSky = RenderSettings.ambientSkyColor,
                    gradientEquator = RenderSettings.ambientEquatorColor,
                    gradientGround = RenderSettings.ambientGroundColor
                }.Ctor(),
                
                lightmapParameters = _renderers.Select(p => new LightmapParameter()
                    {index = p.lightmapIndex, scaleOffset = p.lightmapScaleOffset}).ToArray(),
                lightmapColors = LightmapSettings.lightmaps.Select(p => p.lightmapColor).ToArray(),

                reflectionTexture = RenderSettings.customReflectionTexture,
                reflectionIntensity =  1,
            };
        }

        private static readonly string kInterpolateKeyword = "_INTERPOLATE";
        private static readonly int kInterpolation = Shader.PropertyToID("_Interpolation");
        
        private static readonly int kSHAr = Shader.PropertyToID("_SHAr"); private static readonly int kSHAg = Shader.PropertyToID("_SHAg"); private static readonly int kSHAb = Shader.PropertyToID("_SHAb");  
        private static readonly int kSHBr = Shader.PropertyToID("_SHBr"); private static readonly int kSHBg = Shader.PropertyToID("_SHBg");  private static readonly int kSHBb = Shader.PropertyToID("_SHBb");
        private static readonly int kSHC = Shader.PropertyToID("_SHC");

        private static readonly int kSpecCube0ID = Shader.PropertyToID("_SpecCube"); private static readonly int kSpecCube0InterpolateID = Shader.PropertyToID("_SpecCube_Interpolate");
        private static readonly int kSpecCube0IntensityID = Shader.PropertyToID("_SpecCube_Intensity");  private static readonly int kSpecCube0IntensityInterpolateID = Shader.PropertyToID("_SpecCube_Intensity_Interpolate");

        private static readonly int kLightmapSTID = Shader.PropertyToID("_LightmapST");        private static readonly int kLightmapInterpolateSTID = Shader.PropertyToID("_LightmapInterpolateST");
        private static readonly int kLightmapID = Shader.PropertyToID("_Lightmap");        private static readonly int kLightmapInterpolateID = Shader.PropertyToID("_Lightmap_Interpolate");

        public void Apply(MeshRenderer[] _renderers) => Apply(_renderers,this,this,0f);
        
        private static readonly Dictionary<Material, Material> kLightmappedMaterials = new Dictionary<Material, Material>();

        private static Material ValidateMaterial(Material _src)
        {
            if (kLightmappedMaterials.ContainsKey(_src))
                return kLightmappedMaterials[_src];
            
            var dst = new Material(_src){hideFlags = HideFlags.HideAndDontSave};
            dst.EnableKeyword("LIGHTMAP_ON");
            kLightmappedMaterials.Add(_src,dst);
            return dst;
        }

        private static void SetupLightmapMaterials(Renderer _renderer)
        {
            var length = _renderer.sharedMaterials.Length;
            Material[] sharedMaterials = new Material[length];
            
            for (int i = 0; i < length; i++)
                sharedMaterials[i] = ValidateMaterial(_renderer.sharedMaterials[i]);
            _renderer.sharedMaterials = sharedMaterials;
        }
        
        public static void Apply(MeshRenderer[] _renderers, GlobalIlluminationOverrideData _collection1,GlobalIlluminationOverrideData _collection2,float _interpolation)
        {
            MaterialPropertyBlock block=new MaterialPropertyBlock();

            GlobalIlluminationOverrideData baseCollection = _collection1;
            bool interpolate = false;
            
            if (_interpolation >= 1f - float.Epsilon)
                baseCollection = _collection2;
            else
                interpolate = true;

            URender.EnableGlobalKeywords(interpolate?EGIOverride.GI_INTERPOLATE: EGIOverride.GI_OVERRIDE);
            
            ref SHGradient gradient = ref _collection1.gradient;
            if(interpolate)
                gradient=SHGradient.Interpolate(_collection1.gradient,_collection2.gradient,_interpolation);
            gradient.shData.OutputSH(out var SHAr,out var SHAg,out var SHAb,out var SHBr,out var SHBg,out var SHBb,out var SHC);
            Shader.SetGlobalVector(kSHAr,SHAr);
            Shader.SetGlobalVector(kSHAg,SHAg);
            Shader.SetGlobalVector(kSHAb,SHAb);
            Shader.SetGlobalVector(kSHBr,SHBr);
            Shader.SetGlobalVector(kSHBg,SHBg);
            Shader.SetGlobalVector(kSHBb,SHBb);
            Shader.SetGlobalVector(kSHC,SHC);
                
            for (int i = 0; i < _renderers.Length; i++)
            {
                var renderer = _renderers[i];
                renderer.sharedMaterial.EnableKeyword(kInterpolateKeyword,interpolate);
                block.Clear();
                block.SetTexture(kSpecCube0ID,baseCollection.reflectionTexture);
                block.SetFloat(kSpecCube0IntensityID,baseCollection.reflectionIntensity);
                
                var parameter = baseCollection.lightmapParameters[i];
                if (parameter.index >= 0)
                {
                    SetupLightmapMaterials(renderer);
                    block.SetVector(kLightmapSTID,parameter.scaleOffset);
                    block.SetTexture(kLightmapID,baseCollection.lightmapColors[parameter.index]);
                }
                
                if (interpolate)
                {
                    block.SetFloat(kInterpolation,_interpolation);
                    var parameter2 = _collection2.lightmapParameters[i];
                    if (parameter2.index >= 0)
                    {
                        block.SetVector(kLightmapInterpolateSTID,parameter2.scaleOffset);
                        block.SetTexture(kLightmapInterpolateID,_collection2.lightmapColors[parameter2.index]);
                    }
                    
                    block.SetTexture(kSpecCube0InterpolateID,_collection2.reflectionTexture);
                    block.SetFloat(kSpecCube0IntensityInterpolateID,_collection2.reflectionIntensity);
                }
                renderer.SetPropertyBlock(block);
            }
        }

        public void Dispose()
        {
            URender.EnableGlobalKeywords(EGIOverride.None);
            foreach (var lightmapMaterials in kLightmappedMaterials.Values)
                GameObject.DestroyImmediate(lightmapMaterials);
            kLightmappedMaterials.Clear();
        }
    }
}
