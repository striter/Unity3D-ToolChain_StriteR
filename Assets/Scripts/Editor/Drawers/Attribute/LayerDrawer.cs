using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using UnityEngine;

namespace UnityEditor.Extensions.AttributeDrawers
{
    [CustomPropertyDrawer(typeof(LayerAttribute))]
    public class LayerDrawer : PropertyDrawer
    {
        private static List<(int, string)> kAllLayers = new();
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            UECommon.GetAllLayers().FillList(kAllLayers);
            
            var layerIndex = property.intValue;
            var index = kAllLayers.FindIndex(p => p.Item1 == layerIndex);
            index = EditorGUI.Popup(position, label.text, index, kAllLayers.Select(p => p.Item2).ToArray());
            property.intValue = kAllLayers[index].Item1;
        }
    }
}