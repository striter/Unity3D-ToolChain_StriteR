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
        Vector2Field,
        Vector3Field,
        Vector4Field,
        Object,
        Struct,
    }
    public static class UEGUIExtension
    {
        private static Dictionary<string, bool> kStructFoldout = new();
        private static Dictionary<Type, EGUIFieldType> kTypeLookupTable = new() {
            {typeof(string),EGUIFieldType.String},
            {typeof(float),EGUIFieldType.Float},
            {typeof(int),EGUIFieldType.Integer},
            {typeof(float2),EGUIFieldType.Float2Field},
            {typeof(float3),EGUIFieldType.Float3Field},
            {typeof(float4),EGUIFieldType.Float4Field},
            {typeof(Vector2),EGUIFieldType.Vector2Field},
            {typeof(Vector3),EGUIFieldType.Vector3Field},
            {typeof(Vector4),EGUIFieldType.Vector4Field},
        };
        
        private static Dictionary<EGUIFieldType, object> kDefaultValue = new() {
            {EGUIFieldType.Integer,0},
            {EGUIFieldType.String,""},
            {EGUIFieldType.Float,0f},
            {EGUIFieldType.Float2Field,kfloat2.zero},
            {EGUIFieldType.Float3Field,kfloat3.zero},
            {EGUIFieldType.Float4Field,kfloat4.zero},
            {EGUIFieldType.Vector2Field,Vector2.zero},
            {EGUIFieldType.Vector3Field,Vector3.zero},
            {EGUIFieldType.Vector4Field,Vector4.zero},
            {EGUIFieldType.Object,null},
        };
        
        private static readonly BindingFlags kBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        
        static EGUIFieldType LookUpType(Type _type)
        {
            if(_type.IsSubclassOf(typeof(UnityEngine.Object)))
                return EGUIFieldType.Object;
                
            var lookUpType = kTypeLookupTable.GetValueOrDefault(_type, EGUIFieldType.NotSupported);
            if (lookUpType == EGUIFieldType.NotSupported && _type.IsValueType)
                return EGUIFieldType.Struct;
            return lookUpType;
        }
        
        static object GetValueOrDefault(object _value,Type _type,out EGUIFieldType _lookup)
        {
            _lookup = LookUpType(_type);
            if (_value == null)
            {
                if (!kDefaultValue.TryGetValue(_lookup, out _value) && _lookup == EGUIFieldType.Struct)
                    _value = Activator.CreateInstance(_type);
            }
            return _value;
        }

        public static bool CheckFoldout(string _value, Func<bool,bool> _checkFold = null)
        {
            if (string.IsNullOrEmpty(_value))
                return false;
            
            kStructFoldout.TryAdd(_value, false);
            if(_checkFold != null)
                kStructFoldout[_value] = _checkFold(kStructFoldout[_value]);
            return kStructFoldout[_value];
        }
        
        #region LayoutField
        public static object LayoutField(object _value,Type _type,string _name)
        {
            _value = GetValueOrDefault(_value,_type,out var lookup);
            switch (lookup)
            {
                case EGUIFieldType.Float: _value = EditorGUILayout.FloatField(_name,(float)_value); break;
                case EGUIFieldType.Integer: _value = EditorGUILayout.IntField(_name,(int)_value); break;
                case EGUIFieldType.String: _value = EditorGUILayout.TextField(_name,(string)_value); break;
                case EGUIFieldType.Float2Field: _value = (float2)EditorGUILayout.Vector2Field(_name,(float2)_value); break;
                case EGUIFieldType.Float3Field: _value = (float3)EditorGUILayout.Vector3Field(_name,(float3)_value); break;
                case EGUIFieldType.Float4Field: _value = (float4)EditorGUILayout.Vector4Field(_name,(float4)_value); break;
                case EGUIFieldType.Vector2Field: _value = EditorGUILayout.Vector2Field(_name,(Vector2)_value); break;
                case EGUIFieldType.Vector3Field: _value = EditorGUILayout.Vector3Field(_name,(Vector3)_value); break;
                case EGUIFieldType.Vector4Field: _value = EditorGUILayout.Vector4Field(_name,(Vector4)_value); break;
                case EGUIFieldType.Object: _value = EditorGUILayout.ObjectField((UnityEngine.Object)_value,_type,true); break;
                case EGUIFieldType.Struct: _value = StructLayoutField(_value,_name); break;
                default: case EGUIFieldType.NotSupported: EditorGUILayout.LabelField($"Not Supported Type {_type}");break;
            }
            return _value;
        }

        public static object StructLayoutField(object value, string label = null) 
        {
            if (CheckFoldout(label,(_value) => EditorGUILayout.Foldout(_value, label)))
                return value;
            
            EditorGUI.indentLevel++;
            value = StructLayoutFieldRecursive(value);
            EditorGUI.indentLevel--;
        
            return value;
        }

        private static object StructLayoutFieldRecursive(object _struct)
        {
            var fields = _struct.GetType().GetFields(kBindingFlags);
            foreach (var field in fields)
            {
                if (field.IsNotSerialized)
                    continue;
                
                var newValue = LayoutField(field.GetValue(_struct),field.FieldType, field.Name);
                field.SetValue(_struct, newValue);
            }
            return _struct;
        }
        #endregion
        
        #region Field

        private static float kStructTitleHeight = 12;
        private static float kFieldHeight = 18;
        private static float kFieldPadding = 2;
        private static float kDepthOffset = 18;
        public static float FieldHeight(object value,Type _type,string _name)
        {
            value = GetValueOrDefault(value,_type,out var lookup);
            switch (lookup)
            {
                case EGUIFieldType.Struct: return StructHeightRecursive(value,_name);
                default: return kFieldHeight;
            }
        }
        
        private static float StructHeightRecursive(object _value,string _name)
        {
            if (!CheckFoldout(_name))
                return kStructTitleHeight;

            var height = kStructTitleHeight + kFieldPadding;
            var fields = _value.GetType().GetFields(kBindingFlags);
            foreach (var field in fields)
            {
                if (field.IsNotSerialized)
                    continue;

                height += kFieldPadding + FieldHeight(field.GetValue(_value),field.FieldType,field.Name);
            }
            height -= kFieldPadding;
            return height;
        }
        
        public static object Field(Rect position,object _value,Type _type, string _name)
        {
            _value = GetValueOrDefault(_value,_type,out var lookup);
            switch (lookup)
            {
                case EGUIFieldType.Float: _value = EditorGUI.FloatField(position,_name,(float)_value); break;
                case EGUIFieldType.Integer: _value = EditorGUI.IntField(position,_name,(int)_value); break;
                case EGUIFieldType.String: _value = EditorGUI.TextField(position,_name,(string)_value); break;
                case EGUIFieldType.Float2Field: _value = (float2)EditorGUI.Vector2Field(position,_name,(float2)_value); break;
                case EGUIFieldType.Float3Field: _value = (float3)EditorGUI.Vector3Field(position,_name,(float3)_value); break;
                case EGUIFieldType.Float4Field: _value = (float4)EditorGUI.Vector4Field(position,_name,(float4)_value); break;
                case EGUIFieldType.Vector2Field: _value = EditorGUI.Vector2Field(position,_name,(Vector2)_value); break;
                case EGUIFieldType.Vector3Field: _value = EditorGUI.Vector3Field(position,_name,(Vector3)_value); break;
                case EGUIFieldType.Vector4Field: _value = EditorGUI.Vector4Field(position,_name,(Vector4)_value); break;
                case EGUIFieldType.Object: _value = EditorGUI.ObjectField(position,_name,(UnityEngine.Object)_value,_type,true); break;
                case EGUIFieldType.Struct: _value = StructField(position,_value,_name); break;
                default: 
                    EditorGUI.LabelField(position,$"Not Supported Type {_type} - {lookup}");break;
            }
            return _value;
        }

        private static object StructField(Rect position,object _value, string _name)
        {
            position = position.ResizeY(kStructTitleHeight);
            if (!CheckFoldout(_name,(value)=>EditorGUI.Foldout(position,value, _name)))
                return _value;

            var fields = _value.GetType().GetFields(kBindingFlags);
            position = position.MoveY().Collapse(new Vector2(kDepthOffset,0),new Vector2(1f,0f));
            foreach (var field in fields)
            {
                if (field.IsNotSerialized)
                    continue;
                position = position.MoveY(kFieldPadding);
                position = position.ResizeY(kFieldHeight);
                var newValue = Field(position,field.GetValue(_value),field.FieldType, field.Name);
                field.SetValue(_value, newValue);
                position = position.MoveY();
            }
            return _value;
        }

        #endregion
    }
}