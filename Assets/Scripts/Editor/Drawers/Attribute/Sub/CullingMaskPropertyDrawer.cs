using System.Collections.Generic;
using System.Linq.Extensions;
using UnityEngine;

namespace UnityEditor.Extensions
{
 
    [CustomPropertyDrawer(typeof(CullingMaskAttribute))]
    public class CullingMaskPropertyDrawer : ASubAttributePropertyDrawer<CullingMaskAttribute>
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!OnGUIAttributePropertyCheck(position, property, SerializedPropertyType.Integer))
                return;
            var allLayers = UECommon.GetAllLayers(true);
            var values = new List<string>();
            foreach (var key in allLayers.Keys)
                values.Add(allLayers[key] == string.Empty ? null : allLayers[key]);
            for (int i = allLayers.Count - 1; i >= 0; i--)
            {
                if (allLayers.SelectValue(i) == string.Empty)
                    values.RemoveAt(i);
                else
                    break;
            }

            property.intValue = EditorGUI.MaskField(position, label.text, property.intValue, values.ToArray());
        }
    }
}