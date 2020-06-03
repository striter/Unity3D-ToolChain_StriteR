using System;
using System.Collections.Generic;
using UnityEngine;

public class TouchInputManager : SingletonMono<TouchInputManager>
{
    public TouchCheckBase m_Check { get; private set; }
    public void Init(TouchCheckBase check)
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
public enum enum_TouchCheckType
{
    Invalid = -1,
    TouchLR = 1,
    TouchDrag = 2,
    TouchDown = 3,
}
public abstract class TouchCheckBase
{
    public abstract enum_TouchCheckType m_Type { get; }
    public abstract void Enable();
    public abstract void Tick(float deltaTime);
    public abstract void Disable();
}
public class TouchCheckLRJoystick : TouchCheckBase
{
    public override enum_TouchCheckType m_Type => enum_TouchCheckType.TouchLR;
    TouchTracker m_TrackLeft, m_TrackRight;
    Action<Vector2> OnLeftDelta, OnRightDelta;
    Func<bool> OnCanSendDelta;
    Vector2 m_leftDelta, m_rightDelta;
    public TouchCheckLRJoystick(Action<Vector2> _OnLeftDelta, Action<Vector2> _OnRightDelta, Func<bool> _OnCanSendDelta)
    {
        m_leftDelta = Vector2.zero;
        m_rightDelta = Vector2.zero;
        OnLeftDelta = _OnLeftDelta;
        OnRightDelta = _OnRightDelta;
        OnCanSendDelta = _OnCanSendDelta;
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
                TouchTracker track = new TouchTracker(t);
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
public class TouchCheckScreenDrag : TouchCheckBase
{
    public override enum_TouchCheckType m_Type => enum_TouchCheckType.TouchDrag;
    Action<bool, Vector2> OnDragDown;
    Action<Vector2> OnDrag;
    int m_DragTrackID;
    public TouchCheckScreenDrag(Action<bool, Vector2> _OnDragDown, Action<Vector2> _OnDrag)
    {
        OnDragDown = _OnDragDown;
        OnDrag = _OnDrag;
    }

    public override void Enable()
    {
        m_DragTrackID = -1;
    }
    public override void Tick(float deltaTime)
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
            OnDragDown(true, Input.mousePosition);
        else if (Input.GetMouseButtonUp(0))
            OnDragDown(false, Input.mousePosition);
        else if (Input.GetMouseButton(0))
            OnDrag(Input.mousePosition);
#endif
        foreach (Touch touch in Input.touches)
        {
            if (m_DragTrackID == -1 && touch.phase == TouchPhase.Began)
            {
                m_DragTrackID = touch.fingerId;
                OnDragDown(true, touch.position);
            }

            if (m_DragTrackID != touch.fingerId)
                return;

            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                m_DragTrackID = -1;
                OnDragDown(false, touch.position);
                return;
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                OnDrag(touch.position);
            }
        }
    }
    public override void Disable()
    {
        m_DragTrackID = -1;
        OnDragDown(false, Vector2.zero);
    }
}
public class TouchCheckDown : TouchCheckBase
{
    public override enum_TouchCheckType m_Type => enum_TouchCheckType.TouchDown;
    Action<bool, Vector2> OnDown;
    public TouchCheckDown(Action<bool, Vector2> _OnDown)
    {
        OnDown = _OnDown;
    }

    public override void Tick(float deltaTime)
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
            OnDown(true, Input.mousePosition);
        else if (Input.GetMouseButtonUp(0))
            OnDown(false, Input.mousePosition);
#endif

        foreach (Touch touch in Input.touches)
        {
            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                OnDown(false, touch.position);
            }
            else if (touch.phase == TouchPhase.Began)
            {
                OnDown(true, touch.position);
            }
        }
    }

    public override void Disable()
    {
    }

    public override void Enable()
    {
    }
}

public class TouchTracker
{
    static float f_halfHorizontal = Screen.width / 2;
    static float f_halfVertical = Screen.height / 2;
    const float f_minOffset = 50;
    public Touch m_Touch { get; private set; }
    public Vector2 v2_startPos { get; private set; }
    public bool isLeft => v2_startPos.x < f_halfHorizontal;
    public bool isDown => v2_startPos.y < f_halfVertical;
    public bool trackSuccessful;
    public TouchTracker(Touch touchTrack)
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