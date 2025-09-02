using UnityEngine;

namespace UnityEditor.Extensions.MaterialPropertyDrawer
{
    public class Vector2Drawer: AMaterialPropertyDrawerBase
    {
        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)=> EditorGUI.GetPropertyHeight( SerializedPropertyType.Vector2,new GUIContent(label));
        public override bool PropertyTypeCheck(MaterialProperty.PropType type) => type == MaterialProperty.PropType.Vector;
        public override void OnPropertyGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            base.OnPropertyGUI(position, prop, label, editor);
            prop.vectorValue = EditorGUI.Vector2Field(position, label,prop.vectorValue);
        }
    }
}