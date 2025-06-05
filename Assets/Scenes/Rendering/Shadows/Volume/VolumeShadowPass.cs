using Rendering;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Examples.Rendering.Shadows.Volume
{
    public class VolumeShadowPass : ScriptableRenderPass
    {
        private VolumeShadowData m_Data;
        private Material m_Material;
        private static readonly int kVolumeShadow = Shader.PropertyToID("_VolumeShadowTexture");
        private static readonly RenderTargetIdentifier kVolumeShadowRT = new RenderTargetIdentifier(kVolumeShadow);
        private static readonly int kShadowVolumeParams = Shader.PropertyToID("_ShadowVolumeParams");
        private static readonly string kKeyword = "Volume Shadow";
        public VolumeShadowPass Setup(VolumeShadowData _data, Material _material)
        {
            m_Data = _data;
            m_Material = _material;
            renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
            return this;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            base.Configure(cmd, cameraTextureDescriptor);
            cameraTextureDescriptor.colorFormat = RenderTextureFormat.R8;
            cameraTextureDescriptor.depthBufferBits = 0;
            cmd.GetTemporaryRT(kVolumeShadow, cameraTextureDescriptor);
            cmd.EnableKeyword(kKeyword,true);
        }

        public override void Execute(ScriptableRenderContext _context, ref RenderingData _renderingData)
        {
            if (!m_Material)
                return;
            
            var cmd = CommandBufferPool.Get(kKeyword);
            cmd.BeginSample(kKeyword);
            cmd.SetGlobalVector(kShadowVolumeParams,new float4(m_Data.bias,m_Data.normal,0,0));
            _context.ExecuteCommandBuffer(cmd);
            var drawingSettings = UPipeline.CreateDrawingSettings(true, _renderingData.cameraData.camera);
            drawingSettings.perObjectData = PerObjectData.None;
            drawingSettings.overrideMaterial = m_Material;
            drawingSettings.overrideMaterialPassIndex = 0;
            var filterSettings = new FilteringSettings(RenderQueueRange.opaque) { layerMask = _renderingData.cameraData.camera.cullingMask };
            _context.DrawRenderers(_renderingData.cullResults, ref drawingSettings, ref filterSettings);
            
            drawingSettings.overrideMaterialPassIndex = 1;
            _context.DrawRenderers(_renderingData.cullResults, ref drawingSettings, ref filterSettings);
            
            cmd.Clear();
            cmd.SetRenderTarget(kVolumeShadowRT,_renderingData.cameraData.renderer.cameraDepthTargetHandle);
            cmd.ClearRenderTarget(false,true,Color.black);
            cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, m_Material,0,2);
            cmd.EndSample(kKeyword);
            _context.ExecuteCommandBuffer(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            base.FrameCleanup(cmd);
            cmd.EnableKeyword(kKeyword,false);
            cmd.ReleaseTemporaryRT(kVolumeShadow);
        }
    }
}