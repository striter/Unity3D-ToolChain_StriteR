using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityEditor.Extensions
{
    [CustomPropertyDrawer(typeof(DefaultAssetAttribute))]
    public class AssetAttributeDrawer : AAttributePropertyDrawer<DefaultAssetAttribute>
    {
        const float kButtonHeight = 20f;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = base.GetPropertyHeight(property, label);
            if (property.objectReferenceValue == null)
                return height + kButtonHeight;
            
            return height;
        }

        static Object LoadDefaultAsset(Type _type,string _path) => _type switch {
                not null when _type == typeof(Shader) => Shader.Find(_path),
                not null when _type.IsSubclassOf(typeof(Object)) => AssetDatabase.LoadAssetAtPath<Object>(_path),
                _ => null
            };

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!OnGUIAttributePropertyCheck(position, property, SerializedPropertyType.ObjectReference))
                return;

            position = position.ResizeY(base.GetPropertyHeight(property,label));
            base.OnGUI(position, property, label);

            if (property.objectReferenceValue != null) return;
            var buttonSize = position.size.x / 5f;
            position = position.MoveY().ResizeY(kButtonHeight).ResizeX(buttonSize).MoveX(position.size.x - buttonSize);
            if (GUI.Button(position, "Load Default"))
            {
                var type = property.GetFieldInfo(out _).FieldType;
                var path = attribute.m_RelativePath;
                var defaultAsset = LoadDefaultAsset(type, path);
                Debug.Assert(defaultAsset != null, $"Failed to load default asset Type:{type} Path:{path}");
                property.objectReferenceValue = defaultAsset;                
            }

        }
    }
}