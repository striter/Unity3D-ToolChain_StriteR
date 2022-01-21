using System.Linq;
using System.Collections.Generic;
using Geometry.Voxel;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Rendering.Pipeline
{
    using PostProcess;
    public  class SRP_Reflection: ISRPBase
    {
        private readonly PPCore_Blurs m_Blurs;
        private readonly IReflectionPass m_Pass;
        private readonly SRD_ReflectionData m_Data;
        public SRP_Reflection(SRD_ReflectionData _data,RenderPassEvent _event)
        {
            m_Data = _data;
            m_Blurs = new PPCore_Blurs();
            m_Blurs.OnValidate(ref _data.m_BlurParam);

            switch (_data.m_Type)
            {
                case EReflectionSpace.ScreenSpace_Undone:
                    m_Pass = new SRP_ScreenSpaceReflection(m_Blurs){renderPassEvent = _event};
                    break;
                case EReflectionSpace.PlanarMirrorSpace:
                case EReflectionSpace.PlanarScreenSpace:
                    m_Pass = new SRP_PlanarReflectionBase(m_Blurs){renderPassEvent = _event};
                    break;
            }
        }

        public void EnqueuePass(ScriptableRenderer _renderer)
        {
            _renderer.EnqueuePass(m_Pass.Setup(m_Data,_renderer));
        }
        
        public void Dispose()
        {
            m_Blurs.Destroy();
            m_Pass.Dispose();
        }
    }

    interface IReflectionPass:ISRPBase
    {
        ScriptableRenderPass Setup(SRD_ReflectionData _data, ScriptableRenderer _renderer);
    }
    #region Screen Space Reflection

    class SRP_ScreenSpaceReflection:ScriptableRenderPass, IReflectionPass
    {
        private SRD_ReflectionData m_Data;
        private readonly Instance<Shader> m_ReflectionBlit=new Instance<Shader>(()=>RenderResources.FindInclude("Hidden/ScreenSpaceReflection"));
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

        public ScriptableRenderPass Setup(SRD_ReflectionData _data, ScriptableRenderer _renderer)
        {
            m_Data = _data;
            m_Renderer = _renderer;
            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            foreach (var reflection in SRC_ReflectionController.m_Reflections)
                reflection.SetPropertyBlock(propertyBlock,4);
            return this;
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
    public sealed class SRP_PlanarReflectionBase :  ScriptableRenderPass,IReflectionPass
    {
        const int kMaxReflectionTextures = 4;
        private SRD_ReflectionData m_Data;

        private readonly PPCore_Blurs m_Blur;
        private readonly List<APlanarReflection> m_ReflectionPasses = new List<APlanarReflection>();
        public SRP_PlanarReflectionBase(PPCore_Blurs _blurs)
        {
            m_Blur = _blurs;
        }
        public ScriptableRenderPass Setup(SRD_ReflectionData _data,ScriptableRenderer _renderer)
        {
            m_Data = _data;
            m_ReflectionPasses.Clear();
            if (SRC_ReflectionController.m_Reflections.Count == 0)
                return this;
            
            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            foreach (var (index,groups) in SRC_ReflectionController.m_Reflections.FindAll(p=>p.Available).GroupBy(p=>p.m_PlaneData).LoopIndex())
            {
                if (index >= kMaxReflectionTextures)
                {
                    Debug.LogWarning("Reflection Plane Outta Limit!");
                    break;
                }
                foreach (SRC_ReflectionController planeComponent in groups)
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

                m_ReflectionPasses.Add(reflection.Setup(groups.Key, _renderer, index));
            }

            return this;
        }

        public void Dispose()
        {
            foreach (var pass in m_ReflectionPasses)
                pass.Dispose();
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            base.OnCameraSetup(cmd, ref renderingData);
            foreach (var pass in m_ReflectionPasses)
                pass.DoCameraSetup(this,ref m_Data,cmd,ref renderingData);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            foreach (var pass in m_ReflectionPasses)
                pass.DoCameraCleanUp(this,ref m_Data,cmd);
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("Planar Reflection Pass");
            
            foreach (var pass in m_ReflectionPasses)
                pass.Execute(this,ref m_Data,context,ref renderingData,cmd,m_Blur);
            
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
    }

    abstract class APlanarReflection
    {
        #region ID
        const string C_ReflectionTex = "_CameraReflectionTexture";
        const string C_ReflectionTempTexture = "_CameraReflectionBlur";
        #endregion

         private int m_Index;
         private GPlane m_Plane;
         private RenderTextureDescriptor m_ColorDescriptor;
         private RenderTargetIdentifier m_ColorTarget;
         private ScriptableRenderer m_Renderer;
         int m_ReflectionTexture;
         RenderTargetIdentifier m_ReflectionTextureID;
         private int m_ReflectionBlurTexture;
         private RenderTargetIdentifier m_ReflectionBlurTextureID;
         
         public virtual APlanarReflection Setup(GPlane _planeData,ScriptableRenderer _renderer,int _index)
         {
             m_Index = _index;
             m_Renderer = _renderer;
             m_Plane = _planeData;
             m_ReflectionTexture = Shader.PropertyToID( C_ReflectionTex + _index);
             m_ReflectionTextureID = new RenderTargetIdentifier(m_ReflectionTexture);
             m_ReflectionBlurTexture = Shader.PropertyToID(C_ReflectionTempTexture + _index);
             m_ReflectionBlurTextureID = new RenderTargetIdentifier(m_ReflectionBlurTexture);
             return this;
         }

         public void DoCameraSetup(ScriptableRenderPass _pass,  ref SRD_ReflectionData _data, CommandBuffer cmd,ref RenderingData _renderingData)
         {
             m_ColorDescriptor = _renderingData.cameraData.cameraTargetDescriptor;
             ConfigureColorDescriptor(ref m_ColorDescriptor,ref _data);

             cmd.GetTemporaryRT(m_ReflectionTexture, m_ColorDescriptor,FilterMode.Bilinear);
            
             m_ColorTarget = m_ReflectionTextureID;
             if (_data.m_EnableBlur)
             {
                 cmd.GetTemporaryRT(m_ReflectionBlurTexture, m_ColorDescriptor, FilterMode.Bilinear);
                 m_ColorTarget = m_ReflectionBlurTextureID;
             }
             OnCameraSetup(_pass,cmd,m_ColorTarget,m_ColorDescriptor);
         }

         protected virtual void OnCameraSetup(ScriptableRenderPass _pass,CommandBuffer _cmd, RenderTargetIdentifier _target,RenderTextureDescriptor _descriptor)
         {
             
         }

         protected virtual void ConfigureColorDescriptor(ref RenderTextureDescriptor _descriptor,ref SRD_ReflectionData _data)
         {
             int downSample = Mathf.Max(_data.m_DownSample, 1);
             _descriptor.width /= downSample;
             _descriptor.height /= downSample;
         }

         public virtual void DoCameraCleanUp(ScriptableRenderPass _pass,ref SRD_ReflectionData _data,CommandBuffer cmd)
         {
             if (_data.m_EnableBlur)
                 cmd.ReleaseTemporaryRT(m_ReflectionBlurTexture);
             cmd.ReleaseTemporaryRT(m_ReflectionTexture);
         }


         public void Execute(ScriptableRenderPass _pass,ref SRD_ReflectionData _data,  ScriptableRenderContext _context,ref RenderingData _renderingData,CommandBuffer _cmd,PPCore_Blurs _blurs)
         {
             var type = _data.m_Type.ToString()+m_Index;
             _cmd.BeginSample(type);
             Execute(_pass,ref _data,_context,ref _renderingData,_cmd,ref m_Plane,ref m_ColorDescriptor,ref m_ColorTarget,ref m_Renderer);
             if(_data.m_EnableBlur)
                 _blurs.ExecutePostProcessBuffer(_cmd, m_ColorTarget, m_ReflectionTextureID, m_ColorDescriptor ,ref _data.m_BlurParam); 
             _cmd.EndSample(type);
         }

         protected abstract void Execute(ScriptableRenderPass _pass, ref SRD_ReflectionData _data,
             ScriptableRenderContext _context, ref RenderingData _renderingData, CommandBuffer _cmd,
             ref GPlane _plane, ref RenderTextureDescriptor _descriptor, 
             ref RenderTargetIdentifier _target,ref ScriptableRenderer _renderer);
         
         public virtual void Dispose(){}
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
        
        private readonly Instance<ComputeShader> m_ReflectionComputeShader=new Instance<ComputeShader>(()=>RenderResources.FindComputeShader("PlanarReflection"));
        protected override void ConfigureColorDescriptor(ref RenderTextureDescriptor _descriptor, ref SRD_ReflectionData _data)
        {
            base.ConfigureColorDescriptor(ref _descriptor, ref _data);
            _descriptor.enableRandomWrite = true;
            _descriptor.colorFormat = RenderTextureFormat.ARGB32;
            m_Kernels = ((ComputeShader)m_ReflectionComputeShader).FindKernel("Generate");
            m_ThreadGroups = new Int2(_descriptor.width / 8, _descriptor.height / 8);
        }
        protected override void Execute(ScriptableRenderPass _pass, ref SRD_ReflectionData _data, ScriptableRenderContext _context,
            ref RenderingData _renderingData, CommandBuffer _cmd, ref GPlane _plane, ref RenderTextureDescriptor _descriptor,
            ref RenderTargetIdentifier _target,ref ScriptableRenderer _renderer)
        {
            _cmd.SetRenderTarget(_target);
            _cmd.ClearRenderTarget(false,true,Color.clear);
            
            _cmd.SetComputeIntParam(m_ReflectionComputeShader, ID_SampleCount, _data.m_Sample);
            _cmd.SetComputeVectorParam(m_ReflectionComputeShader, ID_PlaneNormal, _plane.normal.normalized);
            _cmd.SetComputeVectorParam(m_ReflectionComputeShader, ID_PlanePosition, _plane.distance * _plane.normal);
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

        readonly List<ShaderTagId> m_ShaderTagIDs = new List<ShaderTagId>();
         int m_ReflectionDepth;
         RenderTargetIdentifier m_ReflectionDepthID ;

         public override APlanarReflection Setup(GPlane _planeData, ScriptableRenderer _renderer, int _index)
         {
             base.Setup(_planeData, _renderer, _index);
             m_ShaderTagIDs.FillWithDefaultTags();
             m_ReflectionDepth = Shader.PropertyToID(kReflectionDepth + _index);
             m_ReflectionDepthID = new RenderTargetIdentifier(m_ReflectionDepth);
             return this;
         }
         protected override void ConfigureColorDescriptor(ref RenderTextureDescriptor _descriptor, ref SRD_ReflectionData _data)
         {
             base.ConfigureColorDescriptor(ref _descriptor, ref _data);
             _descriptor.colorFormat = RenderTextureFormat.ARGB32;
         }

         protected override void OnCameraSetup(ScriptableRenderPass _pass, CommandBuffer _cmd, RenderTargetIdentifier _target,
             RenderTextureDescriptor _descriptor)
         {
             base.OnCameraSetup(_pass, _cmd, _target, _descriptor);
             var depthDescriptor = _descriptor;
             depthDescriptor.colorFormat = RenderTextureFormat.Depth;
             depthDescriptor.depthBufferBits = 32;
             depthDescriptor.enableRandomWrite = false;
             _cmd.GetTemporaryRT(m_ReflectionDepth, depthDescriptor, FilterMode.Point);
         }

         public override void DoCameraCleanUp(ScriptableRenderPass _pass, ref SRD_ReflectionData _data, CommandBuffer cmd)
         {
             base.DoCameraCleanUp(_pass, ref _data, cmd);
             cmd.ReleaseTemporaryRT(m_ReflectionDepth);
         }

         protected override void Execute(ScriptableRenderPass _pass, ref SRD_ReflectionData _data, ScriptableRenderContext _context,
             ref RenderingData _renderingData, CommandBuffer _cmd, ref GPlane _plane, ref RenderTextureDescriptor _descriptor,
             ref RenderTargetIdentifier _target,ref ScriptableRenderer _renderer)
         {
             ref var cameraData = ref _renderingData.cameraData;
             ref Camera camera = ref cameraData.camera;
            
            Matrix4x4 planeMirrorMatrix = _plane.GetMirrorMatrix();
            Matrix4x4 cullingMatrix = camera.cullingMatrix;
            camera.cullingMatrix = cullingMatrix * planeMirrorMatrix;

            DrawingSettings drawingSettings = _pass.CreateDrawingSettings(m_ShaderTagIDs, ref _renderingData,  SortingCriteria.CommonOpaque);
            FilteringSettings m_FilterSettings = new FilteringSettings(_data.m_IncludeTransparent? RenderQueueRange.all : RenderQueueRange.opaque);
            Matrix4x4 projectionMatrix = GL.GetGPUProjectionMatrix(cameraData.GetProjectionMatrix(), cameraData.IsCameraProjectionMatrixFlipped());
            Matrix4x4 viewMatrix = cameraData.GetViewMatrix();
            viewMatrix*= planeMirrorMatrix;
            
            RenderingUtils.SetViewAndProjectionMatrices(_cmd, viewMatrix , projectionMatrix, false);
            var cameraPosition = camera.transform.position;
            _cmd.SetGlobalVector( ID_CameraWorldPosition,planeMirrorMatrix.MultiplyPoint(cameraPosition));
            _cmd.SetInvertCulling(true);
            _cmd.SetRenderTarget(_target,m_ReflectionDepthID);
            _cmd.ClearRenderTarget(true,true,Color.clear);
            _context.ExecuteCommandBuffer(_cmd);

            if (_data.m_Recull)
            {
                if (cameraData.camera.TryGetCullingParameters(out ScriptableCullingParameters cullingParameters))
                {
                    cullingParameters.maximumVisibleLights = _data.m_AdditionalLightcount;
                    _context.DrawRenderers(_context.Cull(ref cullingParameters), ref drawingSettings, ref m_FilterSettings);
                }
            }
            else
            {
                _context.DrawRenderers(_renderingData.cullResults, ref drawingSettings, ref m_FilterSettings);
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