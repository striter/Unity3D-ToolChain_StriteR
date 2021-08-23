using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace TEditor
{
    public static class HorizontalScope
    {
        static Vector2 m_StartPos;
        static Vector2 m_Offset;
        public static float m_CurrentY { get; private set; }
        public static Vector2 m_CurrentPos => m_StartPos + m_Offset;
        public static void Begin(float _startX, float _startY, float _startSizeY)
        {
            m_CurrentY = _startSizeY;
            m_StartPos = new Vector2(_startX, _startY);
            m_Offset = Vector2.zero;
        }
        public static Rect NextRect(float _spacingX, float _sizeX)
        {
            Vector2 originOffset = m_Offset;
            m_Offset.x += _sizeX + _spacingX;
            return new Rect(m_StartPos + originOffset, new Vector2(_sizeX, m_CurrentY));
        }
        public static void NextLine(float _spacingY, float _sizeY)
        {
            m_Offset.y += m_CurrentY + _spacingY;
            m_CurrentY = _sizeY;
            m_Offset.x = 0;
        }
    }
    public static class UEGUI
    {
        public static FieldInfo GetFieldInfo(this SerializedProperty _property)
        {
            string[] paths = _property.propertyPath.Split('.');
            object targetObject = _property.serializedObject.targetObject;
            FieldInfo fieldInfo = null;
            foreach (var fieldName in paths)
            {
                fieldInfo = targetObject.GetType().GetField(fieldName);
                targetObject = fieldInfo.GetValue(targetObject);
            }
            return fieldInfo;
        }
        public static IEnumerable<(FieldInfo, object)> AllRelativeFields(this SerializedProperty _property)
        {
            foreach (var fieldInfos in _property.FieldsSearch())
            {
                foreach (var subfield in fieldInfos.Item1.GetFields())
                    yield return (subfield, subfield.GetValue(fieldInfos.Item2));
            }
        }
        public static Object GetPropertyField(this SerializedProperty _property,Action<FieldInfo> _OnEachField=null)
        {
            Object targetObject=null;
            foreach (var tuple in _property.FieldsSearch())
                targetObject = tuple.Item2;
            return targetObject;
        }
        public static IEnumerable<(Type,object)> FieldsSearch(this SerializedProperty _property)
        {
            string[] paths = _property.propertyPath.Split('.');
            object targetObject = _property.serializedObject.targetObject;
            Type targetType = targetObject.GetType();
            for(int i=0;i< paths.Length; i++)
            {
                yield return (targetType, targetObject);
                if (i==paths.Length-1||paths[i] == "Array")
                    break;
                FieldInfo targetField = targetType.GetField(paths[i], BindingFlags.Instance |  BindingFlags.Public | BindingFlags.NonPublic);
                targetType = targetField.FieldType;
                targetObject = targetField.GetValue(targetObject);
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
    public static class GUI_Extend
    {
        public static bool VectorField(SerializedProperty _property, Rect _rect, GUIContent _content)
        {
            if (EditorGUI.EndChangeCheck())
            {
                switch (_property.propertyType)
                {
                    default: EditorGUI.LabelField(_rect, "<Color=#FF0000>Invalid Property Type!</Color>", UEGUIStyle_SceneView.m_ErrorLabel); return false;
                    case SerializedPropertyType.Vector2: _property.vector2Value = EditorGUI.Vector2Field(_rect, _content, _property.vector2Value); break;
                    case SerializedPropertyType.Vector3: _property.vector3Value = EditorGUI.Vector3Field(_rect, _content, _property.vector3Value); break;
                    case SerializedPropertyType.Vector4: _property.vector4Value = EditorGUI.Vector4Field(_rect, _content, _property.vector4Value); break;
                }
                _property.serializedObject.ApplyModifiedProperties();
                return true;
            }
            return false;
        }
    }
    public static class GUILayout_Extend
    {
        public static T[] ArrayField<T>(T[] _src, string _context = "", bool _allowSceneObjects = false) where T : UnityEngine.Object
        {
            GUILayout.BeginVertical();
            if (_context != "")
                EditorGUILayout.LabelField(_context, UEGUIStyle_Window.m_TitleLabel);
            int length = Mathf.Clamp(EditorGUILayout.IntField("Length", _src.Length), 1, 128);
            if (length != _src.Length)
                _src = _src.Resize(length);

            Type type = typeof(T);
            T[] modifiedField = _src.DeepCopy();
            for (int i = 0; i < modifiedField.Length; i++)
                modifiedField[i] = (T)EditorGUILayout.ObjectField(modifiedField[i], type, _allowSceneObjects);
            GUILayout.EndVertical();
            for (int i = 0; i < modifiedField.Length; i++)
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