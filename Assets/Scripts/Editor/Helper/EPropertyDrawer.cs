using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;
using Geometry.Voxel;
using OSwizzling;

namespace UnityEditor.Extensions
{
    [CustomPropertyDrawer(typeof(Damper))]
    public class DamperDrawer : PropertyDrawer
    {
        private const int kSize = 120;
        private const int kAxisPadding = 5;
        private const int kAxisWidth = 2;
        private const float kDeltaTime = .05f;
 
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
            int sizeX = (int) textureField.width; int sizeY = (int) textureField.height;
            Texture2D previewTexture = new Texture2D(sizeX,sizeY,TextureFormat.ARGB32,false,true);
            Damper damper = new Damper();
            var fieldInfo = property.GetFieldInfo(out var parentObject);
            UReflection.CopyFields(fieldInfo.GetValue(parentObject),damper);
            damper.Begin(Vector3.zero);
            for (int i = 0; i < sizeX; i++)
            {
                Vector3 point = i>=100? i>=200?Vector3.one * .5f:Vector3.one*.2f:Vector3.one*.8f;
                var value = damper.Tick(kDeltaTime,point);
                previewTexture.SetPixel(i,(int)(value.x*sizeY),Color.cyan);
                previewTexture.SetPixel(i + 1,(int)(value.x*sizeY),Color.cyan);
                previewTexture.SetPixel(i - 1,(int)(value.x*sizeY),Color.cyan);
                previewTexture.SetPixel(i,(int)(value.x*sizeY) - 1,Color.cyan);
                previewTexture.SetPixel(i,(int)(value.x*sizeY) + 1,Color.cyan);
                previewTexture.SetPixel(i,(int)(point.x*sizeY),Color.red);
            }

            previewTexture.Apply();
            
            EditorGUI.DrawPreviewTexture(textureField,previewTexture);
            
            GameObject.DestroyImmediate(previewTexture);
            
            Rect axisX = imageField.Move(kAxisPadding,imageField.size.y-kAxisPadding).Resize(imageField.size.x-kAxisPadding*2,kAxisWidth);
            EditorGUI.DrawRect(axisX,Color.green);
            Rect axisY = imageField.Move(kAxisPadding,kAxisPadding).Resize(kAxisWidth,imageField.size.y-kAxisPadding*2);
            EditorGUI.DrawRect(axisY,Color.blue);

            for (int i = 0; i < 60; i++)
            {
                var xDelta = i / kDeltaTime;
                if (xDelta > sizeX)
                    break;
                
                var xAxis1 = axisX.MoveX(xDelta).Resize(2f,-4f);
                EditorGUI.DrawRect(xAxis1,Color.blue.SetAlpha(.3f));
            }
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

