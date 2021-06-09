using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rendering.ImageEffect
{
    public class PostEffect_DepthCircleArea : PostEffectBase<CameraEffect_DepthCircleArea, PostEffectParam_DepthCirCleArea>
    {
        public override bool m_IsOpaqueProcess => true;

#if UNITY_EDITOR
        public bool m_DrawGizmos = true;
        protected void OnDrawGizmos()
        {
            if (!m_DrawGizmos)
                return;

            Gizmos.color = Color.white;
            Gizmos.DrawSphere(m_EffectData.m_Origin, .1f);

            Gizmos.color = m_EffectData.m_FillColor;
            Gizmos.DrawWireSphere(m_EffectData.m_Origin,m_EffectData.m_Radius);

            Gizmos.color = m_EffectData.m_EdgeColor;
            Gizmos.DrawWireSphere(m_EffectData.m_Origin,m_EffectData.m_Radius+m_EffectData.m_Outline);
        }
#endif
    }
    [Serializable]
    public struct PostEffectParam_DepthCirCleArea
    {
        [Position] public Vector3 m_Origin;
        public float m_Radius;
        public float m_Outline;
        [ColorUsage(true,true)]public Color m_FillColor;
        [ColorUsage(true,true)]public Color m_EdgeColor;
        public Texture2D m_FillTexure;
        [MFold(nameof(m_FillTexure)), RangeVector(-5,5)] public Vector2 m_FillTextureFlow;
        [MFold(nameof(m_FillTexure)),Clamp(0f)] public float m_FillTextureScale;

        public static readonly PostEffectParam_DepthCirCleArea m_Default = new PostEffectParam_DepthCirCleArea()
        {
            m_Origin = Vector3.zero,
            m_Radius = 5f,
            m_Outline = 1f,
            m_FillColor=Color.white,
            m_EdgeColor=Color.black,
            m_FillTextureFlow=Vector2.one,
            m_FillTextureScale=1f,
        };
    }

    public class CameraEffect_DepthCircleArea:ImageEffectBase<PostEffectParam_DepthCirCleArea>
    {
        #region ShaderProperties
        static readonly int ID_Origin = Shader.PropertyToID("_Origin");
        static readonly int ID_FillColor = Shader.PropertyToID("_FillColor");
        static readonly int ID_FillTexture = Shader.PropertyToID("_FillTexture");
        static readonly int ID_FillTextureScale = Shader.PropertyToID("_TextureScale");
        static readonly int ID_FillTextureFlow = Shader.PropertyToID("_TextureFlow");
        static readonly int ID_EdgeColor = Shader.PropertyToID("_EdgeColor");
        static readonly int ID_SqrEdgeMin = Shader.PropertyToID("_SqrEdgeMin");
        static readonly int ID_SqrEdgeMax = Shader.PropertyToID("_SqrEdgeMax");
        #endregion
        public override void OnValidate(PostEffectParam_DepthCirCleArea _data)
        {
            base.OnValidate(_data);
            float edgeMin = _data.m_Radius;
            float edgeMax = _data.m_Radius + _data.m_Outline;
            m_Material.SetFloat(ID_SqrEdgeMax, edgeMax * edgeMax);
            m_Material.SetFloat(ID_SqrEdgeMin, edgeMin * edgeMin);
            m_Material.SetVector(ID_Origin, _data.m_Origin);
            m_Material.SetColor(ID_FillColor, _data.m_FillColor);
            m_Material.SetColor(ID_EdgeColor, _data.m_EdgeColor);
            m_Material.SetTexture(ID_FillTexture, _data.m_FillTexure);
            m_Material.SetFloat(ID_FillTextureScale, _data.m_FillTextureScale);
            m_Material.SetVector(ID_FillTextureFlow, _data.m_FillTextureFlow);
        }
    }
}