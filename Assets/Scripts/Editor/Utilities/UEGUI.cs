using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Linq.Extensions;
using Unity.Mathematics;

namespace UnityEditor.Extensions
{
    public static class UEGUI
    {
        public static bool IsExpanded(this SerializedProperty _property,float _comparer =0)
        {
            _comparer = _comparer > 0 ? _comparer : EditorGUIUtility.singleLineHeight;
            return EditorGUI.GetPropertyHeight(_property) > _comparer;
        }
        
        public static object GetFieldValue(this SerializedProperty _property)
        {
            var fieldInfo = GetFieldInfo(_property, out var targetObject);
            return fieldInfo.GetValue(targetObject);
        }
        
        public static FieldInfo GetFieldInfo(this SerializedProperty _property,out object _parentObject)
        {
            _parentObject = _property.serializedObject.targetObject;
            var paths = _property.propertyPath.Split('.');
            object targetObject = _property.serializedObject.targetObject;
            FieldInfo fieldInfo = null;
            var array = false;
            foreach (var fieldName in paths)
            {

                var type = targetObject.GetType();
                if (type.IsArray || type.IsGenericType)
                {
                    array = true;
                    continue;
                }

                if (array)
                {
                    var start = fieldName.IndexOf('[') + 1;
                    var index = int.Parse(fieldName.Substring(start, fieldName.Length - start - 1));
                    fieldInfo = type.GetElementType().GetField("Data");
                    targetObject = ((Array) targetObject).GetValue(index);
                }
                
                else
                {
                    fieldInfo = targetObject.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    _parentObject = targetObject;
                    targetObject = fieldInfo.GetValue(targetObject);
                }
            }
            return fieldInfo;
        }
        
        public static IEnumerable<MethodInfo> AllMethods(this SerializedProperty _property)=>_property.serializedObject.targetObject.GetType().GetMethods(BindingFlags.Instance |  BindingFlags.Public | BindingFlags.NonPublic);

