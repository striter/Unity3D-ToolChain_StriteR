using System;
using System.ComponentModel;
using UnityEngine;

public class TouchInputManager : SingletonMono<TouchInputManager>
{
    public TouchCheckBase m_Check { get; private set; }
    TouchCheckSingleInput m_SingleInput = new TouchCheckSingleInput();
    TouchCheckDualLRInput m_DualLRInput = new TouchCheckDualLRInput();
    TouchCheckDualStretch m_DualStretch = new TouchCheckDualStretch();
    public TouchCheckSingleInput SwitchToSingle()
    {
        SwitchTo(m_SingleInput);
        return m_SingleInput;
    }

    public TouchCheckDualLRInput SwitchToTrackers()
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
    void SwitchTo(TouchCheckBase check)
    {
        m_Check?.Disable();
        m_Check = check;
        m_Check?.Enable();
    }
    void Update()=>m_Check?.Tick(Time.unscaledDeltaTime);
}
#region TouchChecks
public abstract class TouchCheckBase
{
    public abstract void Enable();
    public abstract void Disable();
    public abstract void Tick(float deltaTime);
}
public class TouchCheckSingleInput : TouchCheckBase
{
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
public class TouchTracker
{
    public static implicit operator bool(TouchTracker tracker)=>tracker!=null;
    public Touch m_Touch { get; private set; } = new Touch() { fingerId = -1 };
    public Vector2 m_TouchOrigin { get; private set; }
    public Vector2 m_Delta { get; private set; }
    Action<Vector2> OnTrackerDelta;
    Func<Vector2, bool> OnTrackerSet;
    public TouchTracker(Action<Vector2> _OnTrackerTick,Func<Vector2,bool> _OnTrackerSet=null)
    {
        OnTrackerDelta = _OnTrackerTick;
        OnTrackerSet = _OnTrackerSet;
    }
    public void Begin(Touch _touch)
    {
        if (OnTrackerSet==null||!OnTrackerSet(_touch.position))
            return;

        m_Touch = _touch;
        m_TouchOrigin = m_Touch.position;
        OnSet();
    }
    public void End(Touch _touch)
    {
        if (_touch.fingerId != m_Touch.fingerId)
            return;

        m_Touch = new Touch() { fingerId = -1 };
        m_Delta = Vector2.zero;
        OnTrackerDelta(m_Delta);
        OnClear();
    }
    public void Record(Touch _touch)
    {
        if (_touch.fingerId != m_Touch.fingerId)
            return;

        m_Touch = _touch;
        m_Delta = OnRecord(_touch);
        OnTrackerDelta(m_Delta);
    }

    public virtual void Tick(float _deltaTime) { }
    public virtual void OnEnable() { }
    public virtual void OnDisable() { End(m_Touch); }
    protected virtual void OnClear() { }
    protected virtual void OnSet() { }
    protected virtual Vector2 OnRecord(Touch _touch) => _touch.deltaPosition;

