using UnityEngine;

namespace UnityEditor.Extensions
{

    [CustomPropertyDrawer(typeof(Rename))]
    public class RenamePropertyDrawer : ASubAttributePropertyDrawer<Rename>
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if(!OnGUIAttributePropertyCheck(position,property))
                return;
            label.text = attribute.name;
            EditorGUI.PropertyField(position, property, label, true);
        }
    }
}