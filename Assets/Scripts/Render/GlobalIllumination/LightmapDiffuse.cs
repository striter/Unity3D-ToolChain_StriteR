using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using Rendering.GI.SphericalHarmonics;
using UnityEngine;

namespace Rendering.Lightmap
{
    [Serializable]
    public struct LightmapParameter
    {
        public int index;
        public Vector4 scaleOffset;
    }
    
    [Serializable]
    public struct LightmapTextures
    {
        public Texture2D color;
        public Texture2D directional;
        public Texture2D shadowMask;

        public static implicit operator LightmapData(LightmapTextures _texture) => new LightmapData() {lightmapColor = _texture.color, lightmapDir = _texture.directional, shadowMask = _texture.shadowMask};
        public static implicit operator LightmapTextures(LightmapData _data) => new LightmapTextures() {color = _data.lightmapColor, directional = _data.lightmapDir, shadowMask = _data.shadowMask};
    }

    [Serializable]
    public struct LightBaking
    {
        public bool isBaked;
        public int probeOcclusionLightIndex;
        public int occlusionMaskChannel;
        public LightmapBakeType lightmapBakeType;
        public MixedLightingMode mixedLightingMode;
        public static implicit operator LightBakingOutput(LightBaking _baking) => new LightBakingOutput() {isBaked = _baking.isBaked, probeOcclusionLightIndex = _baking.probeOcclusionLightIndex, lightmapBakeType = _baking.lightmapBakeType, mixedLightingMode = _baking.mixedLightingMode};
        public static implicit operator LightBaking(LightBakingOutput _baking) => new LightBaking() {isBaked = _baking.isBaked, probeOcclusionLightIndex = _baking.probeOcclusionLightIndex, lightmapBakeType = _baking.lightmapBakeType, mixedLightingMode = _baking.mixedLightingMode};
    }
    
    [Serializable]
    public class GlobalIllumination_LightmapDiffuse
    {
        [Readonly] public LightmapsMode LightmapsMode;
        [Readonly] public LightmapTextures[] lightmaps;
        [Readonly] public LightmapParameter[] parameters;
        [Readonly] public LightBaking[] bakingOutput;
        [Readonly] public Texture[] reflectionProbes;
        public static GlobalIllumination_LightmapDiffuse Export(Transform _root)
        {
            var renderers = _root.GetComponentsInChildren<MeshRenderer>(true);
            var lights = _root.GetComponentsInChildren<Light>(true);
            var reflectionProbes = _root.GetComponentsInChildren<ReflectionProbe>(true);
            
            return new GlobalIllumination_LightmapDiffuse()
            {
                LightmapsMode = LightmapSettings.lightmapsMode,
                reflectionProbes =  reflectionProbes.Select(p => p.texture).ToArray(),
                bakingOutput = lights.Select(p=>(LightBaking)p.bakingOutput).ToArray(),
                lightmaps = LightmapSettings.lightmaps.Select(p => (LightmapTextures)p ).ToArray(),
                parameters = renderers.Select(p => new LightmapParameter() {index = p.lightmapIndex, scaleOffset = p.lightmapScaleOffset}).ToArray(),
            };
        }
    }


