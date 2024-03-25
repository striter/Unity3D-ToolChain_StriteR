using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using CameraController.Inputs;
using UnityEngine;

namespace CameraController.Animation
{
    public enum EControllerPostModiferQueue
    {
        Interpolate,
        Additional,
        Override,
    }
    
    public interface IControllerPostModifer
    {
        public float timeExists { get; set; }
        public void OnBegin(AControllerInput _input);
        public void Tick(float _deltaTime, ref FCameraOutput _output);
        public void OnFinished();
        public bool Disposable { get; }
        public EControllerPostModiferQueue Queue { get; }
        public static void Append(AControllerInput _input, IControllerPostModifer _animation, ref List<IControllerPostModifer> _animations,bool _excludeSame)
        {
            if (_excludeSame && _animations.TryFind(p => p.GetType() == _animation.GetType(),out var element))
            {
                element.OnFinished();
                _animations.Remove(element);
            }

            _animation.timeExists = 0f;
            _animation.OnBegin(_input);
            _animations.Add(_animation);
            _animations.Sort((a,b)=>a.Queue>b.Queue?1:-1);
        }
        
        public static void Tick(float _deltaTime,ref FCameraOutput _output,ref List<IControllerPostModifer> _animations)
        {
            foreach (var animation in _animations)
            {
                animation.Tick(_deltaTime,ref _output);
                animation.timeExists += _deltaTime;
            }

            for (var i = _animations.Count - 1; i >= 0; i--)
            {
                var animation = _animations[i];
                if (!animation.Disposable) continue;
                
                animation.OnFinished();
                _animations.RemoveAt(i);
            }
        }

    }

    public abstract class AControllerPostModifer : ScriptableObject , IControllerPostModifer
    {
        public float timeExists { get; set; }
        public abstract bool Disposable { get;}
        public abstract EControllerPostModiferQueue Queue { get;  }
        public virtual void OnBegin(AControllerInput _input) { }
        public abstract void Tick(float _deltaTime, ref FCameraOutput _output);
        public virtual void OnFinished() { }
    }
}