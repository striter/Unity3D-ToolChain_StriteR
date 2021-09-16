using System.Collections;
using System.Collections.Generic;
using ConvexGrid;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ModuleContainer))]
public class ModuleContainerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var moduleContainer = (target as ModuleContainer);
        if (!moduleContainer.gameObject.activeSelf||moduleContainer.m_Collector==null)
            return;

        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField($"ID:{moduleContainer.m_Collector.m_Identity}");
        EditorGUILayout.LabelField($"Byte:{moduleContainer.m_Collector.m_ModuleByte}");
        EditorGUILayout.EndHorizontal();
    }
}
