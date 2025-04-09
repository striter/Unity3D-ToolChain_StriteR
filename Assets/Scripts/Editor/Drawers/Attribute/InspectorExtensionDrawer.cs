using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityEditor.Extensions
{
    [CustomPropertyDrawer(typeof(InspectorExtensionAttribute))]
    public class InspectorExtensionDrawer : AAttributePropertyDrawer<InspectorExtensionAttribute>
    {
        private object target;
        private List<ButtonAttributeData> inspectorMethods;
        private const int kPadding = 2;
        private const int kMethodButtonHeight = 20;
        private const int kTitleHeight = 18;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            target = property.GetFieldValue();
            inspectorMethods ??= UInspectorExtension.GetInspectorMethods(target);
            var height = EditorGUI.GetPropertyHeight(property, label, true);

            if (height < 20)
                return height;
            
            foreach (var method in inspectorMethods)
            {
                if(!method.attribute.IsElementVisible(target))
                    continue;
                
                height += kPadding;
                if (method.parameters.Length == 0)
                {
                    height += kMethodButtonHeight;
                    continue;
                }

                height += kTitleHeight;
                foreach (var parameter in method.parameters)
                    height += kPadding + UEGUIExtension.FieldHeight(parameter.value, parameter.type,parameter.name);
                height += kMethodButtonHeight;
            }
            
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var propertyHeight = EditorGUI.GetPropertyHeight(property, label);
            position = position.ResizeY(propertyHeight);
            EditorGUI.PropertyField(position, property, label, true);
            if (propertyHeight < 20)
                return;

            position = position.MoveX(25).ResizeX(position.size.x - 25);
            foreach (var data in inspectorMethods)
            {
                if(!data.attribute.IsElementVisible(target))
                    continue;
                
                position = position.MoveY().ResizeY(kPadding);
                if (data.parameters.Length == 0)
                {
                    position = position.MoveY().ResizeY(kMethodButtonHeight);
                    if (GUI.Button(position, data.method.Name))
                        data.method.Invoke(target,data.parameters.Select(p=>p.value).ToArray());
                    continue;
                }
                
                position = position.MoveY().ResizeY(kTitleHeight);
                EditorGUI.LabelField(position,data.method.Name, EditorStyles.boldLabel);
                foreach (var parameter in data.parameters)
                {
                    position = position.MoveY().MoveY(kPadding).ResizeY(UEGUIExtension.FieldHeight(parameter.value,parameter.type,parameter.name));
                    parameter.value = UEGUIExtension.Field(position, parameter.value,parameter.type,parameter.name);
                }
                position = position.MoveY().MoveY(kPadding).ResizeY(kMethodButtonHeight);
                if (GUI.Button(position, "Execute"))
                    data.method.Invoke(target,data.parameters.Select(p=>p.value).ToArray());
            }
        }
    }
}