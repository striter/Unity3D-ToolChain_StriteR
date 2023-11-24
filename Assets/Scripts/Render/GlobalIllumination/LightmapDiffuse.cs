using System;
using System.Collections.Generic;
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
    public class LightmapTextures
    {
        public Texture2D color;
        public Texture2D directional;
        public Texture2D shadowMask;
    }
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

    
    [Serializable]
    public class GlobalIllumination_LightmapDiffuse
    {
        [Readonly] public LightmapTextures[] lightmaps;
        [Readonly] public LightmapParameter[] parameters;
        
        private static readonly int kLightmapSTID = Shader.PropertyToID("_LightmapST"); 
        public static readonly GIShaderProperties kDefault = new GIShaderProperties("");
        
        private static readonly Dictionary<Material, Material> kLightmappedMaterials = new Dictionary<Material, Material>();

        public static GlobalIllumination_LightmapDiffuse Export(Transform _root)
        {
            var _renderers = _root.GetComponentsInChildren<MeshRenderer>(true);
            return new GlobalIllumination_LightmapDiffuse()
            {
                parameters = _renderers.Select(p => new LightmapParameter()
                    {index = p.lightmapIndex, scaleOffset = p.lightmapScaleOffset}).ToArray(),
                lightmaps = LightmapSettings.lightmaps.Select(p => new LightmapTextures()
                {
                  color  = p.lightmapColor,
                  directional = p.lightmapDir,
                  shadowMask = p.shadowMask,
                } ).ToArray(),
            };
        }

        public void Apply(Transform _root) => Apply(_root.GetComponentsInChildren<MeshRenderer>(true),this);
        public static void Apply(MeshRenderer[] _renderers, GlobalIllumination_LightmapDiffuse _collection)
        {
            MaterialPropertyBlock block=new MaterialPropertyBlock();

            GlobalIllumination_LightmapDiffuse baseCollection = _collection;
            for (int i = 0; i < _renderers.Length; i++)
            {
                var renderer = _renderers[i];
                block.Clear();
                
                var parameter = baseCollection.parameters[i];
                if (parameter.index >= 0)
                {
                    SetupLightmapMaterials(renderer);
                    block.SetVector(kLightmapSTID,parameter.scaleOffset);
                    kDefault.Apply(block,baseCollection.lightmaps,parameter.index);
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
        

        public void Dispose()
        {
            foreach (var lightmapMaterials in kLightmappedMaterials.Values)
                GameObject.DestroyImmediate(lightmapMaterials);
            kLightmappedMaterials.Clear();
        }
    }
}
