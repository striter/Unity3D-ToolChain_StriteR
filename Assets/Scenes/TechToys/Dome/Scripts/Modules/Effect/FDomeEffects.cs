using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using UnityEngine;

namespace Dome
{
    public struct FDomeEffectHandle
    {
        public string name;
        public GameObject effect;
        public Transform attaching;
        public float timeToRecycle;
        public static int kInvalidId = -1;
    }
    
    public class FDomeEffects : ADomeController
    {
        public static string kRoot = "Effects";
        public Dictionary<int, FDomeEffectHandle> m_Effects = new Dictionary<int, FDomeEffectHandle>();
        private List<int> m_EffectToRecycle = new List<int>();
        private static int kTotalEffectsSpawned = 0;
        public override void OnInitialized()
        {
            m_Effects.Clear();
            m_EffectToRecycle.Clear();
            IEffect.kEffects = this;
            FAssets.PrecachePrefabsAtPath("Effects");
        }

        public override void Tick(float _deltaTime)
        {
            var curTime = UTime.time;
            m_EffectToRecycle.Clear();
            foreach (var pair in m_Effects)
            {
                var handle = pair.Key;
                var output = pair.Value;

                if (output.attaching!=null)
                    output.effect.transform.SetPositionAndRotation(output.attaching.position,output.attaching.rotation);
                
                if(output.timeToRecycle > 0 && curTime>output.timeToRecycle)
                    m_EffectToRecycle.Add(handle);
            }
            m_EffectToRecycle.Traversal(Recycle);
        }

        public int Spawn(string _effectName,float _duration = 0,Transform _attachRoot = null,TR? _startPR = null)
        {
            var effectName = $"{kRoot}/{_effectName}";
            var spawnedEffect = FAssets.GetModel(effectName);
            if (!spawnedEffect) return FDomeEffectHandle.kInvalidId;
            
            int index = kTotalEffectsSpawned++;
            var element = new FDomeEffectHandle() {
                name = effectName,
                effect = spawnedEffect,
                attaching = _attachRoot,
                timeToRecycle = _duration>0?UTime.time+_duration:0,
            };
            
            
            if(_startPR!=null) _startPR.Value.SyncTransformWS( element.effect.transform);
            else if(_attachRoot) element.effect.transform.SyncPositionRotation(_attachRoot);
            
            m_Effects.Add(index,element);
            return index;
        }
        
        public void Recycle(int _effect)
        {
            if (_effect == FDomeEffectHandle.kInvalidId) return;
            
            var removeElement = m_Effects[_effect];
            FAssets.ClearModel(removeElement.name,removeElement.effect);
            m_Effects.Remove(_effect);
        }

        public override void Dispose()
        {
            m_Effects.Keys.ToList().Traversal(Recycle);
        }
    }
}