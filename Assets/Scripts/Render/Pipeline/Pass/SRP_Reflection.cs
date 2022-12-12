using System;
using System.Linq;
using Geometry;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Rendering.Pipeline
{
    using PostProcess;
    [Serializable]
    public struct SRD_ReflectionData
    {
        public EReflectionSpace m_Type;
        [MFoldout(nameof(m_Type), EReflectionSpace.ScreenSpaceGeometry)] [Range(1, 4)] public int m_Sample;

        [MFoldout(nameof(m_Type), EReflectionSpace.PlanarMirrorSpace)] public bool m_Recull;
        [MFoldout(nameof(m_Type), EReflectionSpace.PlanarMirrorSpace,nameof(m_Recull),true)] [Range(0,8)]public int m_AdditionalLightcount;
        [MFoldout(nameof(m_Type), EReflectionSpace.PlanarMirrorSpace)] public bool m_IncludeTransparent;
        
        [Header("Blur")]
        [Range(1,4)] public int m_DownSample;
        public PPData_Blurs m_BlurParam;

        public static readonly SRD_ReflectionData kDefault = new SRD_ReflectionData
        {
            m_Type = EReflectionSpace.ScreenSpaceGeometry,
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
                case EReflectionSpace.ScreenSpaceGeometry:
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
        void EnqueuePass( SRD_ReflectionData _data,ScriptableRenderer _renderer,RenderPassEvent _event);
    }
    #region Screen Space Reflection

    class SRP_ScreenSpaceReflection:ScriptableRenderPass, IReflectionManager
    {
        private SRD_ReflectionData m_Data;
        private readonly PassiveInstance<Shader> m_ReflectionBlit=new PassiveInstance<Shader>(()=>RenderResources.FindInclude("Hidden/ScreenSpaceReflection"));
        private readonly PPCore_Blurs m_Blur;
        private readonly Material m_Material;
        static readonly int kSSRTex = Shader.PropertyToID("_ScreenSpaceReflectionTexture");
        static readonly RenderTargetIdentifier kSSRTexID = new RenderTargetIdentifier(kSSRTex);

        public SRP_ScreenSpaceReflection(PPCore_Blurs _blurs)
        {
            m_Material = new Material(m_ReflectionBlit){hideFlags = HideFlags.HideAndDontSave};
            m_Blur = _blurs;
        }

        public void Dispose()
        {            
            GameObject.DestroyImmediate(m_Material);
        }

        public void EnqueuePass(SRD_ReflectionData _data,ScriptableRenderer _renderer,RenderPassEvent _event)
        {
            this.renderPassEvent = _event;
            m_Data = _data;
            _renderer.EnqueuePass(this);
            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            foreach (var reflection in SRC_ReflectionConfig.m_Reflections)
                reflection.SetPropertyBlock(propertyBlock,4);
        }

        public override void Configure(CommandBuffer _cmd, RenderTextureDescriptor _cameraTextureDescriptor)
        {
            _cmd.GetTemporaryRT(kSSRTex, _cameraTextureDescriptor.width, _cameraTextureDescriptor.height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
            ConfigureTarget(kSSRTexID);
            base.Configure(_cmd, _cameraTextureDescriptor);
        }

        public override void OnCameraCleanup(CommandBuffer _cmd)
        {
            base.OnCameraCleanup(_cmd);
            _cmd.ReleaseTemporaryRT(kSSRTex);
        }

        public override void Execute(ScriptableRenderContext _context, ref RenderingData _renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("Planar Reflection Pass");
            cmd.Blit(_renderingData.cameraData.renderer.cameraColorTarget,kSSRTexID,m_Material);
            
            _context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
        
    }
    #endregion
    
    #region Geometry Reflections
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
            int index = 0; 
            foreach (var reflectionComponent in SRC_ReflectionConfig.m_Reflections)
            {
                if (index >= kMaxReflectionTextures)
                {
                    Debug.LogWarning("Reflection Plane Outta Limit!");
                    break;
                }
                reflectionComponent.SetPropertyBlock(propertyBlock,index);

                AReflectionBase reflectionPass=null;
                switch (m_Data.m_Type)
                {
                    case EReflectionSpace.ScreenSpaceGeometry:
                        reflectionPass = new FGeometryReflectionScreenSpace();
                        break;
                    case EReflectionSpace.PlanarMirrorSpace:
                        reflectionPass = new FGeometryReflectionMirrorSpace();
                        break;
                }

                _renderer.EnqueuePass(reflectionPass.Setup(m_Data,m_Blur, reflectionComponent,_renderer,index,_event));
                index++;
            }

        }

        public void Dispose()
        {
        }

    }

    abstract class AReflectionBase:ScriptableRenderPass
    {
        #region ID
        const string C_ReflectionTex = "_CameraReflectionTexture";
        const string C_ReflectionTempTexture = "_CameraReflectionBlur";
        #endregion

         protected SRD_ReflectionData m_Data;
         protected PPCore_Blurs m_Blur;
         protected  int m_Index { get; private set; }
         private SRC_ReflectionConfig m_Plane;
         private RenderTextureDescriptor m_ColorDescriptor;
         private RenderTargetIdentifier m_ColorTarget;
         private ScriptableRenderer m_Renderer;
         int m_ReflectionTexture;
         RenderTargetIdentifier m_ReflectionTextureID;
         private int m_ReflectionBlurTexture;
         private RenderTargetIdentifier m_ReflectionBlurTextureID;
         
         public virtual AReflectionBase Setup(SRD_ReflectionData _data,PPCore_Blurs _blur,SRC_ReflectionConfig _component,ScriptableRenderer _renderer,int _index,RenderPassEvent _event)
         {
             renderPassEvent = _event;
             m_Index = _index;
             m_Data = _data;
             m_Blur = _blur;
             m_Renderer = _renderer;
             m_Plane = _component;
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
                 m_Blur.Execute(m_ColorDescriptor ,ref m_Data.m_BlurParam,cmd, m_ColorTarget, m_ReflectionTextureID,m_Renderer,_context,ref _renderingData); 
            
             _context.ExecuteCommandBuffer(cmd);
             cmd.Clear();
             CommandBufferPool.Release(cmd);
         }

         protected abstract void Execute(ref SRD_ReflectionData _data,
             ScriptableRenderContext _context, ref RenderingData _renderingData, CommandBuffer _cmd,
             ref SRC_ReflectionConfig _config, ref RenderTextureDescriptor _descriptor, 
             ref RenderTargetIdentifier _target,ref ScriptableRenderer _renderer);
    }
    
    public enum EReflectionGeometry
    {
        _PLANE = 1,
        _SPHERE = 2,
    }
    
    class FGeometryReflectionScreenSpace : AReflectionBase
    {
        static readonly int kSampleCount = Shader.PropertyToID( "_SAMPLE_COUNT");
        static readonly int kResultTexelSize = Shader.PropertyToID("_Result_TexelSize");

        static readonly int kKernelInput = Shader.PropertyToID("_Input");
        static readonly int kKernelResult = Shader.PropertyToID("_Result");
        static readonly int kPlanePosition = Shader.PropertyToID("_PlanePosition");
        static readonly int kPlaneNormal = Shader.PropertyToID("_PlaneNormal");
        
        static readonly int kSpherePosition = Shader.PropertyToID("_SpherePosition");
        static readonly int kSphereRadius = Shader.PropertyToID("_SphereRadius");
        int m_Kernels;
        Int2 m_ThreadGroups;
        LocalKeyword[] kKeywords;
        
        private readonly PassiveInstance<ComputeShader> m_ReflectionComputeShader=new PassiveInstance<ComputeShader>(()=>RenderResources.FindComputeShader("PlanarReflection"));
        protected override void ConfigureColorDescriptor(ref RenderTextureDescriptor _descriptor, ref SRD_ReflectionData _data)
        {
            base.ConfigureColorDescriptor(ref _descriptor, ref _data);
            _descriptor.enableRandomWrite = true;
            _descriptor.colorFormat = RenderTextureFormat.ARGB32;
            _descriptor.msaaSamples = 1;
            var shader = ((ComputeShader) m_ReflectionComputeShader);
            m_Kernels = ((ComputeShader)m_ReflectionComputeShader).FindKernel("Generate");
            kKeywords = m_ReflectionComputeShader.m_Value.GetLocalKeywords<EReflectionGeometry>();
            m_ThreadGroups = new Int2(_descriptor.width / 8, _descriptor.height / 8);
        }

        protected override void DoConfigure(CommandBuffer _cmd, RenderTextureDescriptor _descriptor, RenderTargetIdentifier _colorTarget)
        {
            base.DoConfigure(_cmd, _descriptor, _colorTarget);
            ConfigureTarget(_colorTarget);
            ConfigureClear(ClearFlag.Color,Color.clear);
        }

        protected override void Execute(ref SRD_ReflectionData _data, ScriptableRenderContext _context,
            ref RenderingData _renderingData, CommandBuffer _cmd, ref SRC_ReflectionConfig _config,  ref RenderTextureDescriptor _descriptor,
            ref RenderTargetIdentifier _target,ref ScriptableRenderer _renderer)
        {
            _cmd.SetComputeIntParam(m_ReflectionComputeShader, kSampleCount, _data.m_Sample);
            _cmd.SetComputeVectorParam(m_ReflectionComputeShader, kResultTexelSize, _descriptor.GetTexelSize());
            
            _cmd.EnableLocalKeywords(m_ReflectionComputeShader.m_Value,kKeywords,_config.m_Geometry);
            if (_config.m_Geometry == EReflectionGeometry._SPHERE)
            {
                var sphere = _config.m_SphereData;
                _cmd.SetComputeVectorParam(m_ReflectionComputeShader, kSpherePosition, sphere.center);
                _cmd.SetComputeVectorParam(m_ReflectionComputeShader, kSphereRadius,new Vector4(sphere.radius,sphere.radius*sphere.radius));
            }
            else if (_config.m_Geometry == EReflectionGeometry._PLANE)
            {
                var _plane = _config.m_PlaneData;
                _cmd.SetComputeVectorParam(m_ReflectionComputeShader, kPlaneNormal, _plane.normal.normalized);
                _cmd.SetComputeVectorParam(m_ReflectionComputeShader, kPlanePosition, _plane.position);
            }
            
            _cmd.SetComputeTextureParam(m_ReflectionComputeShader, m_Kernels, kKernelInput, _renderer.cameraColorTarget);
            _cmd.SetComputeTextureParam(m_ReflectionComputeShader, m_Kernels, kKernelResult, _target);
            _cmd.DispatchCompute(m_ReflectionComputeShader, m_Kernels, m_ThreadGroups.x,m_ThreadGroups.y, 1);
        }
    }
    class FGeometryReflectionMirrorSpace : AReflectionBase
    {
        private const string kReflectionDepth = "_CameraReflectionDepthComparer";
        static readonly int kCameraWorldPosition = Shader.PropertyToID("_WorldSpaceCameraPos");

         int m_ReflectionDepth;
         RenderTargetIdentifier m_ReflectionDepthID;

         public override AReflectionBase Setup(SRD_ReflectionData _data, PPCore_Blurs _blur, SRC_ReflectionConfig _planeData, ScriptableRenderer _renderer,
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
             CommandBuffer _cmd, ref SRC_ReflectionConfig _config,  ref RenderTextureDescriptor _descriptor, ref RenderTargetIdentifier _target,
             ref ScriptableRenderer _renderer)
         {
             ref var cameraData = ref _renderingData.cameraData;
             ref Camera camera = ref cameraData.camera;
            
            Matrix4x4 planeMirrorMatrix = _config.m_PlaneData.GetMirrorMatrix();
            Matrix4x4 cullingMatrix = camera.cullingMatrix;
            camera.cullingMatrix = cullingMatrix * planeMirrorMatrix;

            DrawingSettings drawingSettings = CreateDrawingSettings(UPipeline.kDefaultShaderTags, ref _renderingData,  SortingCriteria.CommonOpaque);
            FilteringSettings filterSettings = new FilteringSettings(_data.m_IncludeTransparent? RenderQueueRange.all : RenderQueueRange.opaque);
            Matrix4x4 projectionMatrix = GL.GetGPUProjectionMatrix(cameraData.GetProjectionMatrix(), cameraData.IsCameraProjectionMatrixFlipped());
            Matrix4x4 viewMatrix = cameraData.GetViewMatrix();
            viewMatrix*= planeMirrorMatrix;
            
            RenderingUtils.SetViewAndProjectionMatrices(_cmd, viewMatrix , projectionMatrix, false);
            var cameraPosition = camera.transform.position;
            _cmd.SetGlobalVector( kCameraWorldPosition,planeMirrorMatrix.MultiplyPoint(cameraPosition));
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
            _cmd.SetGlobalVector( kCameraWorldPosition,cameraPosition);
            RenderingUtils.SetViewAndProjectionMatrices(_cmd, cameraData.GetViewMatrix(), cameraData.GetGPUProjectionMatrix(), false);
            camera.ResetCullingMatrix();
         }
    }
    
    #endregion
}