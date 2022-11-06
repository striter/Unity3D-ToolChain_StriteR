using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Procedural;
using UnityEngine;

namespace UnityEditor.Extensions
{
    public class EProceduralMeshGenerator : EditorWindow
    {
        [SerializeField] public ProceduralMeshInput m_Input = ProceduralMeshInput.kDefault;
        private SerializedObject m_SerializedWindow;
        SerializedProperty m_InputProperty;
        void OnEnable()
        {
            m_SerializedWindow = new SerializedObject(this);
            m_InputProperty = m_SerializedWindow.FindProperty(nameof(m_Input));
        }
        void OnDisable()
        {
            m_InputProperty.Dispose();
        }
        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_InputProperty,true);
            if(EditorGUI.EndChangeCheck())
                m_SerializedWindow.ApplyModifiedPropertiesWithoutUndo();
            if (GUILayout.Button("Populate"))
                PopulateMesh();
            EditorGUILayout.EndVertical();
        }

        void PopulateMesh()
        {
            string meshName = $"{m_Input.meshType}";
            if (!UEAsset.SaveFilePath(out string path, "asset", meshName))
                return;
            
            Mesh mesh = new Mesh {name = meshName};
            m_Input.Output(mesh);
            UEAsset.CreateOrReplaceMainAsset(mesh, UEPath.FileToAssetPath(path));
        }

    }
}
