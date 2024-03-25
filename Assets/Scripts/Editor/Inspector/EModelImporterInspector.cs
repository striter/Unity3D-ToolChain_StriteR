using System;
using System.Reflection;
using UnityEditor;
using System.Linq.Extensions;
using UnityEngine;

namespace UnityEditor.Extensions
{
    [CustomEditor(typeof(ModelImporter))]
    public class ModelImporterInspector : UnityEditor.Editor
    {
        static readonly Type s_ModelImporterType = Type.GetType("UnityEditor.ModelImporterEditor,UnityEditor");
        UnityEditor.Editor m_ModelImporterEditor;
        //FieldInfo m_ActiveEditorIndex;
        private void OnEnable()
        {
            m_ModelImporterEditor = CreateEditor(targets, s_ModelImporterType);
            m_ModelImporterEditor.hideFlags = HideFlags.DontSave;
            (s_ModelImporterType.GetAllFields(BindingFlags.Instance).Find(p => p.Name == "m_InstantApply") as FieldInfo).SetValue(m_ModelImporterEditor,false);
            
            //m_ActiveEditorIndex = s_ModelImporterType.GetAllMembers(BindingFlags.Instance).Find(p => p.Name == "m_ActiveEditorIndex") as FieldInfo;
        }

        public override void OnInspectorGUI()
        {
            m_ModelImporterEditor.OnInspectorGUI();
            //Debug.Log(m_ActiveEditorIndex.GetValue(m_ModelImporterEditor));
        }
    }

}