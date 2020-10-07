using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Rendering.Optimize
{
    public class AnimationInstanceData : ScriptableObject
    {
        public AnimationInstanceParam[] m_Animations;
        public AnimationInstanceExposeBone[] m_ExposeBones;
    }

    [System.Serializable]
    public class AnimationInstanceParam
    {
        public string m_Name;
        public int m_FrameBegin;
        public float m_FrameRate;
        public int m_FrameCount;
        public float m_Length;
        public bool m_Loop;
        public AnimationInstanceEvent[] m_Events;
        public AnimationInstanceParam(string _name, int _startFrame, float _frameRate, float _length, bool _loop, AnimationInstanceEvent[] _events)
        {
            m_Name = _name;
            m_FrameBegin = _startFrame;
            m_FrameRate = _frameRate;
            m_FrameCount = (int)(_frameRate * _length);
            m_Events = _events;
            m_Length = _length;
            m_Loop = _loop;
        }
    }

    [System.Serializable]
    public class AnimationInstanceExposeBone
    {
        public string m_BoneName;
        public int m_BoneIndex;
        public Vector3 m_Position;
        public Vector3 m_Direction;
    }

    [System.Serializable]
    public class AnimationInstanceEvent
    {
        public float m_EventFrame;
        public string m_EventIdentity;
        public AnimationInstanceEvent(AnimationEvent _event, float frameRate)
        {
            m_EventFrame = _event.time * frameRate;
            m_EventIdentity = _event.functionName;
        }
    }
}
