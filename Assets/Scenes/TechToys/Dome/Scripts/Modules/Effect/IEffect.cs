using System.Collections.Generic;
using System.Linq.Extensions;
using Unity.Mathematics;
using UnityEngine;

namespace Dome
{
    public interface IEffect
    {
        public static FDomeEffects kEffects;
        public List<int> relativeEffects { get; set; }
    }

    public static class IEffect_Extension
    {
        public static void OnInitialize(this IEffect _effect, EntityInitializeParameters _parameters)
        {
            _effect.relativeEffects = new List<int>();
        }

        public static void OnRecycle(this IEffect _effect)
        {
            _effect.relativeEffects.Traversal(IEffect.kEffects.Recycle);
            _effect.relativeEffects = null;
        }

        public static void TriggerEffectsAt(this IEffect _effect,string _effectName,float3 _position,quaternion _rotation,float _duration)
        {
            IEffect.kEffects.Spawn(_effectName, _duration,null,new TR(){position = _position,rotation = _rotation});
        }
        
        public static void TriggerEffectsAt(this IEffect _effect,string _effectName,Transform _transform,float _duration)
        {
            IEffect.kEffects.Spawn(_effectName, _duration,_transform,new TR(){position = _transform.position,rotation = _transform.rotation});
        }
        
        public static void AttachEffect(this IEffect _effect,string _effectName,Transform _attachRoot)
        {
            _effect.relativeEffects.Add(IEffect.kEffects.Spawn(_effectName,0,_attachRoot));
        }
    }
}