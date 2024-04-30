using System;
using System.ComponentModel;
using UnityEngine;

namespace Rendering.Lightmap
{
    [Flags]
    public enum ELightmap
    {
        Color = 1,
        Directional = 2,
        ShadowMask = 4,
    }
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

        public Texture2D this[ELightmap _type] => _type switch {ELightmap.Color => color, ELightmap.Directional => directional, ELightmap.ShadowMask => shadowMask, _ => throw new InvalidEnumArgumentException()};
        public static implicit operator LightmapData(LightmapTextures _texture) => new LightmapData() {lightmapColor = _texture.color, lightmapDir = _texture.directional, shadowMask = _texture.shadowMask};
        public static implicit operator LightmapTextures(LightmapData _data) => new LightmapTextures() {color = _data.lightmapColor, directional = _data.lightmapDir, shadowMask = _data.shadowMask};
    }
}