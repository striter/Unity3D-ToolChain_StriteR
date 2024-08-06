using System;
using System.Collections.Generic;
using System.Linq.Extensions;
using Unity.Mathematics;
using UnityEngine;

namespace Rendering.Pipeline.Component
{
    [ExecuteInEditMode]
    public class SurfaceEffectBehaviour : MonoBehaviour
    {
        public CullingMask m_Mask = CullingMask.kAll;
        public static List<SurfaceEffectBehaviour> kBehaviours { get; private set; } = new List<SurfaceEffectBehaviour>();
        public SurfaceEffectCollection m_Collection;
        [SerializeField,Readonly]private List<SurfaceEffectAnimation> m_Playing = new List<SurfaceEffectAnimation>();
        private Renderer[] m_Renderers;
        
        private void OnEnable()
        {
            kBehaviours.Add(this);
            m_Renderers = GetComponentsInChildren<Renderer>(false);
            
#if UNITY_EDITOR
            if(!Application.isPlaying)
                UnityEditor.EditorApplication.update += Update;
#endif
        }

        private void OnDisable()
        {
            kBehaviours.Remove(this);
            m_Renderers = null;
            
#if UNITY_EDITOR
            if(!Application.isPlaying)
                UnityEditor.EditorApplication.update -= Update;
#endif
        }

        private static List<int> kExpired = new List<int>();
        private void Update()
        {
            var deltaTime = UTime.deltaTime;

            if (m_Playing.Count == 0)
                return;
            
            kExpired.Clear();
            for(var i = m_Playing.Count - 1; i >= 0 ; i--)
            {
                m_Playing[i].Tick(deltaTime, m_Renderers, out var selfRecycle);
                if(selfRecycle)
                    kExpired.Add(i);
            }

            foreach (var expire in kExpired)
                Stop(m_Playing[expire]);

            // if (m_Playing.Count == 0)
            // {
                // var propertyBlock = new MaterialPropertyBlock();
                // m_Renderers.Traversal(p=>p.SetPropertyBlock(propertyBlock));
            // }
        }

        [FoldButton(nameof(m_Collection),null)]
        public void Play(string _anim)
        {
            if (m_Collection == null)
                return;
            
            var clipIndex = m_Collection.m_AnimationClips.FindIndex(x => x.name == _anim);
            if (clipIndex == -1)
            {
                Debug.LogError("No such animation: " + _anim);                
                return;
            }

            if (m_Playing.TryFind(p => p.clip.name == m_Collection.m_AnimationClips[clipIndex].name, out var element))
            {
                element.Refresh();
                return;
            }
            
            var animation = new SurfaceEffectAnimation(m_Collection.m_AnimationClips[clipIndex]);
            m_Playing.Add(animation);
            animation.Tick(0, m_Renderers, out _);
        }

        void Stop(SurfaceEffectAnimation _animation)
        {
            _animation.Dispose();
            m_Playing.Remove(_animation);
        }
        
        [FoldButton(nameof(m_Collection),null)]
        void StopAll()
        {
            foreach (var playing in m_Playing)
                playing.Dispose();
            m_Playing.Clear();
        }

        public IEnumerable<(Renderer,Material)> GetRenderers()
        {
            foreach (var animation in m_Playing)
            {
                var material = animation.material;
                if(material == null)
                    continue;
                
                foreach (var renderer in m_Renderers.Collect(p => m_Mask.HasLayer(p.gameObject.layer)))
                    yield return (renderer,material);
            }
        }
        
#if UNITY_EDITOR
        [FoldoutButton(nameof(GetRenderers), null)]
        public void NewCollection()
        {
            m_Collection = UnityEditor.Extensions.UEAsset.CreateScriptableInstanceAtCurrentRoot<SurfaceEffectCollection>("SurfaceEffectCollection");
        }
#endif
    }

    [Serializable]
    public class SurfaceEffectAnimation
    {
        public float timeElapsed;
        public EntityEffectClip clip;
        public Material material;

        public SurfaceEffectAnimation(EntityEffectClip _clip)
        {
            clip = _clip;
            timeElapsed = 0f;
            material = new Material(clip.material){hideFlags = HideFlags.HideAndDontSave};
        }

        public void Tick(float _deltaTime, Renderer[] _renderers, out bool _selfRecycle)
        {
            timeElapsed += _deltaTime;
            Apply(timeElapsed,_renderers);
            _selfRecycle = clip.warpMode == WrapMode.Once && timeElapsed > clip.length;
        }

        public void Refresh()
        {
            timeElapsed = 0f;
        }

        public void Dispose()
        {
            GameObject.DestroyImmediate(material);
        }
        
        
        private static Dictionary<string, float4> kVectors = new Dictionary<string, float4>();
        public void Apply(float _time,Renderer[] _renderers)
        {
            var warpMode = clip.warpMode;
            var length = clip.length;
            
            var block = new MaterialPropertyBlock();
            var time = _time;
            switch (warpMode)
            {
                case WrapMode.Default:
                case WrapMode.Once:
                    time = time > length ? 0 : time;
                    break;
                case WrapMode.Loop:
                    time = math.max(0,time % length);
                    break;
                case WrapMode.ClampForever:
                    time = math.clamp(time,0,length);
                    break;
                case WrapMode.PingPong:
                    time = math.abs(time % (length * 2f) - length);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            kVectors.Clear();
            foreach (var curve in clip.curves)
            {
                var value = curve.curve.Evaluate(time);

                switch (curve.paths.Length)
                {
                    case 2:
                        block.SetFloat(curve.paths[1],value);
                        break;
                    case 3:
                    {
                        var key = curve.paths[1];
                        kVectors.TryGetValue(key, out var vector);
                        switch (curve.paths[2])
                        {
                            case "x": case "r" : vector.x = value; break;
                            case "y": case "g": vector.y = value; break;
                            case "z": case "b": vector.z = value; break;
                            case "w": case "a": vector.w = value; break;
                        }
                        kVectors[key] = vector;
                        break;
                    }
                }
            }

            foreach (var (key, value) in kVectors)
            {
                material.SetVector(key,value);
                // block.SetVector(key,value);
            }
            
            // foreach (var renderer in _renderers)
                // renderer.SetPropertyBlock(block);
        }
    }
    
}