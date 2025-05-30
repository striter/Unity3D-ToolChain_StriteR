using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Examples.Rendering.Shadows.Shadowmap
{

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
            var size = (int)resolution;
            return new RenderTextureDescriptor(size, size, GraphicsFormat.None, GraphicsFormat.D16_UNorm)
            {
                shadowSamplingMode = RenderingUtils.SupportsRenderTextureFormat(RenderTextureFormat.Shadowmap) &&
                                     (SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES2)
                    ? ShadowSamplingMode.CompareDepths
                    : ShadowSamplingMode.None
            };
            ;
        }

        public FilterMode GetFilterMode() => pointSampler ? FilterMode.Point : FilterMode.Bilinear;
    }

    public class ShadowmapFeature : ScriptableRendererFeature
    {
        public FShadowMapConfig m_ShadowMapConfig = FShadowMapConfig.kDefault;
        private ShadowmapPass m_ShadowMapPass;

        public override void Create()
        {
            m_ShadowMapPass = new ShadowmapPass() { renderPassEvent = RenderPassEvent.BeforeRenderingShadows };
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            m_ShadowMapPass.Dispose();
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            var shadowMap = m_ShadowMapPass.Setup(m_ShadowMapConfig, ref renderingData);
            if (shadowMap != null)
                renderer.EnqueuePass(shadowMap);
        }

    }
}
