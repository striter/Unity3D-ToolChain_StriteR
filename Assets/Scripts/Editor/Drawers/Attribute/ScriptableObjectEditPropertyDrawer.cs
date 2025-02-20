using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using System.Reflection;
using UnityEditorInternal;
using UnityEngine;

namespace UnityEditor.Extensions
{
    [CustomPropertyDrawer(typeof(ScriptableObjectEditAttribute))]
    public class ScriptableObjectEditPropertyDrawer : AAttributePropertyDrawer<ScriptableObjectEditAttribute>
    {
        private bool m_FoldoutValue;
        private SerializedObject m_SerializedObject;
        private readonly List<object> m_ChildProperties = new List<object>();
        private readonly List<float> m_Heights = new List<float>();

        bool CheckIsFoldout() => attribute.on ? !m_FoldoutValue : m_FoldoutValue;
        
        void CacheProperties(SerializedProperty _property)
        {
            m_ChildProperties.Clear();
            m_Heights.Clear();
            m_ChildProperties.Add(_property);
            m_Heights.Add(EditorGUI.GetPropertyHeight(_property));
            foreach (var field in _property.objectReferenceValue.GetType().GetFields())
            {
                var childProperty = m_SerializedObject.FindProperty(field.Name);
                if(childProperty==null)
                    continue;
            
                if (!childProperty.isArray || childProperty.propertyType == SerializedPropertyType.String)
                {
                    m_ChildProperties.Add(childProperty);
                    m_Heights.Add(EditorGUI.GetPropertyHeight(childProperty)); 
                }
                else  //Vanilla(or easy one) EditorGUI.DrawPropertyField aint work, so I replaced(re-functionaled) it as much as i could.
                {
                    var list = new ReorderableList(childProperty.serializedObject, childProperty, true, true, true, true){
                        drawElementCallback = (_rect, _index, _, _) =>
                        {
                            var element = childProperty.GetArrayElementAtIndex(_index);
                            EditorGUI.PropertyField(_rect, element, new GUIContent($"Element {_index}"),true);
                        },
                        elementHeightCallback = _index => EditorGUI.GetPropertyHeight(childProperty.GetArrayElementAtIndex(_index),true),
                        drawHeaderCallback = rect => GUI.Label(rect, childProperty.name)
                    };
                    m_ChildProperties.Add(list);
                    m_Heights.Add(list.GetHeight() + 2);
                }
            }
        }
        
        public override float GetPropertyHeight(SerializedProperty _property, GUIContent _label)
        {
            if (_property.objectReferenceValue == null) 
                return base.GetPropertyHeight(_property, _label);
            m_SerializedObject = new SerializedObject(_property.objectReferenceValue);
            
            if (!CheckIsFoldout())
                return base.GetPropertyHeight(_property, _label);
            CacheProperties(_property);
            return m_Heights.Sum() + 2;
        }

        public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            var on = CheckIsFoldout();
            on = EditorGUI.Foldout(_position.Resize(20,20),on,"");
            m_FoldoutValue = attribute.on ? !on : on;
            if (!on || m_SerializedObject == null)
            {
                EditorGUI.PropertyField(_position, _property, _label,true);
                return;
            }
            
            CacheProperties(_property);
            EditorGUI.DrawRect(_position,Color.black.SetA(.1f));
            Rect rect = _position.Resize(_position.size.x, 0f);
            rect = rect.Resize(_position.size.x - 24f,_position.size.y);
            rect = rect.Move(20f, 0f);
            EditorGUI.BeginChangeCheck();
            
            foreach (var (index,child) in m_ChildProperties.LoopIndex())
            {
                float height = m_Heights[index];
                var childRect = rect.ResizeY(height);
                if (child is SerializedProperty childProperty)
                    EditorGUI.PropertyField(childRect,childProperty,true);
                else if (child is ReorderableList list)
                    list.DoList(childRect);
                
                rect = rect.MoveY(height);
            }

            if (EditorGUI.EndChangeCheck())
            {
                m_SerializedObject.ApplyModifiedProperties();
                var type = _property.serializedObject.targetObject.GetType();
                type.GetMethod("OnValidate",BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)?.Invoke(_property.serializedObject.targetObject,null);
            }
        }
    }
    
}