using UnityEngine;

namespace UnityEditor.Extensions.MaterialPropertyDrawer
{
    public class Vector3Drawer : AMaterialPropertyDrawerBase
    {
        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)=> EditorGUI.GetPropertyHeight( SerializedPropertyType.Vector3,new GUIContent(label));
        public override bool PropertyTypeCheck(MaterialProperty.PropType type) => type == MaterialProperty.PropType.Vector;
        public override void OnPropertyGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            base.OnPropertyGUI(position, prop, label, editor);
            prop.vectorValue = EditorGUI.Vector3Field(position, label, prop.vectorValue);
        }
    }

}