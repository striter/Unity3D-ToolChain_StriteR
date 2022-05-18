using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TEditor
{
    public static class ETime
    {
        private static double m_Cur;
        public static float deltaTime
        {
            get
            {
                var last = m_Cur;
                m_Cur = EditorApplication.timeSinceStartup;
                return Mathf.Max(0, (float)(m_Cur-last));
            }
        }
    }
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

    public static class EHorizontalScope
    {
        static Vector2 m_StartPos;
        static Vector2 m_Offset;
        public static float m_CurrentY { get; private set; }
        public static Vector2 m_CurrentPos => m_StartPos + m_Offset;
        public static void Begin(float _startX, float _startY, float _startSizeY)
        {
            m_CurrentY = _startSizeY;
            m_StartPos = new Vector2(_startX, _startY);
            m_Offset = Vector2.zero;
        }
        public static Rect NextRect(float _spacingX, float _sizeX)
        {
            Vector2 originOffset = m_Offset;
            m_Offset.x += _sizeX + _spacingX;
            return new Rect(m_StartPos + originOffset, new Vector2(_sizeX, m_CurrentY));
        }
        public static void NextLine(float _spacingY, float _sizeY)
        {
            m_Offset.y += m_CurrentY + _spacingY;
            m_CurrentY = _sizeY;
            m_Offset.x = 0;
        }
    }
}