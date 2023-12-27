using Rendering.PostProcess;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.Pipeline
{
    class ScreenSpaceReflectionPass : ScriptableRenderPass
    {
        private PlanarReflectionData m_Data;
        private readonly PassiveInstance<Shader> m_ReflectionBlit=new PassiveInstance<Shader>(()=>RenderResources.FindInclude("Hidden/ScreenSpaceReflection"));
        private readonly FBlursCore m_Blur;
        private readonly Material m_Material;
        static readonly int kSSRTex = Shader.PropertyToID("_ScreenSpaceReflectionTexture");
        static readonly RenderTargetIdentifier kSSRTexID = new RenderTargetIdentifier(kSSRTex);

        public ScreenSpaceReflectionPass(FBlursCore _blurs)
        {
            m_Material = new Material(m_ReflectionBlit){hideFlags = HideFlags.HideAndDontSave};
            m_Blur = _blurs;
        }

        public void Dispose()
        {            
            GameObject.DestroyImmediate(m_Material);
        }

        
        public void EnqueuePass(PlanarReflectionData _data,ScriptableRenderer _renderer,RenderPassEvent _event)
        {
            this.renderPassEvent = _event;
            m_Data = _data;
            _renderer.EnqueuePass(this);
            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            foreach (var reflection in PlanarReflection.m_Reflections)
                reflection.SetPropertyBlock(propertyBlock,4);
        }

        public override void Configure(CommandBuffer _cmd, RenderTextureDescriptor _cameraTextureDescriptor)
        {
            _cmd.GetTemporaryRT(kSSRTex, _cameraTextureDescriptor.width, _cameraTextureDescriptor.height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
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
            CommandBuffer cmd = CommandBufferPool.Get("Planar Reflection Pass");
            cmd.Blit(_renderingData.cameraData.renderer.cameraColorTargetHandle,kSSRTexID,m_Material);
            
            _context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
        
    }
}