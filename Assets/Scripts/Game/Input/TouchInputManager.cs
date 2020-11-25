using System;
using System.Collections.Generic;
using UnityEngine;


public enum enum_TouchCheckType
{
    Invalid = -1,
    SingleInput = 1,
    DualStretch = 4,
    DualJoystick = 3,
}
public abstract class TouchCheckBase
{
    public abstract enum_TouchCheckType m_Type { get; }
    public abstract void Enable();
    public abstract void Disable();
    public abstract void Tick(float deltaTime);
}

public class TouchInputManager : SingletonMono<TouchInputManager>
{
    TouchCheckSingleInput m_SingleInput = new TouchCheckSingleInput();
    TouchCheckDualJoystick m_DualJoystick = new TouchCheckDualJoystick();
    TouchCheckDualStretch m_DualStretch = new TouchCheckDualStretch();
    public TouchCheckSingleInput SwitchToSingle()
    {
        SwitchCheck(m_SingleInput);
        return m_SingleInput;
    }

    public TouchCheckDualJoystick SwitchToDualJoystick()
    {
        SwitchCheck(m_DualJoystick);
        return m_DualJoystick;
    }

    public TouchCheckDualStretch SwitchToDualStretch()
    {
        SwitchCheck(m_DualStretch);
        return m_DualStretch;
    }

    public TouchCheckBase m_Check { get; private set; }
    void SwitchCheck(TouchCheckBase check)
    {
        if (m_Check!=null)
            m_Check.Disable();
        m_Check = check;
        m_Check.Enable();
    }

    void Update()
    {
        if (m_Check == null)
            return;

        m_Check.Tick(Time.unscaledDeltaTime);
    }
}
#region TouchChecks
public class TouchCheckSingleInput : TouchCheckBase
{
    public override enum_TouchCheckType m_Type => enum_TouchCheckType.SingleInput;
    Action<bool, Vector2> OnTouchStatus;
    Action<Vector2> OnTouchTick;
    int m_TouchTrackID;
    public void Init(Action<bool, Vector2> _OnTouchStatus, Action<Vector2> _OnTouchTick)
    {
        OnTouchStatus = _OnTouchStatus;
        OnTouchTick = _OnTouchTick;
    }

    public override void Enable()
    {
        m_TouchTrackID = -1;
    }
    public override void Tick(float deltaTime)
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
            OnTouchStatus(true, Input.mousePosition);
        else if (Input.GetMouseButtonUp(0))
            OnTouchStatus(false, Input.mousePosition);
        else if (Input.GetMouseButton(0))
            OnTouchTick(Input.mousePosition);
#endif
        foreach (Touch touch in Input.touches)
        {
            if (m_TouchTrackID == -1 && touch.phase == TouchPhase.Began)
            {
                m_TouchTrackID = touch.fingerId;
                OnTouchStatus(true, touch.position);
            }

            if (m_TouchTrackID != touch.fingerId)
                return;

            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                m_TouchTrackID = -1;
                OnTouchStatus(false, touch.position);
                return;
            }
            else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {
                OnTouchTick(touch.position);
            }
        }
    }
    public override void Disable()
    {
        m_TouchTrackID = -1;
        OnTouchStatus(false, Vector2.zero);
    }
}

public class TouchCheckDualStretch:TouchCheckBase
{
    Action<bool, Vector2, Vector2> OnStretchStatus;
    Action<Vector2, Vector2> OnStretchTick;
    int m_StretchTrackID1 = -1;
    int m_StretchTrackID2 = -1;
    public override enum_TouchCheckType m_Type => enum_TouchCheckType.DualStretch;
    public void Init(Action<bool, Vector2, Vector2> _OnStretchStatus,Action<Vector2,Vector2> _OnStretchTick)
    {
        OnStretchStatus = _OnStretchStatus;
        OnStretchTick = _OnStretchTick;
    }
    public override void Enable()
    {
        m_StretchTrackID1 = -1;
        m_StretchTrackID2 = -1;
    }
    public override void Disable()
    {
        if (m_StretchTrackID1 != 1 || m_StretchTrackID2 != -1)
            OnStretchStatus(false, Vector2.zero, Vector2.zero);
        m_StretchTrackID1 = -1;
        m_StretchTrackID2 = -1;
    }

    #if UNITY_EDITOR
    Vector2 m_stretchRecord;
    bool m_streching;
    #endif

