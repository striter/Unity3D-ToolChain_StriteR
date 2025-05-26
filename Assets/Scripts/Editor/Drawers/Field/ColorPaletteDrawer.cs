﻿using UnityEngine;

namespace UnityEditor.Extensions
{
    [CustomPropertyDrawer(typeof(ColorPalette))]
    public class ColorPaletteDrawer : PropertyDrawer
    {
        private const int kSize = 15;
        private const int kSpacing = 2;
        private const int kAxisPadding = 2;
        private const int kXCollapse = 8;
        private const float kButtonSize = 50f;
        
        private int kPreset = -1;
        private static ColorPalette[] kPalettePresets = {
            new () {a = new Color(.5f, .5f, .5f), b = new Color(.5f, .5f, .5f), c = new Color(1f, 1f, 1f), d = new Color(0, 0.33f, 0.67f)},   
            new () {a = new Color(.5f, .5f, .5f), b = new Color(.5f, .5f, .5f), c = new Color(1f, 1f, 1f), d = new Color(0, 0.1f, 0.2f)},   
            new () {a = new Color(.5f, .5f, .5f), b = new Color(.5f, .5f, .5f), c = new Color(1f, 1f, 1f), d = new Color(0.3f, 0.2f, 0.2f)},   
            new () {a = new Color(.5f, .5f, .5f), b = new Color(.5f, .5f, .5f), c = new Color(1f, 1f, .5f), d = new Color(0.8f, 0.9f, 0.3f)},  
            new () {a = new Color(.5f, .5f, .5f), b = new Color(.5f, .5f, .5f), c = new Color(1f, .7f, .4f), d = new Color(0, 0.15f, 0.2f)},   
            new () {a = new Color(.5f, .5f, .5f), b = new Color(.5f, .5f, .5f), c = new Color(2f, 1f, 1f), d = new Color(0.5f, 0.2f, 0.25f)},   
            new () { a = new Color(.8f, .5f, .4f), b = new Color(.2f, .4f, .2f), c = new Color(2f, 1f, 1f), d = new Color(0, 0.25f, 0.25f)},
        };
        
        public override float GetPropertyHeight(SerializedProperty _property, GUIContent _label) => EditorGUI.GetPropertyHeight(_property, _label,true) + kSize + kSpacing;
        public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            if (_position.size.x < kSize)
                return;
            
            if (_position.size.sqrMagnitude < 10f)   //Some times the value gets too small
                return;
            
            var propertyField = _position.ResizeY(_position.size.y - kSize - kSpacing);
            var fieldInfo = _property.GetFieldInfo(out var parentObject);

            if (GUI.Button(_position.Move(_position.size.x - kButtonSize, 0f).ResizeY(20).ResizeX(kButtonSize), "Preset"))
            {
                kPreset++;
                fieldInfo.SetValue(parentObject,kPalettePresets[kPreset%kPalettePresets.Length]);
                Undo.RegisterCompleteObjectUndo(_property.serializedObject.targetObject, "Color Palette Preset");
            }
            
            EditorGUI.PropertyField(propertyField, _property, _label, true);

            var imageField = _position.MoveY(_position.size.y - kSize).ResizeY(kSize).Collapse(new Vector2(kXCollapse * 2,0f));
            EditorGUI.DrawRect(imageField,Color.black);

            var textureField = imageField.Collapse(new Vector2(kAxisPadding*2,kAxisPadding*2));
            var sizeX = (int) textureField.width*4; 
            var sizeY = (int) textureField.height*2;

            var previewTexture = new Texture2D(sizeX,sizeY,TextureFormat.ARGB32,false,true);
            
            var palette = (ColorPalette)fieldInfo.GetValue(parentObject);
            var colors = new Color[sizeX*sizeY];
            for (var i = 0; i < sizeX; i++)
            {
                var value = palette.Evaluate((float)i/sizeX);
                for(var j=0;j<sizeY;j++)
                    colors[i + j * sizeX] = value;
            }
            
            previewTexture.SetPixels(colors);
            previewTexture.Apply();
            
            EditorGUI.DrawTextureTransparent(textureField,previewTexture);
            GameObject.DestroyImmediate(previewTexture);
        }
    }
}