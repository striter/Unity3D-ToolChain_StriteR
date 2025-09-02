using Rendering;
using UnityEngine;

namespace UnityEditor.Extensions.MaterialPropertyDrawer
{

    public class ToggleTexDrawer: AMaterialPropertyDrawerBase
    {
        protected readonly string m_Keyword;
        public ToggleTexDrawer(string _keyword) {  m_Keyword = _keyword; }
        public override bool PropertyTypeCheck(MaterialProperty.PropType type) => type == MaterialProperty.PropType.Texture;
        public override void OnPropertyGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            EditorGUI.BeginChangeCheck();
            editor.DefaultShaderProperty(prop, label);
            if (!EditorGUI.EndChangeCheck()) 
                return;
            EnableKeyword(prop);
        }
        public override void Apply(MaterialProperty prop)
        {
            base.Apply(prop);
            EnableKeyword(prop);
        }

        void EnableKeyword(MaterialProperty _property)
        {
            foreach (Material material in _property.targets)
                material.EnableKeyword(m_Keyword, _property.textureValue != null);
        }
    }
}