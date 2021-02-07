using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public partial class UIT_TouchConsole : SingletonMono<UIT_TouchConsole> {

    #region Helper
    public static void Init(Action<bool> _OnConsoleShow = null)=>Instance.InitConsole(_OnConsoleShow);
    public static void Header(string _title) => Instance.AddCommandLine().Header(_title);
    public static void EmptyLine() => Instance.AddCommandLine().EmptyLine();
    public static CommandItem Command(string _title) => Instance.AddCommandLine().Command(_title);
    #endregion
    public bool m_ConsoleOpening { get; private set; } = false;

    [Range(0,2f)] public float m_ConsoleTimeScale = .5f;
    public enum_RightPanel m_RightPanelSetting = (enum_RightPanel)int.MaxValue;
    public bool m_LogFiltered = false, m_WarningFiltered = true, m_ErrorFiltered = true;

    Transform m_FrameRate;
    Text m_FrameRateValue;
    ScrollRect m_ConsoleCommandScrollRect;
    TGameObjectPool_Instance_Class<int, CommandItem> m_Commands;
    Action<bool> OnConsoleShow;

    ScrollRect m_RightPanelRect;
    RectTransform m_LogFilter;
    LogToggle m_FilterLog, m_FilterWarning, m_FilterError;
    TGameObjectPool_Instance_Class<int, LogItem> m_Logs;
    StackPanel m_Stack;

    Queue<LogData> m_LogDataQueue = new Queue<LogData>();
    Timer m_FastKeyCooldownTimer = new Timer(.5f);
    Queue<int> m_FrameRateQueue = new Queue<int>();
    protected override void Awake()
    {
        base.Awake();

        m_ConsoleCommandScrollRect = transform.Find("Command").GetComponent<ScrollRect>();
        m_Commands = new TGameObjectPool_Instance_Class<int, CommandItem>(m_ConsoleCommandScrollRect.transform.Find("Viewport/Content"), "GridItem");

        m_RightPanelRect = transform.Find("RightPanel").GetComponent<ScrollRect>();
        Transform rightContent = m_RightPanelRect.transform.Find("Viewport/Content");
        m_LogFilter = rightContent.Find("LogFilter") as RectTransform;
        m_FilterLog = new LogToggle(m_LogFilter.Find("Log"),m_LogFiltered,UpdateLogs);
        m_FilterWarning = new LogToggle(m_LogFilter.Find("Warning"),m_WarningFiltered,UpdateLogs);
        m_FilterError = new LogToggle(m_LogFilter.Find("Error"),m_ErrorFiltered,UpdateLogs);
        m_FrameRate = rightContent.Find("FrameRate");
        m_FrameRateValue = m_FrameRate.Find("Value/Value").GetComponent<Text>();
        m_Logs = new TGameObjectPool_Instance_Class<int, LogItem>(m_RightPanelRect.transform.Find("Viewport/Content"),"LogItem");

        m_Stack = new StackPanel(transform.Find("Stack"));

        m_ConsoleOpening = false;
        m_ConsoleCommandScrollRect.SetActive(m_ConsoleOpening);

        SetConsoleTimeScale(m_ConsoleTimeScale);
        SetRightPanel(m_RightPanelSetting);
    }
    protected UIT_TouchConsole InitConsole(Action<bool> _OnConsoleShow)
    {
        OnConsoleShow = _OnConsoleShow;

        m_FrameRateQueue.Clear();
        m_FrameRateValue.text = "";

        ClearConsoleLog();
        m_Stack.HideStack();

        m_Commands.Clear();
        Header("Console");
        Command("Time Scale").Slider(m_ConsoleTimeScale, 0f, 2f, SetConsoleTimeScale);
        Command("Right Panel").EnumFlagsSelection(m_RightPanelSetting,SetRightPanel);
        Command("Clear Log").Button(ClearConsoleLog);
        EmptyLine();
        return this;
    }

    private void Update()
    {
        m_Commands.m_ActiveItemDic.Traversal((CommandItem command) => { command.KeycodeTick(); });

        m_FastKeyCooldownTimer.Tick(Time.unscaledDeltaTime);
        if (m_FastKeyCooldownTimer .m_Timing)
            return;
        if (Input.touchCount >= 5 || Input.GetKey(KeyCode.BackQuote))
        {
            m_FastKeyCooldownTimer.Replay();
            m_ConsoleOpening = !m_ConsoleOpening;
            m_ConsoleCommandScrollRect.SetActive(m_ConsoleOpening);
            OnConsoleShow?.Invoke(m_ConsoleOpening);
            Time.timeScale = m_ConsoleOpening?m_ConsoleTimeScale:1f;
            UpdateLogs();
        }

        if(m_RightPanelSetting.IsFlagEnable(enum_RightPanel.FPS))
        {
            m_FrameRateQueue.Enqueue(Mathf.CeilToInt(1f / Time.unscaledDeltaTime));
            if (m_FrameRateQueue.Count > 30)
                m_FrameRateQueue.Dequeue();

            int total = 0;
            foreach (var frameRate in m_FrameRateQueue)
                total += frameRate;
            total /= m_FrameRateQueue.Count;

            m_FrameRateValue.text = total.ToString();
        }
    }
    #region Miscs
    protected void SetConsoleTimeScale(float _timeScale)
    {
        m_ConsoleTimeScale = _timeScale;
        if (!m_ConsoleOpening)
            return;
        Time.timeScale = m_ConsoleTimeScale;
    }
    protected void SetRightPanel(enum_RightPanel _panelSetting)
    {
        m_RightPanelSetting = _panelSetting;
        m_FrameRate.SetActive(m_RightPanelSetting.IsFlagEnable(enum_RightPanel.FPS));
        m_LogFilter.SetActive(m_RightPanelSetting.IsFlagEnable(enum_RightPanel.LogFilter));
        m_RightPanelRect.SetActive(!m_RightPanelSetting.IsFlagClear());
        UpdateLogs();
    }
    #endregion
}