    public enum EApplyMode
    {
        Global,
        PropertyBlock,
    }
    public static class GlobalIllumination_LightmapDiffuse_Extension
    {
        private static readonly Dictionary<Material, Material> kLightmappedMaterials = new Dictionary<Material, Material>();
        public static void Apply(this GlobalIllumination_LightmapDiffuse _collection, Transform _root,EApplyMode _mode = EApplyMode.Global)
        {
            var renderers = _root.GetComponentsInChildren<MeshRenderer>(true);
            var lights = _root.GetComponentsInChildren<Light>(true);
            var reflectionProbes = _root.GetComponentsInChildren<ReflectionProbe>(true);
            if(renderers.Length != _collection.parameters.Length)
                throw new ArgumentException("MeshRender Count Not Equals");
            
            if(lights.Length != _collection.bakingOutput.Length)
                throw new ArgumentException("Light Count Not Equals");
            
            if(reflectionProbes.Length != _collection.reflectionProbes.Length)
                throw new ArgumentException("ReflectionProbe Count Not Equals");
            
            for (var i = 0; i < lights.Length; i++)
                lights[i].bakingOutput = _collection.bakingOutput[i];
            
            for(var i = 0; i < reflectionProbes.Length; i++)
                reflectionProbes[i].bakedTexture = _collection.reflectionProbes[i];
            
            switch (_mode)
            {
                default: throw new InvalidEnumArgumentException();
                case EApplyMode.Global:
                    ApplyGlobal(renderers, _collection);
                    break;
                case EApplyMode.PropertyBlock:
                    ApplyPropertyBlock.Apply(renderers, _collection);
                    break;
            }
        }


        private static void ApplyGlobal(MeshRenderer[] _renderers, GlobalIllumination_LightmapDiffuse _collection)
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                var renderer = _renderers[i];
                var parameter = _collection.parameters[i];
                if (parameter.index < 0) continue;
                renderer.lightmapIndex = parameter.index;
                renderer.lightmapScaleOffset = parameter.scaleOffset;
            }
            
            LightmapSettings.lightmapsMode = _collection.LightmapsMode;
            LightmapSettings.lightmaps = _collection.lightmaps.Select(p=>(LightmapData)p).ToArray();
        }

        public static class ApplyPropertyBlock
        {
            private static readonly int kLightmapSTID = Shader.PropertyToID("_LightmapST"); 
            public static readonly GIShaderProperties kDefault = new GIShaderProperties("");
            
            public class GIShaderProperties
            {
                public readonly int kLightmapId;
                public readonly int kDirectionalId;
                public readonly int kShadowMaskId;
                public GIShaderProperties(string _prefix)
                {
                    kLightmapId = Shader.PropertyToID(_prefix + "_Lightmap");
                    kDirectionalId = Shader.PropertyToID(_prefix + "_LightmapInd");
                    kShadowMaskId = Shader.PropertyToID(_prefix + "_ShadowMask");
                }

                public void Apply(MaterialPropertyBlock _block, LightmapTextures[] _dataSet,int _index)
                {
                    var _data = _dataSet[_index];
                    if(_data.color)_block.SetTexture(kLightmapId,_data.color);
                    if(_data.directional)_block.SetTexture(kDirectionalId,_data.directional);
                    if(_data.shadowMask)_block.SetTexture(kShadowMaskId,_data.shadowMask);
                }
            }

            public static void Apply(MeshRenderer[] _renderers, GlobalIllumination_LightmapDiffuse _collection)
            {
                MaterialPropertyBlock block=new MaterialPropertyBlock();

                for (int i = 0; i < _renderers.Length; i++)
                {
                    var renderer = _renderers[i];
                    block.Clear();
                    
                    var parameter = _collection.parameters[i];
                    if (parameter.index >= 0)
                    {
                        SetupLightmapMaterials(renderer);
                        block.SetVector(kLightmapSTID,parameter.scaleOffset);
                        kDefault.Apply(block,_collection.lightmaps,parameter.index);
                    }
                    renderer.SetPropertyBlock(block);
                }
            }

            private static Material ValidateMaterial(Material _src)
            {
                if (!_src) return null;
                
                if (kLightmappedMaterials.TryGetValue(_src, out var material))
                    return material;
                
                var dst = new Material(_src){hideFlags = HideFlags.HideAndDontSave};
                dst.name = $"{dst.name} (Cloned Lightmap)";
                dst.EnableKeyword("LIGHTMAP_ON");
                dst.EnableKeyword("LIGHTMAP_LOCAL");
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
            

            public static void Dispose()
            {
                foreach (var lightmapMaterials in kLightmappedMaterials.Values)
                    GameObject.DestroyImmediate(lightmapMaterials);
                kLightmappedMaterials.Clear();
            }
        }
    }
}
