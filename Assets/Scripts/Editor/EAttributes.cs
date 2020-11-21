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
            if (property.propertyType != SerializedPropertyType.Integer)
            {
                EditorGUI.LabelField(position, label.text, "Attributes For Integer Only!");
                return;
            }
            Dictionary<int, string> allLayers = TEditor.GetAllLayers(true);
            List<string> values = new List<string>();
            foreach(int key in allLayers.Keys)
                values.Add(allLayers[key]== string.Empty?null: allLayers[key]);
            for(int i=allLayers.Count-1;i>=0;i--)
            {
                if (allLayers.GetIndexValue(i) == string.Empty)
                    values.RemoveAt(i);
                else
                    break;
            }

            property.intValue = EditorGUI.MaskField(position, "Culling Mask", property.intValue, values.ToArray());
        }
    }
}

