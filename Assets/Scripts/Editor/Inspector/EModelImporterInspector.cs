using System;
using System.Reflection;
using UnityEditor;
//[CustomEditor(typeof(ModelImporter))]
//public class EModelImporterInspector : Editor
//{
//    static readonly Type s_ModelImporterType = Type.GetType("UnityEditor.ModelImporterEditor,UnityEditor");
//    Editor m_ModelImporterEditor;
//    FieldInfo m_ActiveEditorIndex;
//    private void OnEnable()
//    {
//        m_ModelImporterEditor = CreateEditor(targets,s_ModelImporterType);
//        m_ActiveEditorIndex = s_ModelImporterType.GetInstanceFields().Find(p => p.Name == "m_ActiveEditorIndex");
//    }

//    public override void OnInspectorGUI()
//    {
//        m_ModelImporterEditor.OnInspectorGUI();
//        //Debug.Log(m_ActiveEditorIndex.GetValue(m_ModelImporterEditor));
//    }
//}
