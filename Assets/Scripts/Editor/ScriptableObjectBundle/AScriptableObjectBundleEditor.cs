using System;
using System.Collections.Generic;
using System.Linq;
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
        private bool m_Dirty = false;
        protected ReorderableList m_ObjectsList { get; private set; }
        protected virtual void OnEnable()
        {
            m_Target = target as AScriptableObjectBundle;
            var baseType = m_Target.GetBaseType();
            kInheritTypes = baseType.GetChildTypes().FillList(kInheritTypes);
            m_ObjectsList = new ReorderableList(serializedObject,serializedObject.FindProperty(nameof(m_Target.m_Objects)),true,true,true,true);
            m_ObjectsList.drawElementCallback = (rect, index, isActive, isFocused)=>DrawElement(rect,index,m_ObjectsList.serializedProperty.GetArrayElementAtIndex(index),isActive,isFocused);
            m_ObjectsList.elementHeightCallback = (index) => GetElementHeight(m_ObjectsList.serializedProperty.GetArrayElementAtIndex(index));
            m_ObjectsList.multiSelect = true;
            m_ObjectsList.onAddDropdownCallback = (_, _) => {
                var menu = new GenericMenu();
                if (m_ObjectsList.selectedIndices.Count > 0)
                {
                    menu.AddItem(new GUIContent("Copy Selected"),false,()=> {
                        foreach (var selectIndex in m_ObjectsList.selectedIndices)
                        {
                            var srcClone = m_Target.m_Objects[selectIndex];
                            var instance = ScriptableObject.CreateInstance(srcClone.GetType()) as AScriptableObjectBundleElement;
                            UReflection.CopyFields(srcClone,instance);
                            m_Target.m_Objects.Add(instance);
                            SetBundleDirty();
                        }
                    });
                }
                menu.AddSeparator(string.Empty);
                
                var nameSpace = kInheritTypes[0].Namespace;
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
                        SetBundleDirty();
                    });
                }
                menu.ShowAsContext();
            };

            m_ObjectsList.onRemoveCallback = list =>
            {
                m_Target.m_Objects.RemoveAt(list.index);
                SetBundleDirty();
            };
            m_ObjectsList.onReorderCallback = _ => SetBundleDirty();
            m_ObjectsList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, $"Objects | {baseType.Name}");
            EditorApplication.update += Tick;
        }
        private void OnDisable() => EditorApplication.update -= Tick;
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

        public override void OnInspectorGUI()
        {
            m_ObjectsList.DoLayoutList();
        }
        
        private void SetBundleDirty()
        {
            m_ObjectsList.serializedProperty.serializedObject.ApplyModifiedProperties();
            m_Dirty = true;
        }

        private void Tick()
        {
            if (!m_Dirty)
                return;
            
            if (m_Target.m_Objects.Any(p => p == null))
                return;
            
            m_Dirty = false;
            UEAsset.ClearSubAssets(m_Target);
            foreach (var (index, so) in m_Target.m_Objects.LoopIndex())
            {
                var name = so.m_Title;
                if (string.IsNullOrEmpty(name))
                    name = so.GetType().Name;
                so.name = $"{index}_{name}";
            }
            
            UEAsset.CreateOrReplaceSubAsset(m_Target, m_Target.m_Objects);
            EditorUtility.SetDirty(m_Target);
        }
    }
}