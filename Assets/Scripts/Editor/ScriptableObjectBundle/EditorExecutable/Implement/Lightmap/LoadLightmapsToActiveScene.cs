using System;
using System.Linq;
using Rendering.Lightmap;
using UnityEditor.Extensions.EditorPath;
using UnityEngine;

namespace UnityEditor.Extensions.EditorExecutable.Process.Lightmap
{
    public class LoadLightmapsToActiveScene : EditorExecutableProcess
    {
        [EditorPath] public string m_Folder;
        public ELightmap m_LoadLightmaps = ELightmap.Color;
        [Foldout(nameof(m_LoadLightmaps),ELightmap.Color)] public string m_LightmapIdentity;
        [Foldout(nameof(m_LoadLightmaps),ELightmap.Directional)]public string m_DirectionalIdentity;
        [Foldout(nameof(m_LoadLightmaps),ELightmap.ShadowMask)]public string m_ShadowMaskIdentity;
        public override bool Execute()
        {
            var directory = UEPath.PathRegex(m_Folder);
            var textures = UEAsset.LoadAllAssetsAtDirectory<Texture2D>(directory).ToArray();
            
            if (textures.Length == 0)
            {
                Debug.LogWarning("Invalid lightmap path: " + directory);
                return false;
            }
            
            var lightmapColors = textures.Where(p => p.name.Contains(m_LightmapIdentity)).ToArray();
            var directionalLightmaps = textures.Where(p => p.name.Contains(m_DirectionalIdentity)).ToArray();
            var shadowMask = textures.Where(p => p.name.Contains(m_ShadowMaskIdentity)).ToArray();

            var sanityCount = lightmapColors.Length;
            if (sanityCount == 0)
            {
                Debug.LogWarning("Invalid lightmap path: " + directory);
                return false;
            }
            
            Func<ELightmap,bool> SanityCheck = type => !m_LoadLightmaps.IsFlagEnable(type) || type switch
            {
                ELightmap.Color => lightmapColors.Length == sanityCount,
                ELightmap.Directional => directionalLightmaps.Length == sanityCount,
                ELightmap.ShadowMask => shadowMask.Length == sanityCount,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };

            if (!SanityCheck(ELightmap.Color) || !SanityCheck(ELightmap.Directional) || !SanityCheck(ELightmap.ShadowMask))
            {
                Debug.LogWarning("Invalid lightmap path: " + directory);
                return false;
            }
            
            Func<ELightmap, int,Texture2D> GetTexture = (type, index) =>
            {
                return m_LoadLightmaps.IsFlagEnable(type) ? type switch
                {
                    ELightmap.Color => lightmapColors[index],
                    ELightmap.Directional => directionalLightmaps[index],
                    ELightmap.ShadowMask => shadowMask.ToArray()[index],
                    _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
                } : null;
            };
            
            var lightmapData = new LightmapData[lightmapColors.Length];
            for (var i = 0; i < lightmapColors.Length; i++)
            {
                lightmapData[i] = new LightmapData
                {
                    lightmapColor = lightmapColors[i],
                    lightmapDir = GetTexture(ELightmap.Directional,i),
                    shadowMask = GetTexture(ELightmap.ShadowMask,i)
                };
            }

            LightmapSettings.lightmaps = lightmapData;
            LightmapSettings.lightmapsMode = m_LoadLightmaps.IsFlagEnable(ELightmap.Directional) ? LightmapsMode.NonDirectional : LightmapsMode.CombinedDirectional;
            return true;
        }
    }
}