using UnityEngine;

namespace UnityEditor.Extensions.AttributeDrawers
{
    [CustomPropertyDrawer(typeof(TitleAttribute))]
    public class TitleDrawer : AAttributePropertyDrawer<TitleAttribute>
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.LabelField(position,label.text,UEGUIStyle_Window.m_TitleLabel);
            label.text = new string(' ', label.text.Length);
            EditorGUI.PropertyField(position, property, label, true);
        }
    }
}