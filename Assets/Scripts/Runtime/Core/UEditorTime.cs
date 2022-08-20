using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
public class UEditorTime
{
    public double m_Cur =0f;

    public float deltaTime
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