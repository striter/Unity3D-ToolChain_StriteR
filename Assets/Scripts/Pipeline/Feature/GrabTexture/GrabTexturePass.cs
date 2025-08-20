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
        private GrabTextureBehaviour m_Behaviour;
        public GrabTextureData m_Data;

        private int m_BlurTextureID;
        private RenderTargetIdentifier m_BlurTextureRT;
        private RenderTextureDescriptor m_TargetDescriptor;
        public GrabTexturePass Setup(GrabTextureBehaviour _behaviour)
        {
            m_Behaviour = _behaviour;
            m_Data = _behaviour.m_Data;
            this.renderPassEvent = _behaviour.m_Data.renderPassEvent;
            m_BlurTextureID = Shader.PropertyToID(m_Data.textureName + "_Blur");
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
        }

        public override void Execute(ScriptableRenderContext _context, ref RenderingData _renderingData)
        {
            var renderBuffer = m_Behaviour.GetBuffer(_renderingData.cameraData.camera, m_TargetDescriptor);
            var buffer = CommandBufferPool.Get($"Grab Texture {m_Data.textureName}");
            if (renderBuffer.Capture(m_Behaviour))
            {
                if (m_Data.blurData.Validate())
                {
                    buffer.GetTemporaryRT(m_BlurTextureID, m_TargetDescriptor, FilterMode.Bilinear);
                    buffer.Blit(_renderingData.cameraData.renderer.cameraColorTargetHandle, m_BlurTextureRT);
                    FBlursCore.Instance.Execute(m_TargetDescriptor, ref m_Data.blurData,buffer, m_BlurTextureRT,renderBuffer.texture,_context,ref _renderingData);
                }
                else
                {
                    buffer.Blit(_renderingData.cameraData.renderer.cameraColorTargetHandle, renderBuffer.texture);
                }
            }
            buffer.SetGlobalTexture(m_Data.textureName, renderBuffer.texture);
            _context.ExecuteCommandBuffer(buffer);
            CommandBufferPool.Release(buffer);
        }

        public override void FrameCleanup(CommandBuffer _cmd)
        {
            base.FrameCleanup(_cmd);
            if (m_Data.blurData.Validate())
                _cmd.ReleaseTemporaryRT(m_BlurTextureID);
        }
    }
}