using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Linq.Extensions;
using Unity.Mathematics;

namespace UnityEditor.Extensions
{
    public static class GUILayout_HorizontalScope
    {
        static float2 m_StartPos;
        static float2 m_Offset;
        private static float2 m_CurrentRectSize;
        public static void Begin(float2 _initialPos,float _horizontalSize)
        {
            m_StartPos = _initialPos;
            m_CurrentRectSize.x = _horizontalSize;
            m_CurrentRectSize.y = 0;
            m_Offset = 0;
        }
        public static void Begin(Rect _rect)
        {
            m_Offset = 0;
            m_StartPos = _rect.position;
            m_CurrentRectSize.x = _rect.size.x;
            m_CurrentRectSize.y = 0;
        }

        public static Rect NewLine(float _spacing,float _sizeY,float _padding = 0f)
        {
            m_Offset.y += m_CurrentRectSize.y;
            m_Offset.x = _padding;
            m_Offset.y += _spacing;
            m_CurrentRectSize.y = _sizeY;
            return new Rect(m_StartPos + m_Offset, m_CurrentRectSize);
        }

        public static Rect NextRect(float _spacingX, float _sizeX)
        {
            m_Offset.x += _spacingX;
            var rect = new Rect(m_StartPos + m_Offset, new Vector2(_sizeX, m_CurrentRectSize.y));
            m_Offset.x += _sizeX;
            return rect;
        }

        public static Rect NextRectNormalized(float _spacing, float _sizeXNormalized)=> NextRect(_spacing, _sizeXNormalized * m_CurrentRectSize.x);
        public static Rect FinishLineRect(float _spacing = 0)=> NextRect(_spacing, m_CurrentRectSize.x - m_Offset.x - _spacing);
    }
    
    public static class HorizontalScope
    {
        static Vector2 m_StartPos;
        static Vector2 m_Offset;
        public static float m_CurrentY { get; private set; }
        public static Vector2 m_CurrentPos => m_StartPos + m_Offset;
        private static float m_SizeX;
        private static float m_CurrentSizeX;
        public static void Begin(float _startX, float _startY,float _currentY,float _horizontalSize = -1)
        {
            m_StartPos = new Vector2(_startX, _startY);
            m_Offset = Vector2.zero;
            m_SizeX = _horizontalSize;
            m_CurrentY = _currentY;
            m_CurrentSizeX = 0f;
        }
        public static Rect NextRect(float _spacingX, float _sizeX)
        {
            Vector2 originOffset = m_Offset;
            m_Offset.x += _sizeX + _spacingX;
            m_CurrentSizeX += _sizeX + _spacingX;
            return new Rect(m_StartPos + originOffset, new Vector2(_sizeX, m_CurrentY));
        }
        public static Rect NextLine(float _spacingY, float _sizeY)
        {
            m_CurrentY += 0;
            m_Offset.y += m_CurrentY + _spacingY;
            m_CurrentY = _sizeY;
            m_Offset.x = 0;
            return NextRect(0,m_SizeX);
        }

        public static Rect Finish(float _spacing) => NextRect(_spacing, m_SizeX - m_CurrentSizeX - _spacing);
    }
    public static class UEGUI
    {
        public static bool IsExpanded(SerializedProperty _property,float _comparer =0)
        {
            _comparer = _comparer > 0 ? _comparer : EditorGUIUtility.singleLineHeight;
            return EditorGUI.GetPropertyHeight(_property) > _comparer;
        }
        
        public static FieldInfo GetFieldInfo(this SerializedProperty _property,out object _parentObject)
        {
            _parentObject = _property.serializedObject.targetObject;
            string[] paths = _property.propertyPath.Split('.');
            object targetObject = _property.serializedObject.targetObject;
            FieldInfo fieldInfo = null;
            bool array = false;
            foreach (var fieldName in paths)
            {

                var type = targetObject.GetType();
                if (type.IsArray)
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
            string[] paths = _property.propertyPath.Split('.');
            object targetObject = _property.serializedObject.targetObject;
            Type targetType = targetObject.GetType();
            for(int i=0;i< paths.Length - 1; i++)       //Iterate till it reaches the root
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
                if (propertyPath == _property.propertyPath)
                    continue;
                
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
        public static T[] ArrayField<T>(T[] _src, string _title = "",Func<int,string> _getElementName=null, bool _allowSceneObjects = false) where T : UnityEngine.Object
        {
            GUILayout.BeginVertical();
            if (_title != "")
                EditorGUILayout.LabelField(_title, UEGUIStyle_Window.m_TitleLabel);
            if (_src == null)
                _src = Array.Empty<T>();
            int length = Mathf.Clamp(EditorGUILayout.IntField("Array Length", _src.Length), 0, 128);
            if (length != _src.Length)
                _src = _src.Resize(length);

            Type type = typeof(T);
            T[] modifiedField = _src.DeepCopy();
            for (int i = 0; i < modifiedField.Length; i++)
            {
                GUILayout.BeginHorizontal();
                string name = _getElementName == null ? $"  Element {i}" : _getElementName(i);
                EditorGUILayout.LabelField(name);
                modifiedField[i] = (T)EditorGUILayout.ObjectField(modifiedField[i], type, _allowSceneObjects);
                GUILayout.EndHorizontal();
            }
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