using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityEditor.Extensions
{
    [CustomEditor(typeof(ScriptableObject),editorForChildClasses:true,isFallback = true),CanEditMultipleObjects]
    public class EScriptableObjectExtension : EInspectorExtension {}
    
    [CustomEditor(typeof(MonoBehaviour), editorForChildClasses:true,isFallback = true),CanEditMultipleObjects]
   public class EInspectorExtension : Editor 
    {
        private List<ButtonAttributeData> m_InspectorMethods;
        private bool m_Readonly = true;
        protected virtual void OnEnable()
        {
            m_InspectorMethods = UInspectorExtension.GetInspectorMethods(target);
            m_Readonly = target.GetType().GetCustomAttributes(true).Any(p=>p is ReadonlyAttribute);
        }

        public override void OnInspectorGUI()
        {
            if (m_Readonly)
            {
                EditorGUI.BeginDisabledGroup(true);
                GUI.enabled = false;
            }
            
            base.OnInspectorGUI();
            if (m_Readonly)
            {
                EditorGUI.EndDisabledGroup();
                GUI.enabled = true;
            }
            if (m_InspectorMethods.Count <= 0)
                return;
            
            EditorGUILayout.BeginVertical();
            if(m_InspectorMethods.Any(p=>p.attribute.IsElementVisible(target)))
                EditorGUILayout.LabelField("Extension Buttons", EditorStyles.boldLabel);
            
            foreach (var data in m_InspectorMethods)
            {
                if(!data.attribute.IsElementVisible(target))
                    continue;

                var undo = data.attribute.undo;
                EditorGUILayout.BeginVertical();
                if (data.parameters.Length > 0)
                {
                    EditorGUILayout.LabelField(data.method.Name, EditorStyles.boldLabel);
                    foreach (var parameter in data.parameters)
                        parameter.value = UEGUIExtension.LayoutField(parameter.value, parameter.type, parameter.name);
                }

                if (GUILayout.Button(data.parameters.Length > 0 ? "Execute" :  data.method.Name))
                {
                    if(undo)
                        Undo.RegisterCompleteObjectUndo(targets,"Button Click");
                    foreach (var target in targets)
                    {
                        data.method.Invoke(target,data.parameters.Select(p=>p.value).ToArray());
                        if (undo)
                        {
                            if (target is MonoBehaviour mono)
                            {
                                var method = target.GetType().GetMethod("OnValidate",UReflection.kInstanceBindingFlags);
                                method?.Invoke(target,null);
                            }
                        }
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
        }
    }
}