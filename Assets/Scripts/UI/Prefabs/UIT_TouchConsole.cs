using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UIT_TouchConsole;
public partial class UIT_TouchConsole : SingletonMono<UIT_TouchConsole>
{
    #region Helper
    public static void Init(Action<bool> _OnConsoleShow = null) => Instance.InitConsole(_OnConsoleShow);
    public static void EmptyLine() => Instance.AddCommandLine();
    public static void Header(string _title) => Instance.AddCommandLine().Insert<CommandItem_Header>().m_HeaderTitle.text = _title;
    public static CommandContainer Command(string _title, KeyCode _keyCode = KeyCode.None)
    {
        CommandContainer container = Instance.AddCommandLine(_keyCode);
        container.Insert<CommandItem_CommandTitle>().m_CommandTitle.text = _title;
        return container;
    }
    #endregion

    [Range(0, 2f)] public readonly Ref<float> m_ConsoleTimeScale = .5f;
    protected override void Awake()
    {
        base.Awake();
        ConsoleAwake();
        LogFrameAwake();

        SetConsoleTimeScale(m_ConsoleTimeScale.Value);
        SetLogFramePanel(m_RightPanelSetting.Value);
    }
    protected UIT_TouchConsole InitConsole(Action<bool> _OnConsoleShow)
    {
        ConsoleReset(_OnConsoleShow);
        LogFrameReset();

        Header("Console");
        Command("Time Scale").Slider(m_ConsoleTimeScale,  SetConsoleTimeScale, 0f, 2f);
        Command("Right Panel").FlagsSelection(m_RightPanelSetting, SetLogFramePanel);
        Command("Clear Log").Button(ClearConsoleLog);
        EmptyLine();
        return this;
    }

    private void Update()
    {
        float deltaTime = Time.unscaledDeltaTime;
        ConsoleTick(deltaTime);

        if (m_RightPanelSetting.Value.IsFlagEnable(enum_RightPanel.FPS))
            LogFrame(deltaTime);
    }

    #region Miscs
    protected void SetConsoleTimeScale(float _timeScale)
    {
        if (!m_ConsoleOpening)
            return;
        Time.timeScale = _timeScale;
    }
    protected void SetLogFramePanel(enum_RightPanel _panelSetting)
    {
        m_FrameRate.SetActive(_panelSetting.IsFlagEnable(enum_RightPanel.FPS));
        m_LogFilter.SetActive(_panelSetting.IsFlagEnable(enum_RightPanel.LogFilter));
        m_RightPanelRect.SetActive(!_panelSetting.IsFlagClear());
        UpdateLogs();
    }
    #endregion
}
//Console
public static class UIT_TouchConsoleHelper
{
    public static string GetKeyCodeString(this KeyCode _keyCode) => _keyCode == KeyCode.None ? "" : _keyCode.ToString();

