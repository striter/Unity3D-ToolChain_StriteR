using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TEditor
{
    [CustomPropertyDrawer(typeof(CullingMaskAttribute))]
    public class CullingMaskPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Integer)
            {
                EditorGUI.LabelField(position, label.text, "Attributes For Integer Only!");
                return;
            }
            Dictionary<int, string> allLayers = TEditor.GetAllLayers(true);
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

    [CustomPropertyDrawer(typeof(RangeVectorAttribute))]
    public class RangeVectorPropertyDrawer:PropertyDrawer
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
            if (property.propertyType != SerializedPropertyType.Vector2  && property.propertyType != SerializedPropertyType.Vector3 && property.propertyType != SerializedPropertyType.Vector4)
            {
                EditorGUI.LabelField(position, "Attributes For Vectors(2/3/4) Only!"); 
                return;
            }
            string format="";
            switch (property.propertyType)
            {
                case SerializedPropertyType.Vector2: format = "X:{1:0.00} Y:{2:0.00}"; m_Vector = property.vector2Value; break;
                case SerializedPropertyType.Vector3: format = "X:{1:0.00} Y:{2:0.00} Z:{3:0.00}"; m_Vector = property.vector3Value; break;
                case SerializedPropertyType.Vector4: format = "X:{1:0.00} Y:{2:0.00} Z:{3:0.00} W:{4:0.00}"; m_Vector = property.vector4Value; break;
            }
            RangeVectorAttribute m_Attribute = attribute as RangeVectorAttribute;
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
            m_Vector.x = EditorGUI.Slider(position, m_Vector.x, m_Attribute.m_Min, m_Attribute.m_Max);

            position.x += position.width;
            position.width = labelWidth;
            EditorGUI.LabelField(position, "Y", EditorStyles.miniBoldLabel);
            position.x += position.width;
            position.width = halfWidth - labelWidth;
            m_Vector.y = EditorGUI.Slider(position, m_Vector.y, m_Attribute.m_Min, m_Attribute.m_Max);

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
            m_Vector.z = EditorGUI.Slider(position, m_Vector.z, m_Attribute.m_Min, m_Attribute.m_Max);
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
            m_Vector.w = EditorGUI.Slider(position, m_Vector.w, m_Attribute.m_Min, m_Attribute.m_Max);
            property.vector4Value = m_Vector;
        }
    }
}

