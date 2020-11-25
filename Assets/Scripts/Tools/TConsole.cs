using System;
public enum enum_ConsoleLog
{
    Test_1 = 1,
    Test_2 = 2,
    Test_3 = 4,
    Test_4 = 8,
}

public static class TConsole
{
    public static enum_ConsoleLog m_LogFilter { get; private set; } = 0;
    public static void SetLogFilter(enum_ConsoleLog _logFilter)
    {
        m_LogFilter = _logFilter;
    } 
    public static void Log(enum_ConsoleLog _logType,string _logOutput)
    {
        if ((m_LogFilter & _logType) != _logType)
            return;

        UnityEngine.Debug.LogFormat("{0}:{1}", _logType, _logOutput);
    }
}