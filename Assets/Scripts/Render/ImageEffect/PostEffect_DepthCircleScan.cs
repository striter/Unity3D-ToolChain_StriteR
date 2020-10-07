using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rendering.ImageEffect
{
    public class PostEffect_DepthCircleScan : PostEffectBase<CameraEffect_DepthCircleScan>
    {
        public CameraEffectParam_DepthCircleScan m_Param;
        protected override CameraEffect_DepthCircleScan OnGenerateRequiredImageEffects() => new CameraEffect_DepthCircleScan(() => m_Param);

        SingleCoroutine m_ScanCoroutine;
        public void StartDepthScanCircle(Vector3 origin, Color scanColor, float width = 1f, float radius = 20, float duration = 1.5f)
        {
            if (m_ScanCoroutine==null)
                m_ScanCoroutine =  CoroutineHelper.CreateSingleCoroutine();
            m_ScanCoroutine.Stop();
            enabled = true;

            m_Param.m_Origin = origin;
            m_Param.m_Color = scanColor;
            CoroutineHelper.CreateSingleCoroutine().Start(TIEnumerators.ChangeValueTo((float value) => { m_Param.m_Elapse= radius * value; }, 0, 1, duration, () => { enabled = false; }));
        }
    }

    [System.Serializable]
    public class CameraEffectParam_DepthCircleScan:ImageEffectParamBase
    {
        public Vector3 m_Origin;
        public Color m_Color;
        public float m_Elapse=5;
        public float m_Width=5;
        public Texture2D m_Texture=null;
        public float m_TextureScale = 15f;
    }

    public class CameraEffect_DepthCircleScan:ImageEffectBase<CameraEffectParam_DepthCircleScan>
    {
        public CameraEffect_DepthCircleScan(Func<CameraEffectParam_DepthCircleScan> _GetParam):base(_GetParam)
        {

        }
        #region ShaderProperties
        readonly int ID_Origin = Shader.PropertyToID("_Origin");
        readonly int ID_Color = Shader.PropertyToID("_Color");
        readonly int ID_Texture = Shader.PropertyToID("_Texture");
        readonly int ID_TexScale = Shader.PropertyToID("_TextureScale");
        readonly int ID_MinSqrDistance = Shader.PropertyToID("_MinSqrDistance");
        readonly int ID_MaxSqrDistance = Shader.PropertyToID("_MaxSqrDistance");
        #endregion
        protected override void OnValidate(CameraEffectParam_DepthCircleScan _params)
        {
            base.OnValidate(_params);
            m_Material.SetVector(ID_Origin, _params.m_Origin);
            m_Material.SetColor(ID_Color, _params.m_Color);
            m_Material.SetTexture(ID_Texture, _params.m_Texture);
            m_Material.SetFloat(ID_TexScale, _params.m_TextureScale);
            float minDistance = _params.m_Elapse;
            float maxDistance = _params.m_Elapse + _params.m_Width;
            m_Material.SetFloat(ID_MinSqrDistance, minDistance * minDistance);
            m_Material.SetFloat(ID_MaxSqrDistance, maxDistance * maxDistance);
        }
    }
}