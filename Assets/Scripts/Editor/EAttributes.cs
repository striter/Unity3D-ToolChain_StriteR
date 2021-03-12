using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

namespace TEditor
{
    public class PropertyDrawer_Extend<T>: PropertyDrawer where T:Attribute
    {
        public bool OnGUIAttributePropertyCheck(Rect _position, SerializedProperty _property, out T _targetAttribute, params SerializedPropertyType[] _checkTypes) 
        {
            _targetAttribute = null;
            if (!_checkTypes.Any(p => _property.propertyType == p))
            {
                EditorGUI.LabelField(_position,string.Format("<Color=#FF0000>Attribute For {0} Only!</Color>", _checkTypes.ToString_Readable("|", type => type.ToString())),TEditor_GUIStyle.m_ErrorLabel);
                return false;
            }
            _targetAttribute = attribute as T;
            return true;
        }
    }

    [CustomPropertyDrawer(typeof(CullingMaskAttribute))]
    public class CullingMaskPropertyDrawer : PropertyDrawer_Extend<CullingMaskAttribute>
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!OnGUIAttributePropertyCheck(position, property, out CullingMaskAttribute attribute, SerializedPropertyType.Integer))
                return;
            Dictionary<int, string> allLayers = EUCommon.GetAllLayers(true);
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

            property.intValue = EditorGUI.MaskField(position, "Culling Mask", property.intValue, values.ToArray());
        }
    }

    [CustomPropertyDrawer(typeof(RangeIntAttribute))]
    public class RangeIntPropertyDrawer: PropertyDrawer_Extend<RangeIntAttribute>
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!OnGUIAttributePropertyCheck(position, property, out RangeIntAttribute attribute, SerializedPropertyType.Integer))
                return;

            property.intValue = EditorGUI.IntSlider(position,property.name.ToString_FieldName(),property.intValue, attribute.m_Min, attribute.m_Max);
        }

    }

    [CustomPropertyDrawer(typeof(RangeVectorAttribute))]
    public class RangeVectorPropertyDrawer:PropertyDrawer_Extend<RangeVectorAttribute>
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
            float labelWidth = halfWidth / 6;
            float startX = position.x;
            position.height = 18;
            m_Foldout = EditorGUI.Foldout(position,m_Foldout, string.Format("{0} | "+format, property.name.ToString_FieldName(),m_Vector.x,m_Vector.y,m_Vector.z,m_Vector.w));
            if (!m_Foldout)
                return;

            position.y += 20;
            position.width = labelWidth;
            EditorGUI.LabelField(position,"X",EditorStyles.miniBoldLabel);
            position.x += position.width;
            position.width = halfWidth - labelWidth;
            m_Vector.x = EditorGUI.Slider(position, m_Vector.x, attribute.m_Min, attribute.m_Max);

            position.x += position.width;
            position.width = labelWidth;
            EditorGUI.LabelField(position, "Y", EditorStyles.miniBoldLabel);
            position.x += position.width;
            position.width = halfWidth - labelWidth;
            m_Vector.y = EditorGUI.Slider(position, m_Vector.y, attribute.m_Min, attribute.m_Max);

            if (property.propertyType== SerializedPropertyType.Vector2)
            {
                property.vector2Value = m_Vector;
                return;
            }
            position.x = startX;
            position.y += 20;

            position.width = labelWidth;
            EditorGUI.LabelField(position, "Z", EditorStyles.miniBoldLabel);
            position.x += position.width;
            position.width = halfWidth - labelWidth;
            m_Vector.z = EditorGUI.Slider(position, m_Vector.z, attribute.m_Min, attribute.m_Max);
            if(property.propertyType== SerializedPropertyType.Vector3)
            {
                property.vector3Value = m_Vector;
                return;
            }

            position.x += position.width;
            position.width = labelWidth;
            EditorGUI.LabelField(position, "W", EditorStyles.miniBoldLabel);
            position.x += position.width;
            position.width = halfWidth - labelWidth;
            m_Vector.w = EditorGUI.Slider(position, m_Vector.w, attribute.m_Min, attribute.m_Max);
            property.vector4Value = m_Vector;
        }
    }
}

