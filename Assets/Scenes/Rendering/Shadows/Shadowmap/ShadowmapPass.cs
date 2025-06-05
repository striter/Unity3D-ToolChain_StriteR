using Pipeline;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Examples.Rendering.Shadows.Shadowmap
{
    using static FShadowMapConstants;
    using static UShadow;
    public struct FShadowMapConstants
    {
        public static readonly string kShadowmapName = "_ShadowmapTexture";
        public static readonly int kWorldSpaceCameraPos = Shader.PropertyToID("_WorldSpaceCameraPos");
        public static readonly int kShadowBias = Shader.PropertyToID("_ShadowBias");
        public static readonly int kLightDirection = Shader.PropertyToID("_LightDirection");
        public static readonly int kLightPosition = Shader.PropertyToID("_LightPosition");
        public static readonly int kShadowmapSize = Shader.PropertyToID("_MainLightShadowmapSize");
        public static readonly int kShadowParams = Shader.PropertyToID("_ShadowParams");
        public static readonly int kWorldToShadow = Shader.PropertyToID("_WorldToShadow");
    }

    public class ShadowmapPass :ScriptableRenderPass
    {
        private RTHandle m_ShadowmapRT;
        private ValueChecker<FShadowMapConfig> m_Config = new ValueChecker<FShadowMapConfig>(default);
        private bool supportsMainLightShadows;
        public ShadowmapPass Setup(FShadowMapConfig _config, ref RenderingData _data)
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
            m_ShadowmapRT = null;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            ConfigureTarget(m_ShadowmapRT);
            ConfigureClear(ClearFlag.All,Color.black);
        }

        public override void Execute(ScriptableRenderContext _context, ref RenderingData _renderingData)
        {
            if (!supportsMainLightShadows)
                return;

            var config = m_Config.m_Value;
            var shadowLightIndex = _renderingData.lightData.mainLightIndex;
            var cullResults = _renderingData.cullResults;
            var lightData = _renderingData.lightData;
            var shadowData = _renderingData.shadowData;
            if (shadowLightIndex == -1)
                return;
            
            var shadowLight = lightData.visibleLights[shadowLightIndex];
            var light = shadowLight.light;
            
            
            if (!_renderingData.cullResults.GetShadowCasterBounds(shadowLightIndex, out var bounds))
                return;
            
            CalculateDirectionalShadowMatrices(light, bounds, out var viewMatrix, out var projMatrix);

            var resolution = (float)config.resolution;
            
            var cmd = CommandBufferPool.Get("_ShadowmapBuffer");
            cmd.SetGlobalVector(kWorldSpaceCameraPos,_renderingData.cameraData.worldSpaceCameraPos);
            
            cmd.SetViewProjectionMatrices( viewMatrix,projMatrix);
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
            cmd.SetGlobalMatrix(kWorldToShadow,CalculateWorldToShadowMatrix(projMatrix,viewMatrix));
            cmd.SetGlobalDepthBias(0f,0f);
            cmd.SetGlobalTexture(m_ShadowmapRT.name,m_ShadowmapRT.nameID);
            
            GetScaleAndBiasForLinearDistanceFade(umath.sqr(config.distance),config.border,out var shadowFadeScale,out var shadowFadeBias);
            cmd.SetGlobalVector(kShadowParams,new Vector4(light.shadowStrength,0,shadowFadeScale,shadowFadeBias));
            cmd.SetGlobalVector(kShadowmapSize, new Vector4(1f / resolution, 1f / resolution, resolution, resolution));
            _context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

    }

}