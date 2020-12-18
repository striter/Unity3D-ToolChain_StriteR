using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region Enums
public enum enum_Binding
{
    Invalid = 0,
    Up,
    Down,
    Left,
    Right,
    MainFire,
    AltFire,
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
    }
    public Dictionary<enum_Binding, KeyBindings> m_Bindings=new Dictionary<enum_Binding, KeyBindings>()
    {
        {enum_Binding.Up,new KeyBindings( KeyCode.W)},
        {enum_Binding.Down,new KeyBindings( KeyCode.S)},
        {enum_Binding.Left,new KeyBindings( KeyCode.A)},
        {enum_Binding.Right,new KeyBindings( KeyCode.D)},
        {enum_Binding.MainFire,new KeyBindings( KeyCode.Mouse0)},
        {enum_Binding.AltFire,new KeyBindings( KeyCode.Mouse1)},
        {enum_Binding.Reload,new KeyBindings( KeyCode.R)},
        {enum_Binding.Sprint,new KeyBindings( KeyCode.LeftShift)},
        {enum_Binding.Throw,new KeyBindings( KeyCode.G)},
        {enum_Binding.Interact,new KeyBindings( KeyCode.E)},
        {enum_Binding.Jump,new KeyBindings( KeyCode.Space)},
        {enum_Binding.FlashLight,new KeyBindings( KeyCode.F)},
    };
    public KeyBindings GetKeyBinding(enum_Binding _binding)
    {
        if(!m_Bindings.ContainsKey(_binding))
        {
            Debug.LogError("Preset Binding In Code! " + name.ToString());
            return null;
        }
        return m_Bindings[_binding];
    }
    #region KeyBindings
    public class KeyBindings
    {
        public KeyCode m_ActivateKeyCode { get; private set; }
        public event Action TriggerEventVoid;
        public event Action<bool> TriggerEventBool;
        public KeyBindings(KeyCode _default)
        {
            m_ActivateKeyCode = _default;
        }
        public void SetBindingKey(KeyCode _keyCode)=>m_ActivateKeyCode = _keyCode;
        public void Add(Action trigger)=>TriggerEventVoid += trigger;
        public void Remove(Action d) => TriggerEventVoid -= d;
        public void Trigger()=>TriggerEventVoid?.Invoke();

        public void Add(Action<bool> trigger) => TriggerEventBool += trigger;
        public void Remove(Action<bool> d) => TriggerEventBool -= d;
        public void Trigger(bool b)=> TriggerEventBool?.Invoke(b);

        public void Clear()
        {
            foreach (var del in TriggerEventVoid.GetInvocationList())
                TriggerEventVoid -= (Action)del;
            foreach (var del in TriggerEventBool.GetInvocationList())
                TriggerEventBool -= (Action<bool>)del;
        }
    }
    #endregion
    Vector2 movementDelta;
    Vector2 rotateDelta;
    public Action<Vector2> OnMovementDelta;
    public Action<Vector2> OnRotateDelta;

    public void ClearBinding()
    { 
        foreach(var keyBinding in m_Bindings.Values)
            keyBinding.Clear();
    }
    int up, down, left, right;
    void Update ()
    {
        foreach(var keyEnum in m_Bindings.Keys)
        {
           KeyBindings bindings=m_Bindings[keyEnum];
            KeyCode keyCode = bindings.m_ActivateKeyCode;
            if (Input.GetKeyDown(keyCode))
            {
                bindings.Trigger();
                bindings.Trigger(true);
            }
            else if (Input.GetKeyUp(keyCode))
            {
                bindings.Trigger(false);
            }

            switch (keyEnum)
            {
                case enum_Binding.Up:
                    up = Input.GetKey(keyCode) ? 1 : 0;
                    break;
                case enum_Binding.Down:
                    down = Input.GetKey(keyCode) ? 1 : 0;
                    break;
                case enum_Binding.Left:
                    left = Input.GetKey(keyCode) ? 1 : 0;
                    break;
                case enum_Binding.Right:
                    right = Input.GetKey(keyCode) ? 1 : 0;
                    break;
            }
        }
        movementDelta= new Vector2(right - left, up - down);
        rotateDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        OnMovementDelta?.Invoke(movementDelta);
        OnRotateDelta?.Invoke(rotateDelta);
    }
}
