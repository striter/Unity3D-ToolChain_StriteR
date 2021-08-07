using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using TDataPersistent;
using static UIT_TouchConsole;
public partial class UIT_TouchConsole : SingletonMono<UIT_TouchConsole>,IPartialMethods<EPartialMethods,EPartialSorting>
{
    public enum EPartialMethods
    {
        Init,
        Tick,
        Reset,
        OnEnable,
        OnDisable,
    }
    public enum EPartialSorting
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
        public Ref<EConsoleSetting> m_FilterSetting;
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
            m_FilterSetting = (EConsoleSetting)int.MaxValue;
            m_ConsoleTimeScale = .5f;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        m_Data.ReadPersistentData();
        this.InitMethods();
        this.InvokeMethods(EPartialMethods.Init);
        this.InvokeMethods(EPartialMethods.Reset);
        SetLogFramePanel(m_Data.m_FilterSetting.m_RefValue);
    }

    void OnEnable()
    {
        this.InvokeMethods(EPartialMethods.OnEnable);
    }
    void OnDisable()
    {
        this.InvokeMethods(EPartialMethods.OnDisable);
    }

    public static UIT_TouchConsole InitDefaultCommands() => Instance.Init();
    protected UIT_TouchConsole Init()
    {
        this.InvokeMethods(EPartialMethods.Reset);
        NewPage("Console");
        Command("Time Scale").Slider(0f, 2f, m_Data.m_ConsoleTimeScale, scale => { m_Data.SavePersistentData(); SetConsoleTimeScale(scale); });
        Command("Right Panel").FlagsSelection(m_Data.m_FilterSetting, setting => { m_Data.SavePersistentData(); SetLogFramePanel(setting); });
        SelectPage(0);
        return this;
    }
    private void Update()
    {
        float deltaTime = Time.unscaledDeltaTime;
        this.InvokeMethods( EPartialMethods.Tick,deltaTime);
        TickConsole(deltaTime);
    }
    protected void SetLogFramePanel(EConsoleSetting _panelSetting)
    {
        m_FrameRate.SetActive(_panelSetting.IsFlagEnable(EConsoleSetting.FPS));
        m_LogFilter.SetActive(_panelSetting.IsFlagEnable(EConsoleSetting.LogPanel));
        m_LogPanelRect.SetActive(!_panelSetting.IsFlagClear());
        UpdateLogs();
    }
}