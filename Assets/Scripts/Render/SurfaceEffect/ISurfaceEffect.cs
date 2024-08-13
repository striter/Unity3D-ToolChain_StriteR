
using System;
using System.Collections.Generic;
using System.Linq.Extensions;
using Unity.Mathematics;
using UnityEngine;

namespace Rendering.Pipeline.Component
{

    public interface ISurfaceEffect
    {
        public static List<ISurfaceEffect> kBehaviours { get; private set; } = new List<ISurfaceEffect>();
        public CullingMask CullingMask { get; }
        public IEnumerable<Renderer> GetRenderers(Camera _camera);
        public List<SurfaceEffectAnimation> Playing { get; }
    }

    public static class ISurfaceEffect_Extension
    {
        #if UNITY_EDITOR
        static ISurfaceEffect_Extension()
        {
            UnityEditor.EditorApplication.playModeStateChanged += (state) =>
            {
                foreach (var effect in ISurfaceEffect.kBehaviours.ToArray())
                    OnEffectDisable(effect);
            };
        }
        #endif
        
        public static void OnEffectEnable(this ISurfaceEffect _effect)
        {
            ISurfaceEffect.kBehaviours.Add(_effect);
        }

        public static void OnEffectDisable(this ISurfaceEffect _effect)
        {
            _effect.StopAll();
            ISurfaceEffect.kBehaviours.Remove(_effect);
        }

        private static List<int> kExpired = new List<int>();
        public static void Tick(this ISurfaceEffect _effect,float _deltaTime)
        {
            if (_effect.Playing.Count == 0)
                return;
            
            kExpired.Clear();
            for(var i = _effect.Playing.Count - 1; i >= 0 ; i--)
            {
                _effect.Playing[i].Tick(_deltaTime, out var selfRecycle);
                if(selfRecycle)
                    kExpired.Add(i);
            }

            foreach (var expire in kExpired)
                _effect.Stop(_effect.Playing[expire]);

            // if (m_Playing.Count == 0)
            // {
            // var propertyBlock = new MaterialPropertyBlock();
            // m_Renderers.Traversal(p=>p.SetPropertyBlock(propertyBlock));
            // }
        }

        public static void Play(this ISurfaceEffect _effect, SurfaceEffectClip _clip)
        {
            if (_effect.Playing.TryFind(p => p.clip.name == _clip.name, out var element))
            {
                element.Refresh();
                return;
            }

            var animation = new SurfaceEffectAnimation(_clip);
            _effect.Playing.Add(animation);
            animation.Tick(0, out _);
        }
        
        public static void StopAll(this ISurfaceEffect _effect)
        {
            foreach (var playing in _effect.Playing)
                playing.Dispose();
            _effect.Playing.Clear();
        }

        public static void Stop(this ISurfaceEffect _effect, SurfaceEffectAnimation _animation)
        {
            _animation.Dispose();
            _effect.Playing.Remove(_animation);
        }
        public static IEnumerable<(Renderer,Material)> GetSurfaceEffectDrawCalls(this ISurfaceEffect _effect,Camera _camera)
        {
            if (_effect.Playing.Count == 0)
                yield break;
            
            foreach (var renderer in _effect.GetRenderers(_camera).Collect(p => p.gameObject.activeInHierarchy && _effect.CullingMask.HasLayer(p.gameObject.layer)))
            {
                foreach (var animation in _effect.Playing.Collect(p=>p.material != null))
                    yield return (renderer,animation.material);
            }
        }
    }
    [Serializable]
    public class SurfaceEffectAnimation
    {
        public float timeElapsed;
        public SurfaceEffectClip clip;
        public Material material;

        public SurfaceEffectAnimation(SurfaceEffectClip _clip)
        {
            clip = _clip;
            timeElapsed = 0f;
            material = new Material(clip.material){hideFlags = HideFlags.HideAndDontSave};
        }

        public void Tick(float _deltaTime, out bool _selfRecycle)
        {
            timeElapsed += _deltaTime;
            Apply(timeElapsed);
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
        public void Apply(float _time)
        {
            var warpMode = clip.warpMode;
            var length = clip.length;
            
            // var block = new MaterialPropertyBlock();
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
                        material.SetFloat(curve.paths[1],value);
                        // block.SetFloat(curve.paths[1],value);
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