using System;
using System.Linq;
using System.Collections.Generic;
using Geometry.Voxel;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Rendering.Pipeline
{
    using PostProcess;
    [Serializable]
    public class SRD_ReflectionData
    {
        public EReflectionSpace m_Type;
        [MFoldout(nameof(m_Type), EReflectionSpace.PlanarScreenSpace)] [Range(1, 4)] public int m_Sample;

        [MFoldout(nameof(m_Type), EReflectionSpace.PlanarMirrorSpace)] public bool m_Recull;
        [MFoldout(nameof(m_Type), EReflectionSpace.PlanarMirrorSpace,nameof(m_Recull),true)] [Range(0,8)]public int m_AdditionalLightcount;
        [MFoldout(nameof(m_Type), EReflectionSpace.PlanarMirrorSpace)] public bool m_IncludeTransparent;
        
        [Header("Blur")]
        [Range(1,4)] public int m_DownSample;
        public PPData_Blurs m_BlurParam;

        public static readonly SRD_ReflectionData kDefault = new SRD_ReflectionData
        {
            m_Type = EReflectionSpace.PlanarScreenSpace,
            m_IncludeTransparent = false,
            m_Recull = false,
            m_DownSample=2,
            m_AdditionalLightcount=8,
            m_Sample = 1,
            m_BlurParam = UPipeline.GetDefaultPostProcessData<PPData_Blurs>(),
        };
    }
    
    public  class SRP_Reflection: ISRPBase
    {
        private readonly PPCore_Blurs m_Blurs;
        private readonly IReflectionManager m_Manager;
        private readonly SRD_ReflectionData m_Data;
        private readonly RenderPassEvent m_Event;
        public SRP_Reflection(SRD_ReflectionData _data,RenderPassEvent _event)
        {
            m_Event = _event;
            m_Data = _data;
            m_Blurs = new PPCore_Blurs();
            m_Blurs.OnValidate(ref _data.m_BlurParam);

            switch (_data.m_Type)
            {
                case EReflectionSpace.ScreenSpace_Undone:
                    m_Manager = new SRP_ScreenSpaceReflection(m_Blurs);
                    break;
                case EReflectionSpace.PlanarMirrorSpace:
                case EReflectionSpace.PlanarScreenSpace:
                    m_Manager = new SRP_PlanarReflectionBase(m_Blurs);
                    break;
            }
        }

        public void EnqueuePass(ScriptableRenderer _renderer)
        {
            m_Manager.EnqueuePass(m_Data,_renderer,m_Event);
        }
        
        public void Dispose()
        {
            m_Blurs.Destroy();
            m_Manager.Dispose();
        }
    }

    interface IReflectionManager:ISRPBase
    {
        void EnqueuePass( SRD_ReflectionData _data, ScriptableRenderer _renderer,RenderPassEvent _event);
    }
    #region Screen Space Reflection

    class SRP_ScreenSpaceReflection:ScriptableRenderPass, IReflectionManager
    {
        private SRD_ReflectionData m_Data;
        private readonly PassiveInstance<Shader> m_ReflectionBlit=new PassiveInstance<Shader>(()=>RenderResources.FindInclude("Hidden/ScreenSpaceReflection"));
        private readonly PPCore_Blurs m_Blur;
        private ScriptableRenderer m_Renderer;
        private readonly Material m_Material;
        static readonly int ID_SSRTex = Shader.PropertyToID("_ScreenSpaceReflectionTexture");
        static readonly RenderTargetIdentifier RT_ID_SSR = new RenderTargetIdentifier(ID_SSRTex);

        public SRP_ScreenSpaceReflection(PPCore_Blurs _blurs)
        {
            m_Material = new Material(m_ReflectionBlit){hideFlags = HideFlags.HideAndDontSave};
            m_Blur = _blurs;
        }
        
        public void Dispose()
        {            
            GameObject.DestroyImmediate(m_Material);
        }

        public void EnqueuePass(SRD_ReflectionData _data, ScriptableRenderer _renderer,RenderPassEvent _event)
        {
            this.renderPassEvent = _event;
            _renderer.EnqueuePass(this);
            m_Data = _data;
            m_Renderer = _renderer;
            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            foreach (var reflection in SRC_ReflectionConfig.m_Reflections)
                reflection.SetPropertyBlock(propertyBlock,4);
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            cmd.GetTemporaryRT(ID_SSRTex, cameraTextureDescriptor.width, cameraTextureDescriptor.height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
            ConfigureTarget(RT_ID_SSR);
            base.Configure(cmd, cameraTextureDescriptor);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            base.OnCameraCleanup(cmd);
            cmd.ReleaseTemporaryRT(ID_SSRTex);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("Planar Reflection Pass");
            cmd.Blit(m_Renderer.cameraColorTarget,RT_ID_SSR,m_Material);
            
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
        
    }
    #endregion
    
    #region Planar Reflections
    public sealed class SRP_PlanarReflectionBase :  IReflectionManager
    {
        const int kMaxReflectionTextures = 4;
        private SRD_ReflectionData m_Data;

        private readonly PPCore_Blurs m_Blur;
        public SRP_PlanarReflectionBase(PPCore_Blurs _blurs)
        {
            m_Blur = _blurs;
        }


        public void EnqueuePass(SRD_ReflectionData _data,ScriptableRenderer _renderer,RenderPassEvent _event)
        {
            m_Data = _data;
            if (SRC_ReflectionConfig.m_Reflections.Count == 0)
                return;
            
            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            foreach (var (index,groups) in SRC_ReflectionConfig.m_Reflections.FindAll(p=>p.Available).GroupBy(p=>p.m_PlaneData,GPlane.kComparer).LoopIndex())
            {
                if (index >= kMaxReflectionTextures)
                {
                    Debug.LogWarning("Reflection Plane Outta Limit!");
                    break;
                }
                foreach (SRC_ReflectionConfig planeComponent in groups)
                    planeComponent.SetPropertyBlock(propertyBlock,index);

                APlanarReflection reflection=null;
                switch (m_Data.m_Type)
                {
                    case EReflectionSpace.PlanarScreenSpace:
                        reflection = new FPlanarReflection_ScreenSpace();
                        break;
                    case EReflectionSpace.PlanarMirrorSpace:
                        reflection = new FPlanarReflection_MirrorSpace();
                        break;
                }

                _renderer.EnqueuePass(reflection.Setup(m_Data,m_Blur, groups.Key,_renderer,index,_event));
            }

        }

        public void Dispose()
        {
        }
    }

    abstract class APlanarReflection:ScriptableRenderPass
    {
        #region ID
        const string C_ReflectionTex = "_CameraReflectionTexture";
        const string C_ReflectionTempTexture = "_CameraReflectionBlur";
        #endregion

         protected SRD_ReflectionData m_Data;
         protected PPCore_Blurs m_Blur;
         protected  int m_Index { get; private set; }
         private GPlane m_Plane;
         private RenderTextureDescriptor m_ColorDescriptor;
         private RenderTargetIdentifier m_ColorTarget;
         private ScriptableRenderer m_Renderer;
         int m_ReflectionTexture;
         RenderTargetIdentifier m_ReflectionTextureID;
         private int m_ReflectionBlurTexture;
         private RenderTargetIdentifier m_ReflectionBlurTextureID;
         
         public virtual APlanarReflection Setup(SRD_ReflectionData _data,PPCore_Blurs _blur, GPlane _planeData,ScriptableRenderer _renderer,int _index,RenderPassEvent _event)
         {
             renderPassEvent = _event;
             m_Index = _index;
             m_Data = _data;
             m_Blur = _blur;
             m_Renderer = _renderer;
             m_Plane = _planeData;
             m_ReflectionTexture = Shader.PropertyToID( C_ReflectionTex + _index);
             m_ReflectionTextureID = new RenderTargetIdentifier(m_ReflectionTexture);
             m_ReflectionBlurTexture = Shader.PropertyToID(C_ReflectionTempTexture + _index);
             m_ReflectionBlurTextureID = new RenderTargetIdentifier(m_ReflectionBlurTexture);
             return this;
         }

         public sealed override void Configure(CommandBuffer _cmd, RenderTextureDescriptor _cameraTextureDescriptor)
         {
             base.Configure(_cmd, _cameraTextureDescriptor);
             m_ColorDescriptor = _cameraTextureDescriptor;
             ConfigureColorDescriptor(ref m_ColorDescriptor,ref m_Data);

             _cmd.GetTemporaryRT(m_ReflectionTexture, m_ColorDescriptor,FilterMode.Bilinear);
            
             m_ColorTarget = m_ReflectionTextureID;
             if (m_Data.m_BlurParam.m_BlurType!=EBlurType.None)
             {
                 _cmd.GetTemporaryRT(m_ReflectionBlurTexture, m_ColorDescriptor, FilterMode.Bilinear);
                 m_ColorTarget = m_ReflectionBlurTextureID;
             }
             DoConfigure(_cmd,m_ColorDescriptor,m_ColorTarget);
         }

         protected virtual void ConfigureColorDescriptor(ref RenderTextureDescriptor _descriptor,ref SRD_ReflectionData _data)
         {
             int downSample = Mathf.Max(_data.m_DownSample, 1);
             _descriptor.width /= downSample;
             _descriptor.height /= downSample;
         }
         protected virtual void DoConfigure(CommandBuffer _cmd, RenderTextureDescriptor _descriptor,  RenderTargetIdentifier _colorTarget)
         {
             
         }


         public override void OnCameraCleanup(CommandBuffer _cmd)
         {
             base.OnCameraCleanup(_cmd);
             if (m_Data.m_BlurParam.m_BlurType!=EBlurType.None)
                 _cmd.ReleaseTemporaryRT(m_ReflectionBlurTexture);
             _cmd.ReleaseTemporaryRT(m_ReflectionTexture);
         }

         public sealed override void Execute(ScriptableRenderContext _context, ref RenderingData _renderingData)
         {
             CommandBuffer cmd = CommandBufferPool.Get($"Planar Reflection Pass ({m_Index})");
             
             Execute(ref m_Data,_context,ref _renderingData,cmd,ref m_Plane,ref m_ColorDescriptor,ref m_ColorTarget,ref m_Renderer);
             if (m_Data.m_BlurParam.m_BlurType!=EBlurType.None)
                 m_Blur.ExecutePostProcessBuffer(cmd, m_ColorTarget, m_ReflectionTextureID, m_ColorDescriptor ,ref m_Data.m_BlurParam); 
            
             _context.ExecuteCommandBuffer(cmd);
             cmd.Clear();
             CommandBufferPool.Release(cmd);
         }

         protected abstract void Execute(ref SRD_ReflectionData _data,
             ScriptableRenderContext _context, ref RenderingData _renderingData, CommandBuffer _cmd,
             ref GPlane _plane, ref RenderTextureDescriptor _descriptor, 
             ref RenderTargetIdentifier _target,ref ScriptableRenderer _renderer);
    }
    
    class FPlanarReflection_ScreenSpace : APlanarReflection
    {
        static readonly int ID_SampleCount = Shader.PropertyToID( "_SAMPLE_COUNT");
        static readonly int ID_Result_TexelSize = Shader.PropertyToID("_Result_TexelSize");

        static readonly int ID_Input = Shader.PropertyToID("_Input");
        static readonly int ID_Result = Shader.PropertyToID("_Result");
        static readonly int ID_PlaneNormal = Shader.PropertyToID("_PlaneNormal");
        static readonly int ID_PlanePosition = Shader.PropertyToID("_PlanePosition");
        int m_Kernels;
        Int2 m_ThreadGroups;
        
        private readonly PassiveInstance<ComputeShader> m_ReflectionComputeShader=new PassiveInstance<ComputeShader>(()=>RenderResources.FindComputeShader("PlanarReflection"));
        protected override void ConfigureColorDescriptor(ref RenderTextureDescriptor _descriptor, ref SRD_ReflectionData _data)
        {
            base.ConfigureColorDescriptor(ref _descriptor, ref _data);
            _descriptor.enableRandomWrite = true;
            _descriptor.colorFormat = RenderTextureFormat.ARGB32;
            _descriptor.msaaSamples = 1;
            m_Kernels = ((ComputeShader)m_ReflectionComputeShader).FindKernel("Generate");
            m_ThreadGroups = new Int2(_descriptor.width / 8, _descriptor.height / 8);
        }

        protected override void DoConfigure(CommandBuffer _cmd, RenderTextureDescriptor _descriptor, RenderTargetIdentifier _colorTarget)
        {
            base.DoConfigure(_cmd, _descriptor, _colorTarget);
            ConfigureTarget(_colorTarget);
            ConfigureClear(ClearFlag.Color,Color.clear);
        }

        protected override void Execute(ref SRD_ReflectionData _data, ScriptableRenderContext _context,
            ref RenderingData _renderingData, CommandBuffer _cmd, ref GPlane _plane, ref RenderTextureDescriptor _descriptor,
            ref RenderTargetIdentifier _target,ref ScriptableRenderer _renderer)
        {
            _cmd.SetComputeIntParam(m_ReflectionComputeShader, ID_SampleCount, _data.m_Sample);
            _cmd.SetComputeVectorParam(m_ReflectionComputeShader, ID_PlaneNormal, _plane.normal.normalized);
            _cmd.SetComputeVectorParam(m_ReflectionComputeShader, ID_PlanePosition, _plane.position);
            _cmd.SetComputeVectorParam(m_ReflectionComputeShader, ID_Result_TexelSize, _descriptor.GetTexelSize());
            
            _cmd.SetComputeTextureParam(m_ReflectionComputeShader, m_Kernels, ID_Input, _renderer.cameraColorTarget);
            _cmd.SetComputeTextureParam(m_ReflectionComputeShader, m_Kernels, ID_Result, _target);
            _cmd.DispatchCompute(m_ReflectionComputeShader, m_Kernels, m_ThreadGroups.x,m_ThreadGroups.y, 1);
        }
    }
    class FPlanarReflection_MirrorSpace : APlanarReflection
    {
        private const string kReflectionDepth = "_CameraReflectionDepthComparer";
        static readonly int ID_CameraWorldPosition = Shader.PropertyToID("_WorldSpaceCameraPos");

         int m_ReflectionDepth;
         RenderTargetIdentifier m_ReflectionDepthID ;

         public override APlanarReflection Setup(SRD_ReflectionData _data, PPCore_Blurs _blur, GPlane _planeData, ScriptableRenderer _renderer,
             int _index,RenderPassEvent _event)
         {
             m_ReflectionDepth = Shader.PropertyToID(kReflectionDepth + _index);
             m_ReflectionDepthID = new RenderTargetIdentifier(m_ReflectionDepth);
             return base.Setup(_data, _blur, _planeData, _renderer, _index,_event);
         }

         protected override void ConfigureColorDescriptor(ref RenderTextureDescriptor _descriptor, ref SRD_ReflectionData _data)
         {
             base.ConfigureColorDescriptor(ref _descriptor, ref _data);
             _descriptor.colorFormat = RenderTextureFormat.ARGB32;
         }

         protected override void DoConfigure(CommandBuffer _cmd, RenderTextureDescriptor _descriptor, RenderTargetIdentifier _colorTarget)
         {
             base.DoConfigure(_cmd, _descriptor, _colorTarget);
             var depthDescriptor = _descriptor;
             depthDescriptor.colorFormat = RenderTextureFormat.Depth;
             depthDescriptor.depthBufferBits = 32;
             depthDescriptor.enableRandomWrite = false;
             _cmd.GetTemporaryRT(m_ReflectionDepth, depthDescriptor, FilterMode.Point);
             ConfigureTarget(_colorTarget,m_ReflectionDepth);
             ConfigureClear(ClearFlag.All,Color.clear);
         }


         public override void OnCameraCleanup(CommandBuffer _cmd)
         {
             base.OnCameraCleanup(_cmd);
             _cmd.ReleaseTemporaryRT(m_ReflectionDepth);
         }

         protected override void Execute(ref SRD_ReflectionData _data, ScriptableRenderContext _context, ref RenderingData _renderingData,
             CommandBuffer _cmd, ref GPlane _plane, ref RenderTextureDescriptor _descriptor, ref RenderTargetIdentifier _target,
             ref ScriptableRenderer _renderer)
         {
             ref var cameraData = ref _renderingData.cameraData;
             ref Camera camera = ref cameraData.camera;
            
            Matrix4x4 planeMirrorMatrix = _plane.GetMirrorMatrix();
            Matrix4x4 cullingMatrix = camera.cullingMatrix;
            camera.cullingMatrix = cullingMatrix * planeMirrorMatrix;

            DrawingSettings drawingSettings = CreateDrawingSettings(UPipeline.kDefaultShaderTags, ref _renderingData,  SortingCriteria.CommonOpaque);
            FilteringSettings filterSettings = new FilteringSettings(_data.m_IncludeTransparent? RenderQueueRange.all : RenderQueueRange.opaque);
            Matrix4x4 projectionMatrix = GL.GetGPUProjectionMatrix(cameraData.GetProjectionMatrix(), cameraData.IsCameraProjectionMatrixFlipped());
            Matrix4x4 viewMatrix = cameraData.GetViewMatrix();
            viewMatrix*= planeMirrorMatrix;
            
            RenderingUtils.SetViewAndProjectionMatrices(_cmd, viewMatrix , projectionMatrix, false);
            var cameraPosition = camera.transform.position;
            _cmd.SetGlobalVector( ID_CameraWorldPosition,planeMirrorMatrix.MultiplyPoint(cameraPosition));
            _cmd.SetInvertCulling(true);
            _context.ExecuteCommandBuffer(_cmd);

            if (_data.m_Recull)
            {
                if (cameraData.camera.TryGetCullingParameters(out ScriptableCullingParameters cullingParameters))
                {
                    cullingParameters.maximumVisibleLights = _data.m_AdditionalLightcount;
                    _context.DrawRenderers(_context.Cull(ref cullingParameters), ref drawingSettings, ref filterSettings);
                }
            }
            else
            {
                _context.DrawRenderers(_renderingData.cullResults, ref drawingSettings, ref filterSettings);
            }
            
            _cmd.Clear();
            _cmd.SetInvertCulling(false);
            _cmd.SetGlobalVector( ID_CameraWorldPosition,cameraPosition);
            RenderingUtils.SetViewAndProjectionMatrices(_cmd, cameraData.GetViewMatrix(), cameraData.GetGPUProjectionMatrix(), false);
            camera.ResetCullingMatrix();
         }
    }
    
    #endregion
}