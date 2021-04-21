using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using TDataPersistent;
using static UIT_TouchConsole;

public partial class UIT_TouchConsole : SingletonMono<UIT_TouchConsole>
{
    #region Helper
    public static void InitDefaultCommands(Action<bool> _OnConsoleShow = null) => Instance.InitConsole(_OnConsoleShow);
    public static void EmptyLine() => Instance.AddCommandLine();
    public static void Header(string _title) => Instance.AddCommandLine().Insert<CommandItem_Header>().m_HeaderTitle.text = _title;
    public static CommandContainer Command(string _title, KeyCode _keyCode = KeyCode.None)
    {
        CommandContainer container = Instance.AddCommandLine(_keyCode);
        container.Insert<CommandItem_CommandTitle>().m_CommandTitle.text = _title;
        return container;
    }
    public static void InitSerializeCommands<T>(T _target, Action<T> _OnSerializeDataChanged) where T:MonoBehaviour => Instance.CommandSerialize(_target, _OnSerializeDataChanged);
    public static UIT_JoyStick GetHelperJoystick() => Instance.m_HelperJoystick;
    #endregion

    public TouchConsoleSaveData m_Data = new TouchConsoleSaveData();
    UIT_JoyStick m_HelperJoystick;

    [Serializable]
    public class TouchConsoleSaveData : CDataSave<TouchConsoleSaveData>
    {
        public override bool DataCrypt() => false;
        public Ref<float> m_ConsoleTimeScale;
        public Ref<enum_ConsoleSetting> m_FilterSetting;
        [Header("Log Filter")]
        public Ref<bool> m_Log;
        public Ref<bool> m_Warning;
        public Ref<bool> m_Error;
        public Ref<bool> m_Collapse;
        public TouchConsoleSaveData()
        {
            m_Warning =  true;
            m_Log = true;
            m_Error = true;
            m_Collapse = true;
            m_FilterSetting = (enum_ConsoleSetting)int.MaxValue;
            m_ConsoleTimeScale = .5f;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        m_HelperJoystick = new UIT_JoyStick(transform.Find("Joystick"));
        m_Data.ReadPersistentData();

        ConsoleAwake();
        LogFrameAwake();
        SetLogFramePanel(m_Data.m_FilterSetting.m_RefValue);
    }
    protected UIT_TouchConsole InitConsole(Action<bool> _OnConsoleShow)
    {
        ConsoleReset(_OnConsoleShow);
        LogFrameReset();

        Header("Console");
        Command("Time Scale").Slider(0f, 2f, m_Data.m_ConsoleTimeScale, scale => { m_Data.SavePersistentData(); SetConsoleTimeScale(scale); });
        Command("Right Panel").FlagsSelection(m_Data.m_FilterSetting, setting => { m_Data.SavePersistentData(); SetLogFramePanel(setting); });
        EmptyLine();
        return this;
    }

