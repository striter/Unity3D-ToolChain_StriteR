using UnityEngine;

namespace UnityEditor.Extensions
{

    [CustomPropertyDrawer(typeof(ReadonlyAttribute))]
    public class ReadonlyPropertyDrawer : AAttributePropertyDrawer<ReadonlyAttribute>
    {
        public override float GetPropertyHeight(SerializedProperty _property, GUIContent _label) => EditorGUI.GetPropertyHeight(_property, _label, true);

        public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(_position, _property, _label, true);
            GUI.enabled = true;
        }
    }
}