using System.Linq;
using UnityEngine;

namespace UnityEditor.Extensions
{

    [CustomPropertyDrawer(typeof(IntEnumAttribute))]
    public class IntEnumPropertyDrawer : AAttributePropertyDrawer<IntEnumAttribute>
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if(!OnGUIAttributePropertyCheck(position,property,SerializedPropertyType.Float,SerializedPropertyType.Integer))
                return;
            property.intValue = EditorGUI.IntPopup(position,label,property.intValue,attribute.m_Values.Select(p=>new GUIContent( p.ToString())).ToArray(),attribute.m_Values);
        }
    }

}