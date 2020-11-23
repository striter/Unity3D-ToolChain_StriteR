using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXRelativeBase : MonoBehaviour {
    protected SFXBase m_SFXSource { get; private set; }
    public virtual void Init()
    {

    }
    public virtual void Play(SFXBase _source)
    {
        m_SFXSource = _source;
    }
    public virtual void OnPlay()
    {

    }
    public virtual void OnStop()
    {
        
    }
    public virtual void OnRecycle()
    {

    }
}
