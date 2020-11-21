using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
namespace TEditor
{
    [CustomPropertyDrawer(typeof(CullingMaskAttribute))]
    public class CullingMaskPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // First get the attribute since it contains the range for the slider
            CullingMaskAttribute range = attribute as CullingMaskAttribute;
            if (property.propertyType != SerializedPropertyType.Integer)
            {
                EditorGUI.LabelField(position, label.text, "Use Range Integer Only!");
                return;
            }
            Dictionary<int, string> allLayers = TEditor.GetAllLayers();


            property.intValue = EditorGUI.MaskField(position, "Culling Mask", property.intValue, allLayers.Values.ToArray());
        }
    }
}

