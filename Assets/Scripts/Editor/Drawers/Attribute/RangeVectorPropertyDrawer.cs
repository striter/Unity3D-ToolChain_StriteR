using UnityEngine;
namespace UnityEditor.Extensions
{
    [CustomPropertyDrawer(typeof(RangeVectorAttribute))]
    public class RangeVectorPropertyDrawer:AAttributePropertyDrawer<RangeVectorAttribute>
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
            if (!OnGUIAttributePropertyCheck(position, property, SerializedPropertyType.Vector2,SerializedPropertyType.Vector3,SerializedPropertyType.Vector4))
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
}