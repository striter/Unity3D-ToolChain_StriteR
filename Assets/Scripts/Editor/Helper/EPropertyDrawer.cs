using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TEditor
{
    [CustomPropertyDrawer(typeof(GSpaceData))]
    public class GDirectionPositionPropertyDrawer : PropertyDrawer
    {
        public bool m_Selected = false;
        SerializedProperty m_PositionProperty;
        SerializedProperty m_DirecitonProperty;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            m_PositionProperty = property.FindPropertyRelative(nameof(GSpaceData.m_Position));
            m_DirecitonProperty = property.FindPropertyRelative(nameof(GSpaceData.m_Direction));
            return 20+EditorGUI.GetPropertyHeight(m_PositionProperty,true)+EditorGUI.GetPropertyHeight(m_DirecitonProperty,true);
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float depthOffset = property.depth * 15;
            position.x += depthOffset;
            position.size = new Vector2(position.size.x - depthOffset, position.size.y);
            float width = position.size.x;
            HorizontalScope.Begin(position.x, position.y, 20);
            GUI.Label( HorizontalScope.NextRect(0f,width*2/3f),label);
            if (GUI.Button(HorizontalScope.NextRect(0f, width / 3f), "Edit"))
                GDirectedPositionHelper.Begin(m_PositionProperty, m_DirecitonProperty);
            HorizontalScope.NextLine(0, EditorGUI.GetPropertyHeight(m_PositionProperty, true));
            EditorGUI.PropertyField( new Rect(HorizontalScope.m_CurrentPos, new Vector2(position.width, HorizontalScope.m_CurrentY)), m_PositionProperty);
            HorizontalScope.NextLine(0, EditorGUI.GetPropertyHeight(m_DirecitonProperty, true));
            EditorGUI.PropertyField(new Rect(HorizontalScope.m_CurrentPos, new Vector2(position.width, HorizontalScope.m_CurrentY)), m_DirecitonProperty);
        }
        public static class GDirectedPositionHelper
        {
            static SerializedProperty m_PositionProperty;
            static SerializedProperty m_DirectionProperty;
            static ValueChecker<Vector3> m_PositionChecker = new ValueChecker<Vector3>(Vector3.zero);
            static ValueChecker<Quaternion> m_RotationChecker = new ValueChecker<Quaternion>(Quaternion.identity);
            public static void Begin(SerializedProperty _positionProperty,SerializedProperty _directionProperty )
            {
                if (m_PositionProperty != null || m_DirectionProperty!=null)
                    End();

                m_PositionProperty = _positionProperty;
                m_DirectionProperty = _directionProperty;
                SceneView.duringSceneGui += OnSceneGUI;
                m_PositionChecker.Check(m_PositionProperty.vector3Value);
                m_RotationChecker.Check(Quaternion.LookRotation(m_DirectionProperty.vector3Value));
                Tools.current = Tool.None;
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

                    Handles_Extend.DrawArrow(m_PositionChecker.m_Value, m_RotationChecker.m_Value, 1f, .2f);
                    m_RotationChecker.Check(Handles.DoRotationHandle(m_RotationChecker.m_Value, m_PositionChecker.m_Value));
                    m_DirectionProperty.vector3Value = m_RotationChecker.m_Value * Vector3.forward;
                    m_DirectionProperty.serializedObject.ApplyModifiedProperties();

                    m_PositionChecker.Check(Handles.DoPositionHandle(m_PositionChecker.m_Value, Quaternion.identity));
                    m_PositionProperty.vector3Value = m_PositionChecker.m_Value;
                    m_PositionProperty.serializedObject.ApplyModifiedProperties();
                }
                catch       //
                {
                    End();
                }
            }
        }
    }
}