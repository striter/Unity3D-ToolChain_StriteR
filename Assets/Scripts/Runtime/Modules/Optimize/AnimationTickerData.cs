using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rendering.Optimize
{
    [Serializable]
    public struct AnimationTickerClip
    {
        public string name;
        public int frameBegin;
        public int frameCount;
        public bool loop;
        
        public float length;
        public float frameRate;
        public AnimationTickerEvent[] m_Events;
        public AnimationTickerClip(string _name, int _startFrame, float _frameRate, float _length, bool _loop, AnimationTickerEvent[] _events)
        {
            name = _name;
            frameBegin = _startFrame;
            frameRate = _frameRate;
            frameCount = (int)(_frameRate * _length);
            m_Events = _events;
            length = _length;
            loop = _loop;
        }
    }
    
    [Serializable]
    public struct AnimationTickerEvent
    {
        public float keyFrame;
        public string identity;
        public AnimationTickerEvent(UnityEngine.AnimationEvent _event, float frameRate)
        {
            keyFrame = _event.time * frameRate;
            identity = _event.functionName;
        }
    }

    public struct AnimationTickerOutput
    {
        public int cur;
        public int next;
        public float interpolate;
    }
}