using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.Extensions
{
    
    public class MainAttributePropertyDrawer<T> : PropertyDrawer where T : Attribute
    {
        static readonly Type kPropertyDrawerType = typeof(PropertyDrawer);
        private PropertyDrawer m_SubPropertyDrawer;
        PropertyDrawer GetSubPropertyDrawer(SerializedProperty _property)
        {
            if (m_SubPropertyDrawer != null)
                return m_SubPropertyDrawer;

            FieldInfo targetField = _property.GetFieldInfo(out var parentObject);
            IEnumerable<Attribute> attributes = targetField.GetCustomAttributes();
            int order = attribute.order + 1;
            if (order >= attributes.Count())
                return null;

            Attribute nextAttribute = attributes.ElementAt(order);
            Type attributeType = nextAttribute.GetType();
            Type propertyDrawerType = (Type)Type.GetType("UnityEditor.ScriptAttributeUtility,UnityEditor").GetMethod("GetDrawerTypeForType", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { attributeType });
            m_SubPropertyDrawer = (PropertyDrawer)Activator.CreateInstance(propertyDrawerType);
            kPropertyDrawerType.GetField("m_FieldInfo", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(m_SubPropertyDrawer, targetField);
            kPropertyDrawerType.GetField("m_Attribute", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(m_SubPropertyDrawer, nextAttribute);
            m_SubPropertyDrawer.attribute.order = order;
            return m_SubPropertyDrawer;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var customDrawerHeight = GetSubPropertyDrawer(property)?.GetPropertyHeight(property, label);
            return customDrawerHeight ?? EditorGUI.GetPropertyHeight(property, label, true);
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var customDrawer = GetSubPropertyDrawer(property);
            if (customDrawer != null)
            {
                customDrawer.OnGUI(position, property, label);
                return;
            }
            EditorGUI.PropertyField(position, property, label, true);
        }
        public bool CheckPropertyAvailable(bool fold, SerializedProperty _property, MFoldoutAttribute _attribute)
        {
            IEnumerable<(FieldInfo, object)> fields = _property.AllRelativeFields();
            return _attribute.m_FieldsMatches.All(fieldMatch => fields.Any(field => {
                if (field.Item1.Name != fieldMatch.Key)
                    return false;
                bool equals = fieldMatch.Value?.Contains(field.Item2) ?? field.Item2 is null;
                return fold ? !equals : equals;
            }));
        }
        
    }
    public class SubAttributePropertyDrawer<T> : PropertyDrawer where T : Attribute
    {
        public bool OnGUIAttributePropertyCheck(Rect _position, SerializedProperty _property, out T _targetAttribute, params SerializedPropertyType[] _checkTypes)
        {
            _targetAttribute = null;
            if (_checkTypes.Length!=0&&_checkTypes.All(p => _property.propertyType != p))
            {
                EditorGUI.LabelField(_position,
                    $"<Color=#FF0000>Attribute For {_checkTypes.ToString('|', type => type.ToString())} Only!</Color>", UEGUIStyle_Window.m_TitleLabel);
                return false;
            }
            _targetAttribute = attribute as T;
            return true;
        }
    }

    #region MainAttribute
    [CustomPropertyDrawer(typeof(MTitleAttribute))]
    public class MTitlePropertyDrawer : MainAttributePropertyDrawer<MTitleAttribute>
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) + 2f;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect titleRect = position;
            titleRect.height = 18;
            EditorGUI.LabelField(titleRect, label, UEGUIStyle_Window.m_TitleLabel);
            label.text = " ";
            base.OnGUI(position, property, label);
        }
    }

    [CustomPropertyDrawer(typeof(MFoldoutAttribute))]
    public class MFoldoutPropertyDrawer : MainAttributePropertyDrawer<MFoldoutAttribute>
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!CheckPropertyAvailable(false, property, attribute as MFoldoutAttribute))
                return -2;

            return base.GetPropertyHeight(property, label);
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!CheckPropertyAvailable(false, property, attribute as MFoldoutAttribute))
                return;
            base.OnGUI(position, property, label);
        }
    }
    
    [CustomPropertyDrawer(typeof(MFoldAttribute))]
    public class MFoldPropertyDrawer : MainAttributePropertyDrawer<MFoldoutAttribute>
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!CheckPropertyAvailable(true, property, attribute as MFoldoutAttribute))
                return -2;

            return base.GetPropertyHeight(property, label);
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!CheckPropertyAvailable(true, property, attribute as MFoldoutAttribute))
                return;
            base.OnGUI(position, property, label);
        }
    }
    #endregion
    
    #region SubAttribute
    [CustomPropertyDrawer(typeof(Readonly))]
    public class ReadonlyPropertyDrawer : SubAttributePropertyDrawer<Readonly>
    {
        public override float GetPropertyHeight(SerializedProperty _property, GUIContent _label) => EditorGUI.GetPropertyHeight(_property, _label, true);

        public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(_position, _property, _label, true);
            GUI.enabled = true;
        }
    }
    
    [CustomPropertyDrawer(typeof(Rename))]
    public class RenamePropertyDrawer : SubAttributePropertyDrawer<Rename>
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if(!OnGUIAttributePropertyCheck(position,property,out Rename attribute))
                return;
            label.text = attribute.name;
            EditorGUI.PropertyField(position, property, label, true);
        }
    }

    
    [CustomPropertyDrawer(typeof(IntEnumAttribute))]
    public class IntEnumPropertyDrawer : SubAttributePropertyDrawer<IntEnumAttribute>
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if(!OnGUIAttributePropertyCheck(position,property,out IntEnumAttribute attribute,SerializedPropertyType.Float,SerializedPropertyType.Integer))
                return;
            property.intValue = EditorGUI.IntPopup(position,label,property.intValue,attribute.m_Values.Select(p=>new GUIContent( p.ToString())).ToArray(),attribute.m_Values);
        }
    }

    [CustomPropertyDrawer(typeof(ScriptableObjectEditAttribute))]
    public class ScriptableObjectEditPropertyDrawer : SubAttributePropertyDrawer<ScriptableObjectEditAttribute>
    {
        private bool m_Foldout;
        private SerializedObject m_SerializedObject;
        private readonly List<object> m_ChildProperties = new List<object>();
        private readonly List<float> m_Heights = new List<float>();

        void CacheProperties(SerializedProperty _property)
        {
            if (_property.objectReferenceValue == null) return;
            m_SerializedObject = new SerializedObject(_property.objectReferenceValue);

            m_ChildProperties.Clear();
            m_Heights.Clear();
            m_ChildProperties.Add(_property);
            m_Heights.Add(EditorGUI.GetPropertyHeight(_property)); 
            foreach (var field in _property.objectReferenceValue.GetType().GetFields())
            {
                var childProperty = m_SerializedObject.FindProperty(field.Name);
                if(childProperty==null)
                    continue;
            
                if (!childProperty.isArray)
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
                            EditorGUI.PropertyField(_rect, element, new GUIContent($"Element {_index}"));
                        },
                        drawHeaderCallback = rect => GUI.Label(rect, childProperty.name)
                    };
                    m_ChildProperties.Add(list);
                    m_Heights.Add(list.GetHeight() + 2);
                }
            }
        }
        
        public override float GetPropertyHeight(SerializedProperty _property, GUIContent _label)
        {
            CacheProperties(_property);
            if (!m_Foldout || m_SerializedObject == null)
                return base.GetPropertyHeight(_property, _label);
            return m_Heights.Sum() + 2;
        }

        public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            CacheProperties(_property);
            m_Foldout = EditorGUI.Foldout(_position.Resize(20,20), m_Foldout,"");
            if (!m_Foldout || m_SerializedObject == null)
            {
                EditorGUI.PropertyField(_position, _property, _label,true);
                return;
            }
            
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
                _property.serializedObject.targetObject.GetType().GetMethod("OnValidate",BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)?.Invoke(_property.serializedObject.targetObject,null);
            }
        }
    }
    
    [CustomPropertyDrawer(typeof(ExtendButtonAttribute))]
    public class ExtendButtonPropertyDrawer : SubAttributePropertyDrawer<ExtendButtonAttribute>
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

            if (!OnGUIAttributePropertyCheck(_position, _property, out var buttonAttribute))
                return;
            
            EditorGUI.PropertyField(_position.Resize(_position.size-new Vector2(0,20*buttonAttribute.m_Buttons.Length)), _property, _label,true);
            _position = _position.Reposition(_position.x, _position.y + EditorGUI.GetPropertyHeight(_property, _label,true) + 2);
            foreach (var (title,method,parameters) in buttonAttribute.m_Buttons)
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
    
    [CustomPropertyDrawer(typeof(ClampAttribute))]
    public class ClampPropertyDrawer : SubAttributePropertyDrawer<ClampAttribute>
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!OnGUIAttributePropertyCheck(position, property, out ClampAttribute attribute, SerializedPropertyType.Float, SerializedPropertyType.Integer))
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

    [CustomPropertyDrawer(typeof(MinMaxRangeAttribute))]
    public class MinMaxRangePropertyDrawer : SubAttributePropertyDrawer<MinMaxRangeAttribute>
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if(!OnGUIAttributePropertyCheck(position,property,out MinMaxRangeAttribute attribute,SerializedPropertyType.Float, SerializedPropertyType.Integer))
                return;
            if (!property.propertyPath.ReplaceLast(property.name, attribute.m_MaxTarget,out var maxPropertyPath))
                return;
            var maxProperty = property.serializedObject.FindProperty(maxPropertyPath);
            
            float min = property.floatValue;
            float max = maxProperty.floatValue;

            EditorGUI.BeginChangeCheck();
            Rect minmaxRect = position.Collapse(new Vector2(position.size.x / 7, 0f),new Vector2(0f,0f));
            EditorGUI.MinMaxSlider(minmaxRect,label,ref min,ref max,attribute.m_Min,attribute.m_Max);
            Rect labelRect = position.Collapse(new Vector2(position.size.x* 6f / 7, 0f),new Vector2(1f,0f)).Move(new Vector2(4f,0f));
            GUI.Label(labelRect,$"{min:F2}-{max:F2}");
            if (!EditorGUI.EndChangeCheck())
                return;
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    max = (int) max;
                    property.intValue = (int) min;
                    maxProperty.intValue = (int) max;
                    break;
                case SerializedPropertyType.Float:
                    property.floatValue = min;
                    maxProperty.floatValue = max;
                    break;
            }
        }
    }
    
    [CustomPropertyDrawer(typeof(CullingMaskAttribute))]
    public class CullingMaskPropertyDrawer : SubAttributePropertyDrawer<CullingMaskAttribute>
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!OnGUIAttributePropertyCheck(position, property, out CullingMaskAttribute attribute, SerializedPropertyType.Integer))
                return;
            Dictionary<int, string> allLayers = UECommon.GetAllLayers(true);
            List<string> values = new List<string>();
            foreach (int key in allLayers.Keys)
                values.Add(allLayers[key] == string.Empty ? null : allLayers[key]);
            for (int i = allLayers.Count - 1; i >= 0; i--)
            {
                if (allLayers.SelectValue(i) == string.Empty)
                    values.RemoveAt(i);
                else
                    break;
            }

            property.intValue = EditorGUI.MaskField(position, label.text, property.intValue, values.ToArray());
        }
    }
    [CustomPropertyDrawer(typeof(PositionAttribute))]
    public class PositionPropertyDrawer:SubAttributePropertyDrawer<PositionAttribute>
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight( property, label,true)+20f;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!OnGUIAttributePropertyCheck(position, property, out PositionAttribute attribute, SerializedPropertyType.Vector3))
                return;

            Rect propertyRect = new Rect(position.position,position.size-new Vector2(0,20));
            EditorGUI.PropertyField(propertyRect, property, label, true);
            float buttonWidth = position.size.x / 5f;
            Rect buttonRect = new Rect(position.position+new Vector2(buttonWidth*4f,EditorGUI.GetPropertyHeight(property,label,true)),new Vector2(buttonWidth,20f));
            if (GUI.Button(buttonRect, "Edit"))
                GUITransformHandles.Begin(property);
        }
    }
    [CustomPropertyDrawer(typeof(RangeVectorAttribute))]
    public class RangeVectorPropertyDrawer:SubAttributePropertyDrawer<RangeVectorAttribute>
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            switch(property.propertyType)
            {
                case SerializedPropertyType.Vector2: return m_Foldout ? 40 : 20;
                case SerializedPropertyType.Vector3: return m_Foldout? 60:20; 
                case SerializedPropertyType.Vector4: return m_Foldout? 60:20;
            }
            return base.GetPropertyHeight(property, label);
        }
        Vector4 mTempVector;
        bool m_Foldout = false;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!OnGUIAttributePropertyCheck(position, property, out RangeVectorAttribute attribute, SerializedPropertyType.Vector2,SerializedPropertyType.Vector3,SerializedPropertyType.Vector4))
                return;
            string format="";
            switch (property.propertyType)
            {
                case SerializedPropertyType.Vector2: format = "X:{1:0.00} Y:{2:0.00}"; mTempVector = property.vector2Value; break;
                case SerializedPropertyType.Vector3: format = "X:{1:0.00} Y:{2:0.00} Z:{3:0.00}"; mTempVector = property.vector3Value; break;
                case SerializedPropertyType.Vector4: format = "X:{1:0.00} Y:{2:0.00} Z:{3:0.00} W:{4:0.00}"; mTempVector = property.vector4Value; break;
            }
            float halfWidth = position.width / 2;
            float startX = position.x;
            position.width = halfWidth;
            position.height = 18;
            m_Foldout = EditorGUI.Foldout(position,m_Foldout, string.Format("{0} | "+format, label.text,mTempVector.x,mTempVector.y,mTempVector.z,mTempVector.w));
            if (!m_Foldout)
                return;
            position.y += 20;
            mTempVector.x = EditorGUI.Slider(position, mTempVector.x, attribute.m_Min, attribute.m_Max);
            position.x += position.width;
            mTempVector.y = EditorGUI.Slider(position, mTempVector.y, attribute.m_Min, attribute.m_Max);

            if (property.propertyType== SerializedPropertyType.Vector2)
            {
                property.vector2Value = mTempVector;
                return;
            }
            position.x = startX;
            position.y += 20;
            mTempVector.z = EditorGUI.Slider(position, mTempVector.z, attribute.m_Min, attribute.m_Max);
            if(property.propertyType== SerializedPropertyType.Vector3)
            {
                property.vector3Value = mTempVector;
                return;
            }

            position.x += position.width;
            position.width = halfWidth;
            mTempVector.w = EditorGUI.Slider(position, mTempVector.w, attribute.m_Min, attribute.m_Max);
            property.vector4Value = mTempVector;
        }
    }
    
    //To Be Continued(Unitys Property Array)
    // [CustomPropertyDrawer(typeof(PreloadAssetsAttribute))]
    // public class PreloadAssetsPropertyDrawer : SubAttributePropertyDrawer<PreloadAssetsAttribute>
    // {
    //     public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    //     {
    //         return EditorGUI.GetPropertyHeight(property,label);
    //     }
    //
    //     public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    //     {
    //         if (!OnGUIAttributePropertyCheck(position, property, out PreloadAssetsAttribute attribute, SerializedPropertyType.ObjectReference))
    //             return;
    //
    //         EditorGUI.PropertyField(position, property, label);
    //     }
    // }
    //
    #endregion
    
}