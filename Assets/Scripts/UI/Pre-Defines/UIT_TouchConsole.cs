using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIT_TouchConsole : SingletonMono<UIT_TouchConsole> {
    public bool m_ConsoleOpening { get; private set; } = false;
    public int LogSaveCount = 30;
    Text m_LogText;
    TGameObjectPool_Instance_Class<int, ConsoleCommand> m_ConsoleCommands;
    Action<bool> OnConsoleShow;
    protected override void Awake()
    {
        base.Awake();
        m_LogText = transform.Find("Log").GetComponent<Text>();
        m_LogText.text = "";
        Transform tf_ConsoleCommand = transform.Find("ConsoleCommand");
        m_ConsoleCommands = new TGameObjectPool_Instance_Class<int, ConsoleCommand>(tf_ConsoleCommand, "GridItem");
        m_ConsoleOpening = false;
        m_ConsoleCommands.transform.SetActivate(m_ConsoleOpening);
    }
    public UIT_TouchConsole InitConsole(Action<bool> _OnConsoleShow)
    {
        m_ConsoleCommands.Clear();
        OnConsoleShow = _OnConsoleShow;
        AddConsoleBinding().Set("Clear Log").Play(ClearConsoleLog);
        return this;
    }
    #region Console
    public ConsoleCommand AddConsoleBinding() => m_ConsoleCommands.AddItem(m_ConsoleCommands.Count);
    public class ConsoleCommand : CGameObjectPool_Instance_Class<int>
    {
        InputField m_ValueInput1,m_ValueInput2;
        EnumSelection m_ValueSelection;
        Text m_CommandTitle;
        KeyCode m_KeyCode;
        Button m_CommonButton;
        public ConsoleCommand(Transform _transform):base(_transform)
        {
            m_ValueInput1 = transform.Find("Input1").GetComponent<InputField>();
            m_ValueInput2 = transform.Find("Input2").GetComponent<InputField>();
            m_ValueSelection = new EnumSelection(transform.Find("Select"));
            m_CommonButton = transform.Find("Button").GetComponent<Button>();
            m_CommandTitle = transform.Find("Button/Title").GetComponent<Text>();
        }

        public void KeycodeTick()
        {
            if (Input.GetKeyDown(m_KeyCode))
                m_CommonButton.onClick.Invoke();
        }
        public override void OnAddItem(int identity)
        {
            base.OnAddItem(identity);
            m_ValueInput1.SetActivate(false);
            m_ValueInput2.SetActivate(false);
            m_ValueSelection.transform.SetActivate(false);
            m_KeyCode =  KeyCode.None;
            m_CommandTitle.text = "";
            m_CommonButton.onClick.RemoveAllListeners();
        }
        public ConsoleCommand Set(string title,KeyCode keyCode= KeyCode.None)
        {
            m_KeyCode = keyCode;
            m_CommandTitle.text = title + (keyCode == KeyCode.None ? "":"|" + keyCode);
            m_CommonButton.onClick.RemoveAllListeners();
            return this;
        }

        public void Play(Action OnClick)
        {
            m_CommonButton.onClick.AddListener(()=>OnClick());
        }

        #region EnumSelection
        int selectionIndex = -1;
        public void Play<T>(T defaultEnum ,Action<T> OnClick) where T:Enum
        {
            m_ValueSelection.transform.SetActivate(true);
            selectionIndex = (int)Enum.ToObject(typeof(T),defaultEnum);
            m_ValueSelection.Init(defaultEnum, (int value)=>  selectionIndex=value);
            m_CommonButton.onClick.AddListener(() => OnClick((T)Enum.ToObject(typeof(T),selectionIndex)));
        }

        public void Play(List<string> values, int defaultEnum,Action<string> OnClick)
        {
            m_ValueSelection.transform.SetActivate(true);
            selectionIndex = defaultEnum;
            m_ValueSelection.Init(values,values[ defaultEnum], (int value) => selectionIndex = value);
            m_CommonButton.onClick.AddListener(() => OnClick(values[selectionIndex]));
        }
        #endregion
        public void Play<T>(T _defaultEnum,string _defaultValue, Action<T,string> OnClick) where T:Enum
        {
            m_ValueSelection.transform.SetActivate(true);
            m_ValueInput1.SetActivate(true);
            m_ValueInput1.text = _defaultValue;
            selectionIndex = (int)Enum.ToObject(typeof(T), _defaultEnum);
            m_ValueSelection.Init(_defaultEnum, (int value) => selectionIndex = value);
            m_CommonButton.onClick.AddListener(() => OnClick((T)Enum.ToObject(typeof(T), selectionIndex),m_ValueInput1.text));
        }

        public void Play(string defaultValue,Action<string> OnValueClick)
        {
            m_ValueInput1.SetActivate(true);
            m_ValueInput1.text = defaultValue;
            m_CommonButton.onClick.AddListener(() => OnValueClick(m_ValueInput1.text));
        }

        public void Play(string defaultValue1,string defaultValue2,Action<string,string> OnValueClick)
        {
            m_ValueInput1.SetActivate(true);
            m_ValueInput2.SetActivate(true);
            m_ValueInput1.text = defaultValue1;
            m_ValueInput2.text = defaultValue2;
            m_CommonButton.onClick.AddListener(() => OnValueClick(m_ValueInput1.text, m_ValueInput2.text));
        }

    }
    #endregion

    float m_fastKeyCooldown = 0f;
    private void Update()
    {
        m_ConsoleCommands.m_ActiveItemDic.Traversal((ConsoleCommand command) => { command.KeycodeTick(); });

        if (m_fastKeyCooldown>0f)
        {
            m_fastKeyCooldown -= Time.unscaledDeltaTime;
            return;
        }
        if (Input.touchCount >= 4 || Input.GetKey(KeyCode.BackQuote))
        {
            m_fastKeyCooldown = .5f;
            m_ConsoleOpening = !m_ConsoleOpening;
            m_ConsoleCommands.transform.SetActivate(m_ConsoleOpening);
            OnConsoleShow?.Invoke(m_ConsoleOpening);
            UpdateLogUI();
        }
    }
    #region Log
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
