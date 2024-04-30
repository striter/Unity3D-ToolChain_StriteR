using UnityEditor.Extensions.EditorPath;
using UnityEngine;

namespace UnityEditor.Extensions
{
    public class EAssetsBatchRename : EditorWindow
    {
        private string m_Source;
        private string m_Replace;
        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            var folderPath = UEPath.GetCurrentProjectWindowDirectory();
            EditorGUILayout.LabelField("Current Folder:"+folderPath);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical();

            m_Source = EditorGUILayout.TextField("Source", m_Source);
            m_Replace = EditorGUILayout.TextField("Replace", m_Replace);
        
            if (GUILayout.Button("Rename"))
                EditorUtilities.RenameAssets(folderPath,m_Source,m_Replace);
            EditorGUILayout.EndVertical();
        }

    }
}
