using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rendering.ImageEffect
{
    public class PostEffect_DepthCircleScan : PostEffectBase<CameraEffect_DepthCircleScan, CameraEffectParam_DepthCircleScan>
    {
        public override bool m_IsOpaqueProcess => true;
        SingleCoroutine m_ScanCoroutine;
        public void StartDepthScanCircle(Vector3 origin,  float radius = 20, float duration = 1.5f)
        {
            if (m_ScanCoroutine==null)
                m_ScanCoroutine =  CoroutineHelper.CreateSingleCoroutine();
            m_ScanCoroutine.Stop();
            enabled = true;
            m_EffectData.m_Origin = origin;
            m_ScanCoroutine.Start(TIEnumerators.ChangeValueTo((float value) => {
                m_EffectData.m_Elapse= radius * value; 
                OnValidate();
            }, 0, 1, duration, () => { enabled = false; }));
        }

#if UNITY_EDITOR
        public bool m_DrawGizmos = true;
        private void OnDrawGizmos()
        {
            if (!m_DrawGizmos)
                return;

            Gizmos.color = Color.white;
            Gizmos.DrawSphere(m_EffectData.m_Origin, .2f);
            Gizmos.color = m_EffectData.m_Color;
            Gizmos.DrawWireSphere(m_EffectData.m_Origin, m_EffectData.m_Elapse);
            Gizmos.DrawWireSphere(m_EffectData.m_Origin, m_EffectData.m_Elapse + m_EffectData.m_Width);
        }
#endif
    }

    [Serializable]
    public struct CameraEffectParam_DepthCircleScan
    {
        public Vector3 m_Origin;
        [ColorUsage(true,true)]public Color m_Color;
        public float m_Elapse;
        [Range(0,20)]public float m_Width;
        [Range(0.01f,2)]public float m_FadingPow;

        public Texture2D m_MaskTexture;
        [MFold(nameof(m_MaskTexture))] public float m_MaskTextureScale;
        public static readonly CameraEffectParam_DepthCircleScan m_Default = new CameraEffectParam_DepthCircleScan()
        {
            m_Origin = Vector3.zero,
            m_Color = Color.green,
            m_Elapse = 5f,
            m_Width = 2f,
            m_FadingPow = .8f,
            m_MaskTextureScale = 1f,
        };
    }

    public class CameraEffect_DepthCircleScan:ImageEffectBase<CameraEffectParam_DepthCircleScan>
    {
        #region ShaderProperties
        static readonly int ID_Origin = Shader.PropertyToID("_Origin");
        static readonly int ID_Color = Shader.PropertyToID("_Color");
        static readonly int ID_FadingPow = Shader.PropertyToID("_FadingPow");
        const string KW_Mask = "_MASK_TEXTURE";
        static readonly int ID_Texture = Shader.PropertyToID("_MaskTexture");
        static readonly int ID_TexScale = Shader.PropertyToID("_MaskTextureScale");
        static readonly int ID_MinSqrDistance = Shader.PropertyToID("_MinSqrDistance");
        static readonly int ID_MaxSqrDistance = Shader.PropertyToID("_MaxSqrDistance");
        #endregion
        protected override void OnValidate(CameraEffectParam_DepthCircleScan _params, Material _material)
        {
            base.OnValidate(_params, _material);
            _material.SetVector(ID_Origin, _params.m_Origin);
            _material.SetColor(ID_Color, _params.m_Color);
            float minDistance = _params.m_Elapse;
            float maxDistance = _params.m_Elapse + _params.m_Width;
            _material.SetFloat(ID_MinSqrDistance, minDistance * minDistance);
            _material.SetFloat(ID_MaxSqrDistance, maxDistance * maxDistance);
            _material.SetFloat(ID_FadingPow, _params.m_FadingPow);

            bool maskEnable = _params.m_MaskTexture != null;
            _material.EnableKeyword(KW_Mask,maskEnable);
            if(maskEnable)
            {
                _material.SetTexture(ID_Texture, _params.m_MaskTexture);
                _material.SetFloat(ID_TexScale, _params.m_MaskTextureScale);
            }
        }
    }
}