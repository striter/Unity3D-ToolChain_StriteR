using UnityEngine;

namespace UnityEditor.Extensions
{
    [CustomPropertyDrawer(typeof(AssetAttribute))]
    public class AssetAttributeDrawer : AAttributePropertyDrawer<AssetAttribute>
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!OnGUIAttributePropertyCheck(position, property, SerializedPropertyType.ObjectReference))
                return;
            base.OnGUI(position, property, label);
            if (property.objectReferenceValue == null)
                property.objectReferenceValue = attribute.m_Getter();
        }
    }
}