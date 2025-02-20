using UnityEngine;

namespace UnityEditor.Extensions
{

    [CustomPropertyDrawer(typeof(MinMaxRangeAttribute))]
    public class MinMaxRangePropertyDrawer : AAttributePropertyDrawer<MinMaxRangeAttribute>
    {
     
        void DrawMinmaxGUI(Rect position,GUIContent label,MinMaxRangeAttribute attribute,ref float min,ref float max)
        {
            Rect minmaxRect = position.Collapse(new Vector2(position.size.x / 6, 0f),new Vector2(0f,0f));
            EditorGUI.MinMaxSlider(minmaxRect,label,ref min,ref max,attribute.m_Min,attribute.m_Max);
            Rect labelRect = position.Collapse(new Vector2(position.size.x* 5f / 6, 0f),new Vector2(1f,0f)).Move(new Vector2(4f,0f));
            GUI.Label(labelRect,$"{min:F2}-{max:F2}");
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if(!OnGUIAttributePropertyCheck(position,property,SerializedPropertyType.Float, SerializedPropertyType.Integer,SerializedPropertyType.Generic))
                return;

            EditorGUI.BeginChangeCheck();
            
            if (property.propertyType == SerializedPropertyType.Generic)
            {
                if(property.type != nameof(RangeFloat))
                    return;
                
                var minProperty = property.serializedObject.FindProperty($"{property.propertyPath}.{nameof(RangeFloat.start)}");
                var lengthProperty = property.serializedObject.FindProperty($"{property.propertyPath}.{nameof(RangeFloat.length)}");
                
                var min = minProperty.floatValue;
                var max = min + lengthProperty.floatValue;
                
                DrawMinmaxGUI(position,label,attribute,ref min,ref max);
                    
                if (!EditorGUI.EndChangeCheck()) return;
                minProperty.floatValue = min;
                lengthProperty.floatValue = max - min;
            }
            else
            {
                if (!property.propertyPath.ReplaceLast(property.name, attribute.m_MaxTarget,out var maxPropertyPath))
                    return;
                
                var minProperty = property;
                var maxProperty = property.serializedObject.FindProperty(maxPropertyPath);
                var min = minProperty.floatValue;
                var max = maxProperty.floatValue;

                DrawMinmaxGUI(position,label,attribute,ref min,ref max);
                if (!EditorGUI.EndChangeCheck()) return;
                switch (property.propertyType)
                {
                    case SerializedPropertyType.Integer:
                        max = (int) max;
                        property.intValue = (int) min;
                        maxProperty.intValue = (int) max;
                        break;
                    case SerializedPropertyType.Float:
                        property.floatValue = min;
                        maxProperty.floatValue = max;
                        break;
                }
            }
        }
    }
}