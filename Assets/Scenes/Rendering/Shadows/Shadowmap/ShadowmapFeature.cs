using System;
using Rendering.Pipeline;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Examples.Rendering.Shadows.Shadowmap
{

    [Serializable]
    public struct FShadowMapConfig
    {
        public EShadowMapResolution resolution;
        [Min(0)] public float distance;
        [Range(0, 1)] public float border;

        public static FShadowMapConfig kDefault = new FShadowMapConfig()
        {
            resolution = EShadowMapResolution._1024,
            distance = 100f,
            border = 0.8f,
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
        }
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
