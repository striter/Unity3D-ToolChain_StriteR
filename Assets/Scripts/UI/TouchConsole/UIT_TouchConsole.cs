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
    protected UIT_TouchConsole CommandSerialize<T>(T _target,Action<T> _OnSerializeDataChanged) where T:MonoBehaviour
    {
        Type targetType = _target.GetType();
        Command("Enable").Toggle(_target.enabled, value => _target.enabled = value);
        foreach(var fieldStack in targetType.GetBaseTypeFieldStacks(BindingFlags.Instance))
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