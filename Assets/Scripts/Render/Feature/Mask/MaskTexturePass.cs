using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.Pipeline.Mask
{
    
    public class MaskTexturePass : ScriptableRenderPass
    {
        private MaskTextureData m_Data;
        private static readonly PassiveInstance<Material> m_MaskMaterial = new PassiveInstance<Material>(() => {
            var renderMaterial = new Material(RenderResources.FindInclude("Game/Unlit/Color")) { hideFlags = HideFlags.HideAndDontSave };
            renderMaterial.SetInt(KShaderProperties.kCull,(int)CullMode.Off);
            renderMaterial.SetInt(KShaderProperties.kColorMask,(int)ColorWriteMask.All);
            renderMaterial.SetInt(KShaderProperties.kZWrite,0);
            renderMaterial.SetInt(KShaderProperties.kZTest,(int)CompareFunction.LessEqual);
            return renderMaterial;
        },GameObject.DestroyImmediate);
        
        public static readonly int kCameraMaskTexture = Shader.PropertyToID("_CameraMaskTexture");
        public static readonly RenderTargetIdentifier kCameraMaskTextureRT = new RenderTargetIdentifier(kCameraMaskTexture);
        public MaskTexturePass Setup(MaskTextureData _data)
        {
            m_Data = _data;
            return this;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            cameraTextureDescriptor.colorFormat = RenderTextureFormat.R8;
            cameraTextureDescriptor.depthBufferBits = 0;
            cmd.GetTemporaryRT(kCameraMaskTexture, cameraTextureDescriptor);
            ConfigureTarget(RTHandles.Alloc(kCameraMaskTexture));
            base.Configure(cmd, cameraTextureDescriptor);
        }
        public override void FrameCleanup(CommandBuffer _cmd)
        {
            base.FrameCleanup(_cmd);
            _cmd.ReleaseTemporaryRT(kCameraMaskTexture);
        }

        public void Dispose()
        {
            m_MaskMaterial.Dispose();
        }

        public override void Execute(ScriptableRenderContext _context, ref RenderingData _renderingData)
        {
            DrawMask( kCameraMaskTextureRT,_context,ref _renderingData,m_Data);
        }

        private static readonly List<Renderer> kMaskRenderers = new List<Renderer>();

        public static bool Validate(MaskTextureData _data,Camera _camera)
        {
            if (!_data.collectFromProviders)
                return true;
            
            if (IMaskTextureProvider.kMasks.Count == 0)
                return false;

            kMaskRenderers.Clear();
            foreach (var provider in IMaskTextureProvider.kMasks)
                if (provider.Enable)
                    foreach (var renderer in provider.GetRenderers(_camera))
                    {
                        if(renderer.gameObject.activeInHierarchy && CullingMask.HasLayer(provider.CullingMask,renderer.gameObject.layer))
                            kMaskRenderers.Add(renderer);
                    }
            
            return kMaskRenderers.Count != 0;
        }
        
        public static void DrawMask(RenderTargetIdentifier _maskTextureId,ScriptableRenderContext _context,ref RenderingData _renderingData, MaskTextureData _data)
        {
            if(!Validate(_data, _renderingData.cameraData.camera))
                return;

            var buffer = CommandBufferPool.Get("Render Mask");
            if(_data.inheritDepth)
                buffer.SetRenderTarget(_maskTextureId,  _renderingData.cameraData.renderer.cameraDepthTargetHandle);
            else
                buffer.SetRenderTarget(_maskTextureId);
            
            buffer.ClearRenderTarget(false, true, Color.black);
            _context.ExecuteCommandBuffer(buffer);
            buffer.Clear();

            if (_data.collectFromProviders)
            {
                var renderMaterial = _data.overrideMaterial;
                if(renderMaterial == null)
                    renderMaterial = m_MaskMaterial;
                
                foreach (var renderer in kMaskRenderers)
                    for (var i = 0; i < renderer.sharedMaterials.Length; i++)
                        buffer.DrawRenderer(renderer,renderMaterial,i);
                _context.ExecuteCommandBuffer(buffer);
            }
            else
            {
                var drawingSettings = UPipeline.CreateDrawingSettings(true, _renderingData.cameraData.camera);
                if(_data.overrideShader != null)
                    drawingSettings.overrideShader = _data.overrideShader;
                else
                    drawingSettings.overrideMaterial = m_MaskMaterial;
                var filterSettings = new FilteringSettings(RenderQueueRange.all) { layerMask = _data.renderMask };
                _context.DrawRenderers(_renderingData.cullResults, ref drawingSettings, ref filterSettings);
            }

            buffer.SetRenderTarget(_renderingData.cameraData.renderer.cameraColorTargetHandle);
            _context.ExecuteCommandBuffer(buffer);
            CommandBufferPool.Release(buffer);
        }

    }

}