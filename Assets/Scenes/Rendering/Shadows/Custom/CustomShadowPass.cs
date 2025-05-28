using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Examples.Rendering.Shadows.Custom
{
    using static FShadowMapConstants;
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
        private FShadowMapConfig m_Config = FShadowMapConfig.kDefault;
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

            m_ShadowmapRT = RTHandles.Alloc(m_Config.GetDescriptor(),m_Config.GetFilterMode(),TextureWrapMode.Clamp, true,1,0,kShadowmapName);
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

        public override void Execute(ScriptableRenderContext _context, ref RenderingData _renderingData)
        {
            var shadowData = _renderingData.shadowData;
            if (!supportsMainLightShadows)
                return;

            var shadowLightIndex = _renderingData.lightData.mainLightIndex;
            var cullResults = _renderingData.cullResults;
            var lightData = _renderingData.lightData;
            if (shadowLightIndex == -1)
                return;
            
            var shadowLight = lightData.visibleLights[shadowLightIndex];
            var light = shadowLight.light;
            
            if (!cullResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(shadowLightIndex,
                    0, 1, Vector3.one,
                    (int)m_Config.resolution, shadowLight.light.shadowNearPlane,
                    out var viewMatrix, out var projMatrix, out var shadowSplitData))
                return;

            var resolution = (float)m_Config.resolution;
            
            var cmd = CommandBufferPool.Get("_ShadowmapBuffer");
            cmd.SetGlobalVector(kWorldSpaceCameraPos,_renderingData.cameraData.worldSpaceCameraPos);
            cmd.SetGlobalMatrix(kWorldToShadow, GetShadowTransform(projMatrix,viewMatrix));
            
            cmd.SetViewProjectionMatrices(viewMatrix,projMatrix);
            cmd.SetGlobalDepthBias(1f,2.5f);
            cmd.SetViewport(new Rect(0,0,resolution,resolution));
            cmd.SetGlobalVector(kShadowBias,ShadowUtils.GetShadowBias(ref shadowLight,0,ref shadowData,projMatrix,resolution));
            cmd.SetGlobalVector(kLightDirection,-shadowLight.localToWorldMatrix.GetColumn(2));
            cmd.SetGlobalVector(kLightPosition,shadowLight.localToWorldMatrix.GetColumn(3));
            _context.ExecuteCommandBuffer(cmd);
            
            var settings = new ShadowDrawingSettings(cullResults,shadowLightIndex,BatchCullingProjectionType.Orthographic) {
                useRenderingLayerMaskTest = UniversalRenderPipeline.asset.useRenderingLayers,
            };
            _context.DrawShadows(ref settings);
            cmd.Clear();
            cmd.DisableScissorRect();
            _context.ExecuteCommandBuffer(cmd);
            cmd.SetGlobalDepthBias(0f,0f);
            cmd.SetGlobalTexture(m_ShadowmapRT.name,m_ShadowmapRT.nameID);
            
            GetScaleAndBiasForLinearDistanceFade(umath.sqr(m_Config.distance),m_Config.border,out var shadowFadeScale,out var shadowFadeBias);
            var softShadowsProp = SoftShadowQualityToShaderProperty(light, light.shadows == LightShadows.Soft && shadowData.supportsSoftShadows);
            cmd.SetGlobalVector(kShadowParams,new Vector4(light.shadowStrength,softShadowsProp,shadowFadeScale,shadowFadeBias));
            cmd.SetGlobalVector(kShadowmapSize, new Vector4(1f / resolution, 1f / resolution, resolution, resolution));
            _context.ExecuteCommandBuffer(cmd);
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