    public static void Button(this CommandContainer _container, Action OnClick)
    {
        CommandItem_Button button = _container.Insert<CommandItem_Button>();
        button.m_Button.onClick.AddListener(() => OnClick());
        button.m_ButtonTitle.text = _container.m_KeyCode.GetKeyCodeString();
    }
    public static void Toggle(this CommandContainer _container, Action<bool> OnToggleChange, bool startValue = false)
    {
        CommandItem_Toggle toggle = _container.Insert<CommandItem_Toggle>();
        toggle.m_Toggle.onValueChanged.AddListener(value => OnToggleChange(value));
        toggle.m_Toggle.isOn = startValue;
        toggle.m_ToggleTitle.text = _container.m_KeyCode.GetKeyCodeString();
    }
    public static void Toggle(this CommandContainer _container,Ref<bool> _refValue,Action<bool> OnToggleChange)
    {
        CommandItem_Toggle toggle = _container.Insert<CommandItem_Toggle>();
        toggle.SetDataUpdate(() => toggle.m_Toggle.isOn=_refValue.Value);
        toggle.m_Toggle.onValueChanged.AddListener(value => {
            _refValue.Value = value;
            OnToggleChange(value);
        });
        toggle.m_ToggleTitle.text = _container.m_KeyCode.GetKeyCodeString();
    }
    public static void Slider(this CommandContainer _container, float _maxValue, Action<float> OnValueChanged) => Slider(_container, 0, OnValueChanged, 0, _maxValue);
    public static void Slider(this CommandContainer _container, Ref<float> _refValue, Action<float> _SetValue, float _minValue, float _maxValue)
    {
        CommandItem_Slider slider = _container.Insert<CommandItem_Slider>();
        slider.SetDataUpdate(() => {
            if (!_refValue)
                return;
            float finalValue = _refValue.Value;
            slider.m_SliderComponent.value = Mathf.InverseLerp(_minValue, _maxValue, finalValue);
            slider.m_SliderValue.text = string.Format("{0:0.0}", finalValue);
        });
        slider.m_SliderComponent.onValueChanged.AddListener(value => {
            float finalValue = Mathf.Lerp(_minValue, _maxValue, value);
            slider.m_SliderValue.text = string.Format("{0:0.0}", finalValue);
            _refValue.Value = finalValue;
            _SetValue(finalValue);
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
    public static void EnumSelection<T>(this CommandContainer _container,Ref<T> _valueRef, Action<T> OnClick, bool foldOut = true) where T : struct, Enum
    {
        CommandItem_ButtonSelection selection = _container.ButtonFoldOutItem<CommandItem_ButtonSelection>(foldOut, out CommandItem_Button foldOutButton);
        selection. SetDataUpdate(() => {
            selection.Play(_valueRef.Value, (int value) => {
                T selectEnum = (T)Enum.ToObject(typeof(T), value);
                OnClick(selectEnum);
                if (foldOutButton != null)
                    foldOutButton.m_ButtonTitle.text = selectEnum.ToString();
            });
            if (foldOutButton != null)
                foldOutButton.m_ButtonTitle.text = _valueRef.Value.ToString();
        });
    }
    public static void EnumSelection(this CommandContainer _container, Ref<int> _refEnum, List<string> _values, Action<string> OnClick,bool foldOut=true)
    {
        CommandItem_ButtonSelection selection = _container.ButtonFoldOutItem<CommandItem_ButtonSelection>(foldOut, out CommandItem_Button foldOutButton);
        selection.SetDataUpdate(() => {
            selection.Play(_values, _refEnum.Value, (int value) => {
                OnClick(_values[value]);
                if (foldOutButton != null)
                    foldOutButton.m_ButtonTitle.text = _values[value];
            });
            if (foldOutButton != null)
                foldOutButton.m_ButtonTitle.text = _values[_refEnum.Value];
        });
    }
    public static void FlagsSelection<T>(this CommandContainer _container, Ref<T> _refFlags, Action<T> _logFilter,bool foldOut=true) where T :struct, Enum
    {
        CommandItem_FlagsSelection selection= _container.ButtonFoldOutItem<CommandItem_FlagsSelection>(foldOut, out CommandItem_Button foldOutButton);
        selection.SetDataUpdate(() => selection.Play(_refFlags.Value, flags => {
            if (foldOutButton != null)
                foldOutButton.m_ButtonTitle.text = flags.GetNumerable().ToString_Readable('|', value => value ? "√" : "×");
            _logFilter(flags);
        }));
        if (foldOutButton != null)
            foldOutButton.m_ButtonTitle.text = _refFlags.Value.GetNumerable().ToString_Readable('|', value => value ? "√" : "×");
    }
    public static void InputField(this CommandContainer _container, Ref<string> _refText, Action<string> OnValueClick)
    {
        CommandItem_InputField input = _container.Insert<CommandItem_InputField>();
        input.SetDataUpdate(() => { input.m_InputField.text = _refText.Value; });
        input.m_InputField.onValueChanged.AddListener(value => { _refText.Value = value; });
        _container.Button(() => OnValueClick(input.m_InputField.text));
    }
    public static void InpuptField(this CommandContainer _container, Ref<string> _refText1, Ref<string> _refText2, Action<string, string> OnValueClick)
    {
        CommandItem_InputField input1 = _container.Insert<CommandItem_InputField>();
        CommandItem_InputField input2 = _container.Insert<CommandItem_InputField>();
        input1.SetDataUpdate(() => input1.m_InputField.text = _refText1.Value);
        input2.SetDataUpdate(() => input2.m_InputField.text = _refText2.Value);
        input1.m_InputField.onValueChanged.AddListener(value => { _refText1.Value = value; });
        input2.m_InputField.onValueChanged.AddListener(value => { _refText2.Value = value; });
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
        TReflection.TraversalAllInheritedClasses<CommandItemBase>(type => m_CommandItems.Add(type, new TGameObjectPool_Instance_Class<int, CommandItemBase>(containerItemPool,type, type.Name)));

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
            
            Time.timeScale = m_ConsoleOpening ? m_ConsoleTimeScale.Value : 1f;
            UpdateLogs();
            UpdateCommandData();
        }
        if (Input.GetKeyDown(KeyCode.Alpha0))
            m_ConsoleTimeScale.Value = 2f;
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
            UCommon.TraversalEnum<T>(value => {
                Toggle tog = m_ToggleGrid.AddItem(Convert.ToInt32(value));
                tog.isOn = defaultValue.IsFlagEnable(value);
                tog.GetComponentInChildren<Text>().text = value.ToString();
                tog.onValueChanged.RemoveAllListeners();
                tog.onValueChanged.AddListener(changed => {
                    int totalIndex = 0;
                    m_ToggleGrid.m_ActiveItemDic.Traversal((index, toggle) => totalIndex += (toggle.isOn ? index : 0));
                    _OnFlagChanged((T)Enum.ToObject(typeof(T), totalIndex));
                });
            });
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
        public void Play<T>(T _defaultValue, Action<int> _OnClick) where T : Enum
        {
            m_DefaultValue = Convert.ToInt32(_defaultValue);
            m_ButtonGrid.Clear();
            UCommon.TraversalEnum<T>(temp =>
            {
                int index = Convert.ToInt32(temp);
                Button btn = m_ButtonGrid.AddItem(index);
                btn.onClick.RemoveAllListeners();
                btn.GetComponentInChildren<Text>().text = temp.ToString();
                btn.onClick.AddListener(() => _OnClick(index));
            });
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
        public Slider m_SliderComponent { get; private set; }
        public Text m_SliderValue { get; private set; }
        public CommandItem_Slider(Transform _transform) : base(_transform)
        {
            m_SliderComponent = transform.Find("Slider").GetComponent<Slider>();
            m_SliderValue = transform.Find("Value").GetComponent<Text>();
        }
        public override void OnAddItem(int identity)
        {
            base.OnAddItem(identity);
            m_SliderComponent.onValueChanged.RemoveAllListeners();
        }
        public override void OnRemoveItem()
        {
            base.OnRemoveItem();
            m_SliderComponent.onValueChanged.RemoveAllListeners();
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
//Right Panel
public partial class UIT_TouchConsole : SingletonMono<UIT_TouchConsole>
{
    [Flags]
    public enum enum_RightPanel
    {
        LogFilter = 1,
        FPS = 2,
        LogItem = 4,
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

        public void Init(LogData _data, Action<LogData> OnStackClick)
        {
            m_Data = _data;
            m_Type.color = UColor.HEXtoColor(GetLogHexColor(m_Data.m_LogType));
            m_Info.text = m_Data.m_LogInfo;
            m_Stack.onClick.RemoveAllListeners();
            m_Stack.onClick.AddListener(() => OnStackClick(m_Data));
        }

    }

    class LogToggle
    {
        public Toggle m_Toggle;
        public Text m_Value;
        public LogToggle(Transform _transform, bool _filtered, Action OnValueChanged)
        {
            m_Toggle = _transform.GetComponent<Toggle>();
            m_Toggle.isOn = _filtered;
            m_Value = _transform.Find("Value").GetComponent<Text>();
            m_Toggle.onValueChanged.AddListener(value => OnValueChanged());
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

    public Ref<enum_RightPanel> m_RightPanelSetting = (enum_RightPanel)int.MaxValue;
    public bool m_LogFiltered = false, m_WarningFiltered = true, m_ErrorFiltered = true;

    Transform m_FrameRate;
    Text m_FrameRateValue;
    Queue<LogData> m_LogDataQueue = new Queue<LogData>();

    ScrollRect m_RightPanelRect;
    RectTransform m_LogFilter;
    LogToggle m_FilterLog, m_FilterWarning, m_FilterError;
    TGameObjectPool_Instance_Class<int, LogItem> m_Logs;
    StackPanel m_Stack;
    Queue<int> m_FrameRateQueue = new Queue<int>();
    public void LogFrameAwake()
    {
        m_RightPanelRect = transform.Find("RightPanel").GetComponent<ScrollRect>();
        Transform rightContent = m_RightPanelRect.transform.Find("Viewport/Content");
        m_LogFilter = rightContent.Find("LogFilter") as RectTransform;
        m_FilterLog = new LogToggle(m_LogFilter.Find("Log"), m_LogFiltered, UpdateLogs);
        m_FilterWarning = new LogToggle(m_LogFilter.Find("Warning"), m_WarningFiltered, UpdateLogs);
        m_FilterError = new LogToggle(m_LogFilter.Find("Error"), m_ErrorFiltered, UpdateLogs);
        m_FrameRate = rightContent.Find("FrameRate");
        m_FrameRateValue = m_FrameRate.Find("Value/Value").GetComponent<Text>();
        m_Logs = new TGameObjectPool_Instance_Class<int, LogItem>(m_RightPanelRect.transform.Find("Viewport/Content"), "LogItem");

        m_Stack = new StackPanel(transform.Find("Stack"));
    }
    public void LogFrameReset()
    {
        m_FrameRateQueue.Clear();
        m_FrameRateValue.text = "";

        m_Stack.HideStack();
        ClearConsoleLog();
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

    int m_ErrorCount, m_WarningCount, m_LogCount;
    void OnLogReceived(string info, string trace, LogType type)
    {
        m_LogDataQueue.Enqueue(new LogData() { m_Time = UTime.GetTimeStampNow(), m_LogInfo = info, m_LogTrace = trace, m_LogType = type });
        switch (type)
        {
            case LogType.Exception:
            case LogType.Error: m_ErrorCount++; break;
            case LogType.Warning: m_WarningCount++; break;
            case LogType.Log: m_LogCount++; break;
        }
        UpdateLogs();
    }

    void UpdateLogs()
    {
        m_FilterLog.Set(m_LogCount);
        m_FilterWarning.Set(m_WarningCount);
        m_FilterError.Set(m_ErrorCount);

        m_Logs.Clear();
        if (!m_ConsoleOpening || !m_RightPanelSetting.Value.IsFlagEnable(enum_RightPanel.LogItem))
            return;
        foreach (var logInfo in m_LogDataQueue)
        {
            bool validateLog = false;
            switch (logInfo.m_LogType)
            {
                case LogType.Warning:
                    validateLog = m_FilterWarning.m_Toggle.isOn;
                    break;
                case LogType.Error:
                case LogType.Exception:
                case LogType.Assert:
                    validateLog = m_FilterError.m_Toggle.isOn;
                    break;
                case LogType.Log:
                    validateLog = m_FilterLog.m_Toggle.isOn;
                    break;
            }
            if (!validateLog)
                continue;
            m_Logs.AddItem(m_Logs.Count).Init(logInfo, m_Stack.ShowTrack);
        }
        m_Logs.Sort((a, b) => a.Key - b.Key);
        LayoutRebuilder.ForceRebuildLayoutImmediate(m_RightPanelRect.transform as RectTransform);
    }

    public void ClearConsoleLog()
    {
        m_LogDataQueue.Clear();
        m_ErrorCount = 0;
        m_WarningCount = 0;
        m_LogCount = 0;
        UpdateLogs();
    }
}