    protected UIT_TouchConsole CommandSerialize<T>(T _target,Action<T> _OnSerializeDataChanged) where T:MonoBehaviour
    {
        Type targetType = _target.GetType();
        Header(targetType.Name);
        Command("Enable").Toggle(_target.enabled, value => _target.enabled = value);
        foreach(var fieldStack in typeof(T).GetBaseTypeFieldStacks(BindingFlags.Instance))
        {
            object startValue = fieldStack.Value.GetValue(_target);
            if (fieldStack.Key.FieldType.IsEnum)
            {
                Command(fieldStack.Key.Name).EnumSelection(startValue,value=>fieldStack.Value.SetValue(_target,value));
                continue;
            }
            if(fieldStack.Key.FieldType==typeof(bool))
            {
                Command(fieldStack.Key.Name).Toggle((bool)startValue,value=>fieldStack.Value.SetValue(_target,value));
                continue;
            }
            if (fieldStack.Key.FieldType == typeof(string))
            {
                Command(fieldStack.Key.Name).InputField((string)startValue, value => fieldStack.Value.SetValue(_target, value));
                continue;
            }

            var attributes = fieldStack.Key.GetCustomAttributes(false);
            foreach (var attribute in attributes)
            {
                if(attribute is RangeAttribute)
                {
                    RangeAttribute rangeAttribute = attribute as RangeAttribute;
                    if(fieldStack.Key.FieldType==typeof(int))
                        Command(fieldStack.Key.Name).Slider((int)rangeAttribute.min, (int)rangeAttribute.max, (int)startValue, value => { fieldStack.Value.SetValue(_target, value); _OnSerializeDataChanged?.Invoke(_target); });
                    else
                        Command(fieldStack.Key.Name).Slider(rangeAttribute.min,rangeAttribute.max,(float)startValue ,value=> { fieldStack.Value.SetValue(_target, value); _OnSerializeDataChanged?.Invoke(_target); });
                    continue;
                }
                else if(attribute is RangeVectorAttribute)
                {
                    RangeVectorAttribute vectorAttribute = attribute as RangeVectorAttribute;
                    CommandContainer command = Command(fieldStack.Key.Name);
                    Type fieldType = fieldStack.Key.FieldType;
                    if (fieldType == typeof(Vector2))
                    {
                        Vector2 startVec = (Vector2)startValue;
                        command.Slider(vectorAttribute.m_Min, vectorAttribute.m_Max, startVec.x, value => { fieldStack.Value.SetValue(_target, ((Vector2)fieldStack.Value.GetValue(_target)).SetX(value)); _OnSerializeDataChanged?.Invoke(_target); }, "X:{0:0.0}");
                        command.Slider(vectorAttribute.m_Min, vectorAttribute.m_Max, startVec.y, value => { fieldStack.Value.SetValue(_target, ((Vector2)fieldStack.Value.GetValue(_target)).SetY(value)); _OnSerializeDataChanged?.Invoke(_target); }, "Y:{0:0.0}");
                    }
                    else if (fieldType == typeof(Vector3))
                    {
                        Vector3 startVec = (Vector3)startValue;
                        command.Slider(vectorAttribute.m_Min, vectorAttribute.m_Max, startVec.x, value => { fieldStack.Value.SetValue(_target, ((Vector3)fieldStack.Value.GetValue(_target)).SetX(value)); _OnSerializeDataChanged?.Invoke(_target); }, "X:{0:0.0}");
                        command.Slider(vectorAttribute.m_Min, vectorAttribute.m_Max, startVec.y, value => { fieldStack.Value.SetValue(_target, ((Vector3)fieldStack.Value.GetValue(_target)).SetY(value)); _OnSerializeDataChanged?.Invoke(_target); }, "Y:{0:0.0}");
                        command.Slider(vectorAttribute.m_Min, vectorAttribute.m_Max, startVec.z, value => { fieldStack.Value.SetValue(_target, ((Vector3)fieldStack.Value.GetValue(_target)).SetZ(value)); _OnSerializeDataChanged?.Invoke(_target); }, "Z:{0:0.0}");
                    }
                    else if (fieldType == typeof(Vector4))
                    {
                        Vector4 startVec = (Vector4)startValue;
                        command.Slider(vectorAttribute.m_Min, vectorAttribute.m_Max, startVec.x, value => { fieldStack.Value.SetValue(_target, ((Vector4)fieldStack.Value.GetValue(_target)).SetX(value)); _OnSerializeDataChanged?.Invoke(_target); }, "X:{0:0.0}");
                        command.Slider(vectorAttribute.m_Min, vectorAttribute.m_Max, startVec.y, value => { fieldStack.Value.SetValue(_target, ((Vector4)fieldStack.Value.GetValue(_target)).SetY(value)); _OnSerializeDataChanged?.Invoke(_target); }, "X:{0:0.0}");
                        command.Slider(vectorAttribute.m_Min, vectorAttribute.m_Max, startVec.z, value => { fieldStack.Value.SetValue(_target, ((Vector4)fieldStack.Value.GetValue(_target)).SetZ(value)); _OnSerializeDataChanged?.Invoke(_target); }, "X:{0:0.0}");
                        command.Slider(vectorAttribute.m_Min, vectorAttribute.m_Max, startVec.w, value => { fieldStack.Value.SetValue(_target, ((Vector4)fieldStack.Value.GetValue(_target)).SetW(value)); _OnSerializeDataChanged?.Invoke(_target); }, "X:{0:0.0}");
                    }
                }
            }
        }
        return this;
    }

    private void Update()
    {
        float deltaTime = Time.unscaledDeltaTime;
        ConsoleTick(deltaTime);

        if (m_Data.m_FilterSetting.m_RefValue.IsFlagEnable(enum_ConsoleSetting.FPS))
            LogFrame(deltaTime);
    }

