using System.Collections.Generic;
using UnityEngine;

namespace Rendering.Pipeline.Component
{
    [ExecuteInEditMode]
    public class SurfaceEffectBehaviour : MonoBehaviour , ISurfaceEffect
    {
        public CullingMask m_Mask = CullingMask.kAll;
        public SurfaceEffectCollection m_Collection;
        [SerializeField,Readonly] private List<SurfaceEffectAnimation> m_Playing = new();
        private Renderer[] m_Renderers;
        private void OnEnable()
        {
            m_Renderers = GetComponentsInChildren<Renderer>(false);
            this.OnEffectEnable();
            
#if UNITY_EDITOR
            if(!Application.isPlaying)
                UnityEditor.EditorApplication.update += Update;
#endif
        }

        private void OnDisable()
        {
            m_Renderers = null;
            this.OnEffectDisable();
            
#if UNITY_EDITOR
            if(!Application.isPlaying)
                UnityEditor.EditorApplication.update -= Update;
#endif
        }

        private void Update()
        {
            this.Tick(UTime.deltaTime);
        }

        [InspectorButtonFold(nameof(m_Collection),null)]
        public void Play(string _anim)
        {
            if (m_Collection == null)
                return;

            var successful = false;
            foreach (var clip in m_Collection.m_AnimationClips)
            {
                if(clip.name != _anim)
                    continue; 
                this.Play(clip);
                successful = true;
            }
            
            if(!successful)
                Debug.LogError($"[{nameof(SurfaceEffectBehaviour)} No animation found: {_anim} in {m_Collection.name}");           
        }

        [InspectorButtonFold(nameof(m_Collection), null)]
        void StopAll() => ((ISurfaceEffect)this).StopAll();
        
// #if UNITY_EDITOR
        // [InspectorFoldoutButton(nameof(m_Collection), null)]
        // public void NewCollection()
        // {
            // m_Collection = UnityEditor.Extensions.UEAsset.CreateScriptableInstanceAtCurrentRoot<SurfaceEffectCollection>("SurfaceEffectCollection");
        // }
// #endif
        public CullingMask CullingMask => m_Mask;
        public IEnumerable<Renderer> GetRenderers(Camera _camera) => m_Renderers;
        public List<SurfaceEffectAnimation> Playing => m_Playing;
    }

    
}