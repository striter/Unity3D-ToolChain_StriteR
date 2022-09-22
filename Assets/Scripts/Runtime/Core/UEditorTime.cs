using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EditorTime
{
    static EditorTime() => UnityEditor.EditorApplication.update += Tick;
    public static float deltaTime { get; private set; } = 0f;
    public static double time { get; private set; } = 0f;
    static void Tick()
    {
        var last = time;
        time = UnityEditor.EditorApplication.timeSinceStartup;
        deltaTime = Mathf.Max(0, (float)(time-last));
    }
}