    #region Miscs
    protected void SetConsoleTimeScale(float _timeScale)
    {
        if (!m_ConsoleOpening)
            return;
        Time.timeScale = _timeScale;
    }
    protected void SetLogFramePanel(enum_ConsoleSetting _panelSetting)
    {
        m_FrameRate.SetActive(_panelSetting.IsFlagEnable(enum_ConsoleSetting.FPS));
        m_LogFilter.SetActive(_panelSetting.IsFlagEnable(enum_ConsoleSetting.LogPanel));
        m_LogPanelRect.SetActive(!_panelSetting.IsFlagClear());
        UpdateLogs();
    }
    #endregion
}
#region Command Console
public static class UIT_TouchConsoleHelper
{
    public static string GetKeyCodeString(this KeyCode _keyCode) => _keyCode == KeyCode.None ? "" : _keyCode.ToString();
    public static void Button(this CommandContainer _container, Action OnClick)
    {
        CommandItem_Button button = _container.Insert<CommandItem_Button>();
        button.m_Button.onClick.AddListener(() => OnClick());
        button.m_ButtonTitle.text = _container.m_KeyCode.GetKeyCodeString();
    }
    public static void Toggle(this CommandContainer _container,Ref<bool> _refValue,Action<bool> OnToggleChange)
    {
        CommandItem_Toggle toggle = _container.Insert<CommandItem_Toggle>();
        toggle.SetDataUpdate(() => toggle.m_Toggle.isOn=_refValue.m_RefValue);
        toggle.m_Toggle.onValueChanged.AddListener(value => {
            _refValue.SetValue(value);
            OnToggleChange(value);
        });
        toggle.m_ToggleTitle.text = _container.m_KeyCode.GetKeyCodeString();
    }
    public static void Slider(this CommandContainer _container, int _minValue, int _maxValue, Ref<int> _refValue, Action<int> _SetValue, string _format = "{0}")=>Slider(_container, _minValue, _maxValue, _refValue.m_RefValue, value => { _refValue.SetValue((int)value); _SetValue(_refValue.m_RefValue); }, _format, true);
    public static void Slider(this CommandContainer _container, float _minValue, float _maxValue, Ref<float> _refValue, Action<float> _SetValue,string _format="{0:0.0}",bool _wholeNumbers=false)
    {
        CommandItem_Slider slider = _container.Insert<CommandItem_Slider>();
        slider.m_Slider.wholeNumbers = _wholeNumbers;
        slider.m_Slider.minValue = _minValue;
        slider.m_Slider.maxValue = _maxValue;
        slider.SetDataUpdate(() => {
            float finalValue = _refValue.m_RefValue;
            slider.m_Slider.value = finalValue;
            slider.m_Value.text =string.Format( _format, finalValue);
        });
        slider.m_Slider.onValueChanged.AddListener(value => {
            slider.m_Value.text =string.Format(_format, value);
            _refValue.SetValue(value);
            _SetValue(value);
        });
    }
    static T ButtonFoldOutItem<T>(this CommandContainer _container,bool foldOut,out CommandItem_Button _button) where T : CommandItemBase
    {
        _button = null;
        if (!foldOut)
            return _container.Insert<T>(); 
        _button = _container.Insert<CommandItem_Button>();
        T item = _container.Insert<T>();
        _button.m_Button.onClick.AddListener(() => item.transform.SetActive(!item.transform.gameObject.activeSelf));
        item.transform.SetActive(false);
        return item;
    }
    public static void EnumSelection<T>(this CommandContainer _container, Ref<T> _valueRef, Action<T> OnClick, bool foldOut = true) where T : struct, Enum => EnumSelection(_container,_valueRef.m_RefValue,enumObj=> { _valueRef.SetValue((T)enumObj);OnClick?.Invoke(_valueRef.m_RefValue); },foldOut);
    public static void EnumSelection(this CommandContainer _container, object _value, Action<object> OnClick, bool foldOut = true)
    {
        Type type = _value.GetType();
        if (!type.IsEnum)
            throw new Exception("Input Must Be Enum!");

        CommandItem_ButtonSelection selection = _container.ButtonFoldOutItem<CommandItem_ButtonSelection>(foldOut, out CommandItem_Button foldOutButton);
        selection.SetDataUpdate(() => {
            selection.Play(_value, value => {
                OnClick(value);
                if (foldOutButton != null)
                {
                    foldOutButton.m_Button.onClick.Invoke();
                    foldOutButton.m_ButtonTitle.text = Enum.ToObject(type, value).ToString();
                }
            });
            if (foldOutButton != null)
                foldOutButton.m_ButtonTitle.text = Enum.ToObject(type, _value).ToString();
        });
    }
    public static void EnumSelection(this CommandContainer _container, Ref<int> _refEnum, List<string> _values, Action<string> OnClick,bool foldOut=true)
    {
        CommandItem_ButtonSelection selection = _container.ButtonFoldOutItem<CommandItem_ButtonSelection>(foldOut, out CommandItem_Button foldOutButton);
        selection.SetDataUpdate(() => {
            selection.Play(_values, _refEnum.m_RefValue, (int value) => {
                OnClick(_values[value]);
                if (foldOutButton != null)
                    foldOutButton.m_ButtonTitle.text = _values[value];
            });
            if (foldOutButton != null)
                foldOutButton.m_ButtonTitle.text = _values[_refEnum.m_RefValue];
        });
    }
    public static void FlagsSelection<T>(this CommandContainer _container, Ref<T> _refFlags, Action<T> _logFilter,bool foldOut=true) where T :struct, Enum
    {
        CommandItem_FlagsSelection selection= _container.ButtonFoldOutItem<CommandItem_FlagsSelection>(foldOut, out CommandItem_Button foldOutButton);
        selection.SetDataUpdate(() => selection.Play(_refFlags.m_RefValue, flags => {
            _refFlags.SetValue(flags);
            if (foldOutButton != null)
                foldOutButton.m_ButtonTitle.text = flags.GetNumerable().ToString('|', value => value ? "√" : "×");
            _logFilter(flags);
        }));
        if (foldOutButton != null)
            foldOutButton.m_ButtonTitle.text = _refFlags.m_RefValue.GetNumerable().ToString('|', value => value ? "√" : "×");
    }
    public static void InputField(this CommandContainer _container, Ref<string> _refText, Action<string> OnValueClick)
    {
        CommandItem_InputField input = _container.Insert<CommandItem_InputField>();
        input.SetDataUpdate(() => { input.m_InputField.text = _refText.m_RefValue; });
        input.m_InputField.onValueChanged.AddListener(_refText.SetValue);
        _container.Button(() => OnValueClick(input.m_InputField.text));
    }
    public static void InpuptField(this CommandContainer _container, Ref<string> _refText1, Ref<string> _refText2, Action<string, string> OnValueClick)
    {
        CommandItem_InputField input1 = _container.Insert<CommandItem_InputField>();
        CommandItem_InputField input2 = _container.Insert<CommandItem_InputField>();
        input1.SetDataUpdate(() => input1.m_InputField.text = _refText1.m_RefValue);
        input2.SetDataUpdate(() => input2.m_InputField.text = _refText2.m_RefValue);
        input1.m_InputField.onValueChanged.AddListener(_refText1.SetValue);
        input2.m_InputField.onValueChanged.AddListener(_refText2.SetValue);
        _container.Button(() => OnValueClick(input1.m_InputField.text, input2.m_InputField.text));
    }
}
public partial class UIT_TouchConsole : SingletonMono<UIT_TouchConsole>
{
    public bool m_ConsoleOpening { get; private set; } = false;
    Timer m_FastKeyCooldownTimer = new Timer(.5f);

