using System;
using System.Collections;
using System.Collections.Generic;
using Rendering.Pipeline;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.PostProcess
{
    public class PostProcess_AntiAliasing : IPostProcessBehaviour
    {
        public string m_Name => "AntiAliasing";
        public bool m_OpaqueProcess => false;
        public bool m_Enabled => m_AliasingData.mode != EAntiAliasing.None;
        public EPostProcess Event => EPostProcess.AntiAliasing;

        private PPData_AntiAliasing m_AliasingData;
        private PPCore_AntiAliasing m_AntiAliasingCore;

        public PostProcess_AntiAliasing(PPData_AntiAliasing _data,SRP_TAAPass _taaPass)
        {
            m_AliasingData = _data;
            m_AntiAliasingCore = new PPCore_AntiAliasing(_taaPass);
        }

        public void Dispose()
        {
            m_AntiAliasingCore.Destroy();
        }
        
        public void Configure(CommandBuffer _buffer, RenderTextureDescriptor _descriptor) => m_AntiAliasingCore.Configure(_buffer, _descriptor, ref m_AliasingData);
        public void Execute(CommandBuffer _buffer, RenderTargetIdentifier _src, RenderTargetIdentifier _dst,
            RenderTextureDescriptor _executeData, ScriptableRenderer _renderer, ScriptableRenderContext _context,
            ref RenderingData _renderingData) => m_AntiAliasingCore.Execute(
            _executeData,ref m_AliasingData,_buffer,_src,_dst,
            _renderer,_context,ref _renderingData
            );

        public void FrameCleanUp(CommandBuffer _buffer) => m_AntiAliasingCore.FrameCleanUp(_buffer,ref m_AliasingData);
        public void ValidateParameters() => m_AntiAliasingCore.OnValidate(ref m_AliasingData);

    }

#region Parameters
    public enum EAntiAliasing
    {
        None,
        FXAA,
        TAA,
    }

    public enum EFXAA
    {
        SubPixel,
        EdgeDetect,
        Both,
    }

    
    [Serializable]
    public struct PPData_AntiAliasing:IPostProcessParameter
    {
        public EAntiAliasing mode;
        [MFoldout(nameof(mode),EAntiAliasing.FXAA)] public EFXAA fxaa;
        [MFoldout(nameof(mode),EAntiAliasing.FXAA)] public bool m_AdditionalSample;
        [MFoldout(nameof(mode),EAntiAliasing.FXAA)] public bool m_UseDepth;
        [MFoldout(nameof(mode),EAntiAliasing.FXAA)] [Range(.01f,1f)] public float m_ContrastSkip;
        [MFoldout(nameof(mode),EAntiAliasing.FXAA)] [Range(.01f,1f)] public float m_RelativeSkip;
        [MFoldout(nameof(mode),EAntiAliasing.FXAA, nameof(fxaa),new object[]{EFXAA.SubPixel,EFXAA.Both})] [Range(.1f,2f)] public float m_SubPixelBlend;
        
        public bool Validate() =>mode != EAntiAliasing.None;
        public static PPData_AntiAliasing kDefault = new PPData_AntiAliasing()
        {
            mode = EAntiAliasing.FXAA,
            fxaa = EFXAA.Both,
            m_ContrastSkip = .1f,
            m_RelativeSkip = .2f,
            m_SubPixelBlend = 1f,
            m_AdditionalSample = true,
        };
    }
#endregion

    public class SRP_TAAPass : ScriptableRenderPass
    {
        private const int kJitterAmount = 16;
        private uint jitterIndex = 0;

        private int kHistoryBufferID;
        private static readonly float2[] kJitters = SamplePattern2D.Halton(kJitterAmount);
        private static readonly Dictionary<int, TAAHistoryBuffer> m_Buffers = new Dictionary<int, TAAHistoryBuffer>();
        
        class TAAHistoryBuffer
        {
            private static uint historyBufferIndex = 0u;
            public RenderTextureDescriptor descriptor;
            public RenderTexture buffer;
            public float4x4 viewProjection;
            public float2 jitter;

            static RenderTextureDescriptor OutputDescriptor(RenderTextureDescriptor _descriptor)
            {
                _descriptor.msaaSamples = 1;
                _descriptor.depthBufferBits = 0;
                _descriptor.mipCount = -1;
                _descriptor.depthStencilFormat = GraphicsFormat.None;
                return _descriptor;
            }
            
            public bool Validate(RenderTextureDescriptor _descriptor)=>descriptor.Equals(OutputDescriptor(_descriptor));
            public TAAHistoryBuffer(RenderTextureDescriptor _descriptor)
            {
                descriptor = OutputDescriptor(_descriptor);
                
                buffer = RenderTexture.GetTemporary(descriptor);
                buffer.name = "_HistoryBuffer" + historyBufferIndex++;
            }
            
            public void Dispose()=> RenderTexture.ReleaseTemporary(buffer);
        }

        private bool m_FirstBuffer;
        
        public SRP_TAAPass()
        {
            kHistoryBufferID = Shader.PropertyToID("_HistoryBuffer");
            m_FirstBuffer = true;
        }

        public void Dispose()
        {
            foreach (var buffer in m_Buffers.Values)
                buffer.Dispose();
            m_Buffers.Clear();
        }

        private TAAHistoryBuffer currentBuffer;
        public override void Execute(ScriptableRenderContext context, ref RenderingData _renderingData)
        {
            var camera = _renderingData.cameraData.camera;
            
            var jitter = kJitters[jitterIndex];
            var projectionMatrix =  camera.projectionMatrix;
            var viewMatrix = camera.worldToCameraMatrix;
            var viewProjectionMatrix =  projectionMatrix * viewMatrix;
            projectionMatrix.m02 += jitter.x / camera.pixelWidth;
            projectionMatrix.m12 += jitter.y / camera.pixelHeight;

            var instanceID = _renderingData.cameraData.camera.GetInstanceID(); 
            var descriptor = _renderingData.cameraData.cameraTargetDescriptor;
            currentBuffer = default;
            if (m_Buffers.TryGetValue(instanceID,out currentBuffer)&&!currentBuffer.Validate(descriptor))
            {
                currentBuffer.Dispose();
                m_Buffers.Remove(instanceID);
            }

            if (!m_Buffers.ContainsKey(instanceID))
            {                
                currentBuffer = new TAAHistoryBuffer(_renderingData.cameraData.cameraTargetDescriptor){viewProjection = viewProjectionMatrix,jitter = jitter};
                m_Buffers.Add(instanceID,currentBuffer);
            }
            
            var cmd = CommandBufferPool.Get("TAA PrePass");
            cmd.SetViewProjectionMatrices(camera.worldToCameraMatrix, projectionMatrix);
            cmd.SetGlobalTexture(kHistoryBufferID,currentBuffer.buffer);
            cmd.SetGlobalFloat("_Blend",.1f);
            cmd.SetGlobalMatrix("_Matrix_VP_Pre",currentBuffer.viewProjection);
            cmd.SetGlobalVector("_Jitter_Pre",currentBuffer.jitter.to4());
            cmd.SetGlobalVector("_Jitter_Cur",jitter.to4());
            
            currentBuffer.viewProjection = viewProjectionMatrix;
            currentBuffer.jitter = jitter;
            
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public void ExecuteBuffer(CommandBuffer _buffer, RenderTargetIdentifier _src, RenderTargetIdentifier _dst,RenderTextureDescriptor _descriptor,
            ref PPData_AntiAliasing _data,Material _material,ref RenderingData _renderingData)
        {
            if (m_FirstBuffer)
            {
                _buffer.Blit(_src,currentBuffer.buffer);
                m_FirstBuffer = false;
            }

                
            
            _buffer.Blit(_src,_dst,_material,1);
            _buffer.Blit(_dst,currentBuffer.buffer);
            _buffer.SetViewProjectionMatrices(_renderingData.cameraData.camera.worldToCameraMatrix,_renderingData.cameraData.camera.projectionMatrix);
            jitterIndex = (jitterIndex + 1) % kJitterAmount;
        }
    }
    
    public class PPCore_AntiAliasing : PostProcessCore<PPData_AntiAliasing>
    {
        const string kFXAA_AdditionalSample = "_FXAA_ADDITIONAL_SAMPLE";
        const string kFXAA_Depth = "_FXAA_DEPTH";
        const string kFXAA_SubPixel = "_FXAA_SUBPIXEL";
        const string KW_FXAAEdgeBlend = "_FXAA_EDGE";
        readonly int kConstrastSkip = Shader.PropertyToID("_FXAAContrastSkip");
        readonly int kRelativeSkip = Shader.PropertyToID("_FXAARelativeSkip");
        readonly int kBlendStrength = Shader.PropertyToID("_FXAABlendStrength");

        private SRP_TAAPass m_TAAPass;
        public PPCore_AntiAliasing(SRP_TAAPass _taaPass)
        {
            m_TAAPass = _taaPass;
        }

        public override void OnValidate(ref PPData_AntiAliasing _data)
        {
            base.OnValidate(ref _data);
            if (_data.mode == EAntiAliasing.FXAA)
            {
                m_Material.SetFloat(kConstrastSkip,_data.m_ContrastSkip);
                m_Material.SetFloat(kRelativeSkip,_data.m_RelativeSkip);
                m_Material.EnableKeyword(kFXAA_Depth, _data.m_UseDepth);
                m_Material.EnableKeyword(kFXAA_AdditionalSample,_data.m_AdditionalSample);
            
                bool subPixel = _data.fxaa is EFXAA.SubPixel or EFXAA.Both  ;
                if (m_Material.EnableKeyword(kFXAA_SubPixel,subPixel))
                    m_Material.SetFloat(kBlendStrength,_data.m_SubPixelBlend);
                bool edge = _data.fxaa is EFXAA.Both or EFXAA.EdgeDetect;
                m_Material.EnableKeyword(KW_FXAAEdgeBlend, edge);
            }
        }

        public override void Execute(RenderTextureDescriptor _descriptor, ref PPData_AntiAliasing _data, CommandBuffer _buffer,
            RenderTargetIdentifier _src, RenderTargetIdentifier _dst, ScriptableRenderer _renderer,
            ScriptableRenderContext _context, ref RenderingData _renderingData)
        {
            switch (_data.mode)
            {
                case EAntiAliasing.FXAA:base.Execute(_descriptor, ref _data, _buffer, _src, _dst, _renderer, _context, ref _renderingData);break;
                case EAntiAliasing.TAA:m_TAAPass.ExecuteBuffer(_buffer,_src,_dst,_descriptor,ref _data,m_Material,ref _renderingData);break;
            }

        }
    }

}