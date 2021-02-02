using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rendering.ImageEffect
{
    public class PostEffect_DepthCircleArea : PostEffectBase<CameraEffect_DepthCircleArea, PostEffectParam_DepthCirCleArea>
    {
        [ImageEffectOpaque]
        new void OnRenderImage(RenderTexture _src, RenderTexture _dst) => base.OnRenderImage(_src, _dst);

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
        public Vector3 m_Origin;
        public float m_Radius;
        public float m_Outline;
        [ColorUsage(true,true)]public Color m_FillColor;
        [ColorUsage(true,true)]public Color m_EdgeColor;
        public Texture2D m_FillTexure;
        [RangeVector(-5,5)] public Vector2 m_FillTextureFlow;
        public float m_FillTextureScale;

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
        readonly int ID_Origin = Shader.PropertyToID("_Origin");
        readonly int ID_FillColor = Shader.PropertyToID("_FillColor");
        readonly int ID_FillTexture = Shader.PropertyToID("_FillTexture");
        readonly int ID_FillTextureScale = Shader.PropertyToID("_TextureScale");
        readonly int ID_FillTextureFlow = Shader.PropertyToID("_TextureFlow");
        readonly int ID_EdgeColor = Shader.PropertyToID("_EdgeColor");
        readonly int ID_SqrEdgeMin = Shader.PropertyToID("_SqrEdgeMin");
        readonly int ID_SqrEdgeMax = Shader.PropertyToID("_SqrEdgeMax");

        #region ShaderProperties
        #endregion
        protected override void OnValidate(PostEffectParam_DepthCirCleArea _params, Material _material)
        {
            base.OnValidate(_params, _material);
            float edgeMin = _params.m_Radius;
            float edgeMax = _params.m_Radius + _params.m_Outline;
            _material.SetFloat(ID_SqrEdgeMax, edgeMax * edgeMax);
            _material.SetFloat(ID_SqrEdgeMin, edgeMin * edgeMin);
            _material.SetVector(ID_Origin, _params.m_Origin);
            _material.SetColor(ID_FillColor, _params.m_FillColor);
            _material.SetColor(ID_EdgeColor, _params.m_EdgeColor);
            _material.SetTexture(ID_FillTexture, _params.m_FillTexure);
            _material.SetFloat(ID_FillTextureScale, _params.m_FillTextureScale);
            _material.SetVector(ID_FillTextureFlow, _params.m_FillTextureFlow);
        }
    }
}