    int m_totalCommands;
    int m_totalItems;

    ScrollRect m_ConsoleCommandScrollRect;
    TGameObjectPool_Instance_Class<int, CommandContainer> m_CommandContainers;
    Dictionary<Type, TGameObjectPool_Instance_Class<int, CommandItemBase>> m_CommandItems = new Dictionary<Type, TGameObjectPool_Instance_Class<int, CommandItemBase>>();
    Action<bool> OnConsoleShow;
    protected CommandContainer AddCommandLine(KeyCode _keyCode = KeyCode.None) => m_CommandContainers.AddItem(m_totalCommands++).Init(_keyCode, CommandItemCreate, CommandItemRecycle);
    protected CommandItemBase CommandItemCreate(Type type) => m_CommandItems[type].AddItem(m_totalItems++);
    protected void CommandItemRecycle(CommandItemBase item) => m_CommandItems[item.GetType()].RemoveItem(item.m_Identity);
    void ConsoleAwake()
    {
        m_ConsoleCommandScrollRect = transform.Find("Command").GetComponent<ScrollRect>();
        m_CommandContainers = new TGameObjectPool_Instance_Class<int, CommandContainer>(m_ConsoleCommandScrollRect.transform.Find("Viewport/Content"), "GridItem");
        Transform containerItemPool = m_ConsoleCommandScrollRect.transform.Find("Viewport/CommandItemPool");
        UReflection.TraversalAllInheritedClasses<CommandItemBase>(type => m_CommandItems.Add(type, new TGameObjectPool_Instance_Class<int, CommandItemBase>(containerItemPool,type, type.Name)));

        m_ConsoleOpening = false;
        m_ConsoleCommandScrollRect.SetActive(m_ConsoleOpening);
    }
    void ConsoleReset(Action<bool> _OnConsoleShow)
    {
        OnConsoleShow = _OnConsoleShow;

        m_CommandContainers.Clear();
        m_totalItems = 0;
        m_totalCommands = 0;
    }


