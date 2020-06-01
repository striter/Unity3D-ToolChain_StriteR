using System;
using UnityEngine;
public class SFXBase :CObjectPoolStaticPrefabBase<int> {
    public const int I_SFXStopExternalDuration= 4;
    public int m_SourceID { get; private set; } = -1;
    protected float f_delayDuration { get; private set; }
    protected float f_playDuration { get; private set; }
    protected float f_lifeDuration { get; private set; }

    protected float f_lifeTimeCheck { get; private set; }
    protected bool B_Delaying { get; private set; }
    public bool B_Activating { get; private set; }
    public bool B_Playing { get; private set; }
    public bool m_TickLifeTime { get; private set; }
    protected virtual bool m_ScaledDeltaTime => true;
    public float f_playTimeLeft => f_lifeTimeCheck - I_SFXStopExternalDuration;
    public float f_delayTimeLeft { get; private set; }
    public float f_delayLeftScale => f_delayTimeLeft>0? (f_delayTimeLeft / f_delayDuration):0;
    Transform m_AttachTo;
    Vector3 m_localPos, m_localDir;
    protected SFXRelativeBase[] m_relativeSFXs;
    public override void OnPoolInit(int _identity, Action<int, MonoBehaviour> _OnSelfRecycle)
    {
        base.OnPoolInit(_identity, _OnSelfRecycle);
        m_relativeSFXs = GetComponentsInChildren<SFXRelativeBase>();
        m_relativeSFXs.Traversal((SFXRelativeBase relative) => { relative.Init(); });
    }
    protected void PlaySFX(int sourceID,float playDuration,float delayDuration,bool lifeTimeTick)
    {
        B_Activating = true;
        B_Delaying = true;
        B_Playing = false;
        m_TickLifeTime = lifeTimeTick;
        m_SourceID = sourceID;
        f_playDuration = playDuration;
        f_delayDuration = delayDuration;
        f_delayTimeLeft = f_delayDuration;
        SetLifeTime(f_playDuration + f_delayDuration);
        m_relativeSFXs.Traversal((SFXRelativeBase relative) => { relative.Play(this); });
    }

    protected void SetLifeTime(float lifeDuration)
    {
        f_lifeDuration = lifeDuration + I_SFXStopExternalDuration;
        f_lifeTimeCheck = f_lifeDuration;
    }

    protected virtual void OnPlay()
    {
        B_Delaying = false;
        B_Playing = true;
        m_relativeSFXs.Traversal((SFXRelativeBase relative) => { relative.OnPlay(); });
    }

    protected virtual void OnStop()
    {
        f_lifeTimeCheck = I_SFXStopExternalDuration;
        m_TickLifeTime = true;
        B_Delaying = false;
        B_Playing = false;
        m_relativeSFXs.Traversal((SFXRelativeBase sfxRelative) => { sfxRelative.OnStop(); });
    }

    protected virtual void OnRecycle()
    {
        B_Activating = false;
        m_AttachTo = null;
        m_relativeSFXs.Traversal((SFXRelativeBase relative) => { relative.OnRecycle(); });
        DoRecycle();
    }

    public void AttachTo(Transform _attachTo)
    {
        m_AttachTo = _attachTo;
        if (!_attachTo)
            return;
        m_localPos = _attachTo.InverseTransformPoint(transform.position);
        m_localDir = _attachTo.InverseTransformDirection(transform.forward);
    }

    protected virtual void Update()
    {
        if (!B_Activating)
            return;

        if (m_AttachTo)
        {
            transform.position = m_AttachTo.TransformPoint(m_localPos);
            transform.rotation = Quaternion.LookRotation(m_AttachTo.TransformDirection(m_localDir));
        }

        float deltaTime = m_ScaledDeltaTime ? Time.deltaTime : Time.unscaledDeltaTime;
        if (B_Delaying && f_delayTimeLeft >= 0)
        {
            f_delayTimeLeft -= deltaTime;
            if (f_delayTimeLeft < 0)
                OnPlay();
        }

        if (f_lifeTimeCheck < 0)
            return;
        f_lifeTimeCheck -= deltaTime;

        if (!m_TickLifeTime)
            return;
        if (B_Playing && f_playTimeLeft < 0)
            OnStop();
        if (!B_Playing && f_lifeTimeCheck < 0)
            OnRecycle();
    }


    public void Stop()
    {
        OnStop();
    }
    public void Recycle()
    {
        OnRecycle();
    }

#if UNITY_EDITOR
    protected Color EDITOR_GizmosColor()
    {
        Color color = Color.red;
        if (B_Playing)
            color = Color.green;
        if (B_Delaying)
            color = Color.yellow;
        if (!UnityEditor.EditorApplication.isPlaying)
            color = Color.white;
        return color;
    }
#endif
}
