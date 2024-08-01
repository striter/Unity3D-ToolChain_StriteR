using UnityEngine;

namespace UnityEditor.Extensions
{

    [CustomPropertyDrawer(typeof(PositionAttribute))]
    public class PositionPropertyDrawer:ASubAttributePropertyDrawer<PositionAttribute>
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight( property, label,true)+20f;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!OnGUIAttributePropertyCheck(position, property, SerializedPropertyType.Vector3))
                return;

            Rect propertyRect = new Rect(position.position,position.size-new Vector2(0,20));
            EditorGUI.PropertyField(propertyRect, property, label, true);
            float buttonWidth = position.size.x / 5f;
            Rect buttonRect = new Rect(position.position+new Vector2(buttonWidth*4f,EditorGUI.GetPropertyHeight(property,label,true)),new Vector2(buttonWidth,20f));
            if (GUI.Button(buttonRect, "Edit"))
                GUITransformHandles.Begin(property);
        }
    }
}