    void ConsoleTick(float _deltaTime)
    {
        m_CommandContainers.m_ActiveItemDic.Traversal( command => command.KeycodeTick());

        m_FastKeyCooldownTimer.Tick(_deltaTime);
        if (m_FastKeyCooldownTimer.m_Timing)
            return;
        if (Input.touchCount >= 5 || Input.GetKey(KeyCode.BackQuote))
        {
            m_FastKeyCooldownTimer.Replay();
            m_ConsoleOpening = !m_ConsoleOpening;
            m_ConsoleCommandScrollRect.SetActive(m_ConsoleOpening);
            OnConsoleShow?.Invoke(m_ConsoleOpening);

            Time.timeScale = m_ConsoleOpening ? m_Data.m_ConsoleTimeScale.m_RefValue : 1f;
            UpdateLogs();
            UpdateCommandData();
        }
    }
    void UpdateCommandData()=> m_CommandContainers.m_ActiveItemDic.Traversal(command => command.UpdateItems());

    public class CommandContainer : CGameObjectPool_Instance_Class<int>
    {
        #region Predefine Classes
        #endregion
        List<CommandItemBase> m_Items  = new List<CommandItemBase>();
        public CommandContainer(Transform _transform) : base(_transform) { }
        public int m_PageIndex { get; private set; }
        public KeyCode m_KeyCode { get; private set; }
        Func<Type, CommandItemBase> CreateItem;
        Action<CommandItemBase> RecycleItem;
        public CommandContainer Init(KeyCode _keyCode, Func<Type, CommandItemBase> _CreateItem, Action<CommandItemBase> _RecycleItem)
        {
            m_KeyCode = _keyCode;
            CreateItem = _CreateItem;
            RecycleItem = _RecycleItem;
            return this;
        }
        public void KeycodeTick()
        {
            if (m_KeyCode == KeyCode.None)
                return;

            if (Input.GetKeyDown(m_KeyCode))
                m_Items.Traversal(item => item.OnFastKeyTrigger());
        }
        public void UpdateItems() => m_Items.Traversal(item => item.OnDataUpdated?.Invoke());
        public T Insert<T>() where T : CommandItemBase
        {
            T item = CreateItem(typeof(T)) as T;
            item.transform.SetParent(transform);
            m_Items.Add(item);
            return item;
        }
        public override void OnRemoveItem()
        {
            base.OnRemoveItem();
            m_Items.Traversal(RecycleItem);
            m_Items.Clear();
            m_KeyCode = KeyCode.None;
        }
    }
    public class CommandItemBase : CGameObjectPool_Instance_Class<int>
    {
        public CommandItemBase(Transform _transform) : base(_transform) {  }
        public virtual void OnFastKeyTrigger() {  }
        public Action OnDataUpdated { get; private set; }
        public void SetDataUpdate(Action _OnDataUpdated)
        {
            OnDataUpdated = _OnDataUpdated;
            OnDataUpdated?.Invoke();
        }
    }
    public class CommandItem_FlagsSelection : CommandItemBase
    {
        TGameObjectPool_Component<int, Toggle> m_ToggleGrid;
        public CommandItem_FlagsSelection(Transform _transform) : base(_transform)
        {
            m_ToggleGrid = new TGameObjectPool_Component<int, Toggle>(_transform, "GridItem");
        }
        public void Play<T>(T defaultValue, Action<T> _OnFlagChanged) where T : Enum
        {
            m_ToggleGrid.Clear();
            foreach(T enumValue in UCommon.GetEnumValues<T>())
            {
                Toggle tog = m_ToggleGrid.AddItem(Convert.ToInt32(enumValue));
                tog.isOn = defaultValue.IsFlagEnable(enumValue);
                tog.GetComponentInChildren<Text>().text = enumValue.ToString();
                tog.onValueChanged.RemoveAllListeners();
                tog.onValueChanged.AddListener(changed => {
                    int totalIndex = 0;
                    m_ToggleGrid.m_ActiveItemDic.Traversal((index, toggle) => totalIndex += (toggle.isOn ? index : 0));
                    _OnFlagChanged((T)Enum.ToObject(typeof(T), totalIndex));
                });
            }
        }
    }
    public class CommandItem_ButtonSelection : CommandItemBase
    {
        public int m_DefaultValue;
        TGameObjectPool_Component<int, Button> m_ButtonGrid;
        public CommandItem_ButtonSelection(Transform _transform) : base(_transform)
        {
            m_ButtonGrid = new TGameObjectPool_Component<int, Button>(_transform, "GridItem");
        }
        public void Play(object _defaultValue,Action<object> _OnClick)
        {
            m_DefaultValue = Convert.ToInt32(_defaultValue);
            m_ButtonGrid.Clear();
            foreach(var enumObj in UCommon.GetEnumValues(_defaultValue.GetType()))
            {
                int index = Convert.ToInt32(enumObj);
                Button btn = m_ButtonGrid.AddItem(index);
                btn.onClick.RemoveAllListeners();
                btn.GetComponentInChildren<Text>().text = enumObj.ToString();
                btn.onClick.AddListener(() => _OnClick(index));
            }
        }
        public void Play(List<string> values,int defaultValue, Action<int> OnClick)
        {
            m_DefaultValue = defaultValue;
            m_ButtonGrid.Clear();
            values.Traversal((int index, string temp) =>
            {
                Button btn = m_ButtonGrid.AddItem(index);
                btn.onClick.RemoveAllListeners();
                btn.GetComponentInChildren<Text>().text = temp.ToString();
                btn.onClick.AddListener(() => OnClick(index));
            });
        }
    }
    public class CommandItem_Header : CommandItemBase
    {
        public Text m_HeaderTitle { get; private set; }
        public CommandItem_Header(Transform _transform) : base(_transform)
        {
            m_HeaderTitle = _transform.Find("Title").GetComponent<Text>();
        }
    }
    public class CommandItem_CommandTitle : CommandItemBase
    {
        public Text m_CommandTitle { get; private set; }
        public CommandItem_CommandTitle(Transform _transform) : base(_transform)
        {
            m_CommandTitle = _transform.Find("Title").GetComponent<Text>();
        }
    }
    public class CommandItem_Toggle : CommandItemBase
    {
        public Toggle m_Toggle { get; private set; }
        public Text m_ToggleTitle { get; private set; }
        public CommandItem_Toggle(Transform _transform) : base(_transform)
        {
            m_Toggle = transform.GetComponent<Toggle>();
            m_ToggleTitle = _transform.Find("Title").GetComponent<Text>();
        }
        public override void OnAddItem(int identity)
        {
            base.OnAddItem(identity);
            m_Toggle.onValueChanged.RemoveAllListeners();
        }
        public override void OnRemoveItem()
        {
            base.OnRemoveItem();
            m_Toggle.onValueChanged.RemoveAllListeners();
        }
        public override void OnFastKeyTrigger()
        {
            base.OnFastKeyTrigger();
            m_Toggle.isOn = !m_Toggle.isOn;
            m_Toggle.onValueChanged.Invoke(m_Toggle.isOn);
        }
    }
    public class CommandItem_Slider : CommandItemBase
    {
        public Slider m_Slider { get; private set; }
        public Text m_Value { get; private set; }
        public CommandItem_Slider(Transform _transform) : base(_transform)
        {
            m_Slider = transform.Find("Slider").GetComponent<Slider>();
            m_Value = transform.Find("Value").GetComponent<Text>();
        }
        public override void OnAddItem(int identity)
        {
            base.OnAddItem(identity);
            m_Slider.onValueChanged.RemoveAllListeners();
        }
        public override void OnRemoveItem()
        {
            base.OnRemoveItem();
            m_Slider.onValueChanged.RemoveAllListeners();
        }
    }
    public class CommandItem_Button : CommandItemBase
    {
        public Button m_Button { get; private set; }
        public Text m_ButtonTitle { get; private set; }
        public CommandItem_Button(Transform _transform) : base(_transform)
        {
            m_Button = _transform.GetComponent<Button>();
            m_ButtonTitle = _transform.Find("Title").GetComponent<Text>();
        }
        public override void OnAddItem(int identity)
        {
            base.OnAddItem(identity);
            m_Button.onClick.RemoveAllListeners();
            m_ButtonTitle.text = "";
        }
        public override void OnRemoveItem()
        {
            base.OnRemoveItem();
            m_Button.onClick.RemoveAllListeners();
        }
        public override void OnFastKeyTrigger()
        {
            base.OnFastKeyTrigger();
            m_Button.onClick.Invoke();
        }
    }

