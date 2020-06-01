using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SFXAudioBase : SFXBase
{
    protected override bool m_ScaledDeltaTime => false;
    AudioSource m_Audio;
    public override void OnPoolInit(int _identity, Action<int, MonoBehaviour> _OnRecycle)
    {
        base.OnPoolInit(_identity, _OnRecycle);
        m_Audio = GetComponent<AudioSource>();
        m_Audio.playOnAwake = false;
    }

    public SFXAudioBase Play(int _sourceID,AudioClip _clip,float _volume,bool _loop,Transform _attachTo)
    {
        m_Audio.clip = _clip;
        m_Audio.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
        m_Audio.loop = _loop;
        SetVolume(_volume);
        AttachTo(_attachTo);
        base.PlaySFX(_sourceID,_loop?0:_clip.length,0,true);
        return this;
    }
    public void SetVolume(float volume) => m_Audio.volume = volume;
    public void SetPitch(float _pitch)=> m_Audio.pitch = _pitch;
    protected override void OnPlay()
    {
        base.OnPlay();
        m_Audio.Play();
    }
    protected override void OnStop()
    {
        base.OnStop();
        m_Audio.Stop();
    }
    protected override void OnRecycle()
    {
        AttachTo(null);
        DoRecycle();
    }
}
