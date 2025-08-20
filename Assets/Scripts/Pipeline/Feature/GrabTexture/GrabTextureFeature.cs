using System.Collections.Generic;
using System.Linq.Extensions;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

namespace Rendering.Pipeline.GrabPass
{
    public class GrabTextureFeature : AScriptableRendererFeature
    {
        [Header("Debug")]
        [Readonly] public List<GrabTextureBehaviour> kActiveComponentsDebug = new List<GrabTextureBehaviour>();

        public override void Create()
        {
        }

        protected override void EnqueuePass(ScriptableRenderer _renderer, ref RenderingData _renderingData)
        {
            foreach (var grabTexture in GrabTextureBehaviour.kActiveComponents)
                _renderer.EnqueuePass(grabTexture.m_Pass.Setup(grabTexture));
            
            GrabTextureBehaviour.kActiveComponents.FillList(kActiveComponentsDebug);
        }
    }
}