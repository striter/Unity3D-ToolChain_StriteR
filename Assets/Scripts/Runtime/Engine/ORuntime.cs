using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class TimeScaleController<T> where T : struct
{
    static Dictionary<T, float> m_TimeScales = new Dictionary<T, float>();
    public static void Clear() => m_TimeScales.Clear();

    static float GetLowestScale()
    {
        float scale = 1f;
        scale= m_TimeScales.Values.MinElement(p=>p);
        return scale;
    }

    public static float GetScale(T index) => m_TimeScales.ContainsKey(index) ? m_TimeScales[index] : 1f;
    public static void SetScale(T scaleIndex, float scale)
    {
        if (!m_TimeScales.ContainsKey(scaleIndex))
            m_TimeScales.Add(scaleIndex, 1f);
        m_TimeScales[scaleIndex] = scale;
    }
    static ValueChecker<float> m_BulletTime = new ValueChecker<float>(1f);

    public static void Tick()
    {
        if (m_BulletTime.Check(GetLowestScale()))
            Time.timeScale = m_BulletTime;
    }
}

public class AnimationSingleControl
{
    public Animation m_Animation { get; private set; }
    public AnimationSingleControl(Animation _animation, bool startFromOn = true)
    {
        m_Animation = _animation;
        m_Animation.playAutomatically = false;
        SetPlayPosition(startFromOn);
    }
    public void SetPlayPosition(bool forward) => m_Animation.clip.SampleAnimation(m_Animation.gameObject, forward ? 0 : m_Animation.clip.length);
    public void Play(bool forward)
    {
        m_Animation[m_Animation.clip.name].speed = forward ? 1 : -1;
        m_Animation[m_Animation.clip.name].normalizedTime = forward ? 0 : 1;
        m_Animation.Play(m_Animation.clip.name);
    }
    public void Stop()
    {
        m_Animation.Stop();
    }
}

public class AnimationFrameControl<T> where T : Enum
{
    struct BoneTransformRecord
    {
        public Transform m_Transform { get; private set; }
        public Vector3 m_LocalPos { get; private set; }
        public Quaternion m_LocalRot { get; private set; }
        public Vector3 m_LocalScale { get; private set; }
        public BoneTransformRecord(Transform _transform)
        {
            m_Transform = _transform;
            m_LocalPos = _transform.localPosition;
            m_LocalRot = _transform.localRotation;
            m_LocalScale = _transform.localScale;
        }
        public void Reset()
        {
            m_Transform.localPosition = m_LocalPos;
            m_Transform.localRotation = m_LocalRot;
            m_Transform.localScale = m_LocalScale;
        }
    }
    public AnimationClip[] m_Animations { get; private set; }
    BoneTransformRecord[] m_BoneRecords;
    public GameObject gameObject { get; private set; }
    public float m_TimeElapsed { get; private set; }
    public float m_AnimSpeed { get; private set; }
    public int m_CurPlaying { get; private set; } = -1;
    public AnimationFrameControl(GameObject _gameObject, AnimationClip[] _animations)
    {
        gameObject = _gameObject;
        m_Animations = _animations;
        m_CurPlaying = -1;
        m_BoneRecords = _gameObject.GetComponentsInChildren<Transform>(false).Select(p=>new BoneTransformRecord(p)).ToArray();
    }

    public void ResetAnimation()
    {
        m_CurPlaying = -1;
        m_TimeElapsed = 0f;
        foreach (BoneTransformRecord boneRecord in m_BoneRecords)
            boneRecord.Reset();
    }
    bool CheckIndex(int index)
    {
        if (index < 0 || index >= m_Animations.Length)
            return false;

        if (m_CurPlaying != index)
        {
            ResetAnimation();
            m_CurPlaying = index;
            m_TimeElapsed = 0f;
        }
        return true;
    }

    public void TickLoop(int index, float _deltaTime)
    {
        if (!CheckIndex(index))
            return;

        AnimationClip curClip = m_Animations[m_CurPlaying];
        m_TimeElapsed += _deltaTime;
        curClip.SampleAnimation(gameObject, m_TimeElapsed % curClip.length);
    }
    public void TickScale(int index, float _scale)
    {
        if (!CheckIndex(index))
            return;

        AnimationClip curClip = m_Animations[m_CurPlaying];
        curClip.SampleAnimation(gameObject, curClip.length * _scale);
    }
}

public class ParticleControlBase
{
    public Transform transform { get; private set; }
    public ParticleSystem[] m_Particles { get; private set; }
    public ParticleControlBase(Transform _transform)
    {
        transform = _transform;
        m_Particles = transform ? transform.GetComponentsInChildren<ParticleSystem>() : new ParticleSystem[0];
    }
    public void Play()
    {
        foreach (ParticleSystem particle in m_Particles)
        {
            particle.Simulate(0, true, true);
            particle.Play(true);
            ParticleSystem.MainModule main = particle.main;
            main.playOnAwake = true;
        }
    }
    public void Stop()
    {
        foreach (ParticleSystem particle in m_Particles)
        {
            particle.Stop(true);
            ParticleSystem.MainModule main = particle.main;
            main.playOnAwake = false;
        }
    }
    public void Clear()
    {
        foreach (ParticleSystem particle in m_Particles)
            particle.Clear();
    }
    public void SetActive(bool active)
    {
        foreach (ParticleSystem particle in m_Particles)
            particle.transform.SetActive(active);
    }
}