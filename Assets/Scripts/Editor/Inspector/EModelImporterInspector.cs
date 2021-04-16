using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace TEditor
{
    [CustomEditor(typeof(ModelImporter))]
    public class EModelImporterInspector : Editor
    {
        static readonly Type s_ModelImporterType = Type.GetType("UnityEditor.ModelImporterEditor,UnityEditor");
        Editor m_ModelImporterEditor;
        FieldInfo m_ActiveEditorIndex;
        private void OnEnable()
        {
            m_ModelImporterEditor = CreateEditor(targets, s_ModelImporterType);
            m_ModelImporterEditor.hideFlags = HideFlags.DontSave;
            m_ActiveEditorIndex = s_ModelImporterType.GetAllMembers(BindingFlags.Instance).Find(p => p.Name == "m_ActiveEditorIndex") as FieldInfo;
            (s_ModelImporterType.GetAllMembers(BindingFlags.Instance).Find(p => p.Name == "m_InstantApply") as FieldInfo).SetValue(m_ModelImporterEditor,false);
        }

        public override void OnInspectorGUI()
        {
            m_ModelImporterEditor.OnInspectorGUI();
            //Debug.Log(m_ActiveEditorIndex.GetValue(m_ModelImporterEditor));
        }
    }

}