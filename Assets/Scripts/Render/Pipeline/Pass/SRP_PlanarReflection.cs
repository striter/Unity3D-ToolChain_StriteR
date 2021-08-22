using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Rendering.Pipeline
{
    using PostProcess;
    public  class SRP_Reflection: ISRPBase
    {
        static readonly int ID_ReflectionTextureOn = Shader.PropertyToID("_CameraReflectionTextureOn");
        static readonly int ID_ReflectionTextureIndex = Shader.PropertyToID("_CameraReflectionTextureIndex");
        static readonly int ID_ReflectionNormalDistort = Shader.PropertyToID("_CameraReflectionNormalDistort");
        
        ScriptableRenderer m_Renderer;
        readonly SRP_PlanarReflectionBase[] m_ReflectionPasses;
        
        public const int C_MaxReflectionTextureCount = 4;
        public readonly SRD_PlanarReflectionData m_Data;
        public readonly PPCore_Blurs m_CoreBlurs;
        public readonly MaterialPropertyBlock m_PropertyBlock;
        public SRP_Reflection(SRD_PlanarReflectionData _data,RenderPassEvent _event)
        {
            m_CoreBlurs = new PPCore_Blurs();
            m_PropertyBlock = new MaterialPropertyBlock();
            m_Data = _data;
            m_CoreBlurs.OnValidate(ref m_Data.m_BlurParam);
            m_ReflectionPasses = new SRP_PlanarReflectionBase[C_MaxReflectionTextureCount];
            for (int i = 0; i < C_MaxReflectionTextureCount; i++)
            {
                switch (m_Data.m_ReflectionType)
                {
                    case EReflectionSpace.ScreenSpace:
                        m_ReflectionPasses[i] = new SRP_PlanarReflection_ScreenSpace(this);
                        break;
                    case EReflectionSpace.MirrorSpace:
                        m_ReflectionPasses[i] = new SRP_PlanarReflection_MirrorSpace(this);
                        break;
                }

                m_ReflectionPasses[i].renderPassEvent = _event;
            }
        }

        public void Dispose()
        {
            m_CoreBlurs.Destroy();
            foreach (SRP_PlanarReflectionBase srpPlanarReflectionBase in m_ReflectionPasses)
                srpPlanarReflectionBase.Dispose();
        }
        public void EnqueuePass(ScriptableRenderer _renderer)
        {
            if (SRC_ReflectionPlane.m_ReflectionPlanes.Count == 0)
                return;

            foreach (var (groups,index) in SRC_ReflectionPlane.m_ReflectionPlanes.FindAll(p=>p.m_MeshRenderer.isVisible).GroupBy(p=>p.m_PlaneData).LoopIndex())
            {
                if (index >= C_MaxReflectionTextureCount)
                {
                    Debug.LogWarning("Reflection Plane Outta Limit!");
                    break;
                }
                m_PropertyBlock.SetInt(ID_ReflectionTextureOn, 1);
                m_PropertyBlock.SetInt(ID_ReflectionTextureIndex,index);
                foreach (SRC_ReflectionPlane planeComponent in groups)
                {
                    #if UNITY_EDITOR
                        planeComponent.EditorApplyIndex(index);
                    #endif
                    m_PropertyBlock.SetFloat(ID_ReflectionNormalDistort, planeComponent.m_NormalDistort);
                    planeComponent.m_MeshRenderer.SetPropertyBlock(m_PropertyBlock);
                }
                _renderer.EnqueuePass(m_ReflectionPasses[index].Setup(index,groups.Key, groups,_renderer));
            }
        }
    }
    public abstract class SRP_PlanarReflectionBase : ScriptableRenderPass, ISRPBase
    {
        #region ID
        const string C_ReflectionTex = "_CameraReflectionTexture";
        const string C_ReflectionTempTexture = "_CameraReflectionBlur";
        #endregion
        private readonly SRP_Reflection m_Reflection;
        SRD_PlanarReflectionData m_Data => m_Reflection.m_Data;
        private PPCore_Blurs m_CoreBlurs => m_Reflection.m_CoreBlurs;
        
        RenderTextureDescriptor m_ResultDescriptor;
        RenderTargetIdentifier m_ColorResult;
        
        protected  ScriptableRenderer m_Renderer { get; private set; }
        private  IEnumerable< SRC_ReflectionPlane> m_Planes;
        private GPlane m_PlaneData;
        int m_ReflectionTexture;
        RenderTargetIdentifier m_ReflectionTextureID;
        private int m_ReflectionBlurTexture;
        private RenderTargetIdentifier m_ReflectionBlurTextureID;
        protected SRP_PlanarReflectionBase(SRP_Reflection _reflection)
        {
            m_Reflection = _reflection;
        }
        public virtual ScriptableRenderPass Setup(int _index,GPlane _planeData, IEnumerable< SRC_ReflectionPlane> _planes,ScriptableRenderer _renderer)
        {
            m_Renderer = _renderer;
            m_Planes = _planes;
            m_PlaneData = _planeData;
            m_ReflectionTexture = Shader.PropertyToID( C_ReflectionTex + _index);
            m_ReflectionTextureID = new RenderTargetIdentifier(m_ReflectionTexture);
            m_ReflectionBlurTexture = Shader.PropertyToID(C_ReflectionTempTexture + _index);
            m_ReflectionBlurTextureID = new RenderTargetIdentifier(m_ReflectionBlurTexture);
            return this;
        }
        public sealed override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            base.OnCameraSetup(cmd, ref renderingData);
            m_ResultDescriptor = renderingData.cameraData.cameraTargetDescriptor;
            ConfigureColorDescriptor(ref m_ResultDescriptor,m_Data);

            cmd.GetTemporaryRT(m_ReflectionTexture, m_ResultDescriptor,FilterMode.Bilinear);
            
            m_ColorResult = m_ReflectionTextureID;
            if (m_Data.m_EnableBlur)
            {
                cmd.GetTemporaryRT(m_ReflectionBlurTexture, m_ResultDescriptor, FilterMode.Bilinear);
                m_ColorResult = m_ReflectionBlurTextureID;
            }
            DoCameraSetup(cmd,m_ResultDescriptor,m_ColorResult);
        }
        protected virtual void DoCameraSetup(CommandBuffer cmd, RenderTextureDescriptor _descriptor,RenderTargetIdentifier _target)
        {
            
        }
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            base.OnCameraCleanup(cmd);
            if (m_Data.m_EnableBlur)
                cmd.ReleaseTemporaryRT(m_ReflectionBlurTexture);
            cmd.ReleaseTemporaryRT(m_ReflectionTexture);
        }
        protected virtual void ConfigureColorDescriptor(ref RenderTextureDescriptor _descriptor,SRD_PlanarReflectionData _data)
        {
            int downSample = Mathf.Max(_data.m_DownSample, 1);
            _descriptor.width /= downSample;
            _descriptor.height /= downSample;
        }
        public sealed override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("Generate Reflection Texture");
            
            GenerateTarget(context,ref renderingData,cmd,m_ResultDescriptor,m_ColorResult, m_Data,ref m_PlaneData);
            if (m_Data.m_EnableBlur)
                m_CoreBlurs.ExecutePostProcessBuffer(cmd, m_ColorResult, m_ReflectionTextureID, m_ResultDescriptor ,ref m_Data.m_BlurParam); 
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
        protected abstract void GenerateTarget(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd,
            RenderTextureDescriptor _descriptor, RenderTargetIdentifier _target,SRD_PlanarReflectionData _data,ref GPlane _plane);
        public abstract void Dispose();
    }
    
    public class SRP_PlanarReflection_ScreenSpace:SRP_PlanarReflectionBase
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
        public SRP_PlanarReflection_ScreenSpace(SRP_Reflection _reflection) : base(_reflection)
        {
        }
        protected override void ConfigureColorDescriptor(ref RenderTextureDescriptor _descriptor, SRD_PlanarReflectionData _data)
        {
            base.ConfigureColorDescriptor(ref _descriptor, _data);
            _descriptor.enableRandomWrite = true;
            _descriptor.colorFormat = RenderTextureFormat.ARGB32;
            m_Kernels = ((ComputeShader)m_ReflectionComputeShader).FindKernel("Generate");
            m_ThreadGroups = new Int2(_descriptor.width / 8, _descriptor.height / 8);
        }
        protected override void DoCameraSetup(CommandBuffer cmd, RenderTextureDescriptor _descriptor, RenderTargetIdentifier _target)
        {
            base.DoCameraSetup(cmd, _descriptor, _target);
            ConfigureTarget(_target);
            ConfigureClear(ClearFlag.Color,Color.clear);
        }
        protected override void GenerateTarget(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd,
            RenderTextureDescriptor _descriptor, RenderTargetIdentifier _target,SRD_PlanarReflectionData _data,ref GPlane _plane)
        {
            cmd.SetComputeIntParam(m_ReflectionComputeShader, ID_SampleCount, _data.m_Sample);
            cmd.SetComputeVectorParam(m_ReflectionComputeShader, ID_PlaneNormal, _plane.normal.normalized);
            cmd.SetComputeVectorParam(m_ReflectionComputeShader, ID_PlanePosition, _plane.distance * _plane.normal);
            cmd.SetComputeVectorParam(m_ReflectionComputeShader, ID_Result_TexelSize, _descriptor.GetTexelSize());
            
            cmd.SetComputeTextureParam(m_ReflectionComputeShader, m_Kernels, ID_Input, m_Renderer.cameraColorTarget);
            cmd.SetComputeTextureParam(m_ReflectionComputeShader, m_Kernels, ID_Result, _target);
            cmd.DispatchCompute(m_ReflectionComputeShader, m_Kernels, m_ThreadGroups.m_X,m_ThreadGroups.m_Y, 1);
        }
        public override void Dispose() { }
    }
    public class SRP_PlanarReflection_MirrorSpace : SRP_PlanarReflectionBase
    {
        private const string C_ReflectionDepth = "_CameraReflectionDepthComparer";
        static readonly int ID_CameraWorldPosition = Shader.PropertyToID("_WorldSpaceCameraPos");

        readonly List<ShaderTagId> m_ShaderTagIDs = new List<ShaderTagId>();
         int m_ReflectionDepth;
         RenderTargetIdentifier m_ReflectionDepthID ;
        public SRP_PlanarReflection_MirrorSpace(SRP_Reflection _reflection):base(_reflection)
        {
            m_ShaderTagIDs.FillWithDefaultTags();
        }
        public override ScriptableRenderPass Setup(int _index, GPlane _planeData, IEnumerable<SRC_ReflectionPlane> _planes, ScriptableRenderer _renderer)
        {
            m_ReflectionDepth = Shader.PropertyToID(C_ReflectionDepth + _index);
            m_ReflectionDepthID = new RenderTargetIdentifier(m_ReflectionDepth);
            return base.Setup(_index, _planeData, _planes, _renderer);
        }

        protected override void ConfigureColorDescriptor(ref RenderTextureDescriptor _descriptor, SRD_PlanarReflectionData _data)
        {
            base.ConfigureColorDescriptor(ref _descriptor, _data);
            _descriptor.colorFormat = RenderTextureFormat.ARGB32;
        }

        protected override void DoCameraSetup(CommandBuffer cmd, RenderTextureDescriptor _descriptor, RenderTargetIdentifier _target)
        {
            base.DoCameraSetup(cmd, _descriptor, _target);
            var depthDescriptor = _descriptor;
            depthDescriptor.colorFormat = RenderTextureFormat.Depth;
            depthDescriptor.depthBufferBits = 32;
            depthDescriptor.enableRandomWrite = false;
            cmd.GetTemporaryRT(m_ReflectionDepth, depthDescriptor,FilterMode.Point);
            ConfigureTarget(_target,m_ReflectionDepthID);
            ConfigureClear(ClearFlag.All,Color.clear);
        }
        
        protected override void GenerateTarget(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd,
            RenderTextureDescriptor _descriptor, RenderTargetIdentifier _target,SRD_PlanarReflectionData _data,ref GPlane _plane)
        {
            ref CameraData cameraData = ref renderingData.cameraData;
            ref Camera camera = ref cameraData.camera;
            
            Matrix4x4 planeMirrorMatrix = _plane.GetMirrorMatrix();
            Matrix4x4 cullingMatrix = camera.cullingMatrix;
            camera.cullingMatrix = cullingMatrix * planeMirrorMatrix;
            if (cameraData.camera.TryGetCullingParameters(out ScriptableCullingParameters cullingParameters))
            {
                cullingParameters.maximumVisibleLights = _data.m_LightCount;
                DrawingSettings drawingSettings = CreateDrawingSettings(m_ShaderTagIDs, ref renderingData,  SortingCriteria.CommonOpaque);
                FilteringSettings m_FilterSettings = new FilteringSettings(_data.m_IncludeTransparent? RenderQueueRange.all : RenderQueueRange.opaque);
                Matrix4x4 projectionMatrix = GL.GetGPUProjectionMatrix(cameraData.GetProjectionMatrix(), cameraData.IsCameraProjectionMatrixFlipped());
                Matrix4x4 viewMatrix = cameraData.GetViewMatrix();
                viewMatrix*= planeMirrorMatrix;
            
                RenderingUtils.SetViewAndProjectionMatrices(cmd, viewMatrix , projectionMatrix, false);
                var cameraPosition = camera.transform.position;
                cmd.SetGlobalVector( ID_CameraWorldPosition,planeMirrorMatrix.MultiplyPoint(cameraPosition));
                cmd.SetInvertCulling(true);
                context.ExecuteCommandBuffer(cmd);

                CullingResults cullResults = context.Cull(ref cullingParameters);
                context.DrawRenderers(cullResults, ref drawingSettings, ref m_FilterSettings);
                

                cmd.Clear();
                cmd.SetInvertCulling(false);
                cmd.SetGlobalVector( ID_CameraWorldPosition,cameraPosition);
                RenderingUtils.SetViewAndProjectionMatrices(cmd, cameraData.GetViewMatrix(), cameraData.GetGPUProjectionMatrix(), false);
            }
            camera.ResetCullingMatrix();
        }
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            base.OnCameraCleanup(cmd);
            cmd.ReleaseTemporaryRT(m_ReflectionDepth);
        }
        public override void Dispose() { }
    }
}