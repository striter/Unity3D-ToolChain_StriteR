using Unity.Mathematics;
using UnityEngine;

namespace UnityEditor.Extensions
{
    public abstract class AFunctionDrawer : PropertyDrawer
    {
        private const int kSize = 120;
        private const int kAxisPadding = 3;
        private const float kButtonSize = 50f;
        private bool m_Visualize = false;
        private float AdditionalSize => m_Visualize ? kSize : 0f;
        public sealed override float GetPropertyHeight(SerializedProperty _property, GUIContent _label) => EditorGUI.GetPropertyHeight(_property, _label,true) + (_property.isExpanded ? AdditionalSize : 0);
        protected abstract void OnFunctionDraw(SerializedProperty _property, FTextureDrawer _helper);
        public sealed override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            if (_position.size.x < 10f)
                return;
            
            var propertyField = _position.ResizeY(_position.size.y- AdditionalSize);

            if (_property.isExpanded)
            {
                if (GUI.Button(_position.Move(_position.size.x - kButtonSize, 0f).ResizeY(20).ResizeX(kButtonSize), m_Visualize ? "Hide" : "Visual"))
                    m_Visualize = !m_Visualize;
            }
            else
            {
                m_Visualize = false;
            }

            EditorGUI.PropertyField(propertyField, _property, _label, true);
            if (!m_Visualize || !_property.isExpanded)
                return;
            
            var imageField = _position.MoveY(_position.size.y - kSize).ResizeY(kSize);
            EditorGUI.DrawRect(imageField,Color.grey);

            var textureField = imageField.Collapse(new Vector2(kAxisPadding*2,kAxisPadding*2),Vector2.one * .5f);
            var sizeX = (int) textureField.width*4; 
            var sizeY = (int) textureField.height*2;

            var previewTexture = new Texture2D(sizeX,sizeY,TextureFormat.ARGB32,false,true);

            var colorHelper = new FTextureDrawer(sizeX, sizeY, Color.black.SetA(.5f));
            OnFunctionDraw(_property, colorHelper);
            
            previewTexture.SetPixels(colorHelper.colors);
            previewTexture.Apply();
            
            EditorGUI.DrawTextureTransparent(textureField,previewTexture);
            GameObject.DestroyImmediate(previewTexture);
        }
    }
}