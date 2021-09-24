using System.Collections;
using System.Collections.Generic;
using ConvexGrid;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ModuleVoxel))]
public class ModuleContainerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var moduleContainer = (target as ModuleVoxel);
        if (!moduleContainer.gameObject.activeSelf||moduleContainer.m_Voxel==null)
            return;

        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField($"ID:{moduleContainer.m_Voxel.Identity}");
        EditorGUILayout.LabelField($"Byte:{moduleContainer.m_Voxel.CornerRelations}");
        EditorGUILayout.EndHorizontal();
    }
}
