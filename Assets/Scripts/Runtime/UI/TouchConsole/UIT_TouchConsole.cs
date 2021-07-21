using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using TDataPersistent;
using static UIT_TouchConsole;
public partial class UIT_TouchConsole : SingletonMono<UIT_TouchConsole>,IPartialMethods<enum_PartialMethods,enum_PartialSorting>
{
    public enum enum_PartialMethods
    {
        Init,
        Tick,
        Reset,
        OnEnable,
        OnDisable,
    }
    public enum enum_PartialSorting
    {
        CommandConsole,
        LogPanel,
        Miscs,
    }
    public TouchConsoleSaveData m_Data = new TouchConsoleSaveData();

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
        m_Data.ReadPersistentData();
        this.InitMethods();
        this.InvokeMethods(enum_PartialMethods.Init);
        this.InvokeMethods(enum_PartialMethods.Reset);
        SetLogFramePanel(m_Data.m_FilterSetting.m_RefValue);
    }

    void OnEnable()
    {
        this.InvokeMethods(enum_PartialMethods.OnEnable);
    }
    void OnDisable()
    {
        this.InvokeMethods(enum_PartialMethods.OnDisable);
    }

    public static UIT_TouchConsole InitDefaultCommands() => Instance.Init();
    protected UIT_TouchConsole Init()
    {
        this.InvokeMethods(enum_PartialMethods.Reset);
        NewPage("Console");
        Command("Time Scale").Slider(0f, 2f, m_Data.m_ConsoleTimeScale, scale => { m_Data.SavePersistentData(); SetConsoleTimeScale(scale); });
        Command("Right Panel").FlagsSelection(m_Data.m_FilterSetting, setting => { m_Data.SavePersistentData(); SetLogFramePanel(setting); });
        SelectPage(0);
        return this;
    }
    private void Update()
    {
        float deltaTime = Time.unscaledDeltaTime;
        this.InvokeMethods( enum_PartialMethods.Tick,deltaTime);
        TickConsole(deltaTime);
    }
    protected void SetLogFramePanel(enum_ConsoleSetting _panelSetting)
    {
        m_FrameRate.SetActive(_panelSetting.IsFlagEnable(enum_ConsoleSetting.FPS));
        m_LogFilter.SetActive(_panelSetting.IsFlagEnable(enum_ConsoleSetting.LogPanel));
        m_LogPanelRect.SetActive(!_panelSetting.IsFlagClear());
        UpdateLogs();
    }
}