using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

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
        public static IEnumerable<KeyValuePair<FieldInfo, object>> AllRelativeFields(this SerializedProperty _property)
        {
            string[] paths = _property.propertyPath.Split('.');
            object targetObject = _property.serializedObject.targetObject;
            Type targetType = targetObject.GetType();
            for(int i=0;i< paths.Length-1; i++)
            {
                FieldInfo targetField = targetObject.GetType().GetField(paths[i]);
                targetType = targetField.FieldType;
                targetObject = targetField.GetValue(targetObject);
            }
            foreach (var subFieldInfo in targetType.GetFields())
                yield return new KeyValuePair<FieldInfo, object>(subFieldInfo, subFieldInfo.GetValue(targetObject));
            yield break;
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
            T[] modifiedField = _src.Copy();
            for (int i = 0; i < modifiedField.Length; i++)
                modifiedField[i] = (T)EditorGUILayout.ObjectField(modifiedField[i], type, _allowSceneObjects);
            GUILayout.EndVertical();
            for (int i = 0; i < modifiedField.Length; i++)
                if (modifiedField[i] != _src[i])
                    return modifiedField;
            return _src;
        }
    }
}