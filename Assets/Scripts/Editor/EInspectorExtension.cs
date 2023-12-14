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
        public enum EButtonParameters
        {
            NotSupported,
            String,
            Float,
            Integer,
        }

        private Dictionary<Type, EButtonParameters> kTypeLookupTable = new Dictionary<Type, EButtonParameters>()
        {
            {typeof(string),EButtonParameters.String},
            {typeof(float),EButtonParameters.Float},
            {typeof(int),EButtonParameters.Integer},
        };

        public class ParameterData
        {
            public EButtonParameters type;
            public string name;
            public object value;
        }
        public struct ButtonAttributeData
        {
            public Attribute attribute;
            public MethodInfo method;
            public ParameterData[] parameters;
        }

        private List<ButtonAttributeData> clickMethods = new List<ButtonAttributeData>();
        private void OnEnable()
        {
            foreach (var (method,attribute) in target.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Select(p=>(p,p.GetCustomAttribute<ButtonAttribute>(true))))
            {
                if (attribute == null)
                    continue;

                var parameters = method.GetParameters();
                var buttonData = new ButtonAttributeData()
                {
                    attribute = attribute,
                    parameters = new ParameterData[parameters.Length],
                    method = method,
                };

                for (int i = 0; i < parameters.Length; i++)
                {
                    var parameter = parameters[i];
                    buttonData.parameters[i] = new ParameterData()
                    {
                        type = kTypeLookupTable[parameter.ParameterType],
                        name = parameter.Name,
                        value = parameter.HasDefaultValue ? parameter.DefaultValue : Activator.CreateInstance(parameter.ParameterType),
                    };
                }
                clickMethods.Add(buttonData); 
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
            foreach (var data in clickMethods)
            {
                if(data.attribute is FoldoutButtonAttribute foldOutButton && !foldOutButton.IsElementVisible(target))
                    continue;

                
                EditorGUILayout.BeginVertical();
                GUILayout.Label(data.method.Name);
                
                if (data.parameters.Length > 0)
                {
                    
                    EditorGUILayout.BeginHorizontal();

                    foreach (var parameter in data.parameters)
                    {
                        var key = parameter.type;
                        switch (key)
                        {
                            case EButtonParameters.Float: parameter.value = EditorGUILayout.FloatField(parameter.name,(float)parameter.value); break;
                            case EButtonParameters.Integer: parameter.value = EditorGUILayout.IntField(parameter.name,(int)parameter.value); break;
                            case EButtonParameters.String: parameter.value = EditorGUILayout.TextField(parameter.name,(string)parameter.value); break;
                            case EButtonParameters.NotSupported:EditorGUILayout.LabelField("Not Supported Type");break;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                
                if (GUILayout.Button("Execute"))
                {
                    data.method.Invoke(target,data.parameters.Select(p=>p.value).ToArray());
                    Undo.RegisterCompleteObjectUndo(target,"Button Click");
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
        }
    }
}