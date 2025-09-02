using Unity.Mathematics;
using UnityEngine;

namespace UnityEditor.Extensions.MaterialPropertyDrawer
{
    public class TexFlowDrawer : AMaterialPropertyDrawerBase
    {
        public override void OnPropertyGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            EditorGUI.BeginChangeCheck();
            editor.DefaultShaderProperty(prop,label);
            if (EditorGUI.EndChangeCheck())
            {
                var scalingProperty = MaterialEditor.GetMaterialProperty(editor.targets, prop.name + "_ST");
                scalingProperty.vectorValue = ConvertToST( prop.vectorValue);
            }
        }

        public static float4 ConvertToST(float4 _srcValue) //scale,speed,aspect,angle(deg)
        {
            var scale = _srcValue.x;
            var speed = _srcValue.y;
            var aspect = _srcValue.z;
            var angle = _srcValue.w;
            var scaling = aspect > 0 ? new float2(1f / scale, aspect / scale) : new float2(-aspect/scale,1f/scale);
            var rad = angle * kmath.kDeg2Rad;
            var flow = (new float2(math.cos(rad), math.sin(rad)) * scaling) * speed;
            return new float4(scaling.x,scaling.y,flow.x,flow.y);
        }
    }
}