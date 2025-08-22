using System;
using System.Collections.Generic;
using System.Linq.Extensions;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Rendering.PostProcess;

namespace Rendering.Pipeline
{
    [Flags]
    public enum EPipeLineExtensionFeature
    {
        Normal = 1 << 0,
        MotionVector = 1 << 1,
    }
    

    public class PipelineExtension : AScriptableRendererFeature
    {
        public static PipelineExtension Instance { get; private set; } 
        [DefaultAsset("Assets/Settings/RenderResources.asset")] public RenderResources m_Resources;
        
        public EPipeLineExtensionFeature m_Features;
        [Foldout(nameof(m_Features),EPipeLineExtensionFeature.Normal)] public NormalTexturePassData m_NormalTextureData = NormalTexturePassData.kDefault;
        private GlobalParametersPass m_GlobalParameters;
        private NormalTexturePass m_Normal;
        private MotionVectorTexturePass m_MotionVectorTexture;

        public override void Create()
        {
            Instance = this;
            m_GlobalParameters = new GlobalParametersPass() { renderPassEvent = RenderPassEvent.BeforeRendering };
            m_MotionVectorTexture = new MotionVectorTexturePass() { renderPassEvent = RenderPassEvent.BeforeRenderingOpaques - 1 };
            m_Normal = new NormalTexturePass();
        }

        protected override void Dispose(bool _disposing)
        {
            base.Dispose(_disposing);
            Instance = null;
            m_GlobalParameters.Dispose();
            m_MotionVectorTexture.Dispose();
            m_Normal.Dispose();
        }

        protected override void EnqueuePass(ScriptableRenderer _renderer, ref RenderingData _renderingData)
        {
            _renderer.EnqueuePass(m_GlobalParameters);
            
            if (m_Features.IsFlagEnable(EPipeLineExtensionFeature.Normal))
                _renderer.EnqueuePass(m_Normal.Setup(m_NormalTextureData));

            if (m_Features.IsFlagEnable(EPipeLineExtensionFeature.MotionVector))
                _renderer.EnqueuePass(m_MotionVectorTexture);
        }
    }
}