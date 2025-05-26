using System;
using UnityEngine;
using TDataPersistent;
using static TouchConsole;
public partial class TouchConsole :MonoBehaviour, IPartialMethods<EPartialMethods,EPartialSorting>
{
    public enum EPartialMethods
    {
        Init,
        Destroy,
        Tick,
        Reset,
        OnEnable,
        OnDisable,
        SwitchVisible,
    }
    public enum EPartialSorting
    {
        CommandConsole,
        LogPanel,
        Misc,
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

    private static TouchConsole m_Instance;
    private static TouchConsole Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = TResources.Instantiate("TouchConsole").GetComponent<TouchConsole>();
                m_Instance.Init();
            }
            return m_Instance;
        }
    }
    
    void Init()
    {
        DontDestroyOnLoad(gameObject);
        m_Data.ReadPersistentData();
        this.InitMethods();
        this.InvokeMethods(EPartialMethods.Init);
        this.InvokeMethods(EPartialMethods.Reset);
        SetLogFramePanel(m_Data.m_FilterSetting.value);
    }
    void OnDestroy()
    {
        this.InvokeMethods(EPartialMethods.Destroy);
        m_Instance = null;
    }

    void OnEnable()
    {
        this.InvokeMethods(EPartialMethods.OnEnable);
    }
    void OnDisable()
    {
        this.InvokeMethods(EPartialMethods.OnDisable);
    }

    public static void SwitchVisible() => Instance.DoSwitchVisible();

    bool visible = true;
    void DoSwitchVisible()
    {
        visible = !visible;
        this.InvokeMethods(EPartialMethods.SwitchVisible,visible);
    }

    public static TouchConsole InitDefaultCommands()=>Instance.DefaultCommands();
    private TouchConsole DefaultCommands()
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
    }
    protected void SetLogFramePanel(EConsoleSetting _panelSetting)
    {
        m_FrameRate.SetActive(_panelSetting.IsFlagEnable(EConsoleSetting.FPS));
        m_LogFilter.SetActive(_panelSetting.IsFlagEnable(EConsoleSetting.LogPanel));
        m_LogPanelRect.SetActive(!_panelSetting.IsFlagClear());
        UpdateLogs();
    }
}