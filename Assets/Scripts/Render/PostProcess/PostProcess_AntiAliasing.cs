using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
        
        protected PPData_AntiAliasing m_AliasingData;
        private PPCore_AntiAliasing m_AntiAliasingCore;

        public PostProcess_AntiAliasing(PPData_AntiAliasing _data)
        {
            m_AliasingData = _data;
            m_AntiAliasingCore = new PPCore_AntiAliasing();
        }

        public void Dispose()
        {
            m_AntiAliasingCore.Destroy();
        }
        
        public void ExecuteContext(ScriptableRenderer _renderer, ScriptableRenderContext _context, ref RenderingData _renderingData)=>m_AntiAliasingCore.ExecuteContext(_renderer,_context,ref _renderingData,ref m_AliasingData);
        public void Configure(CommandBuffer _buffer, RenderTextureDescriptor _descriptor) => m_AntiAliasingCore.Configure(_buffer, _descriptor, ref m_AliasingData);
        public void ExecuteBuffer(CommandBuffer _buffer, RenderTargetIdentifier _src, RenderTargetIdentifier _dst, RenderTextureDescriptor _executeData) => m_AntiAliasingCore.ExecutePostProcessBuffer(_buffer,_src,_dst,_executeData,ref m_AliasingData);
        public void FrameCleanUp(CommandBuffer _buffer) => m_AntiAliasingCore.FrameCleanUp(_buffer,ref m_AliasingData);
        public void ValidateParameters() => m_AntiAliasingCore.OnValidate(ref m_AliasingData);

    }

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

    public class PostProcess_TAAPrePass : ScriptableRenderPass
    {
        public override void Execute(ScriptableRenderContext context, ref RenderingData _renderingData)
        {
            // var camera = _renderingData.cameraData.camera;
            // var jitterProjection =  camera.projectionMatrix;
            // jitterProjection.m02 += 
            //     
            // var cmd = CommandBufferPool.Get("TAA PrePass");
            // context.ExecuteCommandBuffer(cmd);
            // CommandBufferPool.Release(cmd);
        }

        public void Dispose()
        {
            
        }
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
    
    public class PPCore_AntiAliasing : PostProcessCore<PPData_AntiAliasing>
    {
        const string kFXAA_AdditionalSample = "_FXAA_ADDITIONAL_SAMPLE";
        const string kFXAA_Depth = "_FXAA_DEPTH";
        const string kFXAA_SubPixel = "_FXAA_SUBPIXEL";
        const string KW_FXAAEdgeBlend = "_FXAA_EDGE";
        readonly int kConstrastSkip = Shader.PropertyToID("_FXAAContrastSkip");
        readonly int kRelativeSkip = Shader.PropertyToID("_FXAARelativeSkip");
        readonly int kBlendStrength = Shader.PropertyToID("_FXAABlendStrength");
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

        public override void ExecutePostProcessBuffer(CommandBuffer _buffer, RenderTargetIdentifier _src, RenderTargetIdentifier _dst,
            RenderTextureDescriptor _descriptor, ref PPData_AntiAliasing _data)
        {
            switch (_data.mode)
            {
                case EAntiAliasing.FXAA:base.ExecutePostProcessBuffer(_buffer, _src, _dst, _descriptor, ref _data);
                    break;
                case EAntiAliasing.TAA:_buffer.Blit(_src,_dst);
                    break;
            }
            
        }
    }

}