    public class CommandItem_InputField : CommandItemBase
    {
        public InputField m_InputField { get; private set; }
        public CommandItem_InputField(Transform _transform) : base(_transform)
        {
            m_InputField = _transform.GetComponent<InputField>();
        }
        public override void OnInitItem(Action<int> DoRecycle)
        {
            base.OnInitItem(DoRecycle);
            m_InputField.onValueChanged.RemoveAllListeners();
        }

    }
}
#endregion
#region LogPanel&FPS
public partial class UIT_TouchConsole : SingletonMono<UIT_TouchConsole>
{
    [Flags]
    public enum enum_ConsoleSetting
    {
        LogPanel = 1,
        LogTrack = 2,
        FPS = 4,
    }
    static string GetLogHexColor(LogType type)
    {
        switch (type)
        {
            case LogType.Log: return "00FF28";
            case LogType.Warning: return "FFA900";
            case LogType.Exception:
            case LogType.Error: return "FF0900";
            case LogType.Assert:
            default: return "00E5FF";
        }
    }

    #region External Class
    struct LogData
    {
        public int m_Time;
        public string m_LogInfo;
        public string m_LogTrace;
        public LogType m_LogType;
    }

    class LogItem : CGameObjectPool_Instance_Class<int>
    {
        LogData m_Data;
        Button m_Stack;
        Image m_Type;
        Text m_Info;
        public LogItem(Transform _transform) : base(_transform)
        {
            m_Type = transform.Find("Type").GetComponent<Image>();
            m_Info = transform.Find("Message").GetComponent<Text>();
            m_Stack = transform.GetComponent<Button>();
        }

