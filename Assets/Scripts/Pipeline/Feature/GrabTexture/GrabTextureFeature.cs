using System.Collections.Generic;
using System.Linq.Extensions;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Rendering.Pipeline.GrabPass
{
    public class GrabTextureFeature : AScriptableRendererFeature
    {
        public GrabTextureConfig m_Config = GrabTextureConfig.kDefault;
        [Header("Debug")]
        [Readonly] public List<GrabTextureBehaviour> kActiveComponents = new List<GrabTextureBehaviour>();

        public override void Create()
        {
        }

        protected override void EnqueuePass(ScriptableRenderer _renderer, ref RenderingData _renderingData)
        {
            GrabTextureBehaviour.kActiveComponents.FillList(kActiveComponents);
            foreach (var grabTexture in GrabTextureBehaviour.kActiveComponents)
                _renderer.EnqueuePass(GrabTexturePass.Spawn(this,grabTexture.m_Data));
        }
    }
}