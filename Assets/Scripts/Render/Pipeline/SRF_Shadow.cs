using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Rendering.Pipeline
{
    public class SRF_Shadow : ScriptableRendererFeature
    {
        private SRP_Shadow[] m_ShadowPasses;
        public override void Create()
        {
            m_ShadowPasses = new SRP_Shadow[4] { new SRP_Shadow(), new SRP_Shadow(), new SRP_Shadow(), new SRP_Shadow() };
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
                if(visibleLight.light.shadows== LightShadows.None)
                    continue;
                
                renderer.EnqueuePass(m_ShadowPasses[shadowIndex].Setup(shadowIndex,visibleLight.light));
                shadowIndex++;
                if(shadowIndex>=m_ShadowPasses.Length)
                    break;
            }
        }
    }

    public class SRP_Shadow:ScriptableRenderPass,ISRPBase
    {
        public SRP_Shadow()
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingShadows;
        }
        private const string KW_ShadowMap = "_RealTimeShadowMap";
        public Light m_Light { get;private set; }
        public int m_Index { get;private set; }
        private int ID_ShadowMap;
        private RenderTargetIdentifier RT_ID_ShadowMap;
        public SRP_Shadow Setup(int _setup,Light _light)
        {
            m_Light = _light;
            m_Index = _setup;
            ID_ShadowMap = Shader.PropertyToID(  KW_ShadowMap + m_Index);
            RT_ID_ShadowMap=new RenderTargetIdentifier(ID_ShadowMap);
            return this;
        }
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            base.Configure(cmd, cameraTextureDescriptor);
            cameraTextureDescriptor.colorFormat = RenderTextureFormat.RHalf;
            cmd.GetTemporaryRT(ID_ShadowMap,cameraTextureDescriptor);
            ConfigureTarget(RT_ID_ShadowMap);
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            ShadowDrawingSettings settings = new ShadowDrawingSettings(renderingData.cullResults, m_Index);
            context.DrawShadows(ref settings);
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