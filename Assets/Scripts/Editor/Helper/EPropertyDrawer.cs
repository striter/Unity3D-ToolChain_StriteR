using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TEditor
{
    [CustomPropertyDrawer(typeof(GRay))]
    public class GRayPropertyDrawer : PropertyDrawer
    {
        public ValueChecker<bool> m_Selected = new ValueChecker<bool>(false);
        SerializedProperty m_PositionProperty;
        SerializedProperty m_DirecitonProperty;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            m_PositionProperty = property.FindPropertyRelative(nameof(GRay.origin));
            m_DirecitonProperty = property.FindPropertyRelative(nameof(GRay.direction));
            return 20+(m_Selected?(4+ EditorGUI.GetPropertyHeight(m_PositionProperty,true)+EditorGUI.GetPropertyHeight(m_DirecitonProperty,true)):0);
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            m_DirecitonProperty.vector3Value=m_DirecitonProperty.vector3Value.normalized;

            float width = position.size.x ;
            HorizontalScope.Begin(position.x, position.y, 20);
            if (m_Selected.Check(EditorGUI.Foldout(HorizontalScope.NextRect(0f, width * 3f / 4f), m_Selected, label)))
                GDirectedPositionHelper.End();
            if (GUI.Button(HorizontalScope.NextRect(0f, width / 4f), "Edit"))
            {
                GDirectedPositionHelper.Begin(m_PositionProperty, m_DirecitonProperty);
                m_Selected.Set(true);
            }

            if(m_Selected)
            {
                HorizontalScope.Begin(position.x + 15, position.y, 20);
                width = position.size.x - 15;
                HorizontalScope.NextLine(2, EditorGUI.GetPropertyHeight(m_PositionProperty, true));
                EditorGUI.PropertyField(new Rect(HorizontalScope.m_CurrentPos, new Vector2(width, HorizontalScope.m_CurrentY)), m_PositionProperty);
                HorizontalScope.NextLine(2, EditorGUI.GetPropertyHeight(m_DirecitonProperty, true));
                EditorGUI.PropertyField(new Rect(HorizontalScope.m_CurrentPos, new Vector2(width, HorizontalScope.m_CurrentY)), m_DirecitonProperty);
            }
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
                    if ( Tools.current != Tool.None || Event.current.keyCode == KeyCode.Escape)
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

                    Handles.Label(m_PositionChecker.m_Value,"Ray Editing",UEGUIStyle_SceneView.m_TitleLabel);
                }
                catch
                {
                    End();
                }
            }
        }
    }
}