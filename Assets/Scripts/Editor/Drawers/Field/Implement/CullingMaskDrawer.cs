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

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var allLayers = UECommon.GetAllLayers(true);
            var layers = allLayers.Keys.Select(key => allLayers[key] == string.Empty ? null : allLayers[key]).ToList();
            m_Mask.intValue =  EditorGUI.MaskField(position, label.text, m_Mask.intValue, layers.ToArray());
        }
    }
}