        public LogItem Init(LogData _data, Action<LogData> OnStackClick)
        {
            m_Data = _data;
            m_Stack.onClick.RemoveAllListeners();
            m_Stack.onClick.AddListener(() => OnStackClick(m_Data));
            return this;
        }
        public void SetData(bool _collapse,int _count=0)
        {
            m_Type.color = UColor.HEXtoColor(GetLogHexColor(m_Data.m_LogType));
            m_Info.text = m_Data.m_LogInfo;
            if (_collapse)
                m_Info.text =string.Format("{0} {1}",_count,  m_Data.m_LogInfo);
            else
                m_Info.text = string.Format("{0} {1}",UTime.GetDateTime( m_Data.m_Time).ToShortTimeString(), m_Data.m_LogInfo);
        }

    }

    class LogToggle
    {
        public Toggle m_Toggle;
        public Text m_Value;
        public LogToggle(Transform _transform, Ref<bool> _filtered, Action OnValueChanged)
        {
            m_Toggle = _transform.GetComponent<Toggle>();
            m_Value = _transform.Find("Value").GetComponent<Text>();
            m_Toggle.isOn = _filtered;
            m_Toggle.onValueChanged.AddListener(value => { _filtered.SetValue(value); OnValueChanged(); });
        }
        public void Set(int count) => m_Value.text = count.ToString();
    }

    class StackPanel
    {
        public Transform transform { get; private set; }
        Text m_Info;
        Text m_Track;
        Text m_Time;
        Image m_Type;
        Button m_Exit;
        public StackPanel(Transform _transform)
        {
            transform = _transform;
            m_Info = transform.Find("Info").GetComponent<Text>();
            m_Track = transform.Find("TrackScrollRect/Viewport/Track").GetComponent<Text>();
            m_Time = transform.Find("Time").GetComponent<Text>();
            m_Type = transform.Find("Type").GetComponent<Image>();
            m_Exit = transform.Find("Exit").GetComponent<Button>();
            m_Exit.onClick.AddListener(HideStack);
            HideStack();
        }

        public void ShowTrack(LogData _data)
        {
            m_Type.color = UColor.HEXtoColor(GetLogHexColor(_data.m_LogType));
            m_Info.text = _data.m_LogInfo;
            m_Time.text = UTime.GetDateTime(_data.m_Time).ToLongTimeString();
            m_Track.text = _data.m_LogTrace;
            transform.SetActive(true);
        }
        public void HideStack() => transform.SetActive(false);
    }
    #endregion
    Transform m_FrameRate;
    Text m_FrameRateValue;
    Queue<LogData> m_LogDataQueue = new Queue<LogData>();

