using System;
using System.Collections;
using System.Collections.Generic;
using Rendering.Pipeline;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace CustomShadow
{
    public enum EShadowResolution
    {
        _128=128,
        _256=256,
        _512=512,
        _1024=1024,
        _2048=2048,
        _4096=4096,
    }
    [Serializable]
    public struct ShadowData
    {
        public EShadowResolution m_ShadowResolution;
    }
    public class CustomShadowRenderFeature : ScriptableRendererFeature
    {
        public ShadowData m_ShadowData;
        private CustomShadowRenderPass[] m_ShadowPasses;
        public override void Create()
        {
            m_ShadowPasses = new CustomShadowRenderPass[4]
            {
                new CustomShadowRenderPass(), new CustomShadowRenderPass(), new CustomShadowRenderPass(), new CustomShadowRenderPass()
            };
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            foreach (var shadowPass in m_ShadowPasses)
                shadowPass.Dispose();
        }
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            int shadowIndex = 0;
            foreach (var visibleLight in renderingData.lightData.visibleLights)
            {
                if (visibleLight.light.shadows == LightShadows.None)
                    continue;

                renderer.EnqueuePass(m_ShadowPasses[shadowIndex].Setup(shadowIndex, visibleLight.light,m_ShadowData));
                shadowIndex++;
                if (shadowIndex >= m_ShadowPasses.Length)
                    break;
            }
        }
    }

    public class CustomShadowRenderPass : ScriptableRenderPass, ISRPBase
    {
        public CustomShadowRenderPass()
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingShadows;
        }
        private const string KW_ShadowMap = "_RealTimeShadowMap";
        public Light m_Light { get; private set; }
        public int m_Index { get; private set; }
        private int ID_ShadowMap;
        private RenderTargetIdentifier RT_ID_ShadowMap;
        private ShadowData m_ShadowData;
        public CustomShadowRenderPass Setup(int _setup, Light _light,ShadowData _shadowData)
        {
            m_Light = _light;
            m_Index = _setup;
            m_ShadowData = _shadowData;
            ID_ShadowMap = Shader.PropertyToID(KW_ShadowMap + m_Index);
            RT_ID_ShadowMap = new RenderTargetIdentifier(ID_ShadowMap);
            return this;
        }
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            base.Configure(cmd, cameraTextureDescriptor);
            int resolution = (int)m_ShadowData.m_ShadowResolution;
            RenderTextureDescriptor descriptor = new RenderTextureDescriptor(resolution, resolution,RenderTextureFormat.Shadowmap,16);
            cmd.GetTemporaryRT(ID_ShadowMap, descriptor);
            ConfigureTarget(RT_ID_ShadowMap);
            ConfigureClear(ClearFlag.All, Color.black);
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // ShadowDrawingSettings settings = new ShadowDrawingSettings(renderingData.cullResults, m_Index);
            // context.DrawShadows(ref settings);
        }
        public override void FrameCleanup(CommandBuffer cmd)
        {
            base.FrameCleanup(cmd);
            cmd.ReleaseTemporaryRT(ID_ShadowMap);
        }
        public void Dispose()
        {
        }
    }
    
}
