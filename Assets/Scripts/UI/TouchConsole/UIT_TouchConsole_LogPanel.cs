using System;
using System.Collections;
using System.Collections.Generic;
using TDataPersistent;
using UnityEngine;
using UnityEngine.UI;

#region LogPanel
public partial class UIT_TouchConsole : SingletonMono<UIT_TouchConsole>
{
    [Flags]
    public  enum enum_ConsoleSetting
    {
        LogPanel = 1,
        LogTrack = 2,
        FPS = 4,
    }
    Queue<LogData> m_LogDataQueue = new Queue<LogData>();

    ScrollRect m_LogPanelRect;
    RectTransform m_LogFilter;
    LogToggle m_FilterLog, m_FilterWarning, m_FilterError, m_FilterCollapse;
    TGameObjectPool_Instance_Class<int, LogItem> m_Logs;
    StackPanel m_Stack;
    [PartialMethod(enum_PartialMethods.Init,enum_PartialSorting.LogPanel)]
    void InitLog()
    {
        m_LogPanelRect = transform.Find("LogPanel").GetComponent<ScrollRect>();
        Transform rightContent = m_LogPanelRect.transform.Find("Viewport/Content");
        m_LogFilter = rightContent.Find("LogFilter") as RectTransform;
        m_FilterLog = new LogToggle(m_LogFilter.Find("Log"), m_Data.m_Log, OnLogToggled);
        m_FilterWarning = new LogToggle(m_LogFilter.Find("Warning"), m_Data.m_Warning, OnLogToggled);
        m_FilterError = new LogToggle(m_LogFilter.Find("Error"), m_Data.m_Error, OnLogToggled);
        m_FilterCollapse = new LogToggle(m_LogFilter.Find("Collapse"), m_Data.m_Collapse, OnLogToggled);
        m_Logs = new TGameObjectPool_Instance_Class<int, LogItem>(m_LogPanelRect.transform.Find("Viewport/Content"), "LogItem");

        m_Stack = new StackPanel(transform.Find("Stack"));

        m_LogFilter.Find("Clear").GetComponent<Button>().onClick.AddListener(ClearConsoleLog);
    }
    [PartialMethod(enum_PartialMethods.Reset, enum_PartialSorting.LogPanel)]
    void LogFrameReset()
    {
        m_Stack.HideStack();
        ClearConsoleLog();
    }
    [PartialMethod(enum_PartialMethods.OnEnable, enum_PartialSorting.LogPanel)]
    void LogEnable()
    {
        Application.logMessageReceived += OnLogReceived;
    }
    [PartialMethod(enum_PartialMethods.OnDisable, enum_PartialSorting.LogPanel)]
    void LogDisable()
    {
        Application.logMessageReceived -= OnLogReceived;
    }
    public void ClearConsoleLog()
    {
        m_LogDataQueue.Clear();
        UpdateLogs();
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
        if (!m_Data.m_Collapse)
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
                    logCollapses.Add(logData, 1);
            }
            foreach (var logCollapse in logCollapses)
                m_Logs.AddItem(m_Logs.Count).Init(logCollapse.Key, m_Stack.ShowTrack).SetData(true, logCollapse.Value);
        }

        m_Logs.Sort((a, b) => b.Key - a.Key);
        LayoutRebuilder.ForceRebuildLayoutImmediate(m_LogPanelRect.transform as RectTransform);
    }
    struct LogData
    {
        public int m_Time;
        public string m_LogInfo;
        public string m_LogTrace;
        public LogType m_LogType;
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
        public void SetData(bool _collapse, int _count = 0)
        {
            m_Type.color = UColor.HEXtoColor(GetLogHexColor(m_Data.m_LogType));
            m_Info.text = m_Data.m_LogInfo;
            if (_collapse)
                m_Info.text = string.Format("{0} {1}", _count, m_Data.m_LogInfo);
            else
                m_Info.text = string.Format("{0} {1}", UTime.GetDateTime(m_Data.m_Time).ToShortTimeString(), m_Data.m_LogInfo);
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
}
#endregion