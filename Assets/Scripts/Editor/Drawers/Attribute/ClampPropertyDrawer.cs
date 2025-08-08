using System.Collections;
using System.Collections.Generic;
using System.Linq.Extensions;
using UnityEngine;

namespace UnityEditor.Extensions
{

    [CustomPropertyDrawer(typeof(ClampAttribute))]
    public class ClampPropertyDrawer : AAttributePropertyDrawer<ClampAttribute>
    {
        private object m_MaxClampField;
        private object m_MinClampField;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            m_MaxClampField = property.AllRelativeFields().Find(p=>p.Item1.Name == attribute.m_ClampMaxField).Item2;
            m_MinClampField = property.AllRelativeFields().Find(p=>p.Item1.Name == attribute.m_ClampMinField).Item2;
            return base.GetPropertyHeight(property, label);
        }

        void GetClampFieldValue(object _field,ref float _value)
        {
            switch (_field)
            {
                case null: return;
                case IList list: _value = list.Count; break;
                default: Debug.LogError($"[{nameof(ClampPropertyDrawer)}] Field Type Not Implemented: {_field.GetType()}"); break;
            }
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!OnGUIAttributePropertyCheck(position, property, SerializedPropertyType.Float, SerializedPropertyType.Integer, SerializedPropertyType.Vector2,SerializedPropertyType.Vector3))
                return;

            var min = attribute.m_Min;
            var max = attribute.m_Max;
            GetClampFieldValue(m_MinClampField,ref min);
            GetClampFieldValue(m_MaxClampField,ref max);
            
            EditorGUI.PropertyField(position, property, label);
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    property.intValue = (int) Mathf.Clamp(property.intValue,min,max);
                    break;
                case SerializedPropertyType.Float:
                    property.floatValue = Mathf.Clamp(property.floatValue,min,max);
                    break;
                case SerializedPropertyType.Vector2:
                    property.vector2Value = Vector2.ClampMagnitude(property.vector2Value,(float)max);
                    break;
                case SerializedPropertyType.Vector3:
                    property.vector3Value = Vector3.ClampMagnitude(property.vector3Value,(float)max);
                    break;
            }
        }
    }
}