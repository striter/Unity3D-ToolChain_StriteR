using System.Collections.Generic;
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
        private static readonly int kBlitTempRTID = Shader.PropertyToID("_OverdrawProfilerBlitTempRT");
        private static readonly int kOverdrawAlphaEndID = Shader.PropertyToID("_OverdrawAlphaEnd");
        private static readonly int kOverdrawAlphaID = Shader.PropertyToID("_OverdrawAlpha");
        private static readonly int kIncrementPerStackID = Shader.PropertyToID("_IncrementPerStack");
        private static readonly int kOverdrawColorID = Shader.PropertyToID("_OverdrawColor");
        private static readonly int kResultID = Shader.PropertyToID("_Result");
        private static readonly int kOverDrawRTID = Shader.PropertyToID("_OverDrawRT");

        private int m_KernelClear ,m_KernelCompute;
        private ComputeBuffer m_ComputeBuffer;
        private int[] m_Result = new int[2];

        private Dictionary<Camera, float> m_PixelDrawNormalize = new();

        public float QueryPixelDrawNormalize(Camera _camera) => m_PixelDrawNormalize.GetValueOrDefault(_camera, 0);
        

        private const float kIncrementPerStack = 0.05f;
        public OverdrawProfilerPass(OverdrawProfilerData _data)
        {
            m_Data = _data;
            renderPassEvent = _data.m_Event;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData _renderingData)
        {
            base.OnCameraSetup(cmd, ref _renderingData);
            m_Material.Value.SetColor(kOverdrawColorID, m_Data.m_Color);
            m_Material.Value.SetFloat(kIncrementPerStackID, kIncrementPerStack);
            var stackNormalizeMin = kIncrementPerStack * m_Data.m_Stack.start;
            var stackNormalizeMax = kIncrementPerStack * m_Data.m_Stack.end;
            m_Material.Value.SetFloat(kOverdrawAlphaID, stackNormalizeMin);
            m_Material.Value.SetFloat(kOverdrawAlphaEndID, stackNormalizeMax);

            if (m_Data.m_OverdrawCompute)
            {
                m_KernelClear = m_Data.m_OverdrawCompute.FindKernel("Clear");
                m_KernelCompute = m_Data.m_OverdrawCompute.FindKernel("Compute");
            }
            m_ComputeBuffer = new ComputeBuffer(2, sizeof(int));
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            base.OnCameraCleanup(cmd);
            m_ComputeBuffer?.Release();
            m_ComputeBuffer = null;
        }

        public override void Execute(ScriptableRenderContext _context, ref RenderingData _renderingData)
        {
            var clearBuffer = CommandBufferPool.Get("Overdraw Profile Pass");
            clearBuffer.BeginSample("Init");
            
            var descriptor = _renderingData.cameraData.cameraTargetDescriptor;
            descriptor.colorFormat = RenderTextureFormat.RHalf;
            descriptor.depthBufferBits = 0;
            clearBuffer.GetTemporaryRT(kBlitTempRTID,descriptor);
            clearBuffer.SetRenderTarget(kBlitTempRTID, _renderingData.cameraData.renderer.cameraDepthTargetHandle);
            clearBuffer.ClearRenderTarget(RTClearFlags.Color,Color.clear,0,0);
            _context.ExecuteCommandBuffer(clearBuffer);
            CommandBufferPool.Release(clearBuffer);
            
            var drawingSettings = UPipeline.CreateDrawingSettings(true, _renderingData.cameraData.camera);
            drawingSettings.perObjectData = PerObjectData.None;
            drawingSettings.overrideMaterial = m_Material;
            var filterSettings = new FilteringSettings(RenderQueueRange.transparent) { layerMask = m_Data.m_Mask };
            _context.DrawRenderers(_renderingData.cullResults, ref drawingSettings, ref filterSettings);

            if (m_Data.m_OverdrawCompute != null)
            {
                var computeShader = m_Data.m_OverdrawCompute;
                var computeBuffer = CommandBufferPool.Get("Compute Overdraw");
                computeBuffer.SetComputeBufferParam(computeShader,m_KernelClear,kResultID,m_ComputeBuffer);
                computeBuffer.DispatchCompute(computeShader,m_KernelClear,1,1,1);
                computeBuffer.SetComputeTextureParam(computeShader,m_KernelCompute,kOverDrawRTID,kBlitTempRTID);
                computeBuffer.SetComputeBufferParam(computeShader,m_KernelCompute,kResultID,m_ComputeBuffer);
                computeBuffer.SetComputeFloatParam(computeShader,kIncrementPerStackID,kIncrementPerStack);
                computeBuffer.DispatchCompute(computeShader,m_KernelCompute,descriptor.width / 8 ,descriptor.height / 8,1);
                _context.ExecuteCommandBuffer(computeBuffer);
                _context.Submit();

                m_ComputeBuffer.GetData(m_Result);
                var validPixels = m_Result[0];
                var pixelDrawed = m_Result[1];
                m_PixelDrawNormalize.TryAdd(_renderingData.cameraData.camera, 0);
                m_PixelDrawNormalize[_renderingData.cameraData.camera] = pixelDrawed / (float)validPixels;
            }

            var blitBuffer = CommandBufferPool.Get("Blit");
            blitBuffer.Blit(kBlitTempRTID, _renderingData.cameraData.renderer.cameraColorTargetHandle,m_Material,1);
            blitBuffer.SetRenderTarget(_renderingData.cameraData.renderer.cameraColorTargetHandle, _renderingData.cameraData.renderer.cameraDepthTargetHandle);
            blitBuffer.ReleaseTemporaryRT(kBlitTempRTID);
            blitBuffer.EndSample("Init");
            _context.ExecuteCommandBuffer(blitBuffer);
            CommandBufferPool.Release(blitBuffer);
        }
    }
}