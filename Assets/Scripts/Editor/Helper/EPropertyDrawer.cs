using UnityEngine;
using System;
using Geometry;

namespace UnityEditor.Extensions
{
    [CustomPropertyDrawer(typeof(Damper))]
    public class DamperDrawer : PropertyDrawer
    {
        private const int kSize = 120;
        private const int kAxisPadding = 3;
        private const int kAxisWidth = 2;
        private const float kDeltaTime = .05f;
        private const float kEstimateSizeX = 600;
        private const float kButtonSize = 50f;
        private bool m_Visualize = false;
        private float AdditionalSize => (m_Visualize ? kSize : 0f);
        public override float GetPropertyHeight(SerializedProperty _property, GUIContent _label) => EditorGUI.GetPropertyHeight(_property, _label,true) + AdditionalSize;

        public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            if (_position.size.sqrMagnitude < 10f)
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
            Damper damper = new Damper();
            var fieldInfo = _property.GetFieldInfo(out var parentObject);
            UReflection.CopyFields(fieldInfo.GetValue(parentObject),damper);
            damper.Initialize(Vector3.zero);
            int totalSize = sizeX * sizeY;
            Color[] colors = new Color[sizeX*sizeY];
            colors.FillDefault(Color.black.SetAlpha(.5f));
            
            Action<int, int, Color> SetPixel = (_x, _y, _color) =>
            {
                var dst = (_x + _y * sizeX);
                if (dst < 0 || dst >= totalSize)
                    return;
                colors[dst] = _color;
            };
            Action<int, int,int,int, Color> DrawPixel = (_x, _y,_preX,_preY, _color) =>
            {
                var transparent = _color.SetAlpha(.5f);
                SetPixel(_x , _y , _color);
                SetPixel(_x + 1 , _y , transparent);
                SetPixel(_x - 1 , _y , transparent);
                SetPixel(_x , _y + 1 , transparent);
                SetPixel(_x , _y - 1 , transparent);
                int xStart = Mathf.Min(_x, _preX);
                int xEnd = Mathf.Max(_x, _preX);
                int yStart = Mathf.Min(_y, _preY)+1;
                int yEnd = Mathf.Max(_y, _preY)-1;
                for(int i = xStart;i < xEnd;i++)
                    for (int j = yStart; j < yEnd; j++)
                    {
                        SetPixel(i , j , _color);
                        SetPixel(i + 1 , j , transparent);
                        SetPixel(i - 1 , j , transparent);
                        SetPixel(i , j + 1 , transparent);
                        SetPixel(i , j - 1 , transparent);
                    }
            };
            
            float sizeAspect =  sizeX / kEstimateSizeX;
            float deltaTime = kDeltaTime / sizeAspect;
            int division1 =(int)( 10f/kDeltaTime * sizeAspect);
            int division2 = (int)( 20f/kDeltaTime * sizeAspect);
            int preX = 0,preY = 0;
            for (int i = 0; i < sizeX; i++)
            {
                Vector3 point = i>=division1? i>=division2?Vector3.one*.8f:Vector3.one*.2f:Vector3.one * .5f;
                var value = damper.Tick(deltaTime,point);
                int x = i;
                int y = (int) (value.x * sizeY);
                DrawPixel(x,y,preX,preY,Color.cyan);
                preX = x;
                preY = y;
                SetPixel(x , (int)(point.x*sizeY) , Color.red);
            }
            
            for (int i = 0; i < 60; i++)
            {
                var xDelta =(int) (i / deltaTime);
                if (xDelta > sizeX)
                    break;
                
                for(int j=0;j<sizeY;j++)
                    SetPixel(xDelta , j , Color.green.SetAlpha(.3f));
            }
            
            previewTexture.SetPixels(colors);
            previewTexture.Apply();
            
            EditorGUI.DrawTextureTransparent(textureField,previewTexture);
            
            GameObject.DestroyImmediate(previewTexture);
            
