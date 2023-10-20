using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.Pipeline
{
    public class SRF_ShadowMap : ScriptableRendererFeature
    {
        public FShadowMapConfig m_ShadowMapConfig = FShadowMapConfig.kDefault;
        private SRP_ShadowMap m_ShadowMap;
        public override void Create()
        {
            m_ShadowMap = new SRP_ShadowMap(){renderPassEvent = RenderPassEvent.BeforeRenderingShadows};
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            var shadowMap = m_ShadowMap.Setup(m_ShadowMapConfig, ref renderingData);
            if(shadowMap != null)
                renderer.EnqueuePass(shadowMap);
        }
    }

    public enum EShadowResolution
    {
        _64 = 64,
        _128 = 128,
        _256 = 256,
        _512 = 512,
        _1024 = 1024,
        _2048 = 2048,
        _4096 = 4096,
    }
    
    [Serializable]
    public struct FShadowMapConfig
    {
        public EShadowResolution resolution;
        [Min(0)] public float distance;
        public bool pointSampler;
        [Range(0, 1)] public float border;
        public static FShadowMapConfig kDefault = new FShadowMapConfig()
        {
            resolution = EShadowResolution._1024,
            distance = 100f,
            border = 0.8f,
            pointSampler = false,
        };

        public RenderTextureDescriptor GetDescriptor()
        {
            int resolution = (int)this.resolution;
            RenderTextureDescriptor descriptor = new RenderTextureDescriptor(resolution, resolution, GraphicsFormat.None, GraphicsFormat.D16_UNorm);
            descriptor.shadowSamplingMode =
                RenderingUtils.SupportsRenderTextureFormat(RenderTextureFormat.Shadowmap) &&
                (SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES2)
                    ? ShadowSamplingMode.CompareDepths
                    : ShadowSamplingMode.None;
            return descriptor;
        }

        public FilterMode GetFilterMode() => pointSampler ? FilterMode.Point : FilterMode.Bilinear;

    }

    public struct FShadowMapConstants
    {
        public static readonly string kShadowmapName = "_ShadowmapTexture";
        public static readonly int kWorldToShadow = Shader.PropertyToID("_WorldToShadow");
        public static readonly int kWorldSpaceCameraPos = Shader.PropertyToID("_WorldSpaceCameraPos");
        public static readonly int kShadowBias = Shader.PropertyToID("_ShadowBias");
        public static readonly int kLightDirection = Shader.PropertyToID("_LightDirection");
        public static readonly int kLightPosition = Shader.PropertyToID("_LightPosition");
        public static readonly int kShadowmapSize = Shader.PropertyToID("_MainLightShadowmapSize");
        public static readonly int kShadowParams = Shader.PropertyToID("_ShadowParams");
    }
    
    public class SRP_ShadowMap :ScriptableRenderPass
    {
        private RTHandle m_ShadowmapRT;
        private ValueChecker<FShadowMapConfig> m_Config = new ValueChecker<FShadowMapConfig>(default);
        private bool supportsMainLightShadows;
        public SRP_ShadowMap Setup(FShadowMapConfig _config, ref RenderingData _data)
        {
            var shadowLightIndex = _data.lightData.mainLightIndex;
            
            if (!_data.shadowData.supportsSoftShadows || shadowLightIndex == -1)
                return null;
            supportsMainLightShadows = _data.shadowData.supportsMainLightShadows;
            _data.shadowData.supportsMainLightShadows = false;

            var shadowLight = _data.lightData.visibleLights[shadowLightIndex];
            var light = shadowLight.light;
            if (light.shadows == LightShadows.None)
                return null;

            if (light.type != LightType.Directional)
                Debug.LogWarning("Only directional Supported For Main Light shadowmapping");

            if (!_data.cullResults.GetShadowCasterBounds(shadowLightIndex, out var bounds))
                return null;

            if (m_Config.Check(_config))
            {
                m_ShadowmapRT?.Release();
                m_ShadowmapRT = RTHandles.Alloc(m_Config.m_Value.GetDescriptor(),m_Config.m_Value.GetFilterMode(),TextureWrapMode.Clamp, true,1,0,FShadowMapConstants.kShadowmapName);
            }
            return this;
        }
        
        public void Dispose()
        {
            m_ShadowmapRT?.Release();   
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            ConfigureTarget(m_ShadowmapRT);
            ConfigureClear(ClearFlag.All,Color.black);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var shadowData = renderingData.shadowData;
            if (!supportsMainLightShadows)
                return;

            var shadowLightIndex = renderingData.lightData.mainLightIndex;
            var cullResults = renderingData.cullResults;
            var lightData = renderingData.lightData;
            if (shadowLightIndex == -1)
                return;
            
            var shadowLight = lightData.visibleLights[shadowLightIndex];
            var light = shadowLight.light;

            if (!cullResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(shadowLightIndex,
                    0, 1, Vector3.one,
                    (int)m_Config.m_Value.resolution, shadowLight.light.shadowNearPlane,
                    out var viewMatrix, out var projMatrix, out var shadowSplitData))
                return;

            var resolution = (float)m_Config.m_Value.resolution;
            var shadowBias = ShadowUtils.GetShadowBias(ref shadowLight,0,ref shadowData,projMatrix,resolution);

            var cmd = CommandBufferPool.Get("_ShadowmapBuffer");
            cmd.SetGlobalVector(FShadowMapConstants.kWorldSpaceCameraPos,renderingData.cameraData.worldSpaceCameraPos);
            cmd.SetGlobalMatrix(FShadowMapConstants.kWorldToShadow, GetShadowTransform(projMatrix,viewMatrix));
            
            cmd.SetViewProjectionMatrices(viewMatrix,projMatrix);
            cmd.SetGlobalDepthBias(1f,2.5f);
            cmd.SetViewport(new Rect(0,0,resolution,resolution));
            cmd.SetGlobalVector(FShadowMapConstants.kShadowBias,shadowBias);
            cmd.SetGlobalVector(FShadowMapConstants.kLightDirection,-shadowLight.localToWorldMatrix.GetColumn(2));
            cmd.SetGlobalVector(FShadowMapConstants.kLightPosition,shadowLight.localToWorldMatrix.GetColumn(3));
            context.ExecuteCommandBuffer(cmd);
            
            var settings = new ShadowDrawingSettings(cullResults,shadowLightIndex,BatchCullingProjectionType.Orthographic) {
                useRenderingLayerMaskTest = UniversalRenderPipeline.asset.useRenderingLayers,
            };
            context.DrawShadows(ref settings);
            cmd.Clear();
            cmd.DisableScissorRect();
            context.ExecuteCommandBuffer(cmd);
            
            cmd.Clear();
            cmd.SetGlobalDepthBias(0f,0f);
            cmd.SetGlobalTexture(m_ShadowmapRT.name,m_ShadowmapRT.nameID);

            var invShadowAtlasWidth = 1f / resolution;
            var invShadowAtlasHeight = 1f / resolution;

            var config = m_Config.m_Value;
            var maxShadowDistanceSQ = umath.sqr(config.distance);
            var border = config.border;
            GetScaleAndBiasForLinearDistanceFade(maxShadowDistanceSQ,border,out var shadowFadeScale,out var shadowFadeBias);
            float softShadowsProp = SoftShadowQualityToShaderProperty(light, light.shadows == LightShadows.Soft && shadowData.supportsSoftShadows);
            cmd.SetGlobalVector(FShadowMapConstants.kShadowParams,new Vector4(light.shadowStrength,softShadowsProp,shadowFadeScale,shadowFadeBias));
            cmd.SetGlobalVector(FShadowMapConstants.kShadowmapSize, new Vector4(invShadowAtlasWidth, invShadowAtlasHeight, resolution, resolution));
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        static void GetScaleAndBiasForLinearDistanceFade(float _fadeDistance, float _border, out float _scale, out float _bias)
        {
            if (_border < 0.0001f)
            {
                var multiplier = 1000f;
                _scale = multiplier;
                _bias = -_fadeDistance * multiplier;
                return;
            }

            _border = 1 - _border;
            _border = umath.pow2(_border);
            var distanceFadeNear = _border * _fadeDistance;
            _scale = 1f / (_fadeDistance - distanceFadeNear);
            _bias = -distanceFadeNear / (_fadeDistance - distanceFadeNear);
        }
        static float SoftShadowQualityToShaderProperty(Light light, bool softShadowsEnabled)
        {
            float softShadows = softShadowsEnabled ? 1.0f : 0.0f;
            if (light.TryGetComponent(out UniversalAdditionalLightData additionalLightData))
            {
                var softShadowQuality = (additionalLightData.softShadowQuality == SoftShadowQuality.UsePipelineSettings)
                    ? UReflection.GetFieldValue<SoftShadowQuality>(UniversalRenderPipeline.asset,"m_SoftShadowQuality") 
                    : additionalLightData.softShadowQuality;
                softShadows *= Math.Max((int)softShadowQuality, (int)SoftShadowQuality.Low);
            }

            return softShadows;
        }
        
        static Matrix4x4 GetShadowTransform(Matrix4x4 proj, Matrix4x4 view)
        {
            // Currently CullResults ComputeDirectionalShadowMatricesAndCullingPrimitives doesn't
            // apply z reversal to projection matrix. We need to do it manually here.
            if (SystemInfo.usesReversedZBuffer)
            {
                proj.m20 = -proj.m20;
                proj.m21 = -proj.m21;
                proj.m22 = -proj.m22;
                proj.m23 = -proj.m23;
            }

            Matrix4x4 worldToShadow = proj * view;

            var textureScaleAndBias = Matrix4x4.identity;
            textureScaleAndBias.m00 = 0.5f;
            textureScaleAndBias.m11 = 0.5f;
            textureScaleAndBias.m22 = 0.5f;
            textureScaleAndBias.m03 = 0.5f;
            textureScaleAndBias.m23 = 0.5f;
            textureScaleAndBias.m13 = 0.5f;
            // textureScaleAndBias maps texture space coordinates from [-1,1] to [0,1]

            // Apply texture scale and offset to save a MAD in shader.
            return textureScaleAndBias * worldToShadow;
        }
    }
}