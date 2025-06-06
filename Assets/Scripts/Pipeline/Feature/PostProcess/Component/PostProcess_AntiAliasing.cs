using System;
using System.Collections.Generic;
using System.Linq.Extensions;
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

        private DAntiAliasing m_AliasingData;
        private DAntiAliasingCore m_AntiAliasingPassCore;

        public PostProcess_AntiAliasing(DAntiAliasing _data,SRP_TAASetupPass _taaPass)
        {
            m_AliasingData = _data;
            m_AntiAliasingPassCore = new DAntiAliasingCore(_taaPass);
        }

        public void Dispose()
        {
            m_AntiAliasingPassCore.Destroy();
        }
        
        public void Configure(CommandBuffer _buffer, RenderTextureDescriptor _descriptor) => m_AntiAliasingPassCore.Configure(_buffer, _descriptor, ref m_AliasingData);
        public void Execute(CommandBuffer _buffer, RenderTargetIdentifier _src, RenderTargetIdentifier _dst,
            RenderTextureDescriptor _executeData, ScriptableRenderContext _context, ref RenderingData _renderingData) => 
            m_AntiAliasingPassCore.Execute(_executeData,ref m_AliasingData,_buffer,_src,_dst, _context,ref _renderingData);

        public void FrameCleanUp(CommandBuffer _buffer) => m_AntiAliasingPassCore.FrameCleanUp(_buffer,ref m_AliasingData);
        public void ValidateParameters() => m_AntiAliasingPassCore.OnValidate(ref m_AliasingData);

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
    public struct DAntiAliasing:IPostProcessParameter
    {
        public EAntiAliasing mode;
        [Foldout(nameof(mode),EAntiAliasing.FXAA)] public EFXAA fxaa;
        [Foldout(nameof(mode),EAntiAliasing.FXAA)] public bool m_AdditionalSample;
        [Foldout(nameof(mode),EAntiAliasing.FXAA)] public bool m_UseDepth;
        [Foldout(nameof(mode),EAntiAliasing.FXAA)] [Range(.01f,1f)] public float m_ContrastSkip;
        [Foldout(nameof(mode),EAntiAliasing.FXAA)] [Range(.01f,1f)] public float m_RelativeSkip;
        [Foldout(nameof(mode),EAntiAliasing.FXAA, nameof(fxaa),new object[]{EFXAA.SubPixel,EFXAA.Both})] [Range(.1f,2f)] public float m_SubPixelBlend;
        
        [Foldout(nameof(mode),EAntiAliasing.TAA)] [Range(0,1)] public float blend;
        public bool Validate() =>mode != EAntiAliasing.None;
        public static DAntiAliasing kDefault = new DAntiAliasing()
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

    public class SRP_TAASetupPass : ScriptableRenderPass
    {
        private const int kJitterAmount = 16;
        private uint jitterIndex = 0;

        private bool m_FirstBuffer = true;
        private static readonly int kHistoryBufferID = Shader.PropertyToID("_HistoryBuffer");
        private static readonly float2[] kJitters = new float2[kJitterAmount].Remake((i,p)=>ULowDiscrepancySequences.Halton2D((uint)i) - .5f);
        private static readonly Dictionary<int, TAAHistoryBuffer> m_Buffers = new();
        
        class TAAHistoryBuffer
        {
            private static uint historyBufferIndex = 0u;
            public RenderTextureDescriptor descriptor;
            public RenderTexture buffer;

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


        public void Dispose()
        {
            foreach (var buffer in m_Buffers.Values)
                buffer.Dispose();
            m_Buffers.Clear();
        }

        private TAAHistoryBuffer currentBuffer;
        public override void Execute(ScriptableRenderContext _context, ref RenderingData _renderingData)
        {
            var camera = _renderingData.cameraData.camera;
            
            var jitter = kJitters[jitterIndex];
            var projectionMatrix =  camera.projectionMatrix;
            var viewMatrix = camera.worldToCameraMatrix;
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
                currentBuffer = new TAAHistoryBuffer(_renderingData.cameraData.cameraTargetDescriptor);
                m_Buffers.Add(instanceID,currentBuffer);
                m_FirstBuffer = true;
            }
            
            var cmd = CommandBufferPool.Get("TAA PrePass");
            cmd.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
            cmd.SetGlobalTexture(kHistoryBufferID,currentBuffer.buffer);
            
            _context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public void ExecuteBuffer(CommandBuffer _cmd, RenderTargetIdentifier _src, RenderTargetIdentifier _dst,RenderTextureDescriptor _descriptor,
            ref DAntiAliasing _data,Material _material,ref RenderingData _renderingData)
        {
            if (m_FirstBuffer)
            {
                _cmd.Blit(_src,currentBuffer.buffer);
                m_FirstBuffer = false;
            }
            
            _cmd.SetGlobalFloat("_Blend",_data.blend);
            _cmd.Blit(_src,_dst,_material,1);
            _cmd.Blit(_dst,currentBuffer.buffer);
            _cmd.SetViewProjectionMatrices(_renderingData.cameraData.camera.worldToCameraMatrix,_renderingData.cameraData.camera.projectionMatrix);
            jitterIndex = (jitterIndex + 1) % kJitterAmount;
        }
    }
    
    public class DAntiAliasingCore : PostProcessCore<DAntiAliasing>
    {
        const string kFXAA_AdditionalSample = "_FXAA_ADDITIONAL_SAMPLE";
        const string kFXAA_Depth = "_FXAA_DEPTH";
        const string kFXAA_SubPixel = "_FXAA_SUBPIXEL";
        const string KW_FXAAEdgeBlend = "_FXAA_EDGE";
        readonly int kConstrastSkip = Shader.PropertyToID("_FXAAContrastSkip");
        readonly int kRelativeSkip = Shader.PropertyToID("_FXAARelativeSkip");
        readonly int kBlendStrength = Shader.PropertyToID("_FXAABlendStrength");

        private SRP_TAASetupPass m_TAAPass;
        public DAntiAliasingCore(SRP_TAASetupPass _taaPass)
        {
            m_TAAPass = _taaPass;
        }

        public override void OnValidate(ref DAntiAliasing _data)
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

        public override void Execute(RenderTextureDescriptor _descriptor, ref DAntiAliasing _data, CommandBuffer _buffer,
            RenderTargetIdentifier _src, RenderTargetIdentifier _dst, ScriptableRenderContext _context, ref RenderingData _renderingData)
        {
            switch (_data.mode)
            {
                case EAntiAliasing.FXAA:base.Execute(_descriptor, ref _data, _buffer, _src, _dst, _context, ref _renderingData);break;
                case EAntiAliasing.TAA:m_TAAPass.ExecuteBuffer(_buffer,_src,_dst,_descriptor,ref _data,m_Material,ref _renderingData);break;
            }

        }
    }

}