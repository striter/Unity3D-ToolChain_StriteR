using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Rendering
{
    public class CommandBufferBase
    {
        protected virtual CameraEvent m_CameraEvent => 0;
        CommandBuffer m_Buffer;
        public bool m_Enabled => m_Manager != null;
        public CameraRenderEffectManager m_Manager { get; private set; } = null;
        public void Play(CameraRenderEffectManager _manager)
        {
            if (!m_Enabled)
            {
                m_Manager = _manager;
                m_Buffer = new CommandBuffer() { name = this.GetType().Name };
                OnBufferInit(m_Buffer);
                m_Manager.m_Camera.AddCommandBuffer(m_CameraEvent, m_Buffer);
                return;
            }
;
            m_Manager.m_Camera.RemoveCommandBuffer(m_CameraEvent, m_Buffer);
            m_Buffer.Clear();
            OnBufferInit(m_Buffer);
            m_Manager.m_Camera.AddCommandBuffer(m_CameraEvent, m_Buffer);
        }

        public void OnDisable()
        {
            if (!m_Enabled)
                return;

            m_Manager.m_Camera.RemoveCommandBuffer(m_CameraEvent, m_Buffer);
            m_Manager = null;

            OnBufferDestroy(m_Buffer);
            m_Buffer.Release();
            m_Buffer = null;
        }

        protected virtual void OnBufferInit(CommandBuffer _buffer)
        {

        }
        protected virtual void OnBufferDestroy(CommandBuffer _buffer)
        {

        }
    }
    public class GeometryCopyBuffer : CommandBufferBase
    {
        #region ShaderProperties
        static readonly int ID_GeometryTexture = Shader.PropertyToID("_CameraGeometryTexture");
        #endregion
        protected override CameraEvent m_CameraEvent => CameraEvent.AfterForwardOpaque;

        RenderTexture m_CopyTexture;
        Material m_BlurMaterial;
        protected override void OnBufferInit(CommandBuffer _buffer)
        {
            base.OnBufferInit(_buffer);
            m_CopyTexture = RenderTexture.GetTemporary(m_Manager.m_Camera.pixelWidth / 2, m_Manager.m_Camera.pixelHeight / 2, 0);
            m_CopyTexture.filterMode = FilterMode.Point;
            _buffer.Blit(BuiltinRenderTextureType.CurrentActive, m_CopyTexture);
            _buffer.SetGlobalTexture(ID_GeometryTexture, m_CopyTexture);

        }
        protected override void OnBufferDestroy(CommandBuffer _buffer)
        {
            base.OnBufferDestroy(_buffer);
            RenderTexture.ReleaseTemporary(m_CopyTexture);
        }
    }

    public class GeometryCopyBlurBuffer:CommandBufferBase
    {
        ImageEffect_Blurs m_Blur;
        protected override CameraEvent m_CameraEvent => CameraEvent.AfterForwardOpaque;
        #region ShaderProperties
        static readonly int ID_BlurSize = Shader.PropertyToID("_BlurSize");
        static readonly int ID_GeometryBlurTexture = Shader.PropertyToID("_CameraGeometryBlurTexture");
        #endregion
        RenderTexture m_TempTexture1, m_TempBlurTexture2;
        
        public GeometryCopyBlurBuffer InitBlur(Func< ImageEffectParams_Blurs> _GetParams)
        {
            m_Blur = new ImageEffect_Blurs(_GetParams);
            m_Blur.DoValidate();
            return this;
        }

        protected override void OnBufferInit(CommandBuffer _buffer)
        {
            base.OnBufferInit(_buffer);
            ImageEffectParams_Blurs _params = m_Blur.GetParams();

            if(_params.blurType== ImageEffect_Blurs.enum_BlurType.AverageSinglePass)
                return;

            int rtW= m_Manager.m_Camera.pixelWidth / _params.downSample;
            int rtH= m_Manager.m_Camera.pixelHeight / _params.downSample;

            m_TempTexture1 = RenderTexture.GetTemporary(rtW, rtH, 0, RenderTextureFormat.ARGB32);
            m_TempTexture1.filterMode = FilterMode.Bilinear;
            m_TempTexture1.name = "Geometry Blur Copy 1";

            m_TempBlurTexture2 = RenderTexture.GetTemporary(rtW, rtH, 0, RenderTextureFormat.ARGB32);
            m_TempBlurTexture2.filterMode = FilterMode.Bilinear;
            m_TempBlurTexture2.name = "Geometry Blur Copy 2";

            _buffer.Blit(BuiltinRenderTextureType.CurrentActive, m_TempTexture1);
            m_Blur.m_Material.SetFloat(ID_BlurSize, _params.blurSize);
            for (int i = 0; i < _params.iteration; i++)
            {
                int passStart = ((int)_params.blurType - 1) * 2;
                _buffer.Blit(m_TempTexture1, m_TempBlurTexture2, m_Blur.m_Material, passStart);
                _buffer.Blit(m_TempBlurTexture2, m_TempTexture1, m_Blur.m_Material, passStart + 1);
            }

            _buffer.SetGlobalTexture(ID_GeometryBlurTexture, m_TempTexture1);
        }

        protected override void OnBufferDestroy(CommandBuffer _buffer)
        {
            base.OnBufferDestroy(_buffer);
            RenderTexture.ReleaseTemporary(m_TempTexture1);
            RenderTexture.ReleaseTemporary(m_TempBlurTexture2);
            m_Blur.OnDestory();
        }
    }
}