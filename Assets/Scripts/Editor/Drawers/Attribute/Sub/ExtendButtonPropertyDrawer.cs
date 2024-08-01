using System.Reflection;
using UnityEngine;

namespace UnityEditor.Extensions
{
    [CustomPropertyDrawer(typeof(ExtendButtonAttribute))]
    public class ExtendButtonPropertyDrawer : ASubAttributePropertyDrawer<ExtendButtonAttribute>
    {
        bool GetMethod(SerializedProperty _property, string _methodName, out MethodInfo _info)
        {
            _info = null;
            foreach (var methodInfo in _property.AllMethods())
            {
                if (methodInfo.Name == _methodName)
                {
                    _info = methodInfo;
                    break;
                }
            }
            if(_info==null)
                Debug.LogWarning($"No Method Found:{_methodName}|{_property.serializedObject.targetObject.GetType()}");
            return _info!=null;
        }
        public override float GetPropertyHeight(SerializedProperty _property, GUIContent label)
        {
            var baseHeight = EditorGUI.GetPropertyHeight(_property, label);
            if (_property.propertyType == SerializedPropertyType.Generic)
                return baseHeight;
            
            var buttonAttribute = attribute as ExtendButtonAttribute;
            return baseHeight + buttonAttribute.m_Buttons.Length*20f;
        }
        public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            if (_property.propertyType == SerializedPropertyType.Generic)
            {
                Debug.LogWarning("Extend Button Attribute is currently not support Generic types");
                EditorGUI.PropertyField(_position, _property, _label,true);
                return;
            }

            if (!OnGUIAttributePropertyCheck(_position, _property))
                return;
            
            EditorGUI.PropertyField(_position.Resize(_position.size-new Vector2(0,20*attribute.m_Buttons.Length)), _property, _label,true);
            _position = _position.Reposition(_position.x, _position.y + EditorGUI.GetPropertyHeight(_property, _label,true) + 2);
            foreach (var (title,method,parameters) in attribute.m_Buttons)
            {
                _position = _position.Resize(new Vector2(_position.size.x, 18));
                if (GUI.Button(_position, title))
                {
                    if (!GetMethod(_property, method, out var info))
                        continue;
                    info?.Invoke(_property.serializedObject.targetObject,parameters);
                }
                
                _position = _position.Reposition(_position.x, _position.y +  20);
            }
        }
    }
}