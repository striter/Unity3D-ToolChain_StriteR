using UnityEngine;

namespace UnityEditor.Extensions.AttributeDrawers
{

    [CustomPropertyDrawer(typeof(RenameAttribute))]
    public class RenameDrawer : AAttributePropertyDrawer<RenameAttribute>
    {
        public override float GetPropertyHeight(SerializedProperty _property, GUIContent _label) => EditorGUI.GetPropertyHeight(_property, _label, true);
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if(!OnGUIAttributePropertyCheck(position,property))
                return;
            label.text = attribute.name;
            EditorGUI.PropertyField(position, property, label, true);
        }
    }
}