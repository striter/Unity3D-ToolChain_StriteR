using System;
using Rendering.PostProcess;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.Pipeline
{
    public enum EPlanarReflectionGeometry
    {
        _PLANE = 1,
        _SPHERE = 2,
    }

    public enum EPlanarReflectionMode
    {
        ScreenSpaceGeometry,
        Render,
    }

    [Serializable]
    public struct PlanarReflectionData
    {
        public EPlanarReflectionMode m_Type;
        [Foldout(nameof(m_Type), EPlanarReflectionMode.ScreenSpaceGeometry)] [Range(1, 4)] public int m_Sample;

        [Foldout(nameof(m_Type), EPlanarReflectionMode.Render)] public bool m_Recull;
        [Foldout(nameof(m_Type), EPlanarReflectionMode.Render,nameof(m_Recull),true)] [Range(0,8)]public int m_AdditionalLightcount;
        [Foldout(nameof(m_Type), EPlanarReflectionMode.Render)] public bool m_IncludeTransparent;

        [Header("Blur")] 
        [Range(1,4)] public int m_DownSample;
        public DBlurs m_BlurParam;

        public static readonly PlanarReflectionData kDefault = new PlanarReflectionData
        {
            m_Type = EPlanarReflectionMode.ScreenSpaceGeometry,
            m_IncludeTransparent = true,
            m_Recull = true,
            m_AdditionalLightcount=8,
            m_Sample = 4,
            m_DownSample = 2,
            m_BlurParam =  DBlurs.kDefault,
        };
    }
    
    abstract class APlanarReflectionBase:ScriptableRenderPass
    {
#region ID
        const string C_ReflectionTex = "_CameraReflectionTexture";
        const string C_ReflectionTempTexture = "_CameraReflectionBlur";
#endregion

         protected PlanarReflectionData m_Data;
         private int m_Index;
         private PlanarReflectionProvider m_Compnent;
         private RenderTextureDescriptor m_ColorDescriptor;
         private RTHandle m_ColorTarget;
         int m_ReflectionTexture;
         RenderTargetIdentifier m_ReflectionTextureID;
         private int m_ReflectionBlurTexture;
         private RenderTargetIdentifier m_ReflectionBlurTextureID;
         
         public APlanarReflectionBase(PlanarReflectionData _data,PlanarReflectionProvider _component,int _index)
         {
             m_Index = _index;
             m_Data = _data;
             m_Compnent = _component;
             
             m_ReflectionTexture = Shader.PropertyToID( C_ReflectionTex + _index);
             m_ReflectionTextureID = new RenderTargetIdentifier(m_ReflectionTexture);
             m_ReflectionBlurTexture = Shader.PropertyToID(C_ReflectionTempTexture + _index);
             m_ReflectionBlurTextureID = new RenderTargetIdentifier(m_ReflectionBlurTexture);
         }

         public sealed override void Configure(CommandBuffer _cmd, RenderTextureDescriptor _cameraTextureDescriptor)
         {
             base.Configure(_cmd, _cameraTextureDescriptor);
             m_ColorDescriptor = _cameraTextureDescriptor;
             ConfigureColorDescriptor(ref m_ColorDescriptor,ref m_Data);

             _cmd.GetTemporaryRT(m_ReflectionTexture, m_ColorDescriptor,FilterMode.Bilinear);
            
             m_ColorTarget = RTHandles.Alloc(m_ReflectionTextureID);
             if (m_Data.m_BlurParam.Validate())
             {
                 _cmd.GetTemporaryRT(m_ReflectionBlurTexture, m_ColorDescriptor, FilterMode.Bilinear);
                 m_ColorTarget = RTHandles.Alloc(m_ReflectionBlurTextureID);
             }
             DoConfigure(_cmd,m_ColorDescriptor,m_ColorTarget);
         }

         protected virtual void ConfigureColorDescriptor(ref RenderTextureDescriptor _descriptor,ref PlanarReflectionData _data)
         {
             var downSample = Mathf.Max(_data.m_DownSample, 1);
             _descriptor.width /= downSample;
             _descriptor.height /= downSample;
         }
         protected virtual void DoConfigure(CommandBuffer _cmd, RenderTextureDescriptor _descriptor,  RTHandle _colorTarget)
         {
             
         }

         public override void OnCameraCleanup(CommandBuffer _cmd)
         {
             base.OnCameraCleanup(_cmd);
             if (m_Data.m_BlurParam.Validate())
                 _cmd.ReleaseTemporaryRT(m_ReflectionBlurTexture);
             _cmd.ReleaseTemporaryRT(m_ReflectionTexture);
         }

         public sealed override void Execute(ScriptableRenderContext _context, ref RenderingData _renderingData)
         {
             var cmd = CommandBufferPool.Get($"Planar Reflection Pass ({m_Index})");
             
             Execute(ref m_Data,_context,ref _renderingData,cmd,ref m_Compnent,ref m_ColorDescriptor,ref m_ColorTarget);
             if (m_Data.m_BlurParam.blurType!=EBlurType.None)
                 FBlursCore.Instance.Execute(m_ColorDescriptor ,ref m_Data.m_BlurParam,cmd, m_ColorTarget, m_ReflectionTextureID,_context,ref _renderingData); 
            
             _context.ExecuteCommandBuffer(cmd);
             cmd.Clear();
             CommandBufferPool.Release(cmd);
         }

         protected abstract void Execute(ref PlanarReflectionData _data,
             ScriptableRenderContext _context, ref RenderingData _renderingData, CommandBuffer _cmd,
             ref PlanarReflectionProvider _config, ref RenderTextureDescriptor _descriptor, ref RTHandle _target);
    }
}