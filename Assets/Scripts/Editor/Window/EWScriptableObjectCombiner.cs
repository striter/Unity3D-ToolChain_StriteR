using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityEngine.Editor.Extensions
{
    public class ScriptableObjectCombiner : EditorWindow
    {
        [SerializeField] private ScriptableObject m_TargetAsset;
        [SerializeField] private ScriptableObject[] m_CombineAssets;
        private bool m_ClearMain;
        private bool m_KeepCombineAssets;

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            m_TargetAsset = (ScriptableObject)EditorGUILayout.ObjectField(m_TargetAsset, typeof(ScriptableObject), false);
            m_CombineAssets = GUILayout_Extend.ArrayField(m_CombineAssets,"Combine Assets");
            m_ClearMain = GUILayout.Toggle(m_ClearMain, "Clear Target Sub Assets");
            m_KeepCombineAssets = GUILayout.Toggle(m_KeepCombineAssets, "Keep Combine Assets");
            if (m_TargetAsset != null && m_CombineAssets != null && m_CombineAssets.Length > 0)
                if (GUILayout.Button("Combine"))
                    CombineAsset(m_TargetAsset,m_CombineAssets,m_KeepCombineAssets);
            EditorGUILayout.EndVertical();
        }

        void CombineAsset(ScriptableObject _src,ScriptableObject[] _combines,bool _keepCombines)
        {
            var subAssets = new List<Object>();
            foreach (var subAsset in _combines)
            {
                if(subAsset==null)
                    continue;
                
                var copyAsset = CreateInstance(subAsset.GetType());
                EditorUtility.CopySerialized(subAsset,copyAsset);
                subAssets.Add(copyAsset);
            }

            var mainPath = AssetDatabase.GetAssetPath(_src);
            if (m_ClearMain)
                UEAsset.ClearSubAssets(mainPath);
            UEAsset.CreateOrReplaceSubAsset(mainPath,subAssets);
            if (_keepCombines)
                return;
            foreach (var asset in _combines)
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(asset));
        }
    }

}