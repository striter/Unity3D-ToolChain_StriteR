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
public enum enum_PressStatus
{
    Down,
    Up,
    Press,
    Both,
}
#endregion
public class PCInputManager : SingletonMono<PCInputManager>
{
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
        KeyBindings.CreatePresetBinding(enum_BindingsName.Up, enum_PressStatus.Both, KeyCode.W);
        KeyBindings.CreatePresetBinding(enum_BindingsName.Down, enum_PressStatus.Both, KeyCode.S);
        KeyBindings.CreatePresetBinding(enum_BindingsName.Left, enum_PressStatus.Both, KeyCode.A);
        KeyBindings.CreatePresetBinding(enum_BindingsName.Right, enum_PressStatus.Both, KeyCode.D);
        KeyBindings.CreatePresetBinding(enum_BindingsName.Aim, enum_PressStatus.Both, KeyCode.Mouse1);
        KeyBindings.CreatePresetBinding(enum_BindingsName.Fire, enum_PressStatus.Both, KeyCode.Mouse0);
        KeyBindings.CreatePresetBinding(enum_BindingsName.Reload, enum_PressStatus.Down, KeyCode.R);
        KeyBindings.CreatePresetBinding(enum_BindingsName.Sprint, enum_PressStatus.Both, KeyCode.LeftShift);
        KeyBindings.CreatePresetBinding(enum_BindingsName.Throw, enum_PressStatus.Down, KeyCode.G);
        KeyBindings.CreatePresetBinding(enum_BindingsName.Interact, enum_PressStatus.Down, KeyCode.E);
        KeyBindings.CreatePresetBinding(enum_BindingsName.Jump, enum_PressStatus.Down, KeyCode.Space);
        KeyBindings.CreatePresetBinding(enum_BindingsName.FlashLight, enum_PressStatus.Down, KeyCode.F);
    }
    #region KeyBindings
    static List<KeyBindings> List_BindingList = new List<KeyBindings>();
    static Dictionary<Type, List<Action>> Dic_BindingRemoval = new Dictionary<Type, List<Action>>();
    #region struct
    public struct KeyBindings
    {
        public enum_BindingsName Name;
        public List<KeyCode> code;
        public enum_PressStatus status;
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
        public static void CreatePresetBinding(enum_BindingsName name, enum_PressStatus status,params KeyCode[] codeList)
        {
            KeyBindings binding = new KeyBindings();
            binding.Name = name;
            binding.code = new List<KeyCode>();
            for (int i = 0; i < codeList.Length; i++)
            {
                binding.code.Add(codeList[i]);
            }
            binding.status = status;
            binding.triggerEventVoid = null;
            binding.triggerEventBool = null;
            List_BindingList.Add(binding);
        }
    }
    #endregion
    public Vector2 m_MovementDelta,m_RotateDelta;
    public void AddBinding<T>(enum_BindingsName name, Action trigger)
    {
        KeyBindings binding = List_BindingList.Find(p => p.Name == name);
        if (binding.Name == enum_BindingsName.Invalid)
        {
            Debug.LogError("Shoulda Preset Binding At Awake "+name.ToString());
            return;
        }
        if (binding.status != enum_PressStatus.Both)
        {
            List_BindingList.Remove(binding);
            binding.triggerEventVoid += trigger;
            List_BindingList.Add(binding);
            AddBindingRemoval(typeof(T), delegate () {
                int bindingIndex = List_BindingList.FindIndex(p => p.Name == name);
                KeyBindings temp = List_BindingList[bindingIndex];
                List_BindingList.RemoveAt(bindingIndex);
                temp.Remove(trigger);
                List_BindingList.Add(temp);
            });
        }
    }
    public void AddBinding<T>(enum_BindingsName name, Action<bool> trigger)
    {
        KeyBindings binding = List_BindingList.Find(p => p.Name == name);
        if (binding.Name == enum_BindingsName.Invalid)
        {
            Debug.LogError("Shoulda Preset Binding At Awake");
            return;
        }
        if (binding.status == enum_PressStatus.Both)
        {
            List_BindingList.Remove(binding);
            binding.triggerEventBool += trigger;
            List_BindingList.Add(binding);
            AddBindingRemoval(typeof(T), delegate () {
                int bindingIndex = List_BindingList.FindIndex(p => p.Name == name);
                KeyBindings temp = List_BindingList[bindingIndex];
                List_BindingList.RemoveAt(bindingIndex);
                temp.Remove(trigger);
                List_BindingList.Add(temp);
            });
        }
    }
    void AddBindingRemoval(Type type, Action del)
    {
        if (!Dic_BindingRemoval.ContainsKey(type))
        {
            Dic_BindingRemoval.Add(type, new List<Action>());
        }
        Dic_BindingRemoval[type].Add(del);
    }
    public void DoBindingRemoval<T>()
    {
        Type t = typeof(T);
        if (Dic_BindingRemoval.ContainsKey(t))
        {
            for (int i = 0; i < Dic_BindingRemoval[t].Count; i++)
            {
                Dic_BindingRemoval[t][i]();
            }
            Dic_BindingRemoval[t] = new List<Action>();
        }
    }
    int up, down, left, right;
    enum_PressStatus status;
    List<KeyCode> keyCode;
    enum_BindingsName keyName;
    bool triggerd;
    void Update ()
    {
        for (int i=0;i<List_BindingList.Count;i++)
        {
            status = List_BindingList[i].status;
            keyCode = List_BindingList[i].code;
            for(int j=0;j<keyCode.Count;j++)
            {
                triggerd = false;
                switch (status)
                {
                    case enum_PressStatus.Down:
                        if (Input.GetKeyDown(keyCode[j]))
                        {
                            List_BindingList[i].Trigger();
                            triggerd = true;
                        }
                        break;
                    case enum_PressStatus.Up:
                        if (Input.GetKeyUp(keyCode[j]))
                        {
                            List_BindingList[i].Trigger();
                            triggerd = true;
                        }
                        break;
                    case enum_PressStatus.Press:
                        if (Input.GetKey(keyCode[j]))
                        {
                            List_BindingList[i].Trigger();
                            triggerd = true;
                        }
                        break;
                    case enum_PressStatus.Both:
                        if (Input.GetKeyDown(keyCode[j]))
                        {
                            List_BindingList[i].Trigger(true);
                            triggerd = true;
                        }
                        else if (Input.GetKeyUp(keyCode[j]))
                        {
                            List_BindingList[i].Trigger(false);
                            triggerd = true;
                        }
                        break;
                    default:
                        Debug.LogError("How A None Status Binding Set?");
                        break;
                }

                keyName = List_BindingList[i].Name;
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

                if (triggerd)
                {
                    break;
                }
            }
        }
        m_MovementDelta= new Vector2(right - left, up - down);
        m_RotateDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
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
