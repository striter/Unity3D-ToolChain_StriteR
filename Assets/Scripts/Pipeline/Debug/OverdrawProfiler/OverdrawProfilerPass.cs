using Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Render.Debug
{
    public class OverdrawProfilerPass : ScriptableRenderPass
    {
        public OverdrawProfilerData m_Data { get; private set; }

        private static PassiveInstance<Material> m_Material = new PassiveInstance<Material>(()=>CoreUtils.CreateEngineMaterial(Shader.Find("Hidden/OverdrawProfiler")));
        private static readonly int kBlitTempRT = Shader.PropertyToID("_OverdrawProfilerBlitTempRT");
        private static readonly int kOverdrawAlphaEnd = Shader.PropertyToID("_OverdrawAlphaEnd");
        private static readonly int kOverdrawAlpha = Shader.PropertyToID("_OverdrawAlpha");
        private static readonly int kIncrementPerStack = Shader.PropertyToID("_IncrementPerStack");
        private static readonly int kOverdrawColor = Shader.PropertyToID("_OverdrawColor");

        public OverdrawProfilerPass(OverdrawProfilerData _data)
        {
            m_Data = _data;
            renderPassEvent = _data.m_Event;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            base.OnCameraSetup(cmd, ref renderingData);
            m_Material.Value.SetColor(kOverdrawColor, m_Data.m_Color);
            var stackEachDraw = 0.05f;
            m_Material.Value.SetFloat(kIncrementPerStack, stackEachDraw);
            var stackNormalizeMin = stackEachDraw * m_Data.m_Stack.start;
            var stackNormalizeMax = stackEachDraw * m_Data.m_Stack.end;
            m_Material.Value.SetFloat(kOverdrawAlpha, stackNormalizeMin);
            m_Material.Value.SetFloat(kOverdrawAlphaEnd, stackNormalizeMax);
        }

        public override void Execute(ScriptableRenderContext _context, ref RenderingData _renderingData)
        {
            var clearBuffer = CommandBufferPool.Get("Overdraw Profile Pass");
            clearBuffer.BeginSample("Init");
            
            var descriptor = _renderingData.cameraData.cameraTargetDescriptor;
            descriptor.colorFormat = RenderTextureFormat.RHalf;
            descriptor.depthBufferBits = 0;
            clearBuffer.GetTemporaryRT(kBlitTempRT,descriptor);
            clearBuffer.SetRenderTarget(kBlitTempRT, _renderingData.cameraData.renderer.cameraDepthTargetHandle);
            clearBuffer.ClearRenderTarget(RTClearFlags.Color,Color.clear,0,0);
            _context.ExecuteCommandBuffer(clearBuffer);
            
            var drawingSettings = UPipeline.CreateDrawingSettings(true, _renderingData.cameraData.camera);
            drawingSettings.perObjectData = PerObjectData.None;
            drawingSettings.overrideMaterial = m_Material;
            var filterSettings = new FilteringSettings(RenderQueueRange.transparent) { layerMask = m_Data.m_Mask };
            _context.DrawRenderers(_renderingData.cullResults, ref drawingSettings, ref filterSettings);

            var blitBuffer = CommandBufferPool.Get("Blit");
            blitBuffer.Blit(kBlitTempRT, _renderingData.cameraData.renderer.cameraColorTargetHandle,m_Material,1);
            blitBuffer.SetRenderTarget(_renderingData.cameraData.renderer.cameraColorTargetHandle, _renderingData.cameraData.renderer.cameraDepthTargetHandle);
            blitBuffer.ReleaseTemporaryRT(kBlitTempRT);
            blitBuffer.EndSample("Init");

            _context.ExecuteCommandBuffer(blitBuffer);
            CommandBufferPool.Release(blitBuffer);
            
        }
    }
}