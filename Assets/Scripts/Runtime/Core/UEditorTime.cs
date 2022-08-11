using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
public static class UEditorTime
{
    public static double m_Cur;

    public static float deltaTime
    {
        get
        {
            var last = m_Cur;
            m_Cur = UnityEditor.EditorApplication.timeSinceStartup;
            return Mathf.Max(0, (float)(m_Cur-last));
        }
    }
}
#endif