using System.Linq;
using UnityEngine;

namespace UnityEditor.Extensions
{

    [CustomPropertyDrawer(typeof(IntEnumAttribute))]
    public class EnumPropertyDrawer : AAttributePropertyDrawer<IntEnumAttribute>
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!OnGUIAttributePropertyCheck(position, property, SerializedPropertyType.Float,
                    SerializedPropertyType.Integer, SerializedPropertyType.String))
                return;

            var value = property.GetFieldValue();
            Debug.Log(value);
            

            // switch (property.propertyType)
            // {
                // case SerializedPropertyType.Integer:
                    // property.intValue = EditorGUI.IntPopup(position, label, property.intValue,
                        // attribute.m_Values.Select(p => new GUIContent(p.ToString())).ToArray(),
                        // attribute.m_Values.Select(p => (int)p).ToArray());
                    // break;
                // case SerializedPropertyType.Float:
                    // property.floatValue = EditorGUI.Popup()
                    // break;
                // case SerializedPropertyType.String:
                    // break;
            // }
        }
    }

}