        public static IEnumerable<(FieldInfo,object)> AllRelativeFields(this SerializedProperty _property,BindingFlags _flags = BindingFlags.Instance |  BindingFlags.Public | BindingFlags.NonPublic)
        {
            var paths = _property.propertyPath.Split('.');
            object targetObject = _property.serializedObject.targetObject;
            var targetType = targetObject.GetType();
            for(var i=0;i< paths.Length - 1; i++)       //Iterate till it reaches the root
            {
                var pathName = paths[i];
                if (targetType.IsArray || targetType.IsGenericType)
                {
                    i++;
                    var indexString = paths[i];
                    var start = indexString.IndexOf('[') + 1;
                    var index = int.Parse(indexString.Substring(start, indexString.Length - start - 1));
                    targetType = targetType.GetElementType();
                    targetObject = ((Array)targetObject).GetValue(index);
                }
                else
                {
                    FieldInfo targetField = targetType.GetField(pathName, _flags);
                    targetType = targetField.FieldType;
                    targetObject = targetField.GetValue(targetObject);
                }
            }

            foreach (var subfield in targetType.GetFields(_flags))
            {
                paths[^1] = subfield.Name;
                var propertyPath =  string.Join(".", paths, 0, paths.Length);
                var property = _property.serializedObject.FindProperty(propertyPath);
                if(property==null)
                    continue;
                
                if(property.propertyType == SerializedPropertyType.ObjectReference)
                    yield return (subfield, property.objectReferenceValue == null ? null : property.objectReferenceValue);
                
                yield return (subfield, subfield.GetValue(targetObject));
            }
        }
        public static bool EditorApplicationPlayingCheck()
        {
            if (Application.isPlaying)
            {
                GUILayout.TextArea("<Color=#FF0000>Editor Window Not Available In Play Mode! </Color>", UEGUIStyle_Window.m_ErrorLabel);
                return false;
            }
            return true;
        }
    }
    public static class GUILayout_Extend
    {
        public static T[] ArrayField<T>(T[] _src, string _title = "",Func<int,string> _getElementName=null, bool _allowSceneObjects = false) where T : UnityEngine.Object
        {
            GUILayout.BeginVertical();
            if (_title != "")
                EditorGUILayout.LabelField(_title, UEGUIStyle_Window.m_TitleLabel);
            _src ??= Array.Empty<T>();
            var length = Mathf.Clamp(EditorGUILayout.IntField("Array Length", _src.Length), 0, 128);
            if (length != _src.Length)
                _src = _src.Resize(length);

            var type = typeof(T);
            var modifiedField = _src.DeepCopy();
            for (var i = 0; i < modifiedField.Length; i++)
            {
                GUILayout.BeginHorizontal();
                string name = _getElementName == null ? $"  Element {i}" : _getElementName(i);
                EditorGUILayout.LabelField(name);
                modifiedField[i] = (T)EditorGUILayout.ObjectField(modifiedField[i], type, _allowSceneObjects);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            for (var i = 0; i < modifiedField.Length; i++)
                if (modifiedField[i] != _src[i])
                    return modifiedField;
            return _src;
        }
    }
    public static class GUITransformHandles
    {
        static SerializedProperty m_PositionProperty;
        static SerializedProperty m_RotationProperty;
        static ValueChecker<Vector3> m_PositionChecker = new ValueChecker<Vector3>(Vector3.zero);
        static ValueChecker<Quaternion> m_RotationChecker = new ValueChecker<Quaternion>(Quaternion.identity);
        public static void Begin(SerializedProperty _positionProperty, SerializedProperty _rotationProperty=null)
        {
            if (m_PositionProperty != null)
                End(true);
            m_PositionProperty = _positionProperty;
            m_RotationProperty = _rotationProperty;
            if (m_RotationProperty != null)
            {
                if (m_RotationProperty.propertyType == SerializedPropertyType.Quaternion)
                    m_RotationChecker.Check(m_RotationProperty.quaternionValue.normalized);
                else
                    m_RotationChecker.Check(Quaternion.LookRotation(m_RotationProperty.vector3Value.normalized));
            }

            Undo.RecordObject(m_PositionProperty.serializedObject.targetObject, "Transform Modify Begin");
            SceneView.duringSceneGui += OnSceneGUI;
            Tools.current = Tool.None;
            SceneView.lastActiveSceneView.pivot = m_PositionChecker.m_Value;
        }
        public static void End(bool _apply)
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            if (_apply)
                Undo.RecordObject(m_PositionProperty.serializedObject.targetObject, "Transform End");
            m_PositionProperty = null;
            m_RotationProperty = null;
        }
        static void OnSceneGUI(SceneView _sceneView)
        {
            try
            {
                if (Tools.current != Tool.None || Event.current.keyCode == KeyCode.Escape)
                {
                    End(true);
                    return;
                }
                Handles.Label(m_PositionChecker.m_Value, "Transforming", UEGUIStyle_SceneView.m_TitleLabel);
                m_PositionChecker.Check(Handles.DoPositionHandle(m_PositionChecker.m_Value, m_RotationChecker.m_Value));
                m_PositionProperty.vector3Value = m_PositionChecker.m_Value;

                if(m_RotationProperty !=null)
                {
                    m_RotationChecker.Check(Handles.DoRotationHandle(m_RotationChecker.m_Value,m_PositionChecker.m_Value));
                    if (m_RotationProperty.propertyType == SerializedPropertyType.Quaternion)
                        m_RotationProperty.quaternionValue = m_RotationChecker.m_Value;
                    else
                        m_RotationProperty.vector3Value = m_RotationChecker.m_Value * Vector3.forward;
                }
                m_PositionProperty.serializedObject.ApplyModifiedProperties();
            }
            catch
            {
                End(false);
            }
        }
    }
}