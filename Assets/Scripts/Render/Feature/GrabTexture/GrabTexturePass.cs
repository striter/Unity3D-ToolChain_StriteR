using System;
using Rendering.PostProcess;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.Pipeline.GrabPass
{
    public class GrabTexturePass : ScriptableRenderPass
    {
        private GrabTextureFeature m_Feature;
        public GrabTextureData m_Data;

        private int m_TextureID;
        private RenderTargetIdentifier m_TextureRT;
        private int m_BlurTextureID;
        private RenderTargetIdentifier m_BlurTextureRT;

        private RTHandle m_TargetRT;
        private RenderTextureDescriptor m_TargetDescriptor;
        public static GrabTexturePass Spawn(GrabTextureFeature _feature,GrabTextureData _data) => TObjectPool.ObjectPool<GrabTexturePass>.Spawn().Setup(_feature,_data);

        GrabTexturePass Setup(GrabTextureFeature _feature,GrabTextureData _data)
        {
            m_Data = _data;
            m_Feature = _feature;

            var passEvent = RenderPassEvent.BeforeRenderingOpaques;
            if (_data.mode == EGrabTextureMode.CopyPass)
                passEvent = _data.renderPassEvent;
            this.renderPassEvent = passEvent;
            m_TextureID = Shader.PropertyToID(_data.textureName);
            m_TextureRT = new RenderTargetIdentifier(m_TextureID);
            m_BlurTextureID = Shader.PropertyToID(_data.textureName + "_Blur");
            m_BlurTextureRT = new RenderTargetIdentifier(m_BlurTextureID);
            return this;
        }
        
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            base.Configure(cmd, cameraTextureDescriptor);

            var downSample = math.max(m_Data.downSample, 1);
            cameraTextureDescriptor.height /= downSample;
            cameraTextureDescriptor.width /= downSample;
            m_TargetDescriptor = cameraTextureDescriptor;
            cmd.GetTemporaryRT(m_TextureID, cameraTextureDescriptor);
            m_TargetRT = RTHandles.Alloc(m_TextureRT);
            if (m_Data.blurData.Validate())
            {
                cmd.GetTemporaryRT(m_BlurTextureID, m_TargetDescriptor, FilterMode.Bilinear);
                m_TargetRT = RTHandles.Alloc(m_BlurTextureRT);
            }
            ConfigureTarget(m_TargetRT);
            ConfigureClear(ClearFlag.All, Color.black);
        }

        public override void Execute(ScriptableRenderContext _context, ref RenderingData _renderingData)
        {
            var buffer = CommandBufferPool.Get($"Grab Texture {m_Data.textureName}");
            switch (m_Data.mode)
            {
                case EGrabTextureMode.Redraw:
                {
                    var drawingSettings = UPipeline.CreateDrawingSettings(true, _renderingData.cameraData.camera);
                    drawingSettings.perObjectData = (PerObjectData)int.MaxValue;
                    var filterSettings = new FilteringSettings(RenderQueueRange.all) { layerMask = m_Data.renderMask };
                    _context.DrawRenderers(_renderingData.cullResults, ref drawingSettings, ref filterSettings);
                }
                    break;
                case EGrabTextureMode.CopyPass:
                {
                    buffer.Blit(_renderingData.cameraData.renderer.cameraColorTargetHandle, m_TargetRT);
                }
                    break;
                default:
                {
                    Debug.LogError("Invalid GrabTextureMode:" + m_Data.mode);
                }
                    break;
            }

            if (m_Feature.m_Config.blurActive && m_Data.blurData.Validate())
                FBlursCore.Instance.Execute(m_TargetDescriptor, ref m_Data.blurData,buffer, m_TargetRT, m_TextureRT,_context,ref _renderingData);

            _context.ExecuteCommandBuffer(buffer);
            CommandBufferPool.Release(buffer);
        }

        public override void FrameCleanup(CommandBuffer _cmd)
        {
            base.FrameCleanup(_cmd);
            if (m_Data.blurData.Validate())
                _cmd.ReleaseTemporaryRT(m_BlurTextureID);
            _cmd.ReleaseTemporaryRT(m_TextureID);
            TObjectPool.ObjectPool<GrabTexturePass>.Recycle(this);
        }
    }
}