using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum enum_Option_JoyStickMode { Invalid = -1, Retarget = 1, Stational = 2, }
public class UIT_JoyStick : SingletonMono<UIT_JoyStick>
{
#if UNITY_EDITOR
    public bool TESTMODE = false;
    private void Update()
    {
        if (!TESTMODE)
            return;

        if (Input.GetKeyDown(KeyCode.Mouse0))
            OnActivate(Input.mousePosition);
        if (Input.GetKey(KeyCode.Mouse0))
            OnMoved(Input.mousePosition);
        if (Input.GetKeyUp(KeyCode.Mouse0))
            OnDeactivate(); 
    }
#endif
    RectTransform rtf_Main;
    RectTransform rtf_Center;
    enum_Option_JoyStickMode m_Mode= enum_Option_JoyStickMode.Invalid;
    JoyStickBase m_JoystickControl;
    Vector2 v2_startPos;
    public float m_JoyStickRaidus { get; private set; }
    protected override void Awake()
    {
        base.Awake();
        rtf_Main = GetComponent<RectTransform>();
        rtf_Center = transform.Find("Center").GetComponent<RectTransform>();
        v2_startPos = rtf_Main.anchoredPosition;
        m_JoyStickRaidus = Mathf.Abs( rtf_Main.sizeDelta.y/2)-Mathf.Abs( rtf_Center.sizeDelta.y/2);
        SetMode(enum_Option_JoyStickMode.Retarget);
    }
    public void SetMode(enum_Option_JoyStickMode mode)
    {
        if (mode == m_Mode) return;

        m_Mode = mode;
        switch (m_Mode)
        {
            case enum_Option_JoyStickMode.Stational:
                m_JoystickControl = new JoyStickStational(v2_startPos, m_JoyStickRaidus);
                break;
            case enum_Option_JoyStickMode.Retarget:
                m_JoystickControl = new JoyStickRetarget(v2_startPos, m_JoyStickRaidus);
                break;
        }
        ResetStatus();
    }

    public void OnActivate(Vector2 pos)
    {
        m_JoystickControl.OnActivate( pos);
        ResetStatus();
    }
    public void OnDeactivate()
    {
        m_JoystickControl.OnDeactivate();
        ResetStatus();
    }

    public Vector2 OnMoved(Vector2 pos)
    {
        Vector2 delta= m_JoystickControl.OnMoved(pos);
        ResetStatus();
        return delta;
    }
    private void LateUpdate()
    {
        transform.localScale =Vector3.Lerp(transform.localScale, m_JoystickControl.m_JoyStickShow ? Vector3.one : Vector3.zero,.2f);
        rtf_Center.anchoredPosition = m_JoystickControl.m_JoyStickOffset;
    }
    void ResetStatus()
    {
        rtf_Main.anchoredPosition = m_JoystickControl.m_BasePos;
    }

    class JoyStickBase
    {
        public bool m_JoyStickShow { get; protected set; } = false;
        public Vector2 m_BasePos { get; protected set; } = Vector2.zero;
        public Vector2 m_JoyStickOffset { get; protected set; } = Vector2.zero;
        public float m_JoystickRadius { get; protected set; } = 0f;
        public JoyStickBase(Vector2 startPos, float radius)
        {
            m_BasePos = startPos;
            m_JoystickRadius = radius;
            m_JoyStickShow = false;
        }
        public virtual void OnActivate(Vector2 pos)
        {
            m_JoyStickOffset = Vector2.zero;
        }
        public virtual void OnDeactivate()
        {
            m_JoyStickOffset = Vector2.zero;
        }
        public virtual Vector2 OnMoved(Vector2 pos)
        {
            Vector2 centerOffset = Vector2.Distance(pos, m_BasePos) > m_JoystickRadius ? (pos - m_BasePos).normalized * m_JoystickRadius : pos - m_BasePos;
            m_JoyStickOffset = centerOffset;
            return centerOffset / m_JoystickRadius;
        }
    }
    class JoyStickStational:JoyStickBase
    {
        bool enabled;
        public JoyStickStational(Vector2 startPos, float radius) : base(startPos,radius)
        {
            m_JoyStickShow = true;
            enabled = false;
        }

        public override void OnActivate( Vector2 pos)
        {
            base.OnActivate(pos);
            enabled = Vector2.Distance(pos,m_BasePos)<m_JoystickRadius*2f;
         }
        public override void OnDeactivate()
        {
            base.OnDeactivate();
            enabled = false;
        }
        public override Vector2 OnMoved(Vector2 pos)
        {
            if (!enabled) return Vector2.zero;
            return base.OnMoved(pos);
        }
    }
    class JoyStickRetarget : JoyStickBase
    {
        public JoyStickRetarget(Vector2 startPos, float radius) : base(startPos, radius)
        {
        }
        public override void OnActivate( Vector2 pos)
        {
            m_JoyStickShow = true;
            m_BasePos = pos;
        }
        public override void OnDeactivate()
        {
            base.OnDeactivate();
            m_JoyStickShow = false;
        }
    }
}
