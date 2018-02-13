using System.Collections;
using System.Collections.Generic;

public enum LogType { Exception, Trace, Test, D };

static class Debug
{
    private static Dictionary<LogType, bool> logTypeToBool;

    static Debug()
    {
        logTypeToBool = new Dictionary<LogType, bool>()
            {
                { LogType.Exception,    true},
                { LogType.Trace,        false},
                { LogType.Test,         true},
                { LogType.D,            true}
            };
    }

    public static void Log(LogType type, string contents)
    {
        if (!logTypeToBool[type])
            return;
        UnityEngine.Debug.Log(contents);
    }

    public static void Log(LogType type, string format, params object[] args)
    {
        if (!logTypeToBool[type])
            return;
        UnityEngine.Debug.LogFormat(format, args);
    }
}