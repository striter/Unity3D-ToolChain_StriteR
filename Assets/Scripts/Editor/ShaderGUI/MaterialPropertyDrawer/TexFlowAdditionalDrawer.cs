using UnityEngine;

namespace UnityEditor.Extensions.MaterialPropertyDrawer
{
    public class TexFlowAdditionalDrawer : AMaterialPropertyDrawerBase
    {
        private readonly string m_AdditionalValue;
        private MaterialProperty m_NextProperty, m_ST1Property, m_ST2Property;
        public TexFlowAdditionalDrawer(string additionalValue) {  m_AdditionalValue = additionalValue; }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            m_NextProperty = MaterialEditor.GetMaterialProperty(editor.targets, m_AdditionalValue);
            m_ST1Property = MaterialEditor.GetMaterialProperty(editor.targets, prop.name + "_ST");
            m_ST2Property = MaterialEditor.GetMaterialProperty(editor.targets, m_AdditionalValue + "_ST");
            return base.GetPropertyHeight(prop, label, editor) * 2 + 2;
        }

        public override void OnPropertyGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            EditorGUI.BeginChangeCheck();
            var verticalSize = position.size.y / 2;
            position = position.ResizeY(verticalSize);
            prop.vectorValue = editor.VectorProperty(position,prop,label);
            m_NextProperty.vectorValue = editor.VectorProperty(position.MoveY(verticalSize), m_NextProperty, m_NextProperty.displayName);
            if (EditorGUI.EndChangeCheck())
            {
                var srcValue = prop.vectorValue;
                m_ST1Property.vectorValue = TexFlowDrawer.ConvertToST(srcValue);

                var combineValue = m_NextProperty.vectorValue;
                m_ST2Property.vectorValue = TexFlowDrawer.ConvertToST(new Vector4(
                    srcValue.x * combineValue.x,
                    srcValue.y * combineValue.y,
                    srcValue.z * combineValue.z, 
                    srcValue.w + combineValue.w));
            }
        }
    }
}