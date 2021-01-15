using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rendering.ImageEffect
{
    public class PostEffect_DepthCircleScan : PostEffectBase<CameraEffect_DepthCircleScan, CameraEffectParam_DepthCircleScan>
    {

        SingleCoroutine m_ScanCoroutine;
        public void StartDepthScanCircle(Vector3 origin, Color scanColor, float width = 1f, float radius = 20, float duration = 1.5f)
        {
            if (m_ScanCoroutine==null)
                m_ScanCoroutine =  CoroutineHelper.CreateSingleCoroutine();
            m_ScanCoroutine.Stop();
            enabled = true;
            m_EffectData.m_Width = width;
            m_EffectData.m_Origin = origin;
            m_EffectData.m_Color = scanColor;
            m_ScanCoroutine.Start(TIEnumerators.ChangeValueTo((float value) => {
                m_EffectData.m_Elapse= radius * value; 
                OnValidate();
            }, 0, 1, duration, () => { enabled = false; }));
        }
    }

    [Serializable]
    public struct CameraEffectParam_DepthCircleScan
    {
        public Vector3 m_Origin;
        public Color m_Color;
        public float m_Elapse;
        public float m_Width;
        public Texture2D m_Texture;
        public float m_TextureScale;
        public static readonly CameraEffectParam_DepthCircleScan m_Default = new CameraEffectParam_DepthCircleScan()
        {
            m_Origin = Vector3.zero,
            m_Color = Color.green,
            m_Elapse = 5f,
            m_Width = 52f,
            m_TextureScale = 1f,
        };
    }

    public class CameraEffect_DepthCircleScan:ImageEffectBase<CameraEffectParam_DepthCircleScan>
    {
        #region ShaderProperties
        readonly int ID_Origin = Shader.PropertyToID("_Origin");
        readonly int ID_Color = Shader.PropertyToID("_Color");
        readonly int ID_Texture = Shader.PropertyToID("_Texture");
        readonly int ID_TexScale = Shader.PropertyToID("_TextureScale");
        readonly int ID_MinSqrDistance = Shader.PropertyToID("_MinSqrDistance");
        readonly int ID_MaxSqrDistance = Shader.PropertyToID("_MaxSqrDistance");
        #endregion
        protected override void OnValidate(CameraEffectParam_DepthCircleScan _params, Material _material)
        {
            base.OnValidate(_params, _material);
            _material.SetVector(ID_Origin, _params.m_Origin);
            _material.SetColor(ID_Color, _params.m_Color);
            _material.SetTexture(ID_Texture, _params.m_Texture);
            _material.SetFloat(ID_TexScale, _params.m_TextureScale);
            float minDistance = _params.m_Elapse;
            float maxDistance = _params.m_Elapse + _params.m_Width;
            _material.SetFloat(ID_MinSqrDistance, minDistance * minDistance);
            _material.SetFloat(ID_MaxSqrDistance, maxDistance * maxDistance);
        }
    }
}