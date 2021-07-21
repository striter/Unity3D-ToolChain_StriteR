using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIT_JoyStick : SimpleBehaviour, ITouchJoystick
{
    RectTransform rtf_Main;
    RectTransform rtf_Center;
    public float m_Radius { get; private set; }
    public Vector2 m_Origin { get; private set; }
    Timer m_SwitchTimer =new Timer(.2f);
    ValueChecker<bool> m_SwitchChecker = new ValueChecker<bool>(false);
    public UIT_JoyStick (Transform _transform):base(_transform)
    {
        rtf_Main = transform as RectTransform;
        rtf_Center = transform.Find("Center").GetComponent<RectTransform>();
        m_SwitchTimer.Stop();
        m_Origin = rtf_Main.anchoredPosition;
        m_Radius = Mathf.Abs(rtf_Main.sizeDelta.y / 2) - Mathf.Abs(rtf_Center.sizeDelta.y / 2);
        m_SwitchTimer.Replay();
        SetVisible(false);
    }
    public void SetVisible(bool _visible)
    {
        transform.SetActive(_visible);
        rtf_Center.anchoredPosition = m_Origin;
    }
    public void Tick(float _deltaTime,bool _show, Vector2 _basePos, Vector2 _delta)
    {
        rtf_Center.anchoredPosition = Vector2.Lerp(rtf_Center.anchoredPosition, _delta * m_Radius, 30f*_deltaTime);
        if (m_SwitchChecker.Check(_show))
            m_SwitchTimer.Replay();
        if (!m_SwitchTimer.m_Timing)
            return;
        m_SwitchTimer.Tick(_deltaTime);
        rtf_Main.anchoredPosition = _basePos;
        transform.localScale = Vector3.Lerp(transform.localScale, _show ? Vector3.one : Vector3.zero, m_SwitchTimer.m_TimeElapsedScale);
        transform.SetActive( _show || m_SwitchTimer.m_Timing);
    }
}
