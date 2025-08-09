using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace UnityEditor.Extensions
{
    [CustomPropertyDrawer(typeof(ColorPalette))]
    public class ColorPaletteDrawer : PropertyDrawer
    {
        private const int kSize = 15;
        private const int kSpacing = 2;
        private const int kAxisPadding = 2;
        private const int kXCollapse = 8;
        private const float kButtonSize = 200f;
        
        public override float GetPropertyHeight(SerializedProperty _property, GUIContent _label)
        {
            var size = EditorGUI.GetPropertyHeight(_property, _label, true);
            if(_property.IsExpanded())
                size += kSize + kSpacing;
            return size;
        }
        
        static Dictionary<string,ColorPalette> kPresets = new(){
            {"Preset",ColorPalette.kDefault},
            {"Rainbow",new ColorPalette() { baseColor = new (.5f, .5f, .5f), amplitude = new (.5f, .5f, .5f), frequency = new (1f, 1f, 1f), phaseShift = new (0, 0.33f, 0.67f)}},
            {"Preset1",new ColorPalette() { baseColor = new (.5f, .5f, .5f), amplitude = new (.5f, .5f, .5f), frequency = new (1f, 1f, 1f), phaseShift = new (0, 0.1f, 0.2f)}},   
            {"Preset2",new ColorPalette() { baseColor = new (.5f, .5f, .5f), amplitude = new (.5f, .5f, .5f), frequency = new (1f, 1f, 1f), phaseShift = new (0.3f, 0.2f, 0.2f)}},   
            {"Preset3",new ColorPalette() { baseColor = new (.5f, .5f, .5f), amplitude = new (.5f, .5f, .5f), frequency = new (1f, 1f, .5f), phaseShift = new (0.8f, 0.9f, 0.3f)}},  
            {"Preset4",new ColorPalette() { baseColor = new (.5f, .5f, .5f), amplitude = new (.5f, .5f, .5f), frequency = new (1f, .7f, .4f), phaseShift = new (0, 0.15f, 0.2f)}},   
            {"Preset5",new ColorPalette() { baseColor = new (.5f, .5f, .5f), amplitude = new (.5f, .5f, .5f), frequency = new (2f, 1f, 1f), phaseShift = new (0.5f, 0.2f, 0.25f)}},   
            {"Preset6",new ColorPalette() { baseColor = new (.8f, .5f, .4f), amplitude = new (.2f, .4f, .2f), frequency = new (2f, 1f, 1f), phaseShift = new (0, 0.25f, 0.25f)}},
            {"Preset7",new ColorPalette(){baseColor = new (.55f,.45f,.4f),amplitude = new (.35f,.35f,.35f),frequency = new (1.2f,1.1f,.9f),phaseShift = new (.25f,.35f,.55f)}},
            {"Forest",new ColorPalette(){baseColor = new (.4f,.5f,.35f),amplitude = new (.25f,.05f,.1f),frequency = new (.5f,.5f,.3f),phaseShift = new (.3f,.3f,.2f)}},
            {"Pastel",new ColorPalette(){baseColor = new (.8f,.85f,.9f),amplitude = new (.15f,.15f,.2f),frequency = new (.5f,.6f,.7f),phaseShift = new (.1f,.2f,.3f)}},
            {"Desert",new ColorPalette(){baseColor = new (.9f,.75f,.6f),amplitude = new (.2f,.15f,.1f),frequency = new (1f,1.1f,1.2f),phaseShift = new (.1f,.15f,.25f)}},
            {"Coral Reef Symphony",new ColorPalette(){baseColor = new (.95f,.52f,.35f),amplitude = new (.25f,.3f,.15f),frequency = new (.8f,1.7f,.6f),phaseShift = new (.1f,.4f,.8f)}},
            {"Mermaid Lagoon",new ColorPalette(){baseColor = new(.7f,.8f,.75f),amplitude = new(.15f,.25f,.2f),frequency = new(1.2f,.9f,1.5f),phaseShift = new(.1f,.3f,.5f)}},
            {"Pedal",new ColorPalette(){baseColor = new(.9f,.5f,.5f),amplitude = new(.7f,.3f,.25f),frequency = new float3(1f,.7f,.4f),phaseShift = new(0f,0f,.2f)}},
            
        };
        static readonly string[] kPresetsKeys = kPresets.Keys.ToArray();
        
        public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            if (_position.size.x < kSize)
                return;
            
            if (_position.size.sqrMagnitude < 10f)   //Some times the value gets too small
                return;
            
            var propertyField = _position.ResizeY(_position.size.y - kSize - kSpacing);
            var fieldInfo = _property.GetFieldInfo(out var parentObject);

            if (_property.IsExpanded())
            {
                EditorGUI.BeginChangeCheck();
                var preset = EditorGUI.Popup(_position.Move(_position.size.x - kButtonSize, 0f).ResizeY(20).ResizeX(kButtonSize), string.Empty,0, kPresetsKeys);
                if (EditorGUI.EndChangeCheck())
                    _property.boxedValue = kPresets.ElementAt(preset).Value;
            }

            _label.text = _property.displayName;
            EditorGUI.PropertyField(propertyField, _property, _label, true);
            if (!_property.IsExpanded())
                return;
            
            var imageField = _position.MoveY(_position.size.y - kSize).ResizeY(kSize).Collapse(new Vector2(kXCollapse * 2,0f));
            EditorGUI.DrawRect(imageField,Color.black);

            var textureField = imageField.Collapse(new Vector2(kAxisPadding*2,kAxisPadding*2),Vector2.one * .5f);
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