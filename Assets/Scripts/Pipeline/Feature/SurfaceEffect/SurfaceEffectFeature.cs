using System.Collections.Generic;
using Rendering.Pipeline.Component;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.Pipeline
{
    public class SurfaceEffectFeature : AScriptableRendererFeature
    {
        private Dictionary<int,SurfaceEffectPass> m_Passes = new();
        static RenderPassEvent ToRenderPassEvent(int _renderQueue)
        {
            return _renderQueue switch
            {
                < 2000 => RenderPassEvent.BeforeRenderingPrePasses,
                < 2500 => RenderPassEvent.AfterRenderingOpaques,
                < 3000 => RenderPassEvent.AfterRenderingSkybox,
                < 3500 => RenderPassEvent.AfterRenderingTransparents,
                _ => RenderPassEvent.AfterRenderingSkybox
            };
        }
        public override void Create()
        {
            m_Passes.Clear();
            foreach (var renderPassEvent in UEnum.GetEnums<RenderPassEvent>())
                m_Passes.Add((int)renderPassEvent, new SurfaceEffectPass().Init(renderPassEvent));
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            m_Passes.Clear();
        }

        protected override void EnqueuePass(ScriptableRenderer _renderer, ref RenderingData _renderingData)
        {
            if (_renderingData.cameraData.isPreviewCamera)
                return;

            foreach (var pass in m_Passes.Values)
                pass.Setup();
            
            foreach (var obj in ISurfaceEffect.kBehaviours)
            {
                foreach (var (renderer, effectMaterial) in obj.GetSurfaceEffectDrawCalls(_renderingData.cameraData.camera))
                {
                    var pass = m_Passes[(int)ToRenderPassEvent(effectMaterial.renderQueue)];
                    pass.Dispatch(effectMaterial,renderer);
                }
            }
            
            foreach (var pass in m_Passes.Values)
                if(pass.Available)
                    _renderer.EnqueuePass(pass);
        }
    }

    public class SurfaceEffectPass:ScriptableRenderPass
    {
        private string m_ProfilerTag = string.Empty;
        private List<Material> m_Materials = new List<Material>();
        private List<Renderer> m_Renderers = new List<Renderer>();
        public SurfaceEffectPass Init(RenderPassEvent _event)
        {
            renderPassEvent = _event;
            m_ProfilerTag = $"SurfaceEffectPass {_event}";
            return this;
        }

        public void Setup()
        {
            m_Materials.Clear();
            m_Renderers.Clear();
        }

        public bool Available => m_Renderers.Count > 0;
        
        public void Dispatch(Material _material,Renderer _renderer)
        {
            m_Materials.Add(_material);
            m_Renderers.Add(_renderer);
        }
        
        public override void Execute(ScriptableRenderContext _context, ref RenderingData _renderingData)
        {
            var cmd = CommandBufferPool.Get(m_ProfilerTag);
            for (var i = 0; i < m_Renderers.Count; i++)
            {
                var renderer = m_Renderers[i];
                var material = m_Materials[i];
                for(var j=0;j<renderer.sharedMaterials.Length;j++)
                    cmd.DrawRenderer(renderer,material,j);
            }
            
            _context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}