using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TEditor
{
    public static class UEGUI
    {
        public static class HorizontalScope
        {
            static Vector2 m_StartPos;
            static Vector2 m_Offset;
            static float m_SizeY;
            public static void Begin(float _startX, float _startY, float _startSizeY)
            {
                m_SizeY = _startSizeY;
                m_StartPos = new Vector2(_startX, _startY);
                m_Offset = Vector2.zero;
            }
            public static Rect NextRect(float _spacingX, float _sizeX)
            {
                Vector2 originOffset = m_Offset;
                m_Offset.x += _sizeX + _spacingX;
                return new Rect(m_StartPos + originOffset, new Vector2(_sizeX, m_SizeY));
            }
            public static void NextLine(float _spacingY, float _sizeY)
            {
                m_Offset.y += m_SizeY + _spacingY;
                m_SizeY = _sizeY;
                m_Offset.x = 0;
            }
        }
        public static bool EditorApplicationPlayingCheck()
        {
            if (Application.isPlaying)
            {
                GUILayout.TextArea("<Color=#FF0000>Editor Window Not Available In Play Mode! </Color>", UEGUIStyle_Window.m_ErrorLabel);
                return false;
            }
            return true;
        }

        public static class Layout
        {
            public static T[] ArrayField<T>(T[] _src, string _context, bool _allowSceneObjects) where T : UnityEngine.Object
            {
                GUILayout.BeginVertical();
                EditorGUILayout.LabelField(_context,UEGUIStyle_Window.m_TitleLabel);
                int length = Mathf.Clamp(EditorGUILayout.IntField("Length", _src.Length),1,128);
                if (length != _src.Length)
                    _src = _src.Resize(length);

                Type type = typeof(T);
                T[] modifiedField = _src.Copy();
                for (int i = 0; i < modifiedField.Length; i++)
                    modifiedField[i] = (T)EditorGUILayout.ObjectField(modifiedField[i], type, _allowSceneObjects);
                GUILayout.EndVertical();
                for (int i = 0; i < modifiedField.Length; i++)
                    if (modifiedField[i] != _src[i])
                        return modifiedField;

                return _src;
            }
        }
    }
}