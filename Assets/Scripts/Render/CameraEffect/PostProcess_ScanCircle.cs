using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rendering.ImageEffect
{
    public class PostProcess_ScanCircle : PostProcessComponentBase<PPCore_ScanCircle, PPData_CircleScan>
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
    public struct PPData_CircleScan
    {
        [Position] public Vector3 m_Origin;
        [ColorUsage(true,true)]public Color m_Color;
        public float m_Elapse;
        [Range(0,20)]public float m_Width;
        [Range(0.01f,2)]public float m_FadingPow;

        public Texture2D m_MaskTexture;
        [MFold(nameof(m_MaskTexture))] public float m_MaskTextureScale;
        public static readonly PPData_CircleScan m_Default = new PPData_CircleScan()
        {
            m_Origin = Vector3.zero,
            m_Color = Color.green,
            m_Elapse = 5f,
            m_Width = 2f,
            m_FadingPow = .8f,
            m_MaskTextureScale = 1f,
        };
    }

    public class PPCore_ScanCircle:PostProcessCore<PPData_CircleScan>
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
        public override void OnValidate(PPData_CircleScan _data)
        {
            base.OnValidate(_data);
            m_Material.SetVector(ID_Origin, _data.m_Origin);
            m_Material.SetColor(ID_Color, _data.m_Color);
            float minDistance = _data.m_Elapse;
            float maxDistance = _data.m_Elapse + _data.m_Width;
            m_Material.SetFloat(ID_MinSqrDistance, minDistance * minDistance);
            m_Material.SetFloat(ID_MaxSqrDistance, maxDistance * maxDistance);
            m_Material.SetFloat(ID_FadingPow, _data.m_FadingPow);

            bool maskEnable = _data.m_MaskTexture != null;
            m_Material.EnableKeyword(KW_Mask,maskEnable);
            if(maskEnable)
            {
                m_Material.SetTexture(ID_Texture, _data.m_MaskTexture);
                m_Material.SetFloat(ID_TexScale, _data.m_MaskTextureScale);
            }
        }
    }
}