    public override void Tick(float deltaTime)
    {
        #if UNITY_EDITOR
        if(!m_streching)
        {
            if (Input.GetMouseButton(0))
            {
                m_streching = true;
                m_stretchRecord = Input.mousePosition;
                OnStretchStatus(true,m_stretchRecord,Input.mousePosition);
            }
        }
        else
        {
            if(Input.GetMouseButton(0))
            {
                OnStretchTick(m_stretchRecord, Input.mousePosition);
                return;
            }
            OnStretchStatus(false, m_stretchRecord, Input.mousePosition);
            m_streching = false;
            m_stretchRecord = Vector2.zero;
        }
        #endif

        bool tracking = m_StretchTrackID1 != -1 && m_StretchTrackID2 != -1;
        if(!tracking)
        {
            if (Input.touches.Length != 2)
                return;

            m_StretchTrackID1 = Input.touches[0].fingerId;
            m_StretchTrackID2 = Input.touches[1].fingerId;
            OnStretchStatus(true, Input.touches[0].position, Input.touches[1].position);
        }

        bool terminated = false;
        Vector2 stretchPos1 = Vector2.zero;
        Vector2 stretchPos2 = Vector2.zero;
        foreach(Touch touch in Input.touches)
        {
            TickFinger(m_StretchTrackID1,touch ,ref stretchPos1,ref terminated);
            TickFinger(m_StretchTrackID2, touch, ref stretchPos2, ref terminated);
        }

        OnStretchTick(stretchPos1, stretchPos2);
        if (terminated)
        {
            OnStretchStatus(false, stretchPos1, stretchPos2);
            m_StretchTrackID1 = -1;
            m_StretchTrackID2 = -1;
        }
    }
    void TickFinger(int fingerID,Touch touch,ref Vector2 stretchPos,ref bool terminated)
    {
        if (fingerID != touch.fingerId)
            return;

        stretchPos = touch.position;
        terminated |= touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended;
    }
}
public class TouchCheckDualJoystick : TouchCheckBase
{
    public class JoystickTouchTracker
    {
        static float f_halfHorizontal = Screen.width / 2;
        static float f_halfVertical = Screen.height / 2;
        public Touch m_Touch { get; private set; }
        public Vector2 v2_startPos { get; private set; }
        public bool isLeft => v2_startPos.x < f_halfHorizontal;
        public bool isDown => v2_startPos.y < f_halfVertical;
        public bool trackSuccessful;
        public JoystickTouchTracker(Touch touchTrack)
        {
            m_Touch = touchTrack;
            v2_startPos = m_Touch.position;
            trackSuccessful = false;
        }
        public void Record(Touch touchTrack)
        {
            m_Touch = touchTrack;
        }
    }
    public override enum_TouchCheckType m_Type => enum_TouchCheckType.DualJoystick;
    JoystickTouchTracker m_TrackLeft, m_TrackRight;
    Action<Vector2> OnLeftDelta, OnRightDelta;
    Func<bool> OnCanSendDelta;
    Vector2 m_leftDelta, m_rightDelta;
    public void Init(Action<Vector2> _OnLeftDelta, Action<Vector2> _OnRightDelta, Func<bool> _OnTickCheck = null)
    {
        m_leftDelta = Vector2.zero;
        m_rightDelta = Vector2.zero;
        OnLeftDelta = _OnLeftDelta;
        OnRightDelta = _OnRightDelta;
        OnCanSendDelta = _OnTickCheck;
    }

    public override void Enable()
    {

    }
    public override void Disable()
    {
        m_TrackLeft = null;
        m_TrackRight = null;
        m_leftDelta = Vector2.zero;
        m_rightDelta = Vector2.zero;
        OnLeftDelta?.Invoke(m_leftDelta);
        OnRightDelta?.Invoke(m_rightDelta);
        UIT_JoyStick.Instance.OnDeactivate();
    }
    public override void Tick(float deltaTime)
    {
        if (UIT_JoyStick.Instance == null)
            return;

        m_rightDelta = Vector2.zero;
        int touchCount = Input.touchCount;
        for (int i = 0; i < touchCount; i++)
        {
            Touch t = Input.GetTouch(i);
            if (t.phase == TouchPhase.Began)
            {
                JoystickTouchTracker track = new JoystickTouchTracker(t);
                if (m_TrackLeft == null && track.isLeft && track.isDown)
                {
                    m_TrackLeft = track;
                    UIT_JoyStick.Instance.OnActivate(m_TrackLeft.v2_startPos);
                    m_leftDelta = Vector2.zero;
                }
                else if (m_TrackRight == null && !track.isLeft)
                {
                    m_TrackRight = track;
                }
            }
            else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
            {
                if (m_TrackLeft != null && t.fingerId == m_TrackLeft.m_Touch.fingerId)
                {
                    m_TrackLeft = null;
                    UIT_JoyStick.Instance.OnDeactivate();
                    m_leftDelta = Vector2.zero;
                }
                if (m_TrackRight != null && t.fingerId == m_TrackRight.m_Touch.fingerId)
                {
                    m_TrackRight = null;
                }
            }
            else if (t.phase == TouchPhase.Moved)
            {
                if (m_TrackRight != null && t.fingerId == m_TrackRight.m_Touch.fingerId)
                {
                    m_TrackRight.Record(t);
                    m_rightDelta = t.deltaPosition;
                }
                else if (m_TrackLeft != null && t.fingerId == m_TrackLeft.m_Touch.fingerId)
                {
                    m_TrackLeft.Record(t);
                    m_leftDelta = UIT_JoyStick.Instance.OnMoved(t.position);
                }
            }
        }

#if UNITY_EDITOR
        m_leftDelta = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        m_rightDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
#endif

        if (OnCanSendDelta != null && !OnCanSendDelta())
        {
            m_leftDelta = Vector2.zero;
            m_rightDelta = Vector2.zero;
        }

        OnLeftDelta?.Invoke(m_leftDelta);
        OnRightDelta?.Invoke(m_rightDelta);
    }
}

#endregion