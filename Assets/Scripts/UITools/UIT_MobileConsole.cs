using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIT_MobileConsole : SingletonMono<UIT_MobileConsole> {

    public bool m_ConsoleOpening { get; private set; } = false;
    public int LogExistCount = 10;
    public int LogSaveCount = 30;
    Text m_LogText, m_FrameText;
    ObjectPoolListClass<int, ConsoleCommand> m_ConsoleCommands;
    Action<bool> OnConsoleShow;
    protected override void Awake()
    {
        base.Awake();
        m_LogText = transform.Find("Log").GetComponent<Text>();
        m_LogText.text = "";
        m_FrameText = transform.Find("Frame").GetComponent<Text>();

        Transform tf_ConsoleCommand = transform.Find("ConsoleCommand");
        m_ConsoleCommands = new ObjectPoolListClass<int, ConsoleCommand>(tf_ConsoleCommand, "GridItem");
        m_ConsoleOpening = false;
        m_ConsoleCommands.transform.SetActivate(m_ConsoleOpening);
    }

    public void InitConsole(Action<bool> _OnConsoleShow)
    {
        OnConsoleShow = _OnConsoleShow;
        m_ConsoleCommands.Clear();
    }
#region Console
    public ConsoleCommand AddConsoleBinding() => m_ConsoleCommands.AddItem(m_ConsoleCommands.Count);
    
    public class ConsoleCommand : CObjectPoolClass<int>
    {
        InputField m_ValueInput;
        EnumSelection m_ValueSelection;
        Text m_CommandTitle;
        KeyCode m_KeyCode;
        Button m_CommonButton;
        public ConsoleCommand(Transform _transform):base(_transform)
        {
            m_ValueInput = transform.Find("Input").GetComponent<InputField>();
            m_ValueSelection = new EnumSelection(transform.Find("Select"));
            m_CommonButton = transform.Find("Button").GetComponent<Button>();
            m_CommandTitle = transform.Find("Button/Title").GetComponent<Text>();
        }

        public void EditorKeycodeTick()
        {
            if (Input.GetKeyDown(m_KeyCode))
                m_CommonButton.onClick.Invoke();
        }

        void Play(string title,KeyCode keyCode)
        {
            m_KeyCode = keyCode;
            m_CommandTitle.text = string.Format("{0}|{1}", title, keyCode);
            m_CommonButton.onClick.RemoveAllListeners();
            m_ValueInput.SetActivate(false);
            m_ValueSelection.transform.SetActivate(false);
        }

        public void Play(string title,KeyCode keyCode,Action OnClick)
        {
            Play(title, keyCode);
            m_CommonButton.onClick.AddListener(()=>OnClick());
        }

        int selectionIndex = -1;
        public void Play<T>(string title,KeyCode keyCode,T defaultEnum ,Action<T> OnClick)  
        {
            Play(title, keyCode);
            m_ValueSelection.transform.SetActivate(true);
            selectionIndex = (int)Enum.ToObject(typeof(T),defaultEnum);
            m_ValueSelection.Init(defaultEnum, (int value)=>  selectionIndex=value );
            m_CommonButton.onClick.AddListener(() => OnClick((T)Enum.ToObject(typeof(T),selectionIndex)));
        }


        public void Play(string title,KeyCode keyCode, string defaultValue,Action<string> OnValueClick)
        {
            Play(title, keyCode);
            m_ValueInput.SetActivate(true);
            m_ValueInput.text = defaultValue;
            m_CommonButton.onClick.AddListener(() => OnValueClick(m_ValueInput.text));
        }
    }
#endregion
    float m_fastKeyCooldown = 0f;

    private void Update()
    {
#if UNITY_EDITOR
        m_ConsoleCommands.m_ActiveItemDic.Traversal((ConsoleCommand command) => { command.EditorKeycodeTick(); });
#endif

        m_FrameText.text = ((int)(1 / Time.unscaledDeltaTime)).ToString();
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