            Rect axisX = imageField.Move(kAxisPadding,imageField.size.y-kAxisPadding).Resize(imageField.size.x-kAxisPadding*2,kAxisWidth);
            EditorGUI.DrawRect(axisX,Color.green);
            Rect axisY = imageField.Move(kAxisPadding,kAxisPadding).Resize(kAxisWidth,imageField.size.y-kAxisPadding*2);
            EditorGUI.DrawRect(axisY,Color.blue);
        }
    }

    [CustomPropertyDrawer(typeof(ColorPalette))]
    public class ColorPaletteDrawer : PropertyDrawer
    {
        private const int kSize = 15;
        private const int kSpacing = 2;
        private const int kAxisPadding = 2;
        private const int kXCollapse = 8;
        private const float kButtonSize = 50f;
        
        private int kPreset = -1;
        private static ColorPalette[] kPalettePresets = new[]
        {
            new ColorPalette() {a = new Color(.5f, .5f, .5f), b = new Color(.5f, .5f, .5f), c = new Color(1f, 1f, 1f), d = new Color(0, 0.33f, 0.67f)},   
            new ColorPalette() {a = new Color(.5f, .5f, .5f), b = new Color(.5f, .5f, .5f), c = new Color(1f, 1f, 1f), d = new Color(0, 0.1f, 0.2f)},   
            new ColorPalette() {a = new Color(.5f, .5f, .5f), b = new Color(.5f, .5f, .5f), c = new Color(1f, 1f, 1f), d = new Color(0.3f, 0.2f, 0.2f)},   
            new ColorPalette() {a = new Color(.5f, .5f, .5f), b = new Color(.5f, .5f, .5f), c = new Color(1f, 1f, .5f), d = new Color(0.8f, 0.9f, 0.3f)},  
            new ColorPalette() {a = new Color(.5f, .5f, .5f), b = new Color(.5f, .5f, .5f), c = new Color(1f, .7f, .4f), d = new Color(0, 0.15f, 0.2f)},   
            new ColorPalette() {a = new Color(.5f, .5f, .5f), b = new Color(.5f, .5f, .5f), c = new Color(2f, 1f, 1f), d = new Color(0.5f, 0.2f, 0.25f)},   
            new ColorPalette() { a = new Color(.8f, .5f, .4f), b = new Color(.2f, .4f, .2f), c = new Color(2f, 1f, 1f), d = new Color(0, 0.25f, 0.25f)},
        };
        
        public override float GetPropertyHeight(SerializedProperty _property, GUIContent _label) => EditorGUI.GetPropertyHeight(_property, _label,true) + kSize + kSpacing;
        public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            if (_position.size.sqrMagnitude < 10f)   //Some times the value gets too small
                return;
            
            Rect propertyField = _position.ResizeY(_position.size.y - kSize - kSpacing);
            var fieldInfo = _property.GetFieldInfo(out var parentObject);

            if (GUI.Button(_position.Move(_position.size.x - kButtonSize, 0f).ResizeY(20).ResizeX(kButtonSize), "Preset"))
            {
                kPreset++;
                fieldInfo.SetValue(parentObject,kPalettePresets[kPreset%kPalettePresets.Length]);
            }
            
            EditorGUI.PropertyField(propertyField, _property, _label, true);

            Rect imageField = _position.MoveY(_position.size.y - kSize).ResizeY(kSize).Collapse(new Vector2(kXCollapse * 2,0f));
            EditorGUI.DrawRect(imageField,Color.black);

            Rect textureField = imageField.Collapse(new Vector2(kAxisPadding*2,kAxisPadding*2));
            int sizeX = (int) textureField.width*4; 
            int sizeY = (int) textureField.height*2;

            Texture2D previewTexture = new Texture2D(sizeX,sizeY,TextureFormat.ARGB32,false,true);
            
            ColorPalette palette = (ColorPalette)fieldInfo.GetValue(parentObject);
            Color[] colors = new Color[sizeX*sizeY];
            for (int i = 0; i < sizeX; i++)
            {
                var value = palette.Evaluate((float)i/sizeX);
                for(int j=0;j<sizeY;j++)
                    colors[i + j * sizeX] = value;
            }
            
            previewTexture.SetPixels(colors);
            previewTexture.Apply();
            
            EditorGUI.DrawTextureTransparent(textureField,previewTexture);
            
            GameObject.DestroyImmediate(previewTexture);
        }
    }
    
    #region Transformations (Deprecated)
    // [CustomPropertyDrawer(typeof(GLine))]
    // public class GLinePropertyDrawer : TransformHandlesDrawer
    // {
    //     protected override string PositionPropertyName => nameof(GLine.origin);
    //     protected override string DirectionPropertyName => nameof(GLine.direction);
    // }
    // [CustomPropertyDrawer(typeof(GRay))]
    // public class GRayPropertyDrawer : TransformHandlesDrawer
    // {
    //     protected override string PositionPropertyName => nameof(GRay.origin);
    //     protected override string DirectionPropertyName => nameof(GRay.direction);
    // }
    // [CustomPropertyDrawer(typeof(GCone))]
    // public class GConePropertyDrawer : TransformHandlesDrawer
    // {
    //     protected override string PositionPropertyName => nameof(GCone.origin);
    //     protected override string DirectionPropertyName => nameof(GCone.normal);
    // }
    // [CustomPropertyDrawer(typeof(GHeightCone))]
    // public class GHeightConePropertyDrawer : TransformHandlesDrawer
    // {
    //     protected override string PositionPropertyName => nameof(GHeightCone.origin);
    //     protected override string DirectionPropertyName => nameof(GHeightCone.normal);
    // }
    // public class TransformHandlesDrawer : PropertyDrawer
    // {
    //     protected virtual string PositionPropertyName => throw new Exception("Override This Please");
    //     protected virtual string DirectionPropertyName => throw new Exception("Override This Please");
    //     SerializedProperty m_PositionProperty;
    //     SerializedProperty m_DirectionProperty;
    //     string m_Name, m_ToolTip;
    //     public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    //     {
    //         m_Name = label.text;
    //         m_ToolTip = label.tooltip;
    //         m_PositionProperty = property.FindPropertyRelative(PositionPropertyName);
    //         m_DirectionProperty = property.FindPropertyRelative(DirectionPropertyName);
    //         return EditorGUI.GetPropertyHeight(property, label, true) + (property.isExpanded ? 20f : 0f);
    //     }
    //     public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    //     {
    //         float width = position.size.x;
    //         float propertyHeight = EditorGUI.GetPropertyHeight(property);
    //         HorizontalScope.Begin(position.x, position.y, propertyHeight);
    //         EditorGUI.PropertyField(HorizontalScope.NextRect(0f, width), property, new GUIContent(m_Name, m_ToolTip), true);
    //         if (!property.isExpanded)
    //             return;
    //         HorizontalScope.NextLine(2f, 18f);
    //         HorizontalScope.NextRect(0f, width * 5f / 6f);
    //         if (GUI.Button(HorizontalScope.NextRect(0f, width / 6f), "Edit"))
    //             GUITransformHandles.Begin(m_PositionProperty, m_DirectionProperty);
    //     }
    // }
    #endregion
}

