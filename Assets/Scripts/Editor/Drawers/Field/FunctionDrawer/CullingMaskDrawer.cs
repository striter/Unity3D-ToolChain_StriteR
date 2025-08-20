using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using UnityEngine;

namespace UnityEditor.Extensions
{
    [CustomPropertyDrawer(typeof(CullingMask))]
    public class CullingMaskDrawer : PropertyDrawer
    {
        private SerializedProperty m_Mask;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            m_Mask = property.FindPropertyRelative("value");
            return base.GetPropertyHeight(property, label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) => DrawGUI(position, m_Mask, label);

        private static List<(int, string)> kAllLayers = new();
        public static void DrawGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            UECommon.GetAllLayers().FillList(kAllLayers);
            var srcMask = property.intValue;
            var displayMask = 0;
            for (var i = 0; i < kAllLayers.Count; i++)
            {
                var (layerIndex, _) = kAllLayers[i];
                if (CullingMask.Enabled(srcMask, layerIndex))
                    displayMask |= 1 << i;
            }
            displayMask = EditorGUI.MaskField(position, label, displayMask, kAllLayers.Select(p => p.Item2).ToArray());
            var newMask = 0;
            for (var i = 0; i < kAllLayers.Count; i++)
            {
                var (layerIndex, _) = kAllLayers[i];
                if (CullingMask.Enabled(displayMask,i))
                    newMask |= 1 << layerIndex;
            }
            property.intValue = newMask;
        }
    }
}