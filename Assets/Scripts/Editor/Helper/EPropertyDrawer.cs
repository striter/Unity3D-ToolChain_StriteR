using System;
using Geometry.Three;
using UnityEditor;
using UnityEngine;

namespace TEditor
{
    [CustomPropertyDrawer(typeof(GLine))]
    public class GLinePropertyDrawer : TransformHandlesDrawer
    {
        protected override string PositionPropertyName => nameof(GLine.origin);
        protected override string DirectionPropertyName => nameof(GLine.direction);
    }
    [CustomPropertyDrawer(typeof(GRay))]
    public class GRayPropertyDrawer : TransformHandlesDrawer
    {
        protected override string PositionPropertyName => nameof(GRay.origin);
        protected override string DirectionPropertyName => nameof(GRay.direction);
    }
    [CustomPropertyDrawer(typeof(GCone))]
    public class GConePropertyDrawer : TransformHandlesDrawer
    {
        protected override string PositionPropertyName => nameof(GCone.origin);
        protected override string DirectionPropertyName => nameof(GCone.normal);
    }
    [CustomPropertyDrawer(typeof(GHeightCone))]
    public class GHeightConePropertyDrawer : TransformHandlesDrawer
    {
        protected override string PositionPropertyName => nameof(GHeightCone.origin);
        protected override string DirectionPropertyName => nameof(GHeightCone.normal);
    }
    public class TransformHandlesDrawer : PropertyDrawer
    {
        protected virtual string PositionPropertyName => throw new Exception("Override This Please");
        protected virtual string DirectionPropertyName => throw new Exception("Override This Please");
        SerializedProperty m_PositionProperty;
        SerializedProperty m_DirecitonProperty;
        string m_Name, m_ToolTip;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            m_Name = label.text;
            m_ToolTip = label.tooltip;
            m_PositionProperty = property.FindPropertyRelative(PositionPropertyName);
            m_DirecitonProperty = property.FindPropertyRelative(DirectionPropertyName);
            return EditorGUI.GetPropertyHeight(property, label, true) + (property.isExpanded ? 20f : 0f);
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float width = position.size.x;
            float propertyHeight = EditorGUI.GetPropertyHeight(property);
            HorizontalScope.Begin(position.x, position.y, propertyHeight);
            EditorGUI.PropertyField(HorizontalScope.NextRect(0f, width), property, new GUIContent(m_Name, m_ToolTip), true);
            if (!property.isExpanded)
                return;
            HorizontalScope.NextLine(2f, 18f);
            HorizontalScope.NextRect(0f, width * 5f / 6f);
            if (GUI.Button(HorizontalScope.NextRect(0f, width / 6f), "Edit"))
                GUITransformHandles.Begin(m_PositionProperty, m_DirecitonProperty);
        }
    }
}