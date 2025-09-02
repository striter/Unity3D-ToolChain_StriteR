using UnityEngine;

namespace UnityEditor.Extensions.MaterialPropertyDrawer
{
    public class FoldoutDrawer: FoldDrawer
    {
        public FoldoutDrawer(string _kw1) : base(new string[] { _kw1 }) {}
        public FoldoutDrawer(string _kw1, string _kw2) : base(new string[] { _kw1, _kw2 }) { }
        public FoldoutDrawer(string _kw1, string _kw2,string _kw3) : base(new string[] { _kw1, _kw2 ,_kw3}) { }
        public FoldoutDrawer(string _kw1, string _kw2,string _kw3,string _kw4) : base(new string[] { _kw1, _kw2 ,_kw3,_kw4}) { }
        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return 0;
        }

        public override void OnPropertyGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            if (!PropertyTypeCheck(prop))
                return;
            
            base.OnPropertyGUI(position, prop, label, editor);
            editor.DefaultShaderProperty(prop, label);
        }
    }
}