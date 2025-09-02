using UnityEngine;

namespace UnityEditor.Extensions.MaterialPropertyDrawer
{
    public class MinMaxRangeDrawer : AMaterialPropertyDrawerBase
    {
        private float m_Min;
        private float m_Max;
        private float m_ValueMin;
        private float m_ValueMax;
        private MaterialProperty property;
        public override bool PropertyTypeCheck(MaterialProperty.PropType type) => type == MaterialProperty.PropType.Range;

        public override void OnPropertyGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            var prop0 = MaterialEditor.GetMaterialProperty(editor.targets, prop.name);
            var prop1 =  MaterialEditor.GetMaterialProperty(editor.targets, prop.name + "End");

            float value0 = prop0.floatValue;
            float value1 =prop1.floatValue;
            
            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 0.0f;
            // EditorGUI.showMixedValue = hasMixedValue1;
            EditorGUI.BeginChangeCheck();

            Rect minmaxRect = position.Collapse(new Vector2(position.size.x / 5, 0f),new Vector2(0f,0f));
            EditorGUI.MinMaxSlider(minmaxRect,label,ref value0,ref value1,prop.rangeLimits.x,prop.rangeLimits.y);
            Rect labelRect = position.Collapse(new Vector2(position.size.x*4f / 5, 0f),new Vector2(1f,0f)).Move(new Vector2(2f,0f));
            GUI.Label(labelRect,$"{value0:F1}-{value1:F1}");
            
            if (EditorGUI.EndChangeCheck())
            {
                prop0.floatValue = value0;
                prop1.floatValue = value1;
            }
            
            // EditorGUI.showMixedValue = false;
            EditorGUIUtility.labelWidth = labelWidth;
        }
    }
}