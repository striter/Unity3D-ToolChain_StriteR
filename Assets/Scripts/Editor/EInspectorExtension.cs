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
    [CustomEditor(typeof(ScriptableObject),true),CanEditMultipleObjects]
    public class EScriptableExtension : EInspectorExtension {}
    
    [CustomEditor(typeof(MonoBehaviour), true),CanEditMultipleObjects]
    public class EInspectorExtension : Editor
    {
        public enum EButtonParameters
        {
            NotSupported,
            String,
            Float,
            Integer,
            Vector3,
            Object,
        }

        private Dictionary<Type, EButtonParameters> kTypeLookupTable = new Dictionary<Type, EButtonParameters>()
        {
            {typeof(string),EButtonParameters.String},
            {typeof(float),EButtonParameters.Float},
            {typeof(int),EButtonParameters.Integer},
            {typeof(float3),EButtonParameters.Vector3},
            {typeof(Vector3),EButtonParameters.Vector3},
        };

        EButtonParameters LookUp(Type _type)
        {
            if(_type.IsSubclassOf(typeof(UnityEngine.Object)))
                return EButtonParameters.Object;
            
            return kTypeLookupTable.GetValueOrDefault(_type, EButtonParameters.NotSupported);
        }

        public class ParameterData
        {
            public Type type;
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

                for (var i = 0; i < parameters.Length; i++)
                {
                    var parameter = parameters[i];
                    buttonData.parameters[i] = new ParameterData()
                    {
                        type = parameter.ParameterType,
                        name = parameter.Name,
                        value = parameter.HasDefaultValue ? parameter.DefaultValue : (parameter.ParameterType.IsClass ? null : Activator.CreateInstance(parameter.ParameterType)),
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
                if(data.attribute is ButtonAttribute button && !button.IsElementVisible(target))
                    continue;

                EditorGUILayout.BeginVertical();
                if (data.parameters.Length > 0)
                {
                    EditorGUILayout.LabelField(data.method.Name, EditorStyles.boldLabel);   
                    foreach (var parameter in data.parameters)
                    {
                        var key = parameter.type;
                        switch ( LookUp(key))
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
                        data.method.Invoke(target,data.parameters.Select(p=>p.value).ToArray());
                        Undo.RegisterCompleteObjectUndo(target,"Button Click");
                        return;
                    }
                }
                else
                {
                    if (GUILayout.Button(data.method.Name))
                    {
                        data.method.Invoke(target,null);
                        Undo.RegisterCompleteObjectUndo(target,"Button Click");
                        return;
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
        }
    }
}