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
        public bool OpaqueProcess => false;
        public bool m_Enabled => m_AliasingData.mode != EAntiAliasing.None;
        public EPostProcess Event => EPostProcess.AntiAliasing;

        private DAntiAliasing m_AliasingData;
        private DAntiAliasingCore m_AntiAliasingPassCore;

        public PostProcess_AntiAliasing(DAntiAliasing _data,TAASetupPass _taaPass)
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

        public bool Validate(ref RenderingData _renderingData) => m_AntiAliasingPassCore.Validate(ref _renderingData,ref m_AliasingData);
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
        [Foldout(nameof(mode),EAntiAliasing.FXAA)] public bool additionalSample;
        [Foldout(nameof(mode),EAntiAliasing.FXAA)] public bool useDepth;
        [Foldout(nameof(mode),EAntiAliasing.FXAA)] [Range(.01f,1f)] public float contrastSkip;
        [Foldout(nameof(mode),EAntiAliasing.FXAA)] [Range(.01f,1f)] public float relativeSkip;
        [Foldout(nameof(mode),EAntiAliasing.FXAA, nameof(fxaa),new object[]{EFXAA.SubPixel,EFXAA.Both})] [Range(.1f,2f)] public float subPixelBlend;
        
        [Foldout(nameof(mode),EAntiAliasing.TAA)] [Range(0,1)] public float blend;
        public bool Validate() =>mode != EAntiAliasing.None;
        public static DAntiAliasing kDefault = new DAntiAliasing()
        {
            mode = EAntiAliasing.FXAA,
            fxaa = EFXAA.Both,
            contrastSkip = .1f,
            relativeSkip = .2f,
            subPixelBlend = 1f,
            additionalSample = true,
        };
    }
#endregion

    public class TAASetupPass : ScriptableRenderPass
    {
        private const int kJitterAmount = 16;
        private uint jitterIndex = 0;

        private static readonly int kHistoryBufferID = Shader.PropertyToID("_HistoryBuffer");
        private static readonly float2[] kJitters = new float2[kJitterAmount].Remake((i,p)=>(ULowDiscrepancySequences.Halton2D((uint)i) - .5f) );
        private static readonly Dictionary<int, TAAHistoryBuffer> m_Buffers = new();
        private Camera m_Camera;
        class TAAHistoryBuffer
        {
            private static uint historyBufferIndex = 0u;
            public RenderTextureDescriptor descriptor;
            public RenderTexture renderTexture;
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
                
                renderTexture = RenderTexture.GetTemporary(descriptor);
                renderTexture.name = "_HistoryBuffer" + historyBufferIndex++;
            }
            
            public void Dispose()=> RenderTexture.ReleaseTemporary(renderTexture);
        }

        public TAASetupPass Setup(ref RenderingData _renderingData)
        {
            m_Camera = _renderingData.cameraData.camera;
            
            var jitter = kJitters[jitterIndex];
            jitterIndex = (jitterIndex + 1) % kJitterAmount;
            var projectionMatrix =  m_Camera.projectionMatrix;
            var jitterTranslation = Matrix4x4.Translate(new Vector3(
                jitter.x / _renderingData.cameraData.cameraTargetDescriptor.width,
                jitter.y / _renderingData.cameraData.cameraTargetDescriptor.height, 0));
            m_Camera.projectionMatrix = jitterTranslation * projectionMatrix;
            m_Camera.nonJitteredProjectionMatrix = projectionMatrix;
            return this;
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            base.FrameCleanup(cmd);
            m_Camera.ResetProjectionMatrix();
        }

        public void Dispose()
        {
            foreach (var buffer in m_Buffers.Values)
                buffer.Dispose();
            m_Buffers.Clear();
        }

        private TAAHistoryBuffer GetHistoryBuffer(ref RenderingData _renderingData,out bool m_FirstExecution)
        {
            m_FirstExecution = false;
            var descriptor = _renderingData.cameraData.cameraTargetDescriptor;
            var instanceID = _renderingData.cameraData.camera.GetInstanceID(); 
            if (m_Buffers.TryGetValue(instanceID,out var m_CurrentBuffer)&&!m_CurrentBuffer.Validate(descriptor))
            {
                m_CurrentBuffer.Dispose();
                m_Buffers.Remove(instanceID);
                m_CurrentBuffer = null;
            }

            if (m_Buffers.ContainsKey(instanceID)) 
                return m_CurrentBuffer;
            
            m_CurrentBuffer = new TAAHistoryBuffer(_renderingData.cameraData.cameraTargetDescriptor);
            m_Buffers.Add(instanceID,m_CurrentBuffer);
            m_FirstExecution = true;
            return m_CurrentBuffer;
        }
        
        public override void Execute(ScriptableRenderContext _context, ref RenderingData _renderingData)
        {
        }

        public void ExecuteResolve(CommandBuffer _cmd, RenderTargetIdentifier _src, RenderTargetIdentifier _dst,RenderTextureDescriptor _descriptor,
            ref DAntiAliasing _data,Material _material,ref RenderingData _renderingData)
        {
            var historyBuffer = GetHistoryBuffer(ref _renderingData,out var firstExecution);
            if (firstExecution)
                _cmd.Blit(_src,historyBuffer.renderTexture);
            _cmd.SetGlobalTexture(kHistoryBufferID,historyBuffer.renderTexture);
            _cmd.SetGlobalFloat("_Blend",_data.blend);
            _cmd.Blit(_src,_dst,_material,1);
            _cmd.Blit(_dst,historyBuffer.renderTexture);
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

        private TAASetupPass m_TAAPass;
        public DAntiAliasingCore(TAASetupPass _taaPass)
        {
            m_TAAPass = _taaPass;
        }

        public override bool Validate(ref RenderingData _renderingData,ref DAntiAliasing _data)
        {
            if (_data.mode == EAntiAliasing.FXAA)
            {
                m_Material.SetFloat(kConstrastSkip,_data.contrastSkip);
                m_Material.SetFloat(kRelativeSkip,_data.relativeSkip);
                m_Material.EnableKeyword(kFXAA_Depth, _data.useDepth);
                m_Material.EnableKeyword(kFXAA_AdditionalSample,_data.additionalSample);
            
                var subPixel = _data.fxaa is EFXAA.SubPixel or EFXAA.Both  ;
                if (m_Material.EnableKeyword(kFXAA_SubPixel,subPixel))
                    m_Material.SetFloat(kBlendStrength,_data.subPixelBlend);
                var edge = _data.fxaa is EFXAA.Both or EFXAA.EdgeDetect;
                m_Material.EnableKeyword(KW_FXAAEdgeBlend, edge);
            }
            return base.Validate(ref _renderingData,ref _data);
        }

        public override void Execute(RenderTextureDescriptor _descriptor, ref DAntiAliasing _data, CommandBuffer _buffer,
            RenderTargetIdentifier _src, RenderTargetIdentifier _dst, ScriptableRenderContext _context, ref RenderingData _renderingData)
        {
            switch (_data.mode)
            {
                case EAntiAliasing.FXAA:base.Execute(_descriptor, ref _data, _buffer, _src, _dst, _context, ref _renderingData);break;
                case EAntiAliasing.TAA:m_TAAPass.ExecuteResolve(_buffer,_src,_dst,_descriptor,ref _data,m_Material,ref _renderingData);break;
            }
        }
    }

}