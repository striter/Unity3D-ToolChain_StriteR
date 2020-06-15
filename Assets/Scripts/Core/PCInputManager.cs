using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region Enums
public enum enum_BindingsName
{
    Invalid = 0,
    Up,
    Down,
    Left,
    Right,
    Fire,
    Aim,
    Reload,
    Sprint,
    Throw,
    Interact,
    Jump,
    FlashLight,
}
#endregion
public class PCInputManager : SingletonMono<PCInputManager>
{
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
        KeyBindings.CreatePresetBinding(enum_BindingsName.Up, KeyCode.W);
        KeyBindings.CreatePresetBinding(enum_BindingsName.Down,  KeyCode.S);
        KeyBindings.CreatePresetBinding(enum_BindingsName.Left,  KeyCode.A);
        KeyBindings.CreatePresetBinding(enum_BindingsName.Right, KeyCode.D);
        KeyBindings.CreatePresetBinding(enum_BindingsName.Aim,  KeyCode.Mouse1);
        KeyBindings.CreatePresetBinding(enum_BindingsName.Fire, KeyCode.Mouse0);
        KeyBindings.CreatePresetBinding(enum_BindingsName.Reload,  KeyCode.R);
        KeyBindings.CreatePresetBinding(enum_BindingsName.Sprint,  KeyCode.LeftShift);
        KeyBindings.CreatePresetBinding(enum_BindingsName.Throw,  KeyCode.G);
        KeyBindings.CreatePresetBinding(enum_BindingsName.Interact, KeyCode.E);
        KeyBindings.CreatePresetBinding(enum_BindingsName.Jump,  KeyCode.Space);
        KeyBindings.CreatePresetBinding(enum_BindingsName.FlashLight, KeyCode.F);
    }
    #region KeyBindings
    static List<KeyBindings> m_BindingList = new List<KeyBindings>();
    #region struct
    public class KeyBindings
    {
        public enum_BindingsName m_Identity { get; private set; }
        public List<KeyCode> code { get; private set; }
        public event Action triggerEventVoid;
        public event Action<bool> triggerEventBool;
        public void Trigger()
        {
            if(triggerEventVoid!=null)
            triggerEventVoid();
        }
        public void Trigger(bool b)
        {
            if (triggerEventBool != null)
                triggerEventBool(b);
        }
        public void Remove(Action<bool> d)
        {
            triggerEventBool -= d;
        }
        public void Remove(Action d)
        {
            triggerEventVoid -= d;
        }
        public static void CreatePresetBinding(enum_BindingsName name,params KeyCode[] codeList)
        {
            KeyBindings binding = new KeyBindings();
            binding.m_Identity = name;
            binding.code = new List<KeyCode>();
            for (int i = 0; i < codeList.Length; i++)
            {
                binding.code.Add(codeList[i]);
            }
            binding.triggerEventVoid = null;
            binding.triggerEventBool = null;
            m_BindingList.Add(binding);
        }
    }
    #endregion
    public Vector2 m_MovementDelta { get; private set; }
    public Vector2 m_RotateDelta { get; private set; }
    public Action<Vector2> OnMovementDelta;
    public Action<Vector2> OnRotateDelta;
    public void AddBinding(enum_BindingsName name, Action trigger)
    {
        KeyBindings binding = m_BindingList.Find(p => p.m_Identity == name);
        if (binding.m_Identity == enum_BindingsName.Invalid)
        {
            Debug.LogError("Shoulda Preset Binding At Awake "+name.ToString());
            return;
        }

        m_BindingList.Remove(binding);
        binding.triggerEventVoid += trigger;
        m_BindingList.Add(binding);
    }
    public void AddBinding(enum_BindingsName name, Action<bool> trigger)
    {
        KeyBindings binding = m_BindingList.Find(p => p.m_Identity == name);
        if (binding.m_Identity == enum_BindingsName.Invalid)
        {
            Debug.LogError("Shoulda Preset Binding At Awake");
            return;
        }
        binding.triggerEventBool += trigger;
    }

    public void ClearBinding() => m_BindingList.Clear();
    int up, down, left, right;
    List<KeyCode> keyCode;
    enum_BindingsName keyName;
    void Update ()
    {
        for (int i=0;i<m_BindingList.Count;i++)
        {
            keyCode = m_BindingList[i].code;
            for(int j=0;j<keyCode.Count;j++)
            {
                if (Input.GetKeyDown(keyCode[j]))
                    m_BindingList[i].Trigger();
                else if (Input.GetKeyUp(keyCode[j]))
                    m_BindingList[i].Trigger(false);
                else if (Input.GetKeyUp(keyCode[j]))
                    m_BindingList[i].Trigger(true);

                keyName = m_BindingList[i].m_Identity;
                switch (keyName)
                {
                    case enum_BindingsName.Up:
                        up = Input.GetKey(keyCode[j]) ? 1 : 0;
                        break;
                    case enum_BindingsName.Down:
                        down = Input.GetKey(keyCode[j]) ? 1 : 0;
                        break;
                    case enum_BindingsName.Left:
                        left = Input.GetKey(keyCode[j]) ? 1 : 0;
                        break;
                    case enum_BindingsName.Right:
                        right = Input.GetKey(keyCode[j]) ? 1 : 0;
                        break;
                }
            }
        }
        m_MovementDelta= new Vector2(right - left, up - down);
        m_RotateDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        OnMovementDelta?.Invoke(m_MovementDelta);
        OnRotateDelta?.Invoke(m_RotateDelta);
    }
    #region For RTS Only
    int ScreenHeight= Screen.height;
    int ScreenWidth=Screen.width;
    int ScreenMoveSensitivity = 20;
    Vector2 mousePos;
    public Vector2 RTSOnMouseScreenMove()
    {
        mousePos = Input.mousePosition;
        return new Vector2(mousePos.x < ScreenMoveSensitivity ? -1 : mousePos.x > ScreenWidth - ScreenMoveSensitivity ? 1 : 0,
        mousePos.y < ScreenMoveSensitivity ? -1 : mousePos.y > ScreenHeight - ScreenMoveSensitivity ? 1 : 0 );
    }
    #endregion
    #region get/set
    Vector2 mouseInput =new Vector2();
    public Vector2 MouseInput
    {
        get
        {
            mouseInput.x = Input.GetAxis("Mouse X");
            mouseInput.y = Input.GetAxis("Mouse Y");
            return mouseInput;
        }
    }
    #endregion
    #endregion
}
