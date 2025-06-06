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
    
    [Serializable]
    public struct FPipelineExtensionParameters
    {
        [Header("Screen Space"), Tooltip("Screen Space World Position Reconstruction")]

        public EPipeLineExtensionFeature m_Features;

        public static FPipelineExtensionParameters kDefault = new FPipelineExtensionParameters()
        {
            m_Features = default,
        };
    }

    public class PipelineExtension : AScriptableRendererFeature
    {
        public static PipelineExtension Instance { get; private set; } 
        [DefaultAsset("Assets/Settings/RenderResources.asset")] public RenderResources m_Resources;
        public FPipelineExtensionParameters m_Data = FPipelineExtensionParameters.kDefault;
        private GlobalParametersPass m_GlobalParameters;
        private NormalTexturePass m_Normal;
        private MotionVectorTexturePass m_MotionVectorTexture;

        public override void Create()
        {
            Instance = this;
            m_GlobalParameters = new GlobalParametersPass() { renderPassEvent = RenderPassEvent.BeforeRendering };
            m_MotionVectorTexture = new MotionVectorTexturePass() { renderPassEvent = RenderPassEvent.BeforeRenderingOpaques - 1 };
            m_Normal = new NormalTexturePass() { renderPassEvent = RenderPassEvent.BeforeRenderingSkybox };
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
            
            if (m_Data.m_Features.IsFlagEnable(EPipeLineExtensionFeature.Normal))
                _renderer.EnqueuePass(m_Normal);

            if (m_Data.m_Features.IsFlagEnable(EPipeLineExtensionFeature.MotionVector))
                _renderer.EnqueuePass(m_MotionVectorTexture);
        }
    }
}