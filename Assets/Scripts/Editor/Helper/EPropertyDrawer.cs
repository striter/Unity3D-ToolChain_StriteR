using System;
using UnityEditor;
using UnityEngine;

namespace TEditor
{
    [CustomPropertyDrawer(typeof(GLine))]
    public class GLinePropertyDrawer : PositionDirectionDrawer
    {
        protected override string PositionPropertyName => nameof(GLine.origin);
        protected override string DirectionPropertyName => nameof(GLine.direction);
    }
    [CustomPropertyDrawer(typeof(GRay))]
    public class GRayPropertyDrawer : PositionDirectionDrawer
    {
        protected override string PositionPropertyName => nameof(GRay.origin);
        protected override string DirectionPropertyName => nameof(GRay.direction);
    }
    [CustomPropertyDrawer(typeof(GCone))]
    public class GConePropertyDrawer : PositionDirectionDrawer
    {
        protected override string PositionPropertyName => nameof(GCone.origin);
        protected override string DirectionPropertyName => nameof(GCone.normal);
    }
    [CustomPropertyDrawer(typeof(GHeightCone))]
    public class GHeightConePropertyDrawer : PositionDirectionDrawer
    {
        protected override string PositionPropertyName => nameof(GHeightCone.origin);
        protected override string DirectionPropertyName => nameof(GHeightCone.normal);
    }
    public class PositionDirectionDrawer:PropertyDrawer
    {
        protected virtual string PositionPropertyName => throw new Exception("Override This Please");
        protected virtual string DirectionPropertyName => throw new Exception("Override This Please");
        SerializedProperty m_PositionProperty;
        SerializedProperty m_DirecitonProperty;
        string m_Name,m_ToolTip;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            m_Name = label.text;
            m_ToolTip = label.tooltip;
            m_PositionProperty = property.FindPropertyRelative(PositionPropertyName);
            m_DirecitonProperty = property.FindPropertyRelative(DirectionPropertyName);
            return EditorGUI.GetPropertyHeight(property,label,true)+ (property .isExpanded? 20f:0f);
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            m_DirecitonProperty.vector3Value = m_DirecitonProperty.vector3Value.normalized;
            float width = position.size.x;
            float propertyHeight = EditorGUI.GetPropertyHeight(property);
            HorizontalScope.Begin(position.x,position.y, propertyHeight);
            EditorGUI.PropertyField(HorizontalScope.NextRect(0f, width), property, new GUIContent(m_Name, m_ToolTip), true);
            if (!property.isExpanded)
                return;
            HorizontalScope.NextLine(2f, 18f);
            HorizontalScope.NextRect(0f, width * 4f / 6f);
            if (GUI.Button(HorizontalScope.NextRect(2f, width / 6f - 2f), "Reset"))
            {
                m_PositionProperty.vector3Value = Vector3.zero;
                m_DirecitonProperty.vector3Value = Vector3.forward;
                property.serializedObject.ApplyModifiedProperties();
                Undo.RecordObject(property.serializedObject.targetObject, "Reset Property");
            }
            if (GUI.Button(HorizontalScope.NextRect(0f, width / 6f), "Edit"))
                GDirectedPositionHelper.Begin(m_PositionProperty, m_DirecitonProperty);
        }

        public static class GDirectedPositionHelper
        {
            static SerializedProperty m_PositionProperty;
            static SerializedProperty m_DirectionProperty;
            static ValueChecker<Vector3> m_PositionChecker = new ValueChecker<Vector3>(Vector3.zero);
            static ValueChecker<Quaternion> m_RotationChecker = new ValueChecker<Quaternion>(Quaternion.identity);
            public static void Begin(SerializedProperty _positionProperty, SerializedProperty _directionProperty)
            {
                if (m_PositionProperty != null || m_DirectionProperty != null)
                    End();

                m_PositionProperty = _positionProperty;
                m_DirectionProperty = _directionProperty;

                SceneView.duringSceneGui += OnSceneGUI;
                m_PositionChecker.Check(m_PositionProperty.vector3Value);
                m_RotationChecker.Check(Quaternion.LookRotation(m_DirectionProperty.vector3Value));
                Tools.current = Tool.None;
                SceneView.lastActiveSceneView.pivot = m_PositionChecker.m_Value;
            }
            public static void End()
            {
                SceneView.duringSceneGui -= OnSceneGUI;
                m_PositionProperty = null;
                m_DirectionProperty = null;
            }
            static void OnSceneGUI(SceneView _sceneView)
            {
                try
                {
                    if (Tools.current != Tool.None || Event.current.keyCode == KeyCode.Escape)
                    {
                        End();
                        return;
                    }
                    m_RotationChecker.Check(Handles.DoRotationHandle(m_RotationChecker.m_Value, m_PositionChecker.m_Value));
                    m_DirectionProperty.vector3Value = m_RotationChecker.m_Value * Vector3.forward;
                    m_DirectionProperty.serializedObject.ApplyModifiedProperties();

                    m_PositionChecker.Check(Handles.DoPositionHandle(m_PositionChecker.m_Value, Quaternion.identity));
                    m_PositionProperty.vector3Value = m_PositionChecker.m_Value;
                    m_PositionProperty.serializedObject.ApplyModifiedProperties();
                    Handles.Label(m_PositionChecker.m_Value, "Transforming", UEGUIStyle_SceneView.m_TitleLabel);
                }
                catch
                {
                    End();
                }
            }
        }
    }
}