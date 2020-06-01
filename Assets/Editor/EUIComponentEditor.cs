using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UIComponentBase),true), CanEditMultipleObjects]
public class EUIComponentEditor : Editor {
    public override void OnInspectorGUI()
    {
        if (!Application.isPlaying&&GUILayout.Button("Test Init"))
            (target as UIComponentBase).SendMessage("Init");
        base.OnInspectorGUI();
    }
}
