using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rendering.Optimize
{
    
    public class AnimationTicker
    {
        public int m_AnimIndex { get; private set; }
        public float m_TimeElapsed { get; private set; }
        public AnimationTickerClip m_Anim => m_Animations[m_AnimIndex];
        AnimationTickerClip[] m_Animations;
        public void Setup(AnimationTickerClip[] _params) { m_Animations = _params; }
        public void Reset()
        {
            m_AnimIndex = 0;
            m_TimeElapsed = 0;
        }

        public void SetTime(float _time) => m_TimeElapsed = _time;
        public void SetNormalizedTime(float _scale)
        {
            if (m_AnimIndex < 0 || m_AnimIndex >= m_Animations.Length)
                return;
            m_TimeElapsed = m_Animations[m_AnimIndex].length * _scale;
        }

        public float GetNormalizedTime()
        {
            if (m_AnimIndex < 0 || m_AnimIndex >= m_Animations.Length)
                return 0f;
            return m_TimeElapsed / m_Animations[m_AnimIndex].length;
        }
        public void SetAnimation(int _animIndex)
        {
            m_TimeElapsed = 0;
            if (_animIndex < 0 || _animIndex >= m_Animations.Length)
            {
                Debug.LogError("Invalid Animation Index Found:" + _animIndex);
                return;
            }
            m_AnimIndex = _animIndex;
        }

        public bool Tick(float _deltaTime,out AnimationTickerOutput output , Action<string> _onEvents = null)
        {
            output = default;
            if (m_AnimIndex < 0 || m_AnimIndex >= m_Animations.Length)
                return false;

            AnimationTickerClip param = m_Animations[m_AnimIndex];
            if (_onEvents != null)
                TickEvents(param, m_TimeElapsed, _deltaTime, _onEvents);
            m_TimeElapsed += _deltaTime;

            int curFrame;
            int nextFrame;
            float framePassed;
            if (param.loop)
            {
                framePassed = (m_TimeElapsed % param.length) * param.frameRate;
                curFrame = Mathf.FloorToInt(framePassed) % param.frameCount;
                nextFrame = (curFrame + 1) % param.frameCount;
            }
            else
            {
                framePassed = Mathf.Min(param.length, m_TimeElapsed) * param.frameRate;
                curFrame = Mathf.Min(Mathf.FloorToInt(framePassed), param.frameCount - 1);
                nextFrame = Mathf.Min(curFrame + 1, param.frameCount - 1);
            }

            curFrame += param.frameBegin;
            nextFrame += param.frameBegin;
            framePassed %= 1;
            output = new AnimationTickerOutput {cur=curFrame,next = nextFrame,interpolate = framePassed};
            return true;
        }

        void TickEvents(AnimationTickerClip _tickerClip, float _timeElapsed, float _deltaTime,Action<string> _onEvents)
        {
            float lastFrame = _timeElapsed * _tickerClip.frameRate;
            float nextFrame = lastFrame + _deltaTime * _tickerClip.frameRate;

            float checkOffset = _tickerClip.loop ? _tickerClip.frameCount * Mathf.Floor((nextFrame / _tickerClip.frameCount)) : 0;
            foreach (AnimationTickerEvent animEvent in _tickerClip.m_Events)
            {
                float frameCheck = checkOffset + animEvent.keyFrame;
                if (lastFrame < frameCheck && frameCheck <= nextFrame)
                    _onEvents(animEvent.identity);
            }
        }
    }
}