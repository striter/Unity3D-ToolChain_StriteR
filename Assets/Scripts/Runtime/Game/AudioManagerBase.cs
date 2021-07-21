using System;
using UnityEngine;
public class AudioManagerBase : SingletonMono <AudioManagerBase>
{
    AudioSource m_AudioBG;
    AudioClip m_Clip;
    float m_baseVolume = 1f;
    public virtual float m_BGVolume => m_baseVolume;
    protected override void Awake()
    {
        base.Awake();
        m_AudioBG = GetComponent<AudioSource>();
        m_AudioBG.loop = true;
        m_AudioBG.playOnAwake = false;
        m_AudioBG.volume = m_BGVolume;
        m_baseVolume = 1f;
    }
    public virtual void Init()
    {
        GameObject obj = new GameObject("Prefab 3D");
        obj.transform.SetParent(transform);
        AudioSource source= obj.AddComponent<AudioSource>();
        source.spatialBlend = 1;
        TGameObjectPool_Static<int, SFXAudioBase>.Register(0, obj.AddComponent<SFXAudioBase>(), 20);

        obj = new GameObject("Prefab 2D");
        obj.transform.SetParent(transform);
        source = obj.AddComponent<AudioSource>();
        source.spatialBlend = 0;
        TGameObjectPool_Static<int, SFXAudioBase>.Register(1, obj.AddComponent<SFXAudioBase>(), 20);
    }
    public virtual void Destroy()=> TGameObjectPool_Static<int, SFXAudioBase>.Destroy();
    protected void SwitchBackground(AudioClip _Clip,bool loop)
    {
        if (m_Clip == _Clip)
            return;
        m_Clip = _Clip;
        m_AudioBG.loop = loop;
    }
    protected void StopBackground() => m_Clip = null;
    protected void SetSFXVolume(float volume) => TGameObjectPool_Static<int, SFXAudioBase>.TraversalAllActive((SFXAudioBase audio) => audio.SetVolume(volume));
    protected void SetBGPitch(float pitch) => m_AudioBG.pitch = pitch;
    protected virtual void Update()
    {
        float deltaTime = Time.unscaledDeltaTime;
        m_AudioBG.volume = m_BGVolume;

        if (m_Clip == null)
        {
            if (m_AudioBG.isPlaying)
            {
                m_baseVolume = Mathf.Lerp(m_baseVolume, 0f, deltaTime * 2);
                if (m_baseVolume <= .05f)
                    m_AudioBG.Stop();
            }
            return;
        }
        else
        {
            if (m_AudioBG.clip == m_Clip)
            {
                m_baseVolume = Mathf.Lerp(m_baseVolume, 1f, deltaTime * 2);
            }
            else
            {
                m_baseVolume = Mathf.Lerp(m_baseVolume, 0f, deltaTime*2);
                if (m_baseVolume <= .05f)
                {
                    m_AudioBG.clip = m_Clip;
                    m_AudioBG.Play();
                }
            }
        }
        TGameObjectPool_Static<int, SFXAudioBase>.TraversalAllActive((SFXAudioBase audio) => { audio.Tick(deltaTime); });
    }

    protected SFXAudioBase PlayClip(int sourceID,AudioClip _clip,float _volume, bool _loop, Transform _target)
    {
        if (_volume == 0)
            return null;
        return TGameObjectPool_Static<int, SFXAudioBase>.Spawn(0, null, _target.position, Quaternion.identity).Play(sourceID, _clip, _volume, _loop, _target);
    }
    protected SFXAudioBase PlayClip(int sourceID, AudioClip _clip, float _volume, bool _loop, Vector3 _position)
    {
        if (_volume == 0)
            return null;
        return TGameObjectPool_Static<int, SFXAudioBase>.Spawn(0, null, _position, Quaternion.identity).Play(sourceID, _clip,_volume, _loop, null);
    }
    protected SFXAudioBase PlayClip(int sourceID, AudioClip _clip, float _volume, bool _loop)
    {
        if (_volume == 0)
            return null;
        return TGameObjectPool_Static<int, SFXAudioBase>.Spawn(1, null, Vector3.zero, Quaternion.identity).Play(sourceID,_clip,_volume,_loop,null);
    }
}
