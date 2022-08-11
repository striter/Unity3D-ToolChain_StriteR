using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityEditor.Extensions
{
    public static class UEGUIStyle_Window
    {
        public static GUIStyle m_TitleLabel => new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold };
        public static GUIStyle m_ErrorLabel => new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 14, fontStyle = FontStyle.BoldAndItalic, richText = true };
    }
    public static class UEGUIStyle_SceneView
    {
        public static GUIStyle m_NormalLabel => new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter,fontSize=14, fontStyle = FontStyle.Normal};
        public static GUIStyle m_TitleLabel => new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter,fontSize=20, fontStyle = FontStyle.Bold };
        public static GUIStyle m_ErrorLabel => new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 24, fontStyle = FontStyle.BoldAndItalic, richText = true };
    }
}