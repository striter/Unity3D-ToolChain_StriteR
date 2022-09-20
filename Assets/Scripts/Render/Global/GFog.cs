using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rendering
{
    [Serializable]
    public struct GFog
    {
        [Clamp(0f)] public float m_DistanceFogBegin;
        [Clamp(0f)] public float m_DistanceFogDistance;
        [Clamp(-5f)]public float m_VerticalFogBegin;
        [Clamp(0f)]public float m_VerticalFogDistance;
        [ColorUsage(true, true)] public Color m_FogColor;

        public static readonly GFog kDefault = new GFog() {
            m_DistanceFogBegin = 20f,
            m_DistanceFogDistance = 5f,
            m_VerticalFogBegin = 0f,
            m_VerticalFogDistance = 2f,
            m_FogColor = Color.white,
        };
    
        private static readonly string kKeyword= "_FOG";
        private static readonly int kFogParameters=Shader.PropertyToID("_FogParameters");
        private static readonly int kFogColor=Shader.PropertyToID("_FogColor");
        public void Apply()
        {
            Shader.EnableKeyword(kKeyword);
            Shader.SetGlobalVector(kFogParameters,new Vector4(m_DistanceFogBegin,m_DistanceFogBegin + m_DistanceFogDistance,m_VerticalFogBegin, m_VerticalFogBegin + m_VerticalFogDistance));
            Shader.SetGlobalColor(kFogColor,m_FogColor);
        }

        public static void Dispose()
        {
            Shader.DisableKeyword(kKeyword);
        }
    }

}