    static readonly float s_HalfWidth = Screen.width / 2;
    static readonly float s_HalfHeight = Screen.height / 2;
    public static readonly Func<Vector2, bool> s_LeftTrack = (vector) => vector.x < s_HalfWidth;
    public static readonly Func<Vector2, bool> s_RightTrack = (vector) => vector.x >= s_HalfWidth;
    public static readonly Func<Vector2, bool> s_LeftBottomTrack = (vector) => vector.x < s_HalfWidth && vector.y < s_HalfHeight;
}
public class TouchCheckDualLRInput : TouchCheckBase
{
    protected TouchTracker[] m_Trackers;
    public  TouchCheckDualLRInput Init(params TouchTracker[] _trackers)
    {
        m_Trackers = _trackers;
        return this;
    }
    public override void Enable() => m_Trackers?.Traversal(tracker => tracker.OnEnable());
    public override void Disable() => m_Trackers?.Traversal(tracker => tracker.OnDisable());
    public override void Tick(float _deltaTime)
    {
        if (m_Trackers == null)
            return;

        int touchCount = Input.touchCount;
        for (int i = 0; i < touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            switch(touch.phase)
            {
                case TouchPhase.Began:
                    m_Trackers.Traversal(tracker => tracker.Begin(touch));
                    break;
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    m_Trackers.Traversal(tracker => tracker.Record(touch));
                    break;
                case TouchPhase.Canceled:
                case TouchPhase.Ended:
                    m_Trackers.Traversal(tracker => tracker.End(touch));
                    break;
            }
        }

#if UNITY_EDITOR
        for (int i = 0; i < m_Trackers.Length; i++)
        {
            if (Input.GetMouseButtonDown(i))
                m_Trackers[i].Begin(new Touch() { fingerId = i, deltaPosition = Vector2.zero, position = Input.mousePosition });
            else if (Input.GetMouseButton(i))
                m_Trackers[i].Record(new Touch() { fingerId = i, deltaPosition = Input.mousePosition.ToVector2() - m_Trackers[i].m_Touch.position, position = Input.mousePosition });
            else if (Input.GetMouseButtonUp(i))
                m_Trackers[i].End(new Touch() { fingerId = i });
        }
#endif
        m_Trackers.Traversal(tracker => tracker.Tick(_deltaTime));
    }
}
public interface ITouchJoystick
{
    public void Tick(float _deltaTime, bool _show, Vector2 _basePos, Vector2 _delta);
    public void SetVisible(bool _visible);
    public float m_Radius { get; }
    public Vector2 m_Origin { get; }
}
public enum enum_Option_JoyStickMode { Retarget = 1, Stational = 2, }
public class TouchTracker_Joystick : TouchTracker
{
    public enum_Option_JoyStickMode m_Mode { get; private set; }
    ITouchJoystick m_Joystick;
    public bool m_JoystickShow { get; private set; }
    public TouchTracker_Joystick(ITouchJoystick _joystick, enum_Option_JoyStickMode _mode, Action<Vector2> _OnTrackerTick, Func<Vector2, bool> _OnTrackerSet = null) : base(_OnTrackerTick, _OnTrackerSet)
    {
        m_Joystick = _joystick;
        SwitchJoystickMode(_mode);
    }
    public void SwitchJoystickMode(enum_Option_JoyStickMode _mode)
    {
        m_Mode = _mode;
        OnClear();
    }
    public override void OnDisable()
    {
        base.OnDisable();
        m_Joystick.SetVisible(false);
        m_JoystickShow = true;
    }
    protected override void OnSet()
    {
        base.OnSet();
        switch (m_Mode)
        {
            case enum_Option_JoyStickMode.Stational: m_JoystickShow = true; break;
            case enum_Option_JoyStickMode.Retarget: m_JoystickShow = true; break;
        }
    }
    protected override void OnClear()
    {
        base.OnClear();
        switch (m_Mode)
        {
            case enum_Option_JoyStickMode.Stational: m_JoystickShow = true; break;
            case enum_Option_JoyStickMode.Retarget: m_JoystickShow = false; break;
        }
    }
    protected override Vector2 OnRecord(Touch _touch)
    {
        Vector2 delta = Vector2.zero;
        switch (m_Mode)
        {
            case enum_Option_JoyStickMode.Retarget: delta = (_touch.position - m_TouchOrigin) / m_Joystick.m_Radius; break;
            case enum_Option_JoyStickMode.Stational: delta = (_touch.position - m_Joystick.m_Origin) / m_Joystick.m_Radius; break;
        }
        return delta.normalized;
    }
    public override void Tick(float _deltaTime)
    {
        base.Tick(_deltaTime);
        switch (m_Mode)
        {
            case enum_Option_JoyStickMode.Stational: m_Joystick.Tick(_deltaTime, m_JoystickShow, m_Joystick.m_Origin, m_Delta); break;
            case enum_Option_JoyStickMode.Retarget: m_Joystick.Tick(_deltaTime, m_JoystickShow, m_TouchOrigin, m_Delta); break;
        }
    }
}
#endregion