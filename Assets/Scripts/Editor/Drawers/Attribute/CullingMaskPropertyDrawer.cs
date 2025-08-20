using System.Collections.Generic;
using System.Linq.Extensions;
using UnityEngine;

namespace UnityEditor.Extensions
{
 
    [CustomPropertyDrawer(typeof(CullingMaskAttribute))]
    public class CullingMaskPropertyDrawer : AAttributePropertyDrawer<CullingMaskAttribute>
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!OnGUIAttributePropertyCheck(position, property, SerializedPropertyType.Integer))
                return;

            CullingMaskDrawer.DrawGUI(position, property, label);
        }
    }
}