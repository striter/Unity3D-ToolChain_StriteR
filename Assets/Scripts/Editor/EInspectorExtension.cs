using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using System.Reflection;
using Unity.Mathematics;
using UnityEngine;
using static System.Activator;

namespace UnityEditor.Extensions
{
    [CustomEditor(typeof(ScriptableObject),editorForChildClasses:true,isFallback = true),CanEditMultipleObjects]
    public class EScriptableExtension : EInspectorExtension {}
    
    [CustomEditor(typeof(MonoBehaviour), editorForChildClasses:true,isFallback = true),CanEditMultipleObjects]
   public class EInspectorExtension : Editor 
    {
        private List<ButtonAttributeData> inspectorMethods;
        protected virtual void OnEnable()
        {
            inspectorMethods = UInspectorExtension.GetInspectorMethods(target);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (inspectorMethods.Count <= 0)
                return;
            
            EditorGUILayout.BeginVertical();
            foreach (var data in inspectorMethods)
            {
                if(!data.attribute.IsElementVisible(target))
                    continue;

                var undo = data.attribute.undo;
                EditorGUILayout.BeginVertical();
                if (data.parameters.Length > 0)
                {
                    EditorGUILayout.LabelField(data.method.Name, EditorStyles.boldLabel);   
                    foreach (var parameter in data.parameters)
                    {
                        var key = parameter.type;
                        switch (UInspectorExtension.LookUp(key))
                        {
                            case EButtonParameters.Float: parameter.value = EditorGUILayout.FloatField(parameter.name,(float)parameter.value); break;
                            case EButtonParameters.Integer: parameter.value = EditorGUILayout.IntField(parameter.name,(int)parameter.value); break;
                            case EButtonParameters.String: parameter.value = EditorGUILayout.TextField(parameter.name,(string)parameter.value); break;
                            case EButtonParameters.Vector3: parameter.value = EditorGUILayout.Vector3Field(parameter.name,(Vector3)parameter.value); break;
                            case EButtonParameters.Object: parameter.value = EditorGUILayout.ObjectField((UnityEngine.Object)parameter.value,parameter.type,true); break;
                            default: case EButtonParameters.NotSupported: EditorGUILayout.LabelField($"Not Supported Type {key}");break;
                        }
                    }
                    
                    if (GUILayout.Button("Execute"))
                    {
                        foreach (var target in targets)
                            data.method.Invoke(target,data.parameters.Select(p=>p.value).ToArray());
                        if(undo)
                            Undo.RegisterCompleteObjectUndo(targets,"Button Click");
                        return;
                    }
                }
                else
                {
                    if (GUILayout.Button(data.method.Name))
                    {
                        foreach (var target in targets)
                            data.method.Invoke(target,null);
                        if(undo)
                            Undo.RegisterCompleteObjectUndo(targets,"Button Click");
                        return;
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
        }
    }
}