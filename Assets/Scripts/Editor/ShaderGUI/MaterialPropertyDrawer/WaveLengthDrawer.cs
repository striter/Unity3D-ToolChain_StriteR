using UnityEngine;

namespace UnityEditor.Extensions.MaterialPropertyDrawer
{
    public class WaveLengthDrawer : AMaterialPropertyDrawerBase
    {
        private string keyword;
        public WaveLengthDrawer(string _keyword)
        {
            keyword = _keyword;
        }
        public override bool PropertyTypeCheck(MaterialProperty.PropType type)=>type == MaterialProperty.PropType.Vector;

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return base.GetPropertyHeight(prop, label, editor) + 20;
        }

        public override void OnPropertyGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            editor.DefaultShaderProperty(position.ResizeY(position.y-20),prop,label);
            Vector4 srcValue = prop.vectorValue;
            Vector3 waveLength = srcValue;
            float strength = srcValue.w;
            MaterialEditor.GetMaterialProperty(editor.targets, keyword).vectorValue = new Vector3(
                Mathf.Pow(400f/waveLength.x,4),Mathf.Pow(400f/waveLength.y,4),Mathf.Pow(400f/waveLength.z,4))*strength;
        }
    }
}