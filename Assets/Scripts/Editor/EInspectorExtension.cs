using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
                        parameter.value = UEGUIExtension.LayoutField(parameter.value,parameter.type,parameter.name);
                    
                    if (GUILayout.Button("Execute"))
                    {
                        foreach (var target in targets)
                            data.method.Invoke(target,data.parameters.Select(p=>UEGUIExtension.Reformat(p.type,p.value)).ToArray());
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