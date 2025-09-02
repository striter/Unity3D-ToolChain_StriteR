using UnityEngine;

namespace UnityEditor.Extensions.MaterialPropertyDrawer
{
    public class ColorUsageDrawer : AMaterialPropertyDrawerBase
    {
        private bool m_Alpha;
        private bool m_HDR;
        public ColorUsageDrawer(string _alpha,string _hdr)
        {
            m_Alpha = bool.Parse(_alpha);
            m_HDR = bool.Parse(_hdr);
        }
        public override bool PropertyTypeCheck(MaterialProperty.PropType type)=>type== MaterialProperty.PropType.Color;
        public override void OnPropertyGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            prop.colorValue = EditorGUI.ColorField(position,new GUIContent(label), prop.colorValue,true,m_Alpha,m_HDR);
        }
    }

}