using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Linq.Extensions;
using UnityEngine;

namespace UnityEditor.Extensions
{
    public abstract class ASubAttributePropertyDrawer<T> : PropertyDrawer where T : PropertyAttribute
    {
        public T attribute => base.attribute as T;
        
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
    }

}