//Console
public partial class UIT_TouchConsole : SingletonMono<UIT_TouchConsole>
{
    int m_totalCommands;
    protected CommandItem AddCommandLine() => m_Commands.AddItem(m_totalCommands++);
    public class CommandItem : CGameObjectPool_Instance_Class<int>
    {
        #region Predefine Classes
        public class ToggleSelection
        {
            public Transform transform { get; private set; }
            TGameObjectPool_Component<int, Toggle> m_ToggleGrid;
            public ToggleSelection(Transform _transform)
            {
                transform = _transform;
                m_ToggleGrid = new TGameObjectPool_Component<int, Toggle>(_transform.Find("Grid"), "GridItem");
            }
            public void Play<T>(T defaultValue, Action<T> _OnFlagChanged) where T : Enum
            {
                m_ToggleGrid.Clear();
                TCommon.TraversalEnum<T>(value => {
                    Toggle tog = m_ToggleGrid.AddItem(Convert.ToInt32(value));
                    tog.isOn = defaultValue.IsFlagEnable((T)value); ;
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
        public class ButtonSelection
        {
            public Transform transform { get; private set; }
            Text m_Text;
            TGameObjectPool_Component<int, Button> m_ButtonGrid;
            public ButtonSelection(Transform _transform)
            {
                transform = _transform;
                m_Text = _transform.Find("Text").GetComponent<Text>();
                m_ButtonGrid = new TGameObjectPool_Component<int, Button>(_transform.Find("Grid"), "GridItem");
                _transform.GetComponent<Button>().onClick.AddListener(() => {
                    m_ButtonGrid.transform.SetActive(!m_ButtonGrid.transform.gameObject.activeSelf);
                });
                m_ButtonGrid.transform.SetActive(false);
            }
            public void Play<T>(T _defaultValue, Action<int> _OnClick) where T : Enum
            {
                m_ButtonGrid.Clear();
                m_Text.text = _defaultValue.ToString();
                TCommon.TraversalEnum<T>(temp =>
                {
                    int index = Convert.ToInt32(temp);
                    Button btn = m_ButtonGrid.AddItem(index);
                    btn.onClick.RemoveAllListeners();
                    btn.GetComponentInChildren<Text>().text = temp.ToString();
                    btn.onClick.AddListener(() => {
                        m_Text.text = temp.ToString();
                        _OnClick(index);
                        m_ButtonGrid.transform.SetActive(false);
                    });
                });
            }
            public void Play(List<string> values, string defaultValue, Action<int> OnClick)
            {
                m_ButtonGrid.Clear();
                m_Text.text = defaultValue.ToString();
                m_ButtonGrid.Clear();
                values.Traversal((int index, string temp) =>
                {
                    Button btn = m_ButtonGrid.AddItem(index);
                    btn.onClick.RemoveAllListeners();
                    btn.GetComponentInChildren<Text>().text = temp.ToString();
                    btn.onClick.AddListener(() => {
                        m_Text.text = temp.ToString();
                        OnClick(index);
                        m_ButtonGrid.transform.SetActive(false);
                    });
                });
            }
        }
        #endregion
        Transform m_Header;
        Text m_HeaderTitle;
        Transform m_Command;
        Text m_CommandTitle;

        KeyCode m_KeyCode;
        InputField m_ValueInput1, m_ValueInput2;
        Button m_Button;
        Text m_ButtonTitle;
        ButtonSelection m_GridSelection;
        Toggle m_Toggle;
        Text m_ToggleTitle;
        ToggleSelection m_ToggleSelection;

        Transform m_Slider;
        Slider m_SliderComponent;
        Text m_SliderValue;
        public CommandItem(Transform _transform) : base(_transform)
        {
            m_Header = transform.Find("Header");
            m_HeaderTitle = m_Header.Find("Title").GetComponent<Text>();
            m_Command = transform.Find("Command");
            m_CommandTitle = transform.Find("Command/Title").GetComponent<Text>();
            m_ValueInput1 = transform.Find("Input1").GetComponent<InputField>();
            m_ValueInput2 = transform.Find("Input2").GetComponent<InputField>();
            m_Button = transform.Find("Button").GetComponent<Button>();
            m_ButtonTitle = transform.Find("Button/Title").GetComponent<Text>();
            m_GridSelection = new ButtonSelection(transform.Find("ButtonSelection"));
            m_Toggle = transform.Find("Toggle").GetComponent<Toggle>();
            m_ToggleTitle = transform.Find("Toggle/Title").GetComponent<Text>();
            m_ToggleSelection = new ToggleSelection(transform.Find("ToggleSelection"));
            m_Slider = transform.Find("Slider");
            m_SliderComponent = transform.Find("Slider/Slider").GetComponent<Slider>();
            m_SliderValue = transform.Find("Slider/Value").GetComponent<Text>();
        }
        public void KeycodeTick()
        {
            if (m_KeyCode == KeyCode.None)
                return;

            if (Input.GetKeyDown(m_KeyCode))
            {
                m_Button.onClick.Invoke();
                m_Toggle.isOn = !m_Toggle.isOn;
                m_Toggle.onValueChanged.Invoke(m_Toggle.isOn);
            }
        }
        public override void OnAddItem(int identity)
        {
            base.OnAddItem(identity);
            m_ValueInput1.SetActive(false);
            m_ValueInput2.SetActive(false);
            m_GridSelection.transform.SetActive(false);
            m_ToggleSelection.transform.SetActive(false);
            m_Button.SetActive(true);

            m_Header.SetActive(false);
            m_Command.SetActive(false);

            m_KeyCode = KeyCode.None;
            m_Button.SetActive(false);
            m_Button.onClick.RemoveAllListeners();

            m_Toggle.SetActive(false);
            m_Toggle.onValueChanged.RemoveAllListeners();

            m_Slider.SetActive(false);
        }
        public void EmptyLine()
        {

        }
        public void Header(string _title)
        {
            m_Header.SetActive(true);
            m_HeaderTitle.text = _title;
        }
        public CommandItem Command(string _title)
        {
            m_Command.SetActive(true);
            m_CommandTitle.text = _title;
            return this;
        }
        public void Button(Action OnClick, KeyCode _keyCode = KeyCode.None)
        {
            m_Button.SetActive(true);
            m_Button.onClick.AddListener(() => OnClick());
            m_KeyCode = _keyCode;
            m_ButtonTitle.text = _keyCode==KeyCode.None?"": _keyCode.ToString();
        }
        public void Toggle(Action<bool> OnToggleChange, bool defaultValue = false, KeyCode _keyCode = KeyCode.None)
        {
            m_Toggle.SetActive(true);
            m_Toggle.onValueChanged.AddListener((value) => OnToggleChange(value));
            m_Toggle.isOn = defaultValue;
            m_KeyCode = _keyCode;
            m_ToggleTitle.text = _keyCode == KeyCode.None ? "" : _keyCode.ToString();
        }
        int selectionIndex = -1;
        public void EnumSelection<T>(T _defaultEnum, Action<T> OnClick, KeyCode _keyCode = KeyCode.None) where T : Enum
        {
            m_GridSelection.transform.SetActive(true);
            selectionIndex = (int)Enum.ToObject(typeof(T), _defaultEnum);
            m_GridSelection.Play(_defaultEnum, (int value) => { selectionIndex = value; OnClick((T)Enum.ToObject(typeof(T), selectionIndex)); });
        }
        public void EnumSelection(int _defaultEnum, List<string> _values, Action<string> OnClick, KeyCode _keyCode = KeyCode.None)
        {
            m_GridSelection.transform.SetActive(true);
            selectionIndex = _defaultEnum;
            m_GridSelection.Play(_values, _values[_defaultEnum], (int value) => { selectionIndex = value; OnClick(_values[selectionIndex]); });
        }
        public void EnumSelection<T>(T _defaultEnum, string _defaultValue, Action<T, string> OnClick, KeyCode keyCode = KeyCode.None) where T : Enum
        {
            m_GridSelection.transform.SetActive(true);
            m_ValueInput1.SetActive(true);
            m_ValueInput1.text = _defaultValue;
            selectionIndex = (int)Enum.ToObject(typeof(T), _defaultEnum);
            m_GridSelection.Play(_defaultEnum, (int value) => { selectionIndex = value; OnClick((T)Enum.ToObject(typeof(T), selectionIndex), m_ValueInput1.text); });
        }
        public void EnumFlagsSelection<T>(T _defaultEnum, Action<T> _logFilter, KeyCode _keyCode = KeyCode.None) where T : Enum
        {
            m_ToggleSelection.transform.SetActive(false);
            m_ToggleSelection.Play(_defaultEnum, _logFilter);
            Button(() => m_ToggleSelection.transform.SetActive(!m_ToggleSelection.transform.gameObject.activeSelf), _keyCode);
        }
        public void InputField(string _defaultValue, Action<string> OnValueClick, KeyCode _keyCode = KeyCode.None)
        {
            m_ValueInput1.SetActive(true);
            m_ValueInput1.text = _defaultValue;
            Button(() => OnValueClick(m_ValueInput1.text), _keyCode);
        }
        public void InpuptField(string _defaultValue1, string _defaultValue2, Action<string, string> OnValueClick, KeyCode _keyCode = KeyCode.None)
        {
            m_ValueInput1.SetActive(true);
            m_ValueInput2.SetActive(true);
            m_ValueInput1.text = _defaultValue1;
            m_ValueInput2.text = _defaultValue2;
            Button(() => OnValueClick(m_ValueInput1.text, m_ValueInput2.text), _keyCode);
        }
        public void Slider(float _maxValue, Action<float> OnValueChanged) => Slider(0, 0, _maxValue, OnValueChanged);
        public void Slider(float _startValue, float _minValue, float _maxValue, Action<float> OnValueChanged)
        {
            m_Slider.SetActive(true);
            m_SliderComponent.value = Mathf.InverseLerp(_minValue, _maxValue, _startValue);
            m_SliderValue.text = _startValue.ToString();
            m_SliderComponent.onValueChanged.AddListener((float value) => {
                float finalValue = Mathf.Lerp(_minValue, _maxValue, value);
                m_SliderValue.text = string.Format("{0:0.0}", value * _maxValue);
                OnValueChanged(finalValue);
            });
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
            m_Type.color = TColor.HEXtoColor(GetLogHexColor(m_Data.m_LogType));
            m_Info.text = m_Data.m_LogInfo;
            m_Stack.onClick.RemoveAllListeners();
            m_Stack.onClick.AddListener(() => OnStackClick(m_Data));
        }

    }

    class LogToggle
    {
        public Toggle m_Toggle;
        public Text m_Value;
        public LogToggle(Transform _transform,bool _filtered, Action OnValueChanged)
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
            m_Type.color = TColor.HEXtoColor(GetLogHexColor(_data.m_LogType));
            m_Info.text = _data.m_LogInfo;
            m_Time.text = TTime.TTimeTools.GetDateTime( _data.m_Time).ToLongTimeString();
            m_Track.text = _data.m_LogTrace;
            transform.SetActive(true);
        }
        public void HideStack()=> transform.SetActive(false);
    }
    #endregion
    private void OnEnable()
    {
        Application.logMessageReceived += OnLogReceived;
    }
    private void OnDisbable()
    {
        Application.logMessageReceived -= OnLogReceived;
    }

    int m_ErrorCount, m_WarningCount, m_LogCount;
    void OnLogReceived(string info, string trace, LogType type)
    {
        m_LogDataQueue.Enqueue(new LogData() { m_Time=TTime.TTimeTools.GetTimeStampNow(), m_LogInfo = info, m_LogTrace = trace, m_LogType = type });
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
        if (!m_ConsoleOpening||!m_RightPanelSetting.IsFlagEnable(enum_RightPanel.LogItem))
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
            m_Logs.AddItem(m_Logs.Count).Init(logInfo,m_Stack.ShowTrack);
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