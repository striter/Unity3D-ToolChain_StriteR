using System;
using System.Linq;
using UnityEngine;

namespace UnityEditor.Extensions.EditorPath
{

    [AttributeUsage(AttributeTargets.Field)]
    public class EditorPathAttribute : PropertyAttribute{}


    [CustomPropertyDrawer(typeof(EditorPathAttribute))]
    public class PathPropertyDrawer : SubAttributePropertyDrawer<EditorPathAttribute>
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
            if (!OnGUIAttributePropertyCheck(_position, property, out EditorPathAttribute attribute, SerializedPropertyType.String))
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
            if(availablePath && GUI.Button(GUILayout_HorizontalScope.FinishLineRect(kDepth), "Direct"))
                UEPath.SetCurrentProjectWindowDirectory(path);
            if(!m_Foldout)
                return;

            label.text = "Path Regex";
            EditorGUI.PropertyField(GUILayout_HorizontalScope.NewLine(kPadding,kOutputHeight,kDepth),property,label,true);
            
            GUILayout_HorizontalScope.NewLine(kPadding, kAppendRegexHeigth);
            EditorGUI.BeginChangeCheck();
            var popupIndex = EditorGUI.Popup(GUILayout_HorizontalScope.NextRectNormalized(kDepth,.5f),"Regex:",UEPath.kReplacementRegex.Count, UEPath.kReplacementRegex.Select(p=>$"{p.Key} {p.Value()}".Replace('/','\\')).Append("Select Regex To Append").ToArray());
            if (EditorGUI.EndChangeCheck())
                property.stringValue += UEPath.kReplacementRegex.ElementAt(popupIndex).Key;
            
            EditorGUI.BeginChangeCheck();
            popupIndex = EditorGUI.Popup(GUILayout_HorizontalScope.FinishLineRect(kPadding),"Constant:",UEPath.kActivePath.Count, UEPath.kActivePath.Select(p=>$"{p.Key} {p.Value()}".Replace('/','\\')).Append("Select Regex To Append").ToArray());
            if (EditorGUI.EndChangeCheck())
                property.stringValue += UEPath.kActivePath.ElementAt(popupIndex).Value();
        }
    }
}