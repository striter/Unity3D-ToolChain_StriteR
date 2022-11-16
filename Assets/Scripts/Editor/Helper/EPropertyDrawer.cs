using UnityEngine;
using System;
using Geometry;

namespace UnityEditor.Extensions
{
    [CustomPropertyDrawer(typeof(Damper))]
    public class DamperDrawer : PropertyDrawer
    {
        private const int kSize = 120;
        private const int kAxisPadding = 5;
        private const int kAxisWidth = 2;
        private const float kDeltaTime = .05f;
        private const float kEstimateSizeX = 600;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label,true) + kSize;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect propertyField = position.ResizeY(position.size.y-kSize);
            EditorGUI.PropertyField(propertyField, property, label, true);
            
            Rect imageField = position.MoveY(position.size.y - kSize).ResizeY(kSize);
            EditorGUI.DrawRect(imageField,Color.grey);

            Rect textureField = imageField.Collapse(new Vector2(kAxisPadding*2,kAxisPadding*2));
            int sizeX = (int) textureField.width*4; int sizeY = (int) textureField.height*2;
            Texture2D previewTexture = new Texture2D(sizeX,sizeY,TextureFormat.ARGB32,false,true);
            Damper damper = new Damper();
            var fieldInfo = property.GetFieldInfo(out var parentObject);
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

    #region Transformations
    [CustomPropertyDrawer(typeof(GLine))]
    public class GLinePropertyDrawer : TransformHandlesDrawer
    {
        protected override string PositionPropertyName => nameof(GLine.origin);
        protected override string DirectionPropertyName => nameof(GLine.direction);
    }
    [CustomPropertyDrawer(typeof(GRay))]
    public class GRayPropertyDrawer : TransformHandlesDrawer
    {
        protected override string PositionPropertyName => nameof(GRay.origin);
        protected override string DirectionPropertyName => nameof(GRay.direction);
    }
    [CustomPropertyDrawer(typeof(GCone))]
    public class GConePropertyDrawer : TransformHandlesDrawer
    {
        protected override string PositionPropertyName => nameof(GCone.origin);
        protected override string DirectionPropertyName => nameof(GCone.normal);
    }
    [CustomPropertyDrawer(typeof(GHeightCone))]
    public class GHeightConePropertyDrawer : TransformHandlesDrawer
    {
        protected override string PositionPropertyName => nameof(GHeightCone.origin);
        protected override string DirectionPropertyName => nameof(GHeightCone.normal);
    }
    public class TransformHandlesDrawer : PropertyDrawer
    {
        protected virtual string PositionPropertyName => throw new Exception("Override This Please");
        protected virtual string DirectionPropertyName => throw new Exception("Override This Please");
        SerializedProperty m_PositionProperty;
        SerializedProperty m_DirecitonProperty;
        string m_Name, m_ToolTip;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            m_Name = label.text;
            m_ToolTip = label.tooltip;
            m_PositionProperty = property.FindPropertyRelative(PositionPropertyName);
            m_DirecitonProperty = property.FindPropertyRelative(DirectionPropertyName);
            return EditorGUI.GetPropertyHeight(property, label, true) + (property.isExpanded ? 20f : 0f);
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float width = position.size.x;
            float propertyHeight = EditorGUI.GetPropertyHeight(property);
            HorizontalScope.Begin(position.x, position.y, propertyHeight);
            EditorGUI.PropertyField(HorizontalScope.NextRect(0f, width), property, new GUIContent(m_Name, m_ToolTip), true);
            if (!property.isExpanded)
                return;
            HorizontalScope.NextLine(2f, 18f);
            HorizontalScope.NextRect(0f, width * 5f / 6f);
            if (GUI.Button(HorizontalScope.NextRect(0f, width / 6f), "Edit"))
                GUITransformHandles.Begin(m_PositionProperty, m_DirecitonProperty);
        }
    }

    #endregion
}

