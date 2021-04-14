using System;
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
    TouchCheckDualLRInput m_DualLRInput = new TouchCheckDualLRInput();
    TouchCheckDualStretch m_DualStretch = new TouchCheckDualStretch();
    public TouchCheckSingleInput SwitchToSingle()
    {
        SwitchTo(m_SingleInput);
        return m_SingleInput;
    }

    public TouchCheckDualLRInput SwitchToDualJoystick()
    {
        SwitchTo(m_DualLRInput);
        return m_DualLRInput;
    }

    public TouchCheckDualStretch SwitchToDualStretch()
    {
        SwitchTo(m_DualStretch);
        return m_DualStretch;
    }
    public void SwitchOff() => SwitchTo(null);

    public TouchCheckBase m_Check { get; private set; }
    void SwitchTo(TouchCheckBase check)
    {
        m_Check?.Disable();
        m_Check = check;
        m_Check?.Enable();
    }

    void Update()=>m_Check?.Tick(Time.unscaledDeltaTime);
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
    public override void Disable()
    {
        m_TouchTrackID = -1;
        OnTouchStatus(false, Vector2.zero);
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

public enum enum_Option_JoyStickMode { Retarget = 1, Stational = 2, }
public interface ILeftJoystickPositionHelper
{
    public bool Setshow(bool _visible, Vector2 _basePos);
    public void Tick(bool _show, Vector2 _basePos, Vector2 _delta);
    public float m_Radius { get; }
    public Vector2 m_Origin { get; }
}
public class TouchLRTracker
{
    public static readonly float s_halfHorizontal = Screen.width / 2;
    public static readonly float s_halfVertical = Screen.height / 2;
    public Touch m_Touch { get; private set; } = new Touch() { fingerId = -1 };
    public bool m_Enabled => m_Touch.fingerId >= 0;
    public Vector2 m_Origin { get; private set; }
    public Vector2 m_Delta { get; private set; }
    public void Set(Touch touchTrack)
    {
        m_Touch = touchTrack;
        m_Origin = m_Touch.position;
    }
    public void Clear()
    {
        m_Touch = new Touch() { fingerId = -1 };
    }
    public void Record(Touch _touchTrack)
    {
        m_Touch = _touchTrack;
        m_Delta = _touchTrack.deltaPosition;
    }
}

public class TouchCheckDualLRInput : TouchCheckBase
{
    public override enum_TouchCheckType m_Type => enum_TouchCheckType.DualJoystick;
    protected TouchLRTracker m_TrackLeft { get; private set; } = new TouchLRTracker();
    protected TouchLRTracker m_TrackRight { get; private set; } = new TouchLRTracker();
    Action<Vector2> OnLeftDelta, OnRightDelta;
    Func<bool> OnCanSendDelta;
    public void Init(Action<Vector2> _OnLeftDelta, Action<Vector2> _OnRightDelta, Func<bool> _OnTickCheck = null)
    {
        OnLeftDelta = _OnLeftDelta;
        OnRightDelta = _OnRightDelta;
        OnCanSendDelta = _OnTickCheck;
    }

    public override void Enable()
    {
    }
    public override void Disable()
    {
        OnLeftDelta?.Invoke(Vector2.zero);
        OnRightDelta?.Invoke(Vector2.zero);
    }
    public override void Tick(float deltaTime)
    {
        if (OnCanSendDelta!=null&& !OnCanSendDelta())
            return;
        int touchCount = Input.touchCount;
        for (int i = 0; i < touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            if (touch.phase == TouchPhase.Began)
            {
                bool isLeft=touch.position.x< TouchLRTracker.s_halfHorizontal;
                if (m_TrackLeft == null && isLeft)
                    m_TrackLeft.Set(touch);
                else if (m_TrackRight == null && !isLeft)
                    m_TrackRight.Set(touch);
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                if (touch.fingerId == m_TrackLeft.m_Touch.fingerId)
                    m_TrackLeft.Clear();
                if (touch.fingerId == m_TrackRight.m_Touch.fingerId)
                    m_TrackRight.Clear();
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                if (touch.fingerId == m_TrackRight.m_Touch.fingerId)
                    m_TrackRight.Record(touch);
                else if (touch.fingerId == m_TrackLeft.m_Touch.fingerId)
                    m_TrackLeft.Record(touch);
            }
        }

#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
            m_TrackLeft.Set(new Touch() { fingerId = 0, deltaPosition = Vector2.zero, position = Input.mousePosition });
        else if (Input.GetMouseButton(0))
        {
            Vector2 deltaPosition = m_TrackRight.m_Touch.position - Input.mousePosition.ToVector2();
            m_TrackLeft.Record(new Touch() { fingerId = 0, deltaPosition = deltaPosition, position = Input.mousePosition });
        }
        else if (Input.GetMouseButtonDown(0))
            m_TrackLeft.Clear();

        if (Input.GetMouseButtonDown(1))
            m_TrackRight.Set(new Touch() { fingerId = 1, deltaPosition = Vector2.zero, position = Input.mousePosition });
        else if (Input.GetMouseButton(1))
        {
            Vector2 deltaPosition = m_TrackRight.m_Touch.position - Input.mousePosition.ToVector2();
            m_TrackRight.Record(new Touch() { fingerId = 1, deltaPosition = deltaPosition, position = Input.mousePosition });
        }
        else if (Input.GetMouseButtonDown(1))
            m_TrackRight.Clear();
#endif

        OnLeftDelta(m_TrackLeft.m_Delta);
        OnRightDelta(m_TrackRight.m_Delta);
    }
}

#endregion