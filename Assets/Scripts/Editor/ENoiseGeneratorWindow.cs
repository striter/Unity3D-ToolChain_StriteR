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
        static GUIStyle m_TitleStyle => new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold };
        private void OnGUI()
        {
            TEditorGUIScope_Horizontal.Begin(5,5,20);
            EditorGUI.BeginChangeCheck();
            EditorGUI.LabelField(TEditorGUIScope_Horizontal.NextRect(0, 60),  "Size:", m_TitleStyle);
            m_SizePower = EditorGUI.IntSlider(TEditorGUIScope_Horizontal.NextRect( 5, 120), m_SizePower, C_MinSizePower, C_MaxSizePower);
            int size = Mathf.RoundToInt(Mathf.Pow(2, m_SizePower));
            EditorGUI.LabelField(TEditorGUIScope_Horizontal.NextRect(5,40), size.ToString());
            TEditorGUIScope_Horizontal.NextLine(2,20);
            EditorGUI.LabelField(TEditorGUIScope_Horizontal.NextRect(0, 60), "Scale:", m_TitleStyle);
            m_Scale = EditorGUI.Slider(TEditorGUIScope_Horizontal.NextRect(5, 120), m_Scale,1f,10f);
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
            TEditorGUIScope_Horizontal.NextLine(2,256);
            EditorGUI.DrawPreviewTexture(TEditorGUIScope_Horizontal.NextRect(0,256), m_Texture);
            TEditorGUIScope_Horizontal.NextLine(2, 20);
            if(GUI.Button( TEditorGUIScope_Horizontal.NextRect(0,80),"Export"))
            {
                if(TEditor.SaveFilePath(out string filePath,"png", "CustomNoise"))
                    TEditor.CreateOrReplaceFile(filePath,m_Texture.EncodeToPNG());
            }
        }

    }

}