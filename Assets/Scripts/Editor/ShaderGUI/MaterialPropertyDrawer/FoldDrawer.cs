using System.Linq;
using UnityEngine;

namespace UnityEditor.Extensions.MaterialPropertyDrawer
{
    public class FoldDrawer : AMaterialPropertyDrawerBase
    {
        private string[] m_Keywords;
        public FoldDrawer(string[] _keywords) { m_Keywords = _keywords; }
        public FoldDrawer(string _kw1) : this(new string[] { _kw1 }) { }
        public FoldDrawer(string _kw1, string _kw2) : this(new string[] { _kw1, _kw2 }) { }
        public FoldDrawer(string _kw1, string _kw2, string _kw3) : this(new string[] { _kw1, _kw2, _kw3 }) { }
        public FoldDrawer(string _kw1, string _kw2, string _kw3, string _kw4) : this(new string[] { _kw1, _kw2, _kw3, _kw4 }) { }
        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return 0;
        }

        public bool PropertyTypeCheck(MaterialProperty prop)
        {
            foreach (Material material in prop.targets)
                if (m_Keywords.Any(keyword => material.IsKeywordEnabled(keyword)))
                    return true;
            return false;
        }

        public override void OnPropertyGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            base.OnPropertyGUI(position, prop, label, editor);
            if (PropertyTypeCheck(prop))
                return;
            editor.DefaultShaderProperty(prop, label);
        }
    }
}