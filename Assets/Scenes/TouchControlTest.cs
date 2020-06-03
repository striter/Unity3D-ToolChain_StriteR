using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchControlTest : MonoBehaviour {
    public enum enum_TestDirection
    {
        Invalid,
        Forward,
        Back,
        Left,
        Right,
    }

    TouchCheckLRJoystick m_TouchJoyStickMode;
    TouchCheckDown m_TouchScreenDownMode;
    TouchCheckScreenDrag m_TouchScreenDragMode;
    bool m_JoyStickStational = false;

    RaycastHit m_RaycastHit;
    Rigidbody m_Rigidbody;
    void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
    }
    
    void Start()
    {
        UIT_TouchConsole.Instance.InitConsole(OnConsoleOpen);
        UIT_TouchConsole.Instance.AddConsoleBinding().Play("Jump",  KeyCode.Space,"200", OnConsoleJump);
        UIT_TouchConsole.Instance.AddConsoleBinding().Play("Push", KeyCode.None,  enum_TestDirection.Forward, OnConsoleDirection);
        UIT_TouchConsole.Instance.AddConsoleBinding().Play("Reset", KeyCode.R, OnConsoleReset);
        UIT_TouchConsole.Instance.AddConsoleBinding().Play("Switch Joystick Mode", KeyCode.None, OnJoystickSwitch);
        UIT_TouchConsole.Instance.AddConsoleBinding().Play("Switch Touch Mode", KeyCode.None, enum_TouchCheckType.TouchLR, OnTouchInputSwitch);

        m_JoyStickStational = true;
        OnJoystickSwitch();

        m_TouchJoyStickMode = new TouchCheckLRJoystick(OnTouchLeftDelta, OnTouchRightDelta, null);
        m_TouchScreenDownMode = new TouchCheckDown(OnTouchCheck);
        m_TouchScreenDragMode = new TouchCheckScreenDrag(OnDragDown,OnDrag);
        OnTouchInputSwitch(enum_TouchCheckType.TouchLR);
    }

    void OnConsoleOpen(bool open) => Debug.LogError("Console Status:" + open);
    void OnConsoleClick() => Debug.Log("TestConsole");
    void OnTouchInputSwitch(enum_TouchCheckType type)
    {
        Debug.LogWarning("Switch Touch Mode:" + type);
        switch (type)
        {
            case enum_TouchCheckType.TouchDown: TouchInputManager.Instance.Init(m_TouchScreenDownMode); break;
            case enum_TouchCheckType.TouchLR: TouchInputManager.Instance.Init(m_TouchJoyStickMode); break;
            case enum_TouchCheckType.TouchDrag: TouchInputManager.Instance.Init(m_TouchScreenDragMode); break;
        }
    }

    void OnJoystickSwitch()
    {
        m_JoyStickStational = !m_JoyStickStational;
        Debug.LogWarning("Switch Joystick Stational:" + m_JoyStickStational);
        UIT_JoyStick.Instance.SetMode(m_JoyStickStational ? enum_Option_JoyStickMode.Stational : enum_Option_JoyStickMode.Retarget);
    }


    void OnConsoleDirection(enum_TestDirection direction)
    {
        Vector3 directionVector = Vector3.zero;
        switch(direction)
        {
            case enum_TestDirection.Forward: directionVector = Vector3.forward;  break;
            case enum_TestDirection.Back: directionVector = Vector3.back;  break;
            case enum_TestDirection.Left: directionVector = Vector3.left; break;
            case enum_TestDirection.Right: directionVector = Vector3.right;break;
        }
        m_Rigidbody.AddForce(directionVector * 200f);
    }

    void OnConsoleJump(string jumpValue)
    {
        int value;
        if (int.TryParse(jumpValue, out value))
            m_Rigidbody.AddForce(Vector3.up*value);
    }

    void OnConsoleReset()
    {
        transform.position = Vector3.zero;
        m_Rigidbody.velocity = Vector3.zero;
        m_Rigidbody.angularVelocity = Vector3.zero;
    }

    void OnTouchLeftDelta(Vector2 delta)=>m_Rigidbody.AddForce(new Vector3( delta.x,0,delta.y) * 5f);
    void OnTouchRightDelta(Vector2 delta)=>m_Rigidbody.angularVelocity +=new Vector3(delta.y,0 ,-delta.x);

    void OnDragDown(bool down, Vector2 pos) => m_Rigidbody.AddForce(Vector3.up * 200f);

    void OnDrag(Vector2 screenPos) {

        if(Physics.Raycast(Camera.main.ScreenPointToRay(screenPos), out m_RaycastHit, 1000f,1<<0))
        {
            Vector3 offset = m_RaycastHit.point-transform.position ;
            m_Rigidbody.AddForce(offset * 5f);
        }
    }
    void OnTouchCheck(bool down,Vector2 screenPos)
    {
        if (Physics.Raycast(Camera.main.ScreenPointToRay(screenPos), out m_RaycastHit, 1000f, 1 << 0))
        {
            m_Rigidbody.velocity = Vector3.zero;
            m_Rigidbody.angularVelocity = Vector3.zero;
            Vector3 offset = m_RaycastHit.point - transform.position;
            m_Rigidbody.AddForce(offset * 20f);
        }
    }
}
