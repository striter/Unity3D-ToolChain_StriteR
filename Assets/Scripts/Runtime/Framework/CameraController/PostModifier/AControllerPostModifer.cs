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
        Interpolate = 1000,
        Additional = 2000,
        Override = 3000,
    }

    public interface IControllerPostModifer
    {
        public float timeExists { get; set; }
        public void OnBegin(FCameraControllerCore _input);
        public void Tick(float _deltaTime,AControllerInput _input, ref FCameraControllerOutput _output);
        public void OnFinished();
        public void DrawGizmos(AControllerInput _input);
        public EControllerPostModiferQueue Queue { get; }
        public bool Disposable(bool _reset);
        public static void Append(FCameraControllerCore _controller, IControllerPostModifer _animation, ref List<IControllerPostModifer> _animations,bool _excludeSame)
        {
            if (_excludeSame && _animations.TryFind(p => p.GetType() == _animation.GetType(),out var element))
            {
                element.OnFinished();
                _animations.Remove(element);
            }

            _animation.timeExists = 0f;
            _animation.OnBegin(_controller);
            _animations.Add(_animation);
            _animations.Sort((a,b)=>a.Queue>b.Queue?1:-1);
        }

        public static void Remove(IControllerPostModifer _animation, ref List<IControllerPostModifer> _animations)
        {
            if (_animations.Remove(_animation)) 
                _animation.OnFinished();
        }
        
        public static void Tick(float _deltaTime,AControllerInput _input,ref FCameraControllerOutput _output,ref List<IControllerPostModifer> _animations)
        {
            foreach (var animation in _animations)
            {
                animation.Tick(_deltaTime,_input,ref _output);
                animation.timeExists += _deltaTime;
            }

            for (var i = _animations.Count - 1; i >= 0; i--)
            {
                var animation = _animations[i];
                if (!animation.Disposable(false)) continue;
                
                animation.OnFinished();
                _animations.RemoveAt(i);
            }
        }

    }

    public abstract class AControllerPostModifer : ScriptableObject , IControllerPostModifer
    {
        public float timeExists { get; set; }
        public abstract bool Disposable(bool _reset);
        public abstract EControllerPostModiferQueue Queue { get; }
        public virtual void OnBegin(FCameraControllerCore _input) { }
        public abstract void Tick(float _deltaTime, AControllerInput _input,ref FCameraControllerOutput _output);
        public virtual void OnFinished() { }
        public virtual void DrawGizmos(AControllerInput _input){}
    }
}