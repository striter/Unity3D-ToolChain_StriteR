using System;
using System.Collections.Generic;
using System.Linq.Extensions;
using UnityEditorInternal;
using UnityEngine;

namespace UnityEditor.Extensions.ScriptableObjectBundle
{

    [CustomEditor(typeof(AScriptableObjectBundle),true)]
    public class AScriptableObjectBundleEditor : Editor
    {
        private AScriptableObjectBundle m_Target;
        private List<Type> kInheritTypes = new List<Type>();
        private ReorderableList m_ObjectsList;
        private void SetDirty()
        {
            m_ObjectsList.serializedProperty.serializedObject.ApplyModifiedProperties();
            m_Target.SetDirty();
        }

        protected virtual void DrawElement(Rect _rect,int _index,SerializedProperty _property, bool _isActive, bool _isFocused)
        {
            _rect.x += 10f;
            _rect.ResizeX(_rect.size.x - 10f);
            EditorGUI.PropertyField(_rect, _property);
        }

        protected virtual float GetElementHeight(SerializedProperty _property)
        {
            return EditorGUI.GetPropertyHeight(_property,true);
        }
        
        protected virtual void OnEnable()
        {
            m_Target = target as AScriptableObjectBundle;
            var baseType = m_Target.GetBaseType();
            kInheritTypes = baseType.GetChildTypes().FillList(kInheritTypes);
            m_ObjectsList = new ReorderableList(serializedObject,serializedObject.FindProperty(nameof(m_Target.m_Objects)),true,true,true,true);
            m_ObjectsList.drawElementCallback = (rect, index, isActive, isFocused)=>DrawElement(rect,index,m_ObjectsList.serializedProperty.GetArrayElementAtIndex(index),isActive,isFocused);
            m_ObjectsList.elementHeightCallback = (index) => GetElementHeight(m_ObjectsList.serializedProperty.GetArrayElementAtIndex(index));
            m_ObjectsList.onAddDropdownCallback = (_, _) =>
            {
                var menu = new GenericMenu();
                string nameSpace = kInheritTypes[0].Namespace;
                foreach (var type in kInheritTypes)
                {
                    if (type.Namespace != nameSpace)
                    {
                        menu.AddSeparator(string.Empty);
                        nameSpace = type.Namespace;                        
                    }
                    
                    menu.AddItem(new GUIContent(type.Name), false, () =>
                    {
                        var instance = ScriptableObject.CreateInstance(type) as AScriptableObjectBundleElement;
                        instance.m_Title = type.Name;
                        m_Target.m_Objects.Add(instance);
                        SetDirty();
                    });
                }
                menu.ShowAsContext();
            };

            m_ObjectsList.onRemoveCallback = list =>
            {
                m_Target.m_Objects.RemoveAt(list.index);
                SetDirty();
            };
            m_ObjectsList.onReorderCallback = _ => SetDirty();
            m_ObjectsList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, $"Objects | {baseType.Name}");
        }

        public override void OnInspectorGUI()
        {
            m_ObjectsList.DoLayoutList();
        }
    }
}