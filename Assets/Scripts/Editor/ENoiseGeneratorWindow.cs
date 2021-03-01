using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using TAlthogrim;
namespace TEditor
{
    public class ENoiseGeneratorWindow : EditorWindow
    {
        const int C_MinSizePower = 3;
        const int C_MaxSizePower = 10;
        int m_SizePower=5;
        float m_Scale = 1;
        Texture2D m_Texture;

        public static class EditorGUIHorizontalScope
        {
            static Vector2 m_StartPos;
            static Vector2 m_Offset;
            static float m_SizeY;
            public static void Begin(float _startX,float _startY,float _startSizeY)
            {
                m_SizeY = _startSizeY;
                m_StartPos = new Vector2(_startX,_startY);
                m_Offset = Vector2.zero;
            }
            public static Rect NextRect(float _spacingX,float _sizeX)
            {
                Vector2 originOffset = m_Offset;
                m_Offset.x += _sizeX + _spacingX;
                return new Rect(m_StartPos + originOffset, new Vector2(_sizeX, m_SizeY));
            }
            public static void NextLine(float _spacingY,float _sizeY)
            {
                m_Offset.y += m_SizeY + _spacingY;
                m_SizeY = _sizeY;
                m_Offset.x = 0;
            }
        }
        static GUIStyle m_TitleStyle => new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold };
        private void OnGUI()
        {
            EditorGUIHorizontalScope.Begin(5,5,20);

            EditorGUI.BeginChangeCheck();
            EditorGUI.LabelField(EditorGUIHorizontalScope.NextRect(0, 60),  "Size:", m_TitleStyle);
            m_SizePower = EditorGUI.IntSlider(EditorGUIHorizontalScope.NextRect( 5, 120), m_SizePower, C_MinSizePower, C_MaxSizePower);
            int size = Mathf.RoundToInt(Mathf.Pow(2, m_SizePower));
            EditorGUI.LabelField(EditorGUIHorizontalScope.NextRect(5,40), size.ToString());
            EditorGUIHorizontalScope.NextLine(2,20);
            EditorGUI.LabelField(EditorGUIHorizontalScope.NextRect(0, 60), "Scale:", m_TitleStyle);
            m_Scale = EditorGUI.Slider(EditorGUIHorizontalScope.NextRect(5, 120), m_Scale,1f,10f);
            if(!m_Texture||EditorGUI.EndChangeCheck())
            {
                double sizeD = size;
                Color[] colors = new Color[size * size];
                for (int i = 0; i < size; i++)
                    for (int j = 0; j < size; j++)
                        colors[i * size + j] = new Color((float)TPerlinNoise.Perlin(.5d+j / sizeD*255* m_Scale,.5d+i / sizeD*255* m_Scale, 0),0,0,0);
                m_Texture = new Texture2D(size, size, TextureFormat.R8, true);
                m_Texture.SetPixels(colors);
                m_Texture.Apply();
            }
            EditorGUIHorizontalScope.NextLine(2,256);
            EditorGUI.DrawPreviewTexture(EditorGUIHorizontalScope.NextRect(0,256), m_Texture);
            EditorGUIHorizontalScope.NextLine(2, 20);
            if(GUI.Button( EditorGUIHorizontalScope.NextRect(0,80),"Export"))
            {
                if(TEditor.SaveFilePath(out string filePath,"png", "CustomNoise"))
                    TEditor.CreateOrReplaceFile(filePath,m_Texture.EncodeToPNG());
            }
        }

    }

}