﻿using System;
using System.Linq;
using UnityEngine;

namespace UnityEditor.Extensions.EditorPath
{

    [AttributeUsage(AttributeTargets.Field)]
    public class EditorPathAttribute : PropertyAttribute
    {
        public static string Output(string _value) => UEPath.PathRegex(_value);
    }


    [CustomPropertyDrawer(typeof(EditorPathAttribute))]
    public class PathPropertyDrawer : AAttributePropertyDrawer<EditorPathAttribute>
    {
        private bool m_Foldout;
        private const float kOutputHeight = 20;
        private const float kAppendRegexHeigth = 20;
        private const float kPadding = 2;
        private int m_PopupIndex;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!m_Foldout)
                return base.GetPropertyHeight(property, label);
            
            return base.GetPropertyHeight(property, label) + kPadding + kAppendRegexHeigth + kPadding + kOutputHeight;
        }

        public override void OnGUI(Rect _position, SerializedProperty property, GUIContent label)
        {
            if (!OnGUIAttributePropertyCheck(_position, property, SerializedPropertyType.String))
                return;

            var kDepth = 20f;
            m_Foldout = EditorGUI.Foldout(_position.Resize(10,20), m_Foldout,"");
            GUILayout_HorizontalScope.Begin(_position);
            GUILayout_HorizontalScope.NewLine(0,20);
            EditorGUI.LabelField(GUILayout_HorizontalScope.NextRectNormalized(10,  .3f),property.displayName);

            var path = UEPath.PathRegex(property.stringValue);
            var availablePath = UEPath.IsAvailableProjectWindowDirectory(path);
            if(GUI.Button( availablePath? GUILayout_HorizontalScope.NextRectNormalized(kDepth,.55f): GUILayout_HorizontalScope.FinishLineRect(2), path))
                EditorUtility.DisplayDialog("Output",UEPath.PathRegex(property.stringValue),"Ok");
            if(availablePath && GUI.Button(GUILayout_HorizontalScope.FinishLineRect(kPadding), "Direct"))
                UEPath.SetCurrentProjectWindowDirectory(path);
            if(!m_Foldout)
                return;

            property.stringValue =EditorGUI.TextField(GUILayout_HorizontalScope.NewLine(kPadding,kOutputHeight,kDepth),GUIContent.none,property.stringValue);
            
            GUILayout_HorizontalScope.NewLine(kPadding, kAppendRegexHeigth);
            EditorGUI.BeginChangeCheck();
            var popupIndex = EditorGUI.Popup(GUILayout_HorizontalScope.NextRectNormalized(kDepth,.5f),
                UEPath.kReplacementRegex.Count, UEPath.kReplacementRegex.Select(p=>$"{p.Key} {p.Value()}".Replace('/','\\')).Append("Append Regex").ToArray());
            if (EditorGUI.EndChangeCheck())
                property.stringValue += UEPath.kReplacementRegex.ElementAt(popupIndex).Key;
            
            EditorGUI.BeginChangeCheck();
            popupIndex = EditorGUI.Popup(GUILayout_HorizontalScope.FinishLineRect(kPadding),UEPath.kActivePath.Count, UEPath.kActivePath.Select(p=>$"{p.Key} {p.Value()}".Replace('/','\\')).Append("Append Constant").ToArray());
            if (EditorGUI.EndChangeCheck())
                property.stringValue += UEPath.kActivePath.ElementAt(popupIndex).Value();
        }
    }
}