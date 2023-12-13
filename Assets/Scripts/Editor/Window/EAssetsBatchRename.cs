using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;
using System.IO;
using Directory = UnityEngine.Windows.Directory;

namespace UnityEditor.Extensions
{
    public class EAssetsBatchRename : EditorWindow
    {
        private string m_Source;
        private string m_Replace;
        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            var m_FolderPath = UEAsset.GetCurrentProjectWindowDirectory();
            EditorGUILayout.LabelField("Current Folder:"+m_FolderPath);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical();

            m_Source = EditorGUILayout.TextField("Source", m_Source);
            m_Replace = EditorGUILayout.TextField("Replace", m_Replace);
        
            if (GUILayout.Button("Rename"))
                EditorUtilities.RenameAssets(m_FolderPath,m_Source,m_Replace);
            EditorGUILayout.EndVertical();
        }

    }
}