    ScrollRect m_LogPanelRect;
    RectTransform m_LogFilter;
    LogToggle m_FilterLog, m_FilterWarning, m_FilterError,m_FilterCollapse;
    TGameObjectPool_Instance_Class<int, LogItem> m_Logs;
    StackPanel m_Stack;
    Queue<int> m_FrameRateQueue = new Queue<int>();
    public void LogFrameAwake()
    {
        m_FrameRate = transform.Find("FrameRate");
        m_FrameRateValue = m_FrameRate.Find("Value/Value").GetComponent<Text>();

        m_LogPanelRect = transform.Find("LogPanel").GetComponent<ScrollRect>();
        Transform rightContent = m_LogPanelRect.transform.Find("Viewport/Content");
        m_LogFilter = rightContent.Find("LogFilter") as RectTransform;
        m_FilterLog = new LogToggle(m_LogFilter.Find("Log"), m_Data.m_Log , OnLogToggled);
        m_FilterWarning = new LogToggle(m_LogFilter.Find("Warning"), m_Data.m_Warning, OnLogToggled);
        m_FilterError = new LogToggle(m_LogFilter.Find("Error"), m_Data.m_Error, OnLogToggled);
        m_FilterCollapse = new LogToggle(m_LogFilter.Find("Collapse"), m_Data.m_Collapse, OnLogToggled);
        m_Logs = new TGameObjectPool_Instance_Class<int, LogItem>(m_LogPanelRect.transform.Find("Viewport/Content"), "LogItem");

        m_Stack = new StackPanel(transform.Find("Stack"));

        m_LogFilter.Find("Clear").GetComponent<Button>().onClick.AddListener(ClearConsoleLog);
    }
    public void LogFrameReset()
    {
        m_FrameRateQueue.Clear();
        m_FrameRateValue.text = "";

        m_Stack.HideStack();
        ClearConsoleLog();
    }

    public void ClearConsoleLog()
    {
        m_LogDataQueue.Clear();
        UpdateLogs();
    }
    private void OnEnable()
    {
        Application.logMessageReceived += OnLogReceived;
    }
    private void OnDisbable()
    {
        Application.logMessageReceived -= OnLogReceived;
    }

    public void LogFrame(float _deltaTime)
    {
        m_FrameRateQueue.Enqueue(Mathf.CeilToInt(1f / _deltaTime));
        if (m_FrameRateQueue.Count > 30)
            m_FrameRateQueue.Dequeue();

        int total = 0;
        foreach (var frameRate in m_FrameRateQueue)
            total += frameRate;
        total /= m_FrameRateQueue.Count;

        m_FrameRateValue.text = total.ToString();
    }

    void OnLogReceived(string info, string trace, LogType type)
    {
        m_LogDataQueue.Enqueue(new LogData() { m_Time = UTime.GetTimeStampNow(), m_LogInfo = info, m_LogTrace = trace, m_LogType = type });
        UpdateLogs();
    }
    void OnLogToggled()
    {
        m_Data.SavePersistentData();
        UpdateLogs();
    }

    void UpdateLogs()
    {
        int logCount = 0;
        int warningCount = 0;
        int errorCount = 0;
        foreach (var logInfo in m_LogDataQueue)
        {
            bool validateLog = false;
            switch (logInfo.m_LogType)
            {
                case LogType.Log:
                    {
                        validateLog = m_FilterLog.m_Toggle.isOn;
                        logCount++;
                    }
                    break;
                case LogType.Warning:
                    {
                        validateLog = m_FilterWarning.m_Toggle.isOn;
                        warningCount++;
                    }
                    break;
                case LogType.Error:
                case LogType.Exception:
                case LogType.Assert:
                    {
                        validateLog = m_FilterError.m_Toggle.isOn;
                        errorCount++;
                    }
                    break;
            }
            if (!validateLog)
                continue;
        }
        m_FilterLog.Set(logCount);
        m_FilterWarning.Set(warningCount);
        m_FilterError.Set(errorCount);

        m_Logs.Clear();
        if (!(m_ConsoleOpening && m_Data.m_FilterSetting.m_RefValue.IsFlagEnable(enum_ConsoleSetting.LogTrack)))
            return;
        if(!m_Data.m_Collapse)
        {
            foreach (var logData in m_LogDataQueue)
                m_Logs.AddItem(m_Logs.Count).Init(logData, m_Stack.ShowTrack).SetData(false, 0);
        }
        else
        {
            Dictionary<LogData, int> logCollapses = new Dictionary<LogData, int>();
            foreach (var logData in m_LogDataQueue)
            {
                var collapseData = logCollapses.Keys.Find(p => p.m_LogInfo == logData.m_LogInfo && p.m_LogType == logData.m_LogType && p.m_LogTrace == logData.m_LogTrace);
                if (collapseData.m_Time != 0)
                    logCollapses[collapseData]++;
                else
                    logCollapses.Add(logData,1);
            }
            foreach (var logCollapse in logCollapses)
                m_Logs.AddItem(m_Logs.Count).Init(logCollapse.Key,m_Stack.ShowTrack).SetData(true,logCollapse.Value);
        }

        m_Logs.Sort((a, b) => b.Key - a.Key);
        LayoutRebuilder.ForceRebuildLayoutImmediate(m_LogPanelRect.transform as RectTransform);
    }
}
#endregion