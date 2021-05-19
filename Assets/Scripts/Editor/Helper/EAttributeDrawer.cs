using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine.UIElements;

namespace TEditor
{
    #region Attributes
    public class MainAttributePropertyDrawer<T> : PropertyDrawer where T : Attribute
    {
        static readonly Type s_PropertyDrawerType = typeof(PropertyDrawer);
        PropertyDrawer m_DefaultPropertyDrawer;
        PropertyDrawer GetDefaultPropertyDrawer(SerializedProperty _property)
        {
            if (m_DefaultPropertyDrawer != null)
                return m_DefaultPropertyDrawer;

            FieldInfo targetField = _property.GetFieldInfo();
            IEnumerable<Attribute> attributes = targetField.GetCustomAttributes();
            int order = attribute.order+1;
            if (order >= attributes.Count())
                return null;

            Attribute nextAttribute = attributes.ElementAt(order);
            Type attributeType = nextAttribute.GetType();
            Type propertyDrawerType = (Type)Type.GetType("UnityEditor.ScriptAttributeUtility,UnityEditor").GetMethod("GetDrawerTypeForType", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { attributeType });
            m_DefaultPropertyDrawer = (PropertyDrawer)Activator.CreateInstance(propertyDrawerType);
            s_PropertyDrawerType.GetField("m_FieldInfo", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(m_DefaultPropertyDrawer, targetField);
            s_PropertyDrawerType.GetField("m_Attribute", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(m_DefaultPropertyDrawer, nextAttribute);
            m_DefaultPropertyDrawer.attribute.order = order;
            return m_DefaultPropertyDrawer;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var customDrawerHeight = GetDefaultPropertyDrawer(property)?.GetPropertyHeight(property, label);
            return customDrawerHeight.HasValue ? customDrawerHeight.Value : EditorGUI.GetPropertyHeight(property, label, true);
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var customDrawer = GetDefaultPropertyDrawer(property);
            if (customDrawer != null)
                customDrawer.OnGUI(position, property, label);
            else
                EditorGUI.PropertyField(position, property, label,true);
        }
        public bool CheckPropertyAvailable(bool fold, SerializedProperty _property, MFoldoutAttribute _attribute)
        {
            IEnumerable<KeyValuePair<FieldInfo, object>> fields = _property.AllRelativeFields();
            return _attribute.m_FieldsMatches.All(fieldMatch => fields.Any(field => {
                if (field.Key.Name != fieldMatch.Key)
                    return false;
                bool equals = fieldMatch.Value == null ? field.Value.Equals(null) : fieldMatch.Value.Contains(field.Value);
                return fold ? !equals : equals;
            }));
        }
    }
    public class SubAttributePropertyDrawer<T>: PropertyDrawer where T:Attribute
    {
        public bool OnGUIAttributePropertyCheck(Rect _position, SerializedProperty _property, out T _targetAttribute, params SerializedPropertyType[] _checkTypes) 
        {
            _targetAttribute = null;
            if (!_checkTypes.Any(p => _property.propertyType == p))
            {
                EditorGUI.LabelField(_position,string.Format("<Color=#FF0000>Attribute For {0} Only!</Color>", _checkTypes.ToString('|', type => type.ToString())), UEGUIStyle_Window.m_TitleLabel);
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
    public class MFoldoutProeprtyDrawer : MainAttributePropertyDrawer<MFoldoutAttribute>
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!CheckPropertyAvailable(false,property, attribute as MFoldoutAttribute))
                return -2;

            return base.GetPropertyHeight(property, label);
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!CheckPropertyAvailable(false,property, attribute as MFoldoutAttribute))
                return;
            base.OnGUI(position, property, label);
        }
    }
    [CustomPropertyDrawer(typeof(MFoldAttribute))]
    public class MFoldPropertyDrawer: MainAttributePropertyDrawer<MFoldoutAttribute>
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!CheckPropertyAvailable(true,property, attribute as MFoldoutAttribute))
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
                    property.intValue = Mathf.Clamp(property.intValue, (int)attribute.m_Min, (int)attribute.m_Max);
                    break;
                case SerializedPropertyType.Float:
                    property.floatValue = Mathf.Clamp(property.floatValue, attribute.m_Min, attribute.m_Max);
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
            foreach(int key in allLayers.Keys)
                values.Add(allLayers[key]== string.Empty?null: allLayers[key]);
            for(int i=allLayers.Count-1;i>=0;i--)
            {
                if (allLayers.GetIndexValue(i) == string.Empty)
                    values.RemoveAt(i);
                else
                    break;
            }

            property.intValue = EditorGUI.MaskField(position, label.text, property.intValue, values.ToArray());
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
        Vector4 m_Vector;
        bool m_Foldout = false;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!OnGUIAttributePropertyCheck(position, property, out RangeVectorAttribute attribute, SerializedPropertyType.Vector2,SerializedPropertyType.Vector3,SerializedPropertyType.Vector4))
                return;
            string format="";
            switch (property.propertyType)
            {
                case SerializedPropertyType.Vector2: format = "X:{1:0.00} Y:{2:0.00}"; m_Vector = property.vector2Value; break;
                case SerializedPropertyType.Vector3: format = "X:{1:0.00} Y:{2:0.00} Z:{3:0.00}"; m_Vector = property.vector3Value; break;
                case SerializedPropertyType.Vector4: format = "X:{1:0.00} Y:{2:0.00} Z:{3:0.00} W:{4:0.00}"; m_Vector = property.vector4Value; break;
            }
            float halfWidth = position.width / 2;
            float startX = position.x;
            position.width = halfWidth;
            position.height = 18;
            m_Foldout = EditorGUI.Foldout(position,m_Foldout, string.Format("{0} | "+format, label.text,m_Vector.x,m_Vector.y,m_Vector.z,m_Vector.w));
            if (!m_Foldout)
                return;
            position.y += 20;
            m_Vector.x = EditorGUI.Slider(position, m_Vector.x, attribute.m_Min, attribute.m_Max);
            position.x += position.width;
            m_Vector.y = EditorGUI.Slider(position, m_Vector.y, attribute.m_Min, attribute.m_Max);

            if (property.propertyType== SerializedPropertyType.Vector2)
            {
                property.vector2Value = m_Vector;
                return;
            }
            position.x = startX;
            position.y += 20;
            m_Vector.z = EditorGUI.Slider(position, m_Vector.z, attribute.m_Min, attribute.m_Max);
            if(property.propertyType== SerializedPropertyType.Vector3)
            {
                property.vector3Value = m_Vector;
                return;
            }

            position.x += position.width;
            position.width = halfWidth;
            m_Vector.w = EditorGUI.Slider(position, m_Vector.w, attribute.m_Min, attribute.m_Max);
            property.vector4Value = m_Vector;
        }
    }
    #endregion
    #endregion
}

