using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIT_TouchConsole : SingletonMono<UIT_TouchConsole> {

    #region Helper
    public static void Init(Action<bool> _OnConsoleShow = null,bool _defaultFrameRateShow=true,float _defaultTimeScale=.5f)=>Instance.InitConsole(_OnConsoleShow,_defaultFrameRateShow, _defaultTimeScale);
    public static void Header(string _title) => Instance.AddCommandLine().Header(_title);
    public static void EmtpyLine() => Instance.AddCommandLine().EmptyLine();
    public static ConsoleCommand Command(string _title) => Instance.AddCommandLine().Command(_title);
    #endregion
    public bool m_ConsoleOpening { get; private set; } = false;

    [Range(0,2f)] public float m_ConsoleTimeScale = .5f;
    public bool m_ShowFrameRate = true;
    public int LogSaveCount = 30;

    Text m_LogText;
    Transform m_FrameRate;
    Text m_FrameRateValue;
    ScrollRect m_ConsoleCommandScrollRect;
    TGameObjectPool_Instance_Class<int, ConsoleCommand> m_ConsoleCommands;
    Action<bool> OnConsoleShow;

    protected override void Awake()
    {
        base.Awake();
        m_FrameRate = transform.Find("FrameRate");
        m_FrameRateValue = transform.Find("FrameRate/Value").GetComponent<Text>();

        m_ConsoleCommandScrollRect = transform.Find("ConsoleCommand").GetComponent<ScrollRect>();
        Transform tf_ConsoleCommand = m_ConsoleCommandScrollRect.transform.Find("Viewport/Content");
        m_ConsoleCommands = new TGameObjectPool_Instance_Class<int, ConsoleCommand>(tf_ConsoleCommand, "GridItem");

        m_LogText = transform.Find("Log").GetComponent<Text>();

        m_ConsoleOpening = false;
        m_ConsoleCommands.transform.SetActivate(m_ConsoleOpening);
    }
    protected UIT_TouchConsole InitConsole(Action<bool> _OnConsoleShow,bool _defaultFrameRateShow,float _defaultConsoleTimeScale)
    {
        OnConsoleShow = _OnConsoleShow;

        ShowFrameRate(_defaultFrameRateShow);
        SetConsoleTimeScale(_defaultConsoleTimeScale);

        m_FrameRateQueue.Clear();
        m_FrameRateValue.text = "";

        m_LogQueue.Clear();
        m_LogText.text = "";

        m_ConsoleCommands.Clear();
        Header("Console");
        Command("Show FPS").Button(()=>ShowFrameRate(!m_ShowFrameRate));
        Command("Clear Log").Button(ClearConsoleLog);
        Command("Debug Filter").EnumFlagsSelection<enum_ConsoleLog>(0, TConsole.SetLogFilter);
        Command("Time Scale").Slider(_defaultConsoleTimeScale,0f, 2f, SetConsoleTimeScale);
        EmtpyLine();
        return this;
    }

    Timer m_FastKeyCooldownTimer = new Timer(.5f);
    Queue<int> m_FrameRateQueue = new Queue<int>();
    private void Update()
    {
        m_ConsoleCommands.m_ActiveItemDic.Traversal((ConsoleCommand command) => { command.KeycodeTick(); });

        m_FastKeyCooldownTimer.Tick(Time.unscaledDeltaTime);
        if (m_FastKeyCooldownTimer .m_Timing)
            return;
        if (Input.touchCount >= 5 || Input.GetKey(KeyCode.BackQuote))
        {
            m_FastKeyCooldownTimer.Replay();
            m_ConsoleOpening = !m_ConsoleOpening;
            m_ConsoleCommands.transform.SetActivate(m_ConsoleOpening);
            OnConsoleShow?.Invoke(m_ConsoleOpening);
            Time.timeScale = m_ConsoleOpening?m_ConsoleTimeScale:1f;
            UpdateLogUI();
        }

        if(m_ShowFrameRate)
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

    protected void ShowFrameRate(bool _show)
    {
        m_ShowFrameRate = _show;
        m_FrameRate.SetActivate(m_ShowFrameRate);
    }
    protected void SetConsoleTimeScale(float _timeScale)
    {
        m_ConsoleTimeScale = _timeScale;
        if (!m_ConsoleOpening)
            return;
        Time.timeScale = m_ConsoleTimeScale;
    }
    #endregion
    #region Console
    protected ConsoleCommand AddCommandLine() => m_ConsoleCommands.AddItem(m_ConsoleCommands.Count);
    public class ConsoleCommand : CGameObjectPool_Instance_Class<int>
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
            public void Play<T>(int defaultValue, Action<T> _OnFlagChanged) where T : Enum
            {
                m_ToggleGrid.Clear();
                TCommon.TraversalEnum<T>(value => {
                    int valueIndex = (int)value;
                    Toggle tog = m_ToggleGrid.AddItem(valueIndex);
                    tog.isOn = (defaultValue & valueIndex) == valueIndex;
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
                    m_ButtonGrid.transform.SetActivate(!m_ButtonGrid.transform.gameObject.activeSelf);
                });
                m_ButtonGrid.transform.SetActivate(false);
            }
            public void Play<T>(T _defaultValue, Action<int> _OnClick) where T : Enum
            {
                m_ButtonGrid.Clear();
                m_Text.text = _defaultValue.ToString();
                TCommon.TraversalEnum<T>(temp =>
                {
                    int index = (int)(temp);
                    Button btn = m_ButtonGrid.AddItem(index);
                    btn.onClick.RemoveAllListeners();
                    btn.GetComponentInChildren<Text>().text = temp.ToString();
                    btn.onClick.AddListener(() => {
                        m_Text.text = temp.ToString();
                        _OnClick(index);
                        m_ButtonGrid.transform.SetActivate(false);
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
                        m_ButtonGrid.transform.SetActivate(false);
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
        ButtonSelection m_GridSelection;
        ToggleSelection m_ToggleSelection;
        Button m_Button;
        Text m_ButtonTitle;
        Transform m_Slider;
        Slider m_SliderComponent;
        Text m_SliderValue;
        public ConsoleCommand(Transform _transform) : base(_transform)
        {
            m_Header = transform.Find("Header");
            m_HeaderTitle = m_Header.Find("Title").GetComponent<Text>();
            m_Command = transform.Find("Command");
            m_CommandTitle = transform.Find("Command/Title").GetComponent<Text>();
            m_ValueInput1 = transform.Find("Input1").GetComponent<InputField>();
            m_ValueInput2 = transform.Find("Input2").GetComponent<InputField>();
            m_GridSelection = new ButtonSelection(transform.Find("ButtonSelection"));
            m_ToggleSelection = new ToggleSelection(transform.Find("ToggleSelection"));
            m_Button = transform.Find("Button").GetComponent<Button>();
            m_ButtonTitle = transform.Find("Button/Title").GetComponent<Text>();
            m_Slider = transform.Find("Slider");
            m_SliderComponent = transform.Find("Slider/Slider").GetComponent<Slider>();
            m_SliderValue = transform.Find("Slider/Value").GetComponent<Text>();
        }

        public void KeycodeTick()
        {
            if (Input.GetKeyDown(m_KeyCode))
                m_Button.onClick.Invoke();
        }
        public override void OnAddItem(int identity)
        {
            base.OnAddItem(identity);
            m_ValueInput1.SetActivate(false);
            m_ValueInput2.SetActivate(false);
            m_GridSelection.transform.SetActivate(false);
            m_ToggleSelection.transform.SetActivate(false);
            m_Button.SetActivate(true);

            m_Header.SetActivate(false);
            m_Command.SetActivate(false);

            m_KeyCode = KeyCode.None;
            m_Button.SetActivate(false);
            m_Button.onClick.RemoveAllListeners();

            m_Slider.SetActivate(false);
        }

        public void EmptyLine()
        {

        }

        public void Header(string _title)
        {
            m_Header.SetActivate(true);
            m_HeaderTitle.text = _title;
        }

        public ConsoleCommand Command(string _title)
        {
            m_Command.SetActivate(true);
            m_CommandTitle.text = _title;
            return this;
        }
        public void Button(Action OnClick, KeyCode _keyCode = KeyCode.None)
        {
            m_Button.SetActivate(true);
            m_Button.onClick.AddListener(() => OnClick());
            m_KeyCode = _keyCode;
            m_ButtonTitle.text = _keyCode == KeyCode.None ? "" : _keyCode.ToString();
        }
        int selectionIndex = -1;
        public void EnumSelection<T>(T _defaultEnum, Action<T> OnClick, KeyCode _keyCode = KeyCode.None) where T : Enum
        {
            m_GridSelection.transform.SetActivate(true);
            selectionIndex = (int)Enum.ToObject(typeof(T), _defaultEnum);
            m_GridSelection.Play(_defaultEnum, (int value) => selectionIndex = value);
            Button(() => OnClick((T)Enum.ToObject(typeof(T), selectionIndex)), _keyCode);
        }
        public void EnumSelection(int _defaultEnum, List<string> _values, Action<string> OnClick, KeyCode _keyCode = KeyCode.None)
        {
            m_GridSelection.transform.SetActivate(true);
            selectionIndex = _defaultEnum;
            m_GridSelection.Play(_values, _values[_defaultEnum], (int value) => selectionIndex = value);
            Button(() => OnClick(_values[selectionIndex]), _keyCode);
        }
        public void EnumSelection<T>(T _defaultEnum, string _defaultValue, Action<T, string> OnClick, KeyCode keyCode = KeyCode.None) where T : Enum
        {
            m_GridSelection.transform.SetActivate(true);
            m_ValueInput1.SetActivate(true);
            m_ValueInput1.text = _defaultValue;
            selectionIndex = (int)Enum.ToObject(typeof(T), _defaultEnum);
            m_GridSelection.Play(_defaultEnum, (int value) => selectionIndex = value);
            Button(() => OnClick((T)Enum.ToObject(typeof(T), selectionIndex), m_ValueInput1.text), keyCode);
        }
        public void EnumFlagsSelection<T>(int _defaultEnum, Action<T> _logFilter, KeyCode _keyCode = KeyCode.None) where T : Enum
        {
            m_ToggleSelection.transform.SetActivate(false);
            m_ToggleSelection.Play(_defaultEnum, _logFilter);
            Button(() => m_ToggleSelection.transform.SetActivate(!m_ToggleSelection.transform.gameObject.activeSelf), _keyCode);
        }
        public void InputField(string _defaultValue, Action<string> OnValueClick, KeyCode _keyCode = KeyCode.None)
        {
            m_ValueInput1.SetActivate(true);
            m_ValueInput1.text = _defaultValue;
            Button(() => OnValueClick(m_ValueInput1.text), _keyCode);
        }

        public void InpuptField(string _defaultValue1, string _defaultValue2, Action<string, string> OnValueClick, KeyCode _keyCode = KeyCode.None)
        {
            m_ValueInput1.SetActivate(true);
            m_ValueInput2.SetActivate(true);
            m_ValueInput1.text = _defaultValue1;
            m_ValueInput2.text = _defaultValue2;
            Button(() => OnValueClick(m_ValueInput1.text, m_ValueInput2.text), _keyCode);
        }

        public void Slider(float _maxValue, Action<float> OnValueChanged) => Slider(0,0, _maxValue, OnValueChanged);
        public void Slider(float _startValue,float _minValue, float _maxValue, Action<float> OnValueChanged)
        {
            m_Slider.SetActivate(true);
            m_SliderComponent.value = _startValue;
            m_SliderValue.text = _startValue.ToString();
            m_SliderComponent.onValueChanged.AddListener((float value) => {
                float finalValue = Mathf.Lerp(_minValue, _maxValue, value);
                m_SliderValue.text = string.Format("{0:0.0}", value * _maxValue);
                OnValueChanged(finalValue);
            });
        }
    }
    #endregion
    #region DEBUG LOG VISUALIZE
    private void OnEnable()
    {
        Application.logMessageReceived += OnLogReceived;
    }
    private void OnDisbable()
    {
        Application.logMessageReceived -= OnLogReceived;
    }

    Queue<ConsoleLog> m_LogQueue = new Queue<ConsoleLog>();
    int m_ErrorCount, m_WarningCount, m_LogCount;
    struct ConsoleLog
    {
        public string logInfo;
        public string logTrace;
        public LogType logType;
    }
    void OnLogReceived(string info, string trace, LogType type)
    {
        ConsoleLog tempLog = new ConsoleLog();
        tempLog.logInfo = info;
        tempLog.logTrace = trace;
        tempLog.logType = type;
        m_LogQueue.Enqueue(tempLog);
        switch (type)
        {
            case LogType.Exception:
            case LogType.Error: m_ErrorCount++; break;
            case LogType.Warning: m_WarningCount++; break;
            case LogType.Log: m_LogCount++; break;
        }
        if (m_LogQueue.Count > LogSaveCount)
            m_LogQueue.Dequeue();
        UpdateLogUI();
    }
    void UpdateLogUI()
    {
        if (!m_LogText)
            return;
        if (!m_ConsoleOpening)
        {
            m_LogText.text = string.Format("<color=#FFFFFF>Errors:{0},Warnings:{1},Logs:{2}</color>",m_ErrorCount,m_WarningCount, m_LogCount);
            return;
        }
        
        m_LogText.text = "";
        foreach (ConsoleLog log in m_LogQueue) 
            m_LogText.text += "<color=#" + GetLogHexColor(log.logType) + ">" + log.logInfo + "</color>\n"; 
    }
    string GetLogHexColor(LogType type)
    {
        string colorParam = "";
        switch (type)
        {
            case LogType.Log:
                colorParam = "00FF28";
                break;
            case LogType.Warning:
                colorParam = "FFA900";
                break;
            case LogType.Exception:
            case LogType.Error:
                colorParam = "FF0900";
                break;
            case LogType.Assert:
            default:
                colorParam = "00E5FF";
                break;
        }
        return colorParam;
    }
    public void ClearConsoleLog()
    {
        m_LogQueue.Clear();
        UpdateLogUI();
    }
    #endregion
}
