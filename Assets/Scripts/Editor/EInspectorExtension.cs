using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace UnityEditor.Extensions
{ 
    [CustomEditor(typeof(MonoBehaviour), true)]
    public class EInspectorExtension : Editor
    {
        private readonly Type kButtonMethodType = typeof(ButtonAttribute);
        private List<KeyValuePair<MethodInfo,ButtonAttribute>> clickMethods = new List<KeyValuePair<MethodInfo, ButtonAttribute>>();
        private void OnEnable()
        {
            foreach (var (method,attribute) in target.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Select(p=>(p,p.GetCustomAttribute(kButtonMethodType))))
            {
                if (attribute == null)
                    continue;
                
                clickMethods.Add(new KeyValuePair<MethodInfo, ButtonAttribute>(method,attribute as ButtonAttribute));
            }
        }

        private void OnDisable()
        {
            clickMethods.Clear();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (clickMethods.Count <= 0)
                return;
            
            EditorGUILayout.BeginVertical();
            GUILayout.Label("Buttons",UEGUIStyle_Window.m_TitleLabel);
            foreach (var pair in clickMethods)
            {
                var method = pair.Key;
                if (pair.Value.IsElementVisible(target))
                {
                    if (GUILayout.Button(method.Name))
                    {
                        method.Invoke(target,null);
                        Undo.RegisterCompleteObjectUndo(target,"Button Click");
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }
    }
}