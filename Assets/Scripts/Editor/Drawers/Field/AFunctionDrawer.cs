using Unity.Mathematics;
using UnityEngine;

namespace UnityEditor.Extensions
{
    public abstract class AFunctionDrawer : PropertyDrawer
    {
        private const int kSize = 120;
        private const int kAxisPadding = 3;
        private const int kAxisWidth = 2;
        private const float kButtonSize = 50f;
        private bool m_Visualize = false;
        private float AdditionalSize => (m_Visualize ? kSize : 0f);
        public sealed override float GetPropertyHeight(SerializedProperty _property, GUIContent _label) => EditorGUI.GetPropertyHeight(_property, _label,true) + AdditionalSize;

        protected abstract void OnFunctionDraw(SerializedProperty _property, FTextureDrawer _helper);

        public virtual float2 GetOrigin() => new float2(0,1);
        
        public sealed override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            if (_position.size.x < 10f)
                return;
            
            Rect propertyField = _position.ResizeY(_position.size.y- AdditionalSize);
            if(GUI.Button(_position.Move(_position.size.x-kButtonSize,0f).ResizeY(20).ResizeX(kButtonSize),"Visual"))
                m_Visualize = !m_Visualize;
            EditorGUI.PropertyField(propertyField, _property, _label, true);
            
            if (!m_Visualize) 
                return;
            
            Rect imageField = _position.MoveY(_position.size.y - kSize).ResizeY(kSize);
            EditorGUI.DrawRect(imageField,Color.grey);

            Rect textureField = imageField.Collapse(new Vector2(kAxisPadding*2,kAxisPadding*2));
            int sizeX = (int) textureField.width*4; int sizeY = (int) textureField.height*2;

            Texture2D previewTexture = new Texture2D(sizeX,sizeY,TextureFormat.ARGB32,false,true);

            var colorHelper = new FTextureDrawer(sizeX, sizeY, Color.black.SetA(.5f));
            OnFunctionDraw(_property, colorHelper);
            previewTexture.SetPixels(colorHelper.colors);
            previewTexture.Apply();
            
            EditorGUI.DrawTextureTransparent(textureField,previewTexture);
            
            GameObject.DestroyImmediate(previewTexture);

            var origin = GetOrigin();
            
            Rect axisX = imageField.Move(kAxisPadding, kAxisPadding + (imageField.size.y-kAxisPadding*2)*origin.y).Resize(imageField.size.x-kAxisPadding*2,kAxisWidth);
            EditorGUI.DrawRect(axisX,Color.red.SetA(.7f));
            Rect axisY = imageField.Move(kAxisPadding + (imageField.size.x-kAxisPadding*2)*origin.x,kAxisPadding).Resize(kAxisWidth,imageField.size.y-kAxisPadding*2);
            EditorGUI.DrawRect(axisY,Color.green.SetA(.7f));
        }
    }
}