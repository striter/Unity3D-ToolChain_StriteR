using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.Extensions
{
    public class AssetSelectWindow : EditorWindow
    {
        private static List<UnityEngine.Object> kAssets = new();
        public static void Select<T>(Action<T> _onSelect,string _path = "Assets/") where T: UnityEngine.Object
        {
            kAssets.Clear();
            var guids = AssetDatabase.FindAssets($"t:{typeof(T)}", new[] { _path });
            foreach (string guid in guids)
                kAssets.Add(AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)));

            if (kAssets.Count == 0)
            {
                Debug.LogError($"Can't find any {typeof(T).Name} in {_path}");
                return;
            }
            
            var window = GetWindow<AssetSelectWindow>(typeof(T).Name);
            window.m_Type = typeof(T);
            window.m_Assets = kAssets;
            window.m_OnSelect = (p)=>_onSelect(p as T);
        }

        private Type m_Type;
        private List<UnityEngine.Object> m_Assets;
        private Action<UnityEngine.Object> m_OnSelect;
        private void OnGUI()
        {
            var successful = false;
            GUILayout.BeginVertical();
            foreach (var asset in m_Assets)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(asset,m_Type,false);

                if (GUILayout.Button("Select"))
                {
                    m_OnSelect(asset);
                    successful = true;
                }
                GUILayout.EndHorizontal();
                if (successful)
                    break;
            }

            if (GUILayout.Button("Close"))
                successful = true;

            GUILayout.EndVertical();
            
            if (successful)
                Close();
        }
    }
}