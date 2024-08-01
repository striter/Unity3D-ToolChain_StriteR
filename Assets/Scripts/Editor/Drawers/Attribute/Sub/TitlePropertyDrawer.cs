using UnityEngine;

namespace UnityEditor.Extensions
{
    [CustomPropertyDrawer(typeof(TitleAttribute))]
    public class TitlePropertyDrawer : ASubAttributePropertyDrawer<TitleAttribute>
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            label.text = string.Empty;
            return EditorGUI.GetPropertyHeight(property,label,true) + 2f;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect titleRect = position;
            titleRect.height = 18;
            EditorGUI.LabelField(titleRect, label, UEGUIStyle_Window.m_TitleLabel);
            label.text = " ";
            EditorGUI.PropertyField(position, property, label, true);
        }
    }

}