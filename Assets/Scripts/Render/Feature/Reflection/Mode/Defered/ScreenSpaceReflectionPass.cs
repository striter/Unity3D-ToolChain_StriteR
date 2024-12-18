using System;
using Rendering.PostProcess;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.Pipeline
{
    [Serializable]
    public struct ScreenSpaceReflectionData
    {
        [Range(1,4)] public int downSample;
        public DBlurs blurData;

        public static readonly ScreenSpaceReflectionData kDefault = new ScreenSpaceReflectionData()
        {
            downSample = 1,
            blurData = DBlurs.kDefault,
        };
    }
    
    class ScreenSpaceReflectionPass : ScriptableRenderPass
    {
        private readonly PassiveInstance<Shader> m_ReflectionBlit=new PassiveInstance<Shader>(()=>RenderResources.FindInclude("Hidden/ScreenSpaceReflection"));
        private readonly FBlursCore m_Blur;
        private readonly Material m_Material;
        private ScreenSpaceReflectionData m_Data;
        static readonly int kSSRTex = Shader.PropertyToID("_ScreenSpaceReflectionTexture");
        static readonly RenderTargetIdentifier kSSRTexID = new RenderTargetIdentifier(kSSRTex);

        public ScreenSpaceReflectionPass(ScreenSpaceReflectionData _data, FBlursCore _blurs)
        {
            m_Data = _data;
            m_Material = new Material(m_ReflectionBlit){hideFlags = HideFlags.HideAndDontSave};
            m_Blur = _blurs;
        }

        public void Dispose()
        {            
            GameObject.DestroyImmediate(m_Material);
        }

        public override void Configure(CommandBuffer _cmd, RenderTextureDescriptor _cameraTextureDescriptor)
        {
            _cmd.GetTemporaryRT(kSSRTex, _cameraTextureDescriptor.width / m_Data.downSample, _cameraTextureDescriptor.height / m_Data.downSample, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
            ConfigureTarget(RTHandles.Alloc(kSSRTexID));
            base.Configure(_cmd, _cameraTextureDescriptor);
        }

        public override void OnCameraCleanup(CommandBuffer _cmd)
        {
            base.OnCameraCleanup(_cmd);
            _cmd.ReleaseTemporaryRT(kSSRTex);
        }

        public override void Execute(ScriptableRenderContext _context, ref RenderingData _renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("Screen Space Reflection Texture");
            cmd.Blit(_renderingData.cameraData.renderer.cameraColorTargetHandle,kSSRTexID,m_Material);
            
            _context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
        
    }
}