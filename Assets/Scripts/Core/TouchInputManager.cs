using System;
using System.Collections.Generic;
using UnityEngine;

public enum enum_TouchCheckType
{
    Invalid=-1,
    TouchLR=1,
    TouchDrag=2,
    TouchClick=3,
}
public class TouchInputManager : SingletonMono<TouchInputManager>
{
    public enum_TouchCheckType m_CheckType => m_Check.Count==0 ? enum_TouchCheckType.Invalid : m_Check.Peek().m_Type;
    Stack<TouchCheckBase> m_Check=new Stack<TouchCheckBase>();

    public void AddLRBinding(Action<Vector2> _OnLeftDelta, Action<Vector2> _OnRightDelta, Func<bool> _OnCanSendDelta)=>Push(new TouchCheckLRJoystick(_OnLeftDelta,_OnRightDelta,_OnCanSendDelta));
    public void AddDragBinding(Action<bool,Vector2> _OnDragDown, Action<Vector2> _OnDrag) => Push(new TouchCheckScreenDrag(_OnDragDown,_OnDrag));
    public void AddPressBinding(Action<bool, Vector2> _OnDown) => Push(new TouchCheckScreenCheck(_OnDown));

    public void RemoveExtraBinding()
    {
        if (m_Check.Count > 1)
            Pop();
    }
    public void RemoveAllBinding()
    {
        for(int i=0;i<m_Check.Count;i++)
            m_Check.Pop().Disable();
    }
    
    void Push(TouchCheckBase check)
    {
        if (m_Check.Count > 0)
            m_Check.Peek().Disable();
        m_Check.Push(check);
        m_Check.Peek().Enable();
    }
    void Pop()
    {
        if (m_Check.Count > 0)
            m_Check.Pop().Disable();

        if (m_Check.Count > 0)
            m_Check.Peek().Enable();
    }

    private void Update()
    {
        if (m_Check.Count <= 0)
            return;

        m_Check.Peek().Tick(Time.unscaledDeltaTime);
    }

    abstract class TouchCheckBase
    {
        public abstract enum_TouchCheckType m_Type { get; }
        public abstract void Enable();
        public abstract void Tick(float deltaTime);
        public abstract void Disable();
    }
    class TouchCheckLRJoystick : TouchCheckBase
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
            if (UIT_JoyStick.Instance == null || OnCanSendDelta == null)
                return;

            m_rightDelta = Vector2.zero;
            int touchCount = Input.touchCount;
            for (int i = 0; i < touchCount;i++)
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
            m_leftDelta = PCInputManager.Instance.m_MovementDelta;
            m_rightDelta = PCInputManager.Instance.m_RotateDelta;
#endif

            if (!OnCanSendDelta())
            {
                m_leftDelta = Vector2.zero;
                m_rightDelta = Vector2.zero;
            }

            OnLeftDelta?.Invoke(m_leftDelta);
            OnRightDelta?.Invoke(m_rightDelta);
        }
    }
    class TouchCheckScreenDrag : TouchCheckBase
    {
        public override enum_TouchCheckType m_Type => enum_TouchCheckType.TouchDrag;
        Action<bool,Vector2> OnDragDown;
        Action<Vector2> OnDrag;
        int m_DragTrackID;
        public TouchCheckScreenDrag(Action<bool,Vector2> _OnDragDown, Action<Vector2> _OnDrag)
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
                OnDragDown(true,Input.mousePosition);
            else if (Input.GetMouseButtonUp(0))
                OnDragDown(false,Input.mousePosition);
            else if (Input.GetMouseButton(0))
                OnDrag(Input.mousePosition);
#endif
            foreach (Touch touch in Input.touches)
            {
                if (m_DragTrackID == -1 && touch.phase == TouchPhase.Began)
                {
                    m_DragTrackID = touch.fingerId;
                    OnDragDown(true,touch.position);
                }

                if (m_DragTrackID != touch.fingerId)
                    return;

                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    m_DragTrackID = -1;
                    OnDragDown(false,touch.position);
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
            OnDragDown(false,Vector2.zero);
        }
    }
    class TouchCheckScreenCheck:TouchCheckBase
    {
        public override enum_TouchCheckType m_Type => enum_TouchCheckType.TouchClick;
        Action<bool, Vector2> OnDown;
        public TouchCheckScreenCheck( Action<bool, Vector2> _OnDown)
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

    class TouchTracker
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
}
