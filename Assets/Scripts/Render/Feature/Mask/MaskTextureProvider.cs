using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rendering.Pipeline.Mask
{
    public interface IMaskTextureProvider
    {
        public static List<IMaskTextureProvider> kMasks { get; private set; } = new List<IMaskTextureProvider>();
        IEnumerable<Renderer> Renderers { get; }
        void Register(IMaskTextureProvider _provider) => kMasks.Add(_provider);
        void Unregister(IMaskTextureProvider _provider) => kMasks.Remove(_provider);
    }
    
    [ExecuteInEditMode]
    public class MaskTextureProvider : MonoBehaviour , IMaskTextureProvider
    {
        private Renderer[] m_Renderers;
        private void OnEnable()
        {
            m_Renderers = GetComponentsInChildren<Renderer>(false);
            ((IMaskTextureProvider)this).Register(this);
        }

        private void OnDisable()
        {
            m_Renderers = null;
            ((IMaskTextureProvider)this).Unregister(this);
        }

        public IEnumerable<Renderer> Renderers => m_Renderers;

    }
}