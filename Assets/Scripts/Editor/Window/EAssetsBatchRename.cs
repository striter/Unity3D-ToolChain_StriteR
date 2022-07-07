using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;
using System.IO;
using Directory = UnityEngine.Windows.Directory;

namespace UnityEngine.Editor.Extensions
{
    public class EAssetsBatchRename : EditorWindow
    {
        private string m_FolderPath;
        private string m_Source;
        private string m_Replace;
        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Current Folder:"+m_FolderPath);
            m_FolderPath = UEAsset.GetCurrentProjectWindowDirectory();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical();

            m_Source = EditorGUILayout.TextField("Source", m_Source);
            m_Replace = EditorGUILayout.TextField("Replace", m_Replace);
        
            if (GUILayout.Button("Rename"))
                RenameAssets();
            EditorGUILayout.EndVertical();
        }

        void RenameAssets()
        {
            int count = 0;
            foreach (var assetPath in System.IO.Directory.GetFiles(UEPath.AssetToFilePath(m_FolderPath)))
            {
                count++;
                string assetName = Path.GetFileName(assetPath);
                System.IO.File.Move(assetPath,assetPath.Replace(assetName,assetName.Replace(m_Source,m_Replace)));
            }
            AssetDatabase.Refresh();
            Debug.Log($"{count} Assets Renamed");
        }
    }
}
