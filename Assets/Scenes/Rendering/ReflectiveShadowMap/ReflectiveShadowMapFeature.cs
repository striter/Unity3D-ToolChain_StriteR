using System;
using System.Linq;
using System.Linq.Extensions;
using Pipeline;
using Runtime.Random;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.Pipeline
{
    [Serializable]
    public struct ReflectionShadowMapData
    {
        [Range(1,10)] public float intensity;
        [Clamp(0)] public float sampleRadius;
        [IntEnum(64,128,256,512,1024)] public int maxSampleCount;
        public EShadowMapResolution shadowMapResolution;
        public EDownSample downSample;
        [DefaultAsset("Hidden/ReflectiveShadowMapSample")]public Shader screenSpaceSample;
        public bool debug;
        public static readonly ReflectionShadowMapData kDefault = new(){shadowMapResolution = EShadowMapResolution._2048,sampleRadius = 5f,intensity = 1f};
    }

    public class ReflectiveShadowMapFeature : ScriptableRendererFeature
    {
        public ReflectionShadowMapData m_Data = ReflectionShadowMapData.kDefault;
        private ReflectionShadowMapPass m_ReflectionShadowMapPass;
        public override void Create()
        {
            m_ReflectionShadowMapPass = new ReflectionShadowMapPass(m_Data) {renderPassEvent = RenderPassEvent.BeforeRenderingOpaques};
            if(m_Data.debug)
                m_ReflectionShadowMapPass.renderPassEvent = RenderPassEvent.AfterRendering;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            m_ReflectionShadowMapPass?.Dispose();
            m_ReflectionShadowMapPass = null;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if(m_ReflectionShadowMapPass == null || renderingData.cameraData.isPreviewCamera)
                return;
            
            var pass = m_ReflectionShadowMapPass.Setup(ref renderingData);
            if(pass == null)
                return;
            
            renderer.EnqueuePass(pass);
        }
    }

    public class ReflectionShadowMapPass : ScriptableRenderPass
    {
        private static readonly int kRSMFluxID = Shader.PropertyToID("_RSMFlux");
        private RTHandle m_RSMFluxRT;
        
        private static readonly int kRSMNormalID = Shader.PropertyToID("_RSMNormal");
        private RTHandle m_RSMNormalRT;
        
        private static readonly int kRSMWorldPosID = Shader.PropertyToID("_RSMWorldPos");
        private RTHandle m_RSMWorldPosRT;
        
        private static readonly int kRSMSampleID = Shader.PropertyToID("_RSMSample");
        private RTHandle m_RSMSampleRT;
        
        private static readonly int kRandomVectorCount = Shader.PropertyToID("_RandomVectorCount");
        private static readonly int kRandomVectors = Shader.PropertyToID("_RandomVectors");
        private static string kKeyword = "_RSM";
        private static readonly int kRSMParams = Shader.PropertyToID("_RSMParams");
        
        private ReflectionShadowMapData m_Data;
        private Material m_SampleMaterial;
        private float2[] m_PoissonDisk;
        private Vector4[] m_RandomVectors;
        public ReflectionShadowMapPass(ReflectionShadowMapData _config = default)
        {
            m_Data = _config;
            m_SampleMaterial = CoreUtils.CreateEngineMaterial(m_Data.screenSpaceSample);

            m_PoissonDisk = ULowDiscrepancySequences.PoissonDisk2D(m_Data.maxSampleCount,30,(SystemRandom)new System.Random(nameof(ReflectionShadowMapPass).GetHashCode())).ToArray();
            m_RandomVectors = new Vector4[m_PoissonDisk.Length];
        }
        public void Dispose()
        {
            CoreUtils.Destroy(m_SampleMaterial);
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            base.Configure(cmd, cameraTextureDescriptor);
            cameraTextureDescriptor.depthBufferBits = 0;
            cameraTextureDescriptor.width /= (int)m_Data.downSample;
            cameraTextureDescriptor.height /= (int)m_Data.downSample;
            cameraTextureDescriptor.colorFormat = RenderTextureFormat.ARGB32;
            cmd.GetTemporaryRT(kRSMSampleID, cameraTextureDescriptor);
            m_RSMSampleRT = RTHandles.Alloc(kRSMSampleID);
            
            cameraTextureDescriptor.depthBufferBits = 16;
            cameraTextureDescriptor.width = (int)m_Data.shadowMapResolution;
            cameraTextureDescriptor.height = (int)m_Data.shadowMapResolution;
            cameraTextureDescriptor.colorFormat = RenderTextureFormat.ARGB32;
            cmd.GetTemporaryRT(kRSMFluxID, cameraTextureDescriptor);
            m_RSMFluxRT = RTHandles.Alloc(kRSMFluxID);
            
            cameraTextureDescriptor.colorFormat = RenderTextureFormat.ARGB32;
            cmd.GetTemporaryRT(kRSMNormalID, cameraTextureDescriptor);
            m_RSMNormalRT = RTHandles.Alloc(kRSMNormalID);
            
            cameraTextureDescriptor.colorFormat = RenderTextureFormat.ARGBHalf;
            cmd.GetTemporaryRT(kRSMWorldPosID, cameraTextureDescriptor);
            m_RSMWorldPosRT = RTHandles.Alloc(kRSMWorldPosID);
            
            cmd.EnableKeyword(kKeyword,true);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            base.FrameCleanup(cmd);
            cmd.ReleaseTemporaryRT(kRSMSampleID);
            m_RSMSampleRT?.Release();
            m_RSMSampleRT = null;
            
            cmd.ReleaseTemporaryRT(kRSMFluxID);
            m_RSMFluxRT?.Release();
            m_RSMFluxRT = null;
            
            cmd.ReleaseTemporaryRT(kRSMNormalID);
            m_RSMNormalRT?.Release();
            m_RSMNormalRT = null;
            
            cmd.ReleaseTemporaryRT(kRSMWorldPosID);
            m_RSMWorldPosRT?.Release();
            m_RSMWorldPosRT = null;
            
            cmd.EnableKeyword(kKeyword,false);
        }
        
        public ReflectionShadowMapPass Setup(ref RenderingData _data)
        {
            var shadowLightIndex = _data.lightData.mainLightIndex;
                
            if (!_data.shadowData.supportsSoftShadows || shadowLightIndex == -1)
                return null;

            var shadowLight = _data.lightData.visibleLights[shadowLightIndex];
            var light = shadowLight.light;
            if (light.shadows == LightShadows.None)
                return null;

            if (light.type != LightType.Directional)
                Debug.LogWarning("Only directional Supported");

            if (!_data.cullResults.GetShadowCasterBounds(shadowLightIndex, out var bounds))
                return null;

            return this;
        }
        
        private static string kTitle = "Reflection Shadow Map";
        public override void Execute(ScriptableRenderContext _context, ref RenderingData _renderingData)
        {
            var shadowLightIndex = _renderingData.lightData.mainLightIndex;
            if (!_renderingData.cullResults.GetShadowCasterBounds(shadowLightIndex, out var bounds))
                return;
            var buffer = CommandBufferPool.Get(kTitle);
            
            buffer.BeginSample(kTitle);
            
            var light = _renderingData.lightData.visibleLights[shadowLightIndex].light;
            UShadow.CalculateDirectionalShadowMatrices(light, bounds, out var viewMatrix, out var projMatrix);
            buffer.SetViewProjectionMatrices(viewMatrix, projMatrix);
            _context.ExecuteCommandBuffer(buffer);
            
            var filterSettings = new FilteringSettings( RenderQueueRange.all,light.renderingLayerMask);
            var drawingSettings = new DrawingSettings  {
                sortingSettings = new SortingSettings(_renderingData.cameraData.camera),
                enableDynamicBatching = true,
                enableInstancing = true,
                perObjectData = PerObjectData.None,
            };
            
            ExecuteAdditionalShadowMap(KShaderTagId.kAlbedoAlpha, kRSMFluxID, buffer, _context, ref _renderingData, ref drawingSettings, ref filterSettings);
            ExecuteAdditionalShadowMap(KShaderTagId.kDepthNormals, kRSMNormalID, buffer, _context, ref _renderingData, ref drawingSettings, ref filterSettings);
            ExecuteAdditionalShadowMap(KShaderTagId.kWorldPosition, kRSMWorldPosID, buffer, _context, ref _renderingData, ref drawingSettings, ref filterSettings);
            
            buffer.Clear();
            var worldToShadowProjection = UShadow.CalculateWorldToShadowMatrix(projMatrix,viewMatrix);
            buffer.SetGlobalMatrix(KShaderProperties.kWorldToShadow,worldToShadowProjection);
            
            buffer.SetViewProjectionMatrices(_renderingData.cameraData.camera.worldToCameraMatrix,_renderingData.cameraData.camera.nonJitteredProjectionMatrix);
            if(m_Data.debug)
                buffer.SetRenderTarget(_renderingData.cameraData.renderer.cameraColorTargetHandle);
            else
                buffer.SetRenderTarget(m_RSMSampleRT);
            buffer.ClearRenderTarget(true, true, Color.black);

            var cameraRight = _renderingData.cameraData.camera.transform.right;
            var cameraUp = _renderingData.cameraData.camera.transform.up;
            m_PoissonDisk.Select(p => {
                p = (p - .5f) ;
                var direction = (cameraUp * p.y + cameraRight * p.x) * m_Data.sampleRadius;
                var directionSS = math.mul(worldToShadowProjection, direction.ToVector4(0));
                return new float4(directionSS.xyz,p.magnitude() * 2f);
            }).Traversal((i, p) => m_RandomVectors[i] = p);
            m_SampleMaterial.SetInt(kRandomVectorCount,m_PoissonDisk.Length);
            m_SampleMaterial.SetVectorArray(kRandomVectors,m_RandomVectors);
            Shader.SetGlobalVector(kRSMParams,new Vector4(m_Data.intensity,0,0,0));
            
            buffer.DrawMesh(UPipeline.kFullscreenMesh, Matrix4x4.identity , m_SampleMaterial);
            buffer.EndSample(kTitle);
            _context.ExecuteCommandBuffer(buffer);
            CommandBufferPool.Release(buffer);
        }

        void ExecuteAdditionalShadowMap(ShaderTagId _tag, int rtId, CommandBuffer buffer, ScriptableRenderContext _context, ref RenderingData _renderingData, ref DrawingSettings _drawingSettings, ref FilteringSettings _filterSettings)
        {
            buffer.Clear();
            buffer.SetRenderTarget(rtId);
            buffer.ClearRenderTarget(true, true, Color.black);
            _context.ExecuteCommandBuffer(buffer);
            _drawingSettings.SetShaderPassName(0,_tag);
            _context.DrawRenderers(_renderingData.cullResults, ref _drawingSettings, ref _filterSettings);
        }
        
    }

}