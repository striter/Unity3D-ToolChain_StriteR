using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum ETouchConsoleButton
{
    Main,
    Alt,
    Special1,
    Special2,
}


public partial class TouchConsole
{
    Transform m_FrameRate;
    Text m_FrameRateValue;
    readonly Queue<int> m_FrameRateQueue = new Queue<int>();

    public static RangeFloat kJoystickRange = new RangeFloat(0f, .5f);
    public static RangeFloat kScreenDeltaRange = new RangeFloat(.5f, .5f);
    public const float kJoystickRadius=80f;
    
    private RectTransform m_Joystick;
    private RectTransform m_JoystickCore;
    private readonly Counter m_AnimationCounter=new Counter(.2f);
    private bool m_JoystickActive = false;
    private Dictionary<ETouchConsoleButton,UIEventTriggerListenerExtension> m_ButtonListeners = new Dictionary<ETouchConsoleButton, UIEventTriggerListenerExtension>();
    
    [PartialMethod(EPartialMethods.Init,EPartialSorting.Misc)]
    internal void InitMisc()
    {
        var miscTransform = transform.Find("Misc");
        m_FrameRate = miscTransform.Find("FrameRate");
        m_FrameRateValue = m_FrameRate.Find("Value/Value").GetComponent<Text>();

        m_Joystick = miscTransform.Find("Joystick") as RectTransform;
        m_JoystickCore = m_Joystick.Find("Core") as RectTransform;
        foreach (var buttonEnum in UEnum.GetEnums<ETouchConsoleButton>())
        {
            var buttonTransform = miscTransform.Find(buttonEnum.ToString());
            m_ButtonListeners.Add(buttonEnum,buttonTransform.GetComponent<UIEventTriggerListenerExtension>());
        }
    }
    
    [PartialMethod(EPartialMethods.Reset,EPartialSorting.Misc)]
    internal void ResetMisc()
    {
        m_FrameRateQueue.Clear();
        m_FrameRateValue.text = "";
        ClearButtons();
        SetJoystick(Vector2.zero, false);
    }
    
    [PartialMethod(EPartialMethods.Tick, EPartialSorting.Misc)]
    internal void TickMisc(float _deltaTime)
    {
        if (!m_Data.m_FilterSetting.value.IsFlagEnable(EConsoleSetting.FPS))
            return;
        m_FrameRateQueue.Enqueue(Mathf.CeilToInt(1f / _deltaTime));
        if (m_FrameRateQueue.Count > 30)
            m_FrameRateQueue.Dequeue();

        int total = 0;
        foreach (var frameRate in m_FrameRateQueue)
            total += frameRate;
        total /= m_FrameRateQueue.Count;

        m_FrameRateValue.text = total.ToString();

        if (m_AnimationCounter.Playing)
        {
            m_AnimationCounter.Tick(_deltaTime);

            float destScale =  (m_JoystickActive ? 1f : 0f);
            m_Joystick.transform.localScale = Mathf.Lerp(1f-destScale,destScale,umath.sqr(m_AnimationCounter.TimeElapsedScale)) * Vector3.one;
            if (!m_JoystickActive && !m_AnimationCounter.Playing)
                m_Joystick.SetActive(false);
        }
    }
    
    [PartialMethod(EPartialMethods.SwitchVisible, EPartialSorting.CommandConsole)]
    internal void SwitchMisc(bool _visible)
    {
        m_FrameRate.transform.SetActive(_visible);
        m_Joystick.transform.SetActive(_visible);
    }
    
    
    public static void DoSetJoystick(Vector2 _position, bool _active) => Instance.SetJoystick(_position,_active);
    public static void DoTrackJoystick(Vector2 _normalizedPosition) => Instance.TrackJoystick(_normalizedPosition);
    private void SetJoystick(Vector2 _position, bool _active)
    {
        m_Joystick.anchoredPosition = _position;
        m_JoystickActive = _active;
        m_Joystick.SetActive(true);
        m_AnimationCounter.Replay();
    }

    private void TrackJoystick(Vector2 _normalizedPosition)
    {
        m_JoystickCore.anchoredPosition = kJoystickRadius*_normalizedPosition;
    }

    public static UIEventTriggerListenerExtension InitButton(ETouchConsoleButton _button)
    {
        m_Instance.m_ButtonListeners[_button].SetActive(true);
        return m_Instance.m_ButtonListeners[_button];
    }
    
    public static void ClearButtons()
    {
        foreach (var button in m_Instance.m_ButtonListeners.Values)
        {
            button.Clear();
            button.SetActive(false);
        }
    }
}
