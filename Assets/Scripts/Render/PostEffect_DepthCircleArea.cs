﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rendering.ImageEffect
{
    public class PostEffect_DepthCircleArea : PostEffectBase<CameraEffect_DepthCircleArea>
    {
        public PostEffectParam_DepthCirCleArea m_Param;
        protected override CameraEffect_DepthCircleArea OnGenerateRequiredImageEffects() => new CameraEffect_DepthCircleArea(() => m_Param);

        public void DoValidate() => m_Effect.DoValidate();
    }
    [System.Serializable]
    public class PostEffectParam_DepthCirCleArea:ImageEffectParamBase
    {
        public Vector3 m_Origin;
        public float Radius=5f;
        public float m_SqrOutline=1f;
        public Color m_FillColor;
        public Color m_EdgeColor;
        public Texture2D m_FillTexure;
        public Vector2 m_FillTextureFlow;
        public float m_FillTextureScale=1f;
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

        public CameraEffect_DepthCircleArea(Func<PostEffectParam_DepthCirCleArea> _GetParam) : base(_GetParam) { }
        #region ShaderProperties
        #endregion
        protected override void OnValidate(PostEffectParam_DepthCirCleArea _params)
        {
            base.OnValidate(_params);
            float sqrEdgeMin = _params.Radius;
            float sqrEdgeMax = _params.Radius + _params.m_SqrOutline;
            m_Material.SetFloat(ID_SqrEdgeMax, sqrEdgeMax * sqrEdgeMax);
            m_Material.SetFloat(ID_SqrEdgeMin, sqrEdgeMin * sqrEdgeMin);
            m_Material.SetVector(ID_Origin, _params.m_Origin);
            m_Material.SetColor(ID_FillColor, _params.m_FillColor);
            m_Material.SetColor(ID_EdgeColor, _params.m_EdgeColor);
            m_Material.SetTexture(ID_FillTexture, _params.m_FillTexure);
            m_Material.SetFloat(ID_FillTextureScale, _params.m_FillTextureScale);
            m_Material.SetVector(ID_FillTextureFlow, _params.m_FillTextureFlow);
        }
    }
}