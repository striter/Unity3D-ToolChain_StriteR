using Rendering.GI.SphericalHarmonics;
using UnityEngine;

namespace UnityEditor.Extensions.MaterialPropertyDrawer
{
    public class SHGradientDrawer : AMaterialPropertyDrawerBase
    {   //Dude
        public override bool PropertyTypeCheck(MaterialProperty.PropType _type) => _type == MaterialProperty.PropType.Color;

        private string keyword;
        public SHGradientDrawer(string _keyword)
        {
            keyword = _keyword;
        }

        public override void OnPropertyGUI(Rect _position, MaterialProperty _prop, string _label, MaterialEditor _editor)
        {
            EditorGUI.BeginChangeCheck();
            _prop.colorValue = EditorGUI.ColorField(_position,new GUIContent(_label),_prop.colorValue,true,false,true);

            if (EditorGUI.EndChangeCheck())
            {
                var sky = MaterialEditor.GetMaterialProperty(_editor.targets, keyword+"Sky");
                var equator =  MaterialEditor.GetMaterialProperty(_editor.targets, keyword + "Equator");
                var ground =  MaterialEditor.GetMaterialProperty(_editor.targets, keyword + "Ground");
                var shData = SphericalHarmonicsExport.ExportGradient(sky.colorValue.to3(), equator.colorValue.to3(), ground.colorValue.to3());
                var output = (SHL2ShaderConstants)shData;
                
                MaterialEditor.GetMaterialProperty(_editor.targets, keyword+"SHAr").vectorValue = output.shAr;
                MaterialEditor.GetMaterialProperty(_editor.targets, keyword+"SHAg").vectorValue = output.shAg;
                MaterialEditor.GetMaterialProperty(_editor.targets, keyword+"SHAb").vectorValue = output.shAb;
                MaterialEditor.GetMaterialProperty(_editor.targets, keyword+"SHBr").vectorValue = output.shBr;
                MaterialEditor.GetMaterialProperty(_editor.targets, keyword+"SHBg").vectorValue = output.shBg;
                MaterialEditor.GetMaterialProperty(_editor.targets, keyword+"SHBb").vectorValue = output.shBb;
                MaterialEditor.GetMaterialProperty(_editor.targets, keyword+"SHC").vectorValue = output.shC.to4();
            }
        }
    }
}