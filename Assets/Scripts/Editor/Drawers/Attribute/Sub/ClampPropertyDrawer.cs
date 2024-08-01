using UnityEngine;

namespace UnityEditor.Extensions
{

    [CustomPropertyDrawer(typeof(ClampAttribute))]
    public class ClampPropertyDrawer : ASubAttributePropertyDrawer<ClampAttribute>
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!OnGUIAttributePropertyCheck(position, property, SerializedPropertyType.Float, SerializedPropertyType.Integer))
                return;

            EditorGUI.PropertyField(position, property, label);
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    property.intValue = (int) Mathf.Clamp(property.intValue,attribute.m_Min, attribute.m_Max);
                    break;
                case SerializedPropertyType.Float:
                    property.floatValue = Mathf.Clamp(property.floatValue, attribute.m_Min, attribute.m_Max);
                    break;
            }
        }
    }
}