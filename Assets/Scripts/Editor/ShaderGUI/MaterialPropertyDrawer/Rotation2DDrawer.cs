using UnityEngine;

namespace UnityEditor.Extensions.MaterialPropertyDrawer
{
    public class Rotation2DDrawer : AMaterialPropertyDrawerBase
    {
        public override void OnPropertyGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            EditorGUI.BeginChangeCheck();
            prop.floatValue = EditorGUI.Slider(position, label, prop.floatValue, 0f, 360f);
            if (EditorGUI.EndChangeCheck())
            {
                var rotationMatrix = umath.Rotate2D(prop.floatValue * kmath.kDeg2Rad);

                var matrixProperty = MaterialEditor.GetMaterialProperty(editor.targets, prop.name + "Matrix");
                matrixProperty.vectorValue = new Vector4(rotationMatrix.c0.x, rotationMatrix.c1.x,
                    rotationMatrix.c1.x, rotationMatrix.c1.y);
            }
        }
    }
}