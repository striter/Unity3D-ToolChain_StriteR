using UnityEngine;
using System;
using Runtime.Geometry;
using System.Linq.Extensions;
using Unity.Mathematics;

namespace UnityEditor.Extensions
{
    public class FunctionDrawerColors
    {
        public Color[] colors;
        public int sizeX, sizeY, totalSize;
        public FunctionDrawerColors(int _x,int _y,Color _initial)
        {
            sizeX = _x;
            sizeY = _y;
            totalSize = sizeX * sizeY;
            colors = new Color[sizeX*sizeY];
            colors.FillDefault(_initial);
        }

        public void Pixel(int _x,int _y,Color _color)
        {
            var dst = (_x + _y * sizeX);
            if (dst < 0 || dst >= totalSize)
                return;
            colors[dst] = _color;
        }

        private int preX = 0;
        private int preY = 0;
        public void PixelContinuousStart(int _x, int _y)
        {
            preX = _x;
            preY = _y;
        }
        
        public void PixelContinuous(int _x,int _y,Color _color)
        {
            var transparent = _color.SetA(.5f);
            Pixel(_x , _y , _color);
            Pixel(_x + 1 , _y , transparent);
            Pixel(_x - 1 , _y , transparent);
            Pixel(_x , _y + 1 , transparent);
            Pixel(_x , _y - 1 , transparent);
            int xStart = Mathf.Min(_x, preX);
            int xEnd = Mathf.Max(_x, preX);
            int yStart = Mathf.Min(_y, preY)+1;
            int yEnd = Mathf.Max(_y, preY)-1;
            for(int i = xStart;i < xEnd;i++)
            for (int j = yStart; j < yEnd; j++)
            {
                Pixel(i , j , _color);
                Pixel(i + 1 , j , transparent);
                Pixel(i - 1 , j , transparent);
                Pixel(i , j + 1 , transparent);
                Pixel(i , j - 1 , transparent);
            }

            preX = _x;
            preY = _y;
        }
        
        
        void plot1(int x,int y,Unity.Mathematics.int2 _centre,Color _color)
        {
            Pixel(_centre.x + x, _centre.y + y, _color);
        }
        void plot8(int x,int y,Unity.Mathematics.int2 _centre,Color _color){
            plot1(x,y,_centre,_color);plot1(y,x,_centre,_color);
            plot1(x,-y,_centre,_color);plot1(y,-x,_centre,_color);
            plot1(-x,-y,_centre,_color);plot1(-y,-x,_centre,_color);
            plot1(-x,y,_centre,_color);plot1(-y,x,_centre,_color);
        }

        public void Circle(Unity.Mathematics.int2 _centre,int _radius,Color _color)
        {
            int x = 0;
            int y = _radius;
            int d = 1 - _radius;
            while(x < y)
            {
                if(d < 0)
                {
                    d += 2 * x + 3;
                }
                else
                {
                    d += 2 * (x-y) + 5;
                    y--;
                }
                plot8(x,y, _centre,_color);
                x++;
            }
        }
    }
    
    public abstract class FunctionDrawer : PropertyDrawer
    {
        private const int kSize = 120;
        private const int kAxisPadding = 3;
        private const int kAxisWidth = 2;
        private const float kButtonSize = 50f;
        private bool m_Visualize = false;
        private float AdditionalSize => (m_Visualize ? kSize : 0f);
        public sealed override float GetPropertyHeight(SerializedProperty _property, GUIContent _label) => EditorGUI.GetPropertyHeight(_property, _label,true) + AdditionalSize;

        protected abstract void OnFunctionDraw(SerializedProperty _property, FunctionDrawerColors _helper);

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

            var colorHelper = new FunctionDrawerColors(sizeX, sizeY, Color.black.SetA(.5f));
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
    
    [CustomPropertyDrawer(typeof(IPolynomial),true)]
    public class PolynomialDrawer : FunctionDrawer
    {
        private const float kXRange = 20f;
        public override float2 GetOrigin() => kfloat2.one * .5f;

        protected override void OnFunctionDraw(SerializedProperty _property, FunctionDrawerColors _helper)
        {
            var info = _property.GetFieldInfo(out var parentObject);
            var polynomial = (IPolynomial) info.GetValue(parentObject);
            
            // _helper.DrawPixelContinuousStart(_helper.sizeX/2,_helper.sizeY/2);
            for (int i = 0; i < _helper.sizeX; i++)
            {
                var value = polynomial.Evaluate( ((float)i / _helper.sizeX -.5f)*kXRange) + .5f;
                int x = i;
                int y = (int) (value * _helper.sizeY);
                _helper.PixelContinuous(x,y,Color.cyan);
            }


            var rootCount = polynomial.GetRoots(out var roots);
            for (int i = 0; i < rootCount; i++)
            {
                var rootValue = roots[i]/kXRange;
                rootValue += .5f;
                _helper.Circle(new int2((int)(rootValue * _helper.sizeX),_helper.sizeY/2) ,10,Color.yellow);
            }
        }
    }
    
    [CustomPropertyDrawer(typeof(Damper))]
    public class DamperDrawer : FunctionDrawer
    {
        private const float kDeltaTime = .05f;
        private const float kEstimateSizeX = 600;
        protected override void OnFunctionDraw(SerializedProperty _property, FunctionDrawerColors _helper)
        {
            float sizeAspect =  _helper.sizeX / kEstimateSizeX;
            float deltaTime = kDeltaTime / sizeAspect;
            int division1 =(int)( 10f/kDeltaTime * sizeAspect);
            int division2 = (int)( 20f/kDeltaTime * sizeAspect);
            
            Damper damper = new Damper();
            var info = _property.GetFieldInfo(out var parentObject);
            UReflection.CopyFields(info.GetValue(parentObject),damper);
            damper.Initialize(Vector3.zero);
            
            for (int i = 0; i < _helper.sizeX; i++)
            {
                Vector3 point = i>=division1? i>=division2?Vector3.one*.8f:Vector3.one*.2f:Vector3.one * .5f;
                var value = damper.Tick(deltaTime,point);
                int x = i;
                int y = (int) (value.x * _helper.sizeY);
                _helper.PixelContinuous(x,y,Color.cyan);
                _helper.Pixel(x , (int)(point.x*_helper.sizeY) , Color.red);
            }
            
            for (int i = 0; i < 60; i++)
            {
                var xDelta =(int) (i / deltaTime);
                if (xDelta > _helper.sizeX)
                    break;
                
                for(int j=0;j<_helper.sizeY;j++)
                    _helper.Pixel(xDelta , j , Color.green.SetA(.3f));
            }
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

