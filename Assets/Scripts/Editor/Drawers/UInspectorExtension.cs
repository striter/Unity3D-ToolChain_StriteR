using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Mathematics;
using UnityEngine;

namespace UnityEditor.Extensions
{
    public enum EButtonParameters
    {
        NotSupported,
        String,
        Integer,
        Float,
        Float2Field,
        Float3Field,
        Float4Field,
        Object,
    }

    public class ParameterData
    {
        public Type type;
        public string name;
        public object value;
    }
    public struct ButtonAttributeData
    {
        public InspectorButtonAttribute attribute;
        public MethodInfo method;
        public ParameterData[] parameters;
    }

    public static class UInspectorExtension
    {
        private static Dictionary<Type, EButtonParameters> kTypeLookupTable = new Dictionary<Type, EButtonParameters>()
        {
            {typeof(string),EButtonParameters.String},
            {typeof(float),EButtonParameters.Float},
            {typeof(int),EButtonParameters.Integer},
            {typeof(float2),EButtonParameters.Float2Field},
            {typeof(Vector2),EButtonParameters.Float2Field},
            {typeof(float3),EButtonParameters.Float3Field},
            {typeof(Vector3),EButtonParameters.Float3Field},
            {typeof(float4),EButtonParameters.Float4Field},
            {typeof(Vector4),EButtonParameters.Float4Field},
        };
        private static Dictionary<EButtonParameters, object> kDefaultValue = new Dictionary<EButtonParameters, object>()
        {
            {EButtonParameters.Integer,0},
            {EButtonParameters.String,""},
            {EButtonParameters.Float,0f},
            {EButtonParameters.Float2Field,Vector2.zero},
            {EButtonParameters.Float3Field,Vector3.zero},
            {EButtonParameters.Float4Field,Vector4.zero},
            {EButtonParameters.Object,null},
        };

        static EButtonParameters LookUpType(Type _type)
        {
            if(_type.IsSubclassOf(typeof(UnityEngine.Object)))
                return EButtonParameters.Object;
            
            return kTypeLookupTable.GetValueOrDefault(_type, EButtonParameters.NotSupported);
        }

        public static object Reformat(Type _type, object _value)
        {
            if(_type == typeof(float2))
                return new float2((Vector2)_value);
            
            if(_type == typeof(float3))
                return new float3((Vector3)_value);
            
            if(_type == typeof(float4))
                return new float4((Vector4)_value);
            return _value;
        }

        public static void GUILayoutField(ParameterData parameter)
        {
            var lookup = LookUpType(parameter.type);
            var value = parameter.value ?? kDefaultValue[lookup];
            switch (lookup)
            {
                case EButtonParameters.Float: parameter.value = EditorGUILayout.FloatField(parameter.name,(float)value); break;
                case EButtonParameters.Integer: parameter.value = EditorGUILayout.IntField(parameter.name,(int)value); break;
                case EButtonParameters.String: parameter.value = EditorGUILayout.TextField(parameter.name,(string)value); break;
                case EButtonParameters.Float2Field: parameter.value = EditorGUILayout.Vector2Field(parameter.name,(Vector2)value); break;
                case EButtonParameters.Float3Field: parameter.value = EditorGUILayout.Vector3Field(parameter.name,(Vector3)value); break;
                case EButtonParameters.Float4Field: parameter.value = EditorGUILayout.Vector4Field(parameter.name,(Vector4)value); break;
                case EButtonParameters.Object: parameter.value = EditorGUILayout.ObjectField((UnityEngine.Object)value,parameter.type,true); break;
                default: case EButtonParameters.NotSupported: EditorGUILayout.LabelField($"Not Supported Type {parameter.type}");break;
            }
        }

        public static void LayoutField(Rect position,ParameterData parameter)
        {
            var lookup = LookUpType(parameter.type);
            var value = parameter.value ?? kDefaultValue[lookup];
            switch (lookup)
            {
                case EButtonParameters.Float: parameter.value = EditorGUI.FloatField(position,parameter.name,(float)value); break;
                case EButtonParameters.Integer: parameter.value = EditorGUI.IntField(position,parameter.name,(int)value); break;
                case EButtonParameters.String: parameter.value = EditorGUI.TextField(position,parameter.name,(string)value); break;
                case EButtonParameters.Float2Field: parameter.value = EditorGUI.Vector2Field(position,parameter.name,(Vector2)value); break;
                case EButtonParameters.Float3Field: parameter.value = EditorGUI.Vector3Field(position,parameter.name,(Vector3)value); break;
                case EButtonParameters.Float4Field: parameter.value = EditorGUI.Vector4Field(position,parameter.name,(Vector4)value); break;
                case EButtonParameters.Object: parameter.value = EditorGUI.ObjectField(position,parameter.name,(UnityEngine.Object)value,parameter.type,true); break;
                default: case EButtonParameters.NotSupported: EditorGUI.LabelField(position,$"Not Supported Type {parameter.type}");break;
            }
        }
        
        public static List<ButtonAttributeData> GetInspectorMethods(object _target)
        {
            List<ButtonAttributeData> kClickMethods = new();
            foreach (var (method,attribute) in _target.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Select(p=>(p,p.GetCustomAttribute<InspectorButtonAttribute>(true))))
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
                kClickMethods.Add(buttonData); 
            }

            return kClickMethods;
        }
    }
}