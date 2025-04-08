using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Mathematics;
using UnityEngine;

namespace UnityEditor.Extensions
{
    public enum EGUIFieldType
    {
        NotSupported,
        String,
        Integer,
        Float,
        Float2Field,
        Float3Field,
        Float4Field,
        Object,
        Struct,
    }
    public static class UEGUIExtension
    {
        private static Dictionary<Type, EGUIFieldType> kTypeLookupTable = new Dictionary<Type, EGUIFieldType>()
        {
            {typeof(string),EGUIFieldType.String},
            {typeof(float),EGUIFieldType.Float},
            {typeof(int),EGUIFieldType.Integer},
            {typeof(float2),EGUIFieldType.Float2Field},
            {typeof(Vector2),EGUIFieldType.Float2Field},
            {typeof(float3),EGUIFieldType.Float3Field},
            {typeof(Vector3),EGUIFieldType.Float3Field},
            {typeof(float4),EGUIFieldType.Float4Field},
            {typeof(Vector4),EGUIFieldType.Float4Field},
        };
        private static Dictionary<EGUIFieldType, object> kDefaultValue = new Dictionary<EGUIFieldType, object>()
        {
            {EGUIFieldType.Integer,0},
            {EGUIFieldType.String,""},
            {EGUIFieldType.Float,0f},
            {EGUIFieldType.Float2Field,Vector2.zero},
            {EGUIFieldType.Float3Field,Vector3.zero},
            {EGUIFieldType.Float4Field,Vector4.zero},
            {EGUIFieldType.Object,null},
        };
        
        static EGUIFieldType LookUpType(Type _type)
        {
            if(_type.IsSubclassOf(typeof(UnityEngine.Object)))
                return EGUIFieldType.Object;
                
            var lookUpType = kTypeLookupTable.GetValueOrDefault(_type, EGUIFieldType.NotSupported);
            if (lookUpType == EGUIFieldType.NotSupported && _type.IsValueType)
                return EGUIFieldType.Struct;
            return lookUpType;
        }
        
        public static void Field(Rect position,ParameterData parameter)
        {
            var lookup = LookUpType(parameter.type);
            var value = parameter.value ?? kDefaultValue[lookup];
            switch (lookup)
            {
                case EGUIFieldType.Float: parameter.value = EditorGUI.FloatField(position,parameter.name,(float)value); break;
                case EGUIFieldType.Integer: parameter.value = EditorGUI.IntField(position,parameter.name,(int)value); break;
                case EGUIFieldType.String: parameter.value = EditorGUI.TextField(position,parameter.name,(string)value); break;
                case EGUIFieldType.Float2Field: parameter.value = EditorGUI.Vector2Field(position,parameter.name,(Vector2)value); break;
                case EGUIFieldType.Float3Field: parameter.value = EditorGUI.Vector3Field(position,parameter.name,(Vector3)value); break;
                case EGUIFieldType.Float4Field: parameter.value = EditorGUI.Vector4Field(position,parameter.name,(Vector4)value); break;
                case EGUIFieldType.Object: parameter.value = EditorGUI.ObjectField(position,parameter.name,(UnityEngine.Object)value,parameter.type,true); break;
                // case EButtonParameters.Struct: parameter.value = DrawStruct(parameter.value,parameter.name); break;
                default: 
                    EditorGUI.LabelField(position,$"Not Supported Type {parameter.type} - {lookup}");break;
            }
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

        public static object LayoutField(object _value,Type _type,string _name)
        {
            var lookup = LookUpType(_type);
            if (_value == null)
            {
                if (!kDefaultValue.TryGetValue(lookup, out _value) && lookup == EGUIFieldType.Struct)
                    _value = Activator.CreateInstance(_type);
            }
            var value = _value ?? kDefaultValue[lookup];
            switch (lookup)
            {
                case EGUIFieldType.Float: _value = EditorGUILayout.FloatField(_name,(float)value); break;
                case EGUIFieldType.Integer: _value = EditorGUILayout.IntField(_name,(int)value); break;
                case EGUIFieldType.String: _value = EditorGUILayout.TextField(_name,(string)value); break;
                case EGUIFieldType.Float2Field: _value = (float2)EditorGUILayout.Vector2Field(_name,(float2)value); break;
                case EGUIFieldType.Float3Field: _value = (float3)EditorGUILayout.Vector3Field(_name,(float3)value); break;
                case EGUIFieldType.Float4Field: _value = (float4)EditorGUILayout.Vector4Field(_name,(float4)value); break;
                case EGUIFieldType.Object: _value = EditorGUILayout.ObjectField((UnityEngine.Object)value,_type,true); break;
                case EGUIFieldType.Struct: _value = StructLayoutField(_value,_name); break;
                default: case EGUIFieldType.NotSupported: EditorGUILayout.LabelField($"Not Supported Type {_type}");break;
            }
            return _value;
        }

        private static Dictionary<string, bool> kFoldout = new();
        public static object StructLayoutField(object value, string label = null) 
        {
            if(!kFoldout.ContainsKey(label))
                kFoldout.Add(label,false);
            if (!string.IsNullOrEmpty(label))
                kFoldout[label] = EditorGUILayout.Foldout(kFoldout[label],label);

            if (!kFoldout[label])
                return value;
            
            EditorGUI.indentLevel++;
            value = StructFieldRecursive(value);
            EditorGUI.indentLevel--;
        
            return value;
        }

        private static object StructFieldRecursive(object _struct)
        {
            var fields = _struct.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                if (field.IsNotSerialized)
                    continue;
                
                var newValue = LayoutField(field.GetValue(_struct),field.FieldType, field.Name);
                field.SetValue(_struct, newValue);
            }
            return _struct;
        }
    }
}