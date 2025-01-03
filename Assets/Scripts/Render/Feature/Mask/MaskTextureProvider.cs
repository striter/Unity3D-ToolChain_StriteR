using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rendering.Pipeline.Mask
{
    public interface IMaskTextureProvider
    {
        public static List<IMaskTextureProvider> kMasks { get; private set; } = new List<IMaskTextureProvider>();
        public CullingMask CullingMask { get; }
        public bool Enable { get; }
        IEnumerable<Renderer> GetRenderers(Camera _camera);
    }
    
    [ExecuteInEditMode]
    public class MaskTextureProvider : MonoBehaviour , IMaskTextureProvider
    {
        private Renderer[] m_Renderers;
        [CullingMask] public int m_CullingMask = -1;
        public CullingMask CullingMask => m_CullingMask;
        public bool Enable => this.enabled;
        private void OnEnable()
        {
            m_Renderers = GetComponentsInChildren<Renderer>(false);
            this.OnMaskProviderEnable();
        }

        private void OnDisable()
        {
            m_Renderers = null;
            this.OnMaskProviderDisable();
        }

        IEnumerable<Renderer> IMaskTextureProvider.GetRenderers(Camera _camera) => m_Renderers;
    }

    public static class IMaskTextureProvider_Extension
    {
        public static void OnMaskProviderEnable(this IMaskTextureProvider _effect)
        {
            IMaskTextureProvider.kMasks.Add(_effect);
        }

        public static void OnMaskProviderDisable(this IMaskTextureProvider _effect)
        {
            IMaskTextureProvider.kMasks.Remove(_effect);
        }
    }
}