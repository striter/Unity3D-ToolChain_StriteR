using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Linq.Extensions;
using UnityEngine;

namespace UnityEditor.Extensions
{

    public abstract class AAttributePropertyDrawer<T> : PropertyDrawer where T : PropertyAttribute
    {
        public new T attribute => base.attribute as T;
        private PropertyDrawer m_NextPropertyDrawer;
        
        public bool OnGUIAttributePropertyCheck(Rect _position, SerializedProperty _property, params SerializedPropertyType[] _checkTypes)
        {
            if (_checkTypes.Length!=0&&_checkTypes.All(p => _property.propertyType != p))
            {
                EditorGUI.LabelField(_position,
                    $"<Color=#FF0000>Attribute For {_checkTypes.ToString('|', type => type.ToString())} Only!</Color>", UEGUIStyle_Window.m_TitleLabel);
                return false;
            }
            return true;
        }
        
        PropertyDrawer GetNextPropertyDrawer(SerializedProperty _property)
        {
            if (m_NextPropertyDrawer != null)
                return m_NextPropertyDrawer;

            var targetField = _property.GetFieldInfo(out _);
            var attributes = targetField.GetCustomAttributes();
            var order = attribute.order + 1;
            var drawerType = targetField.FieldType;
            Type propertyDrawerType = null;
            Attribute drawerAttribute = attribute;
            if (order < attributes.Count())
            {
                var nextAttribute = attributes.ElementAt(order);
                drawerAttribute = nextAttribute;
                drawerType = nextAttribute.GetType();
            }
            
            propertyDrawerType = UEDrawer.FindCustomDrawerType(drawerType);
            if (propertyDrawerType == null)
                return null;
            
            m_NextPropertyDrawer = UEDrawer.CreateDrawer(propertyDrawerType, targetField, drawerAttribute);
            m_NextPropertyDrawer.attribute.order = order;
            return m_NextPropertyDrawer;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var customDrawerHeight = GetNextPropertyDrawer(property)?.GetPropertyHeight(property, label);
            return customDrawerHeight ?? EditorGUI.GetPropertyHeight(property, label, true);
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var customDrawer = GetNextPropertyDrawer(property);
            if (customDrawer != null)
            {
                customDrawer.OnGUI(position, property, label);
                return;
            }
            EditorGUI.PropertyField(position, property, label, true);
        }
    }
}