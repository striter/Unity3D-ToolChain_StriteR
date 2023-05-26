using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace UnityEditor.Extensions
{ 
    [CustomEditor(typeof(MonoBehaviour), true)]
    public class EInspectorExtension : Editor
    {
        private MethodInfo[] clickMethods;
        private readonly Type kButtonMethodType = typeof(ButtonAttribute);
        private void OnEnable()
        {
            clickMethods = target.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                .Collect(p => p.GetCustomAttributes().Any(p => p.GetType() == kButtonMethodType)).ToArray();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (clickMethods.Length <= 0)
                return;
            
            EditorGUILayout.BeginVertical();
            GUILayout.Label("Buttons",UEGUIStyle_Window.m_TitleLabel);
            foreach (var method in clickMethods)
            {
                if (GUILayout.Button(method.Name))
                    method.Invoke(target,null);
            }
            EditorGUILayout.EndVertical();
        }
    }
}