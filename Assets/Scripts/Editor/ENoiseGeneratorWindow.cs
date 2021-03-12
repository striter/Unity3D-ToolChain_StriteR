using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UAlthogrim;
namespace TEditor
{
    public class ENoiseGeneratorWindow : EditorWindow
    {
        const int C_MinSizePower = 3;
        const int C_MaxSizePower = 10;
        int m_SizePower=5;
        float m_Scale = 1;
        Texture2D m_Texture;
        private void OnGUI()
        {
            TEditor_GUIScope_Horizontal.Begin(5,5,20);
            EditorGUI.BeginChangeCheck();
            EditorGUI.LabelField(TEditor_GUIScope_Horizontal.NextRect(0, 60),  "Size:", TEditor_GUIStyle. m_TitleLabel);
            m_SizePower = EditorGUI.IntSlider(TEditor_GUIScope_Horizontal.NextRect( 5, 120), m_SizePower, C_MinSizePower, C_MaxSizePower);
            int size = Mathf.RoundToInt(Mathf.Pow(2, m_SizePower));
            EditorGUI.LabelField(TEditor_GUIScope_Horizontal.NextRect(5,40), size.ToString());
            TEditor_GUIScope_Horizontal.NextLine(2,20);
            EditorGUI.LabelField(TEditor_GUIScope_Horizontal.NextRect(0, 60), "Scale:", TEditor_GUIStyle.m_TitleLabel);
            m_Scale = EditorGUI.Slider(TEditor_GUIScope_Horizontal.NextRect(5, 120), m_Scale,1f,10f);
            if(!m_Texture||EditorGUI.EndChangeCheck())
            {
                double sizeD = size;
                Color[] colors = new Color[size * size];
                for (int i = 0; i < size; i++)
                    for (int j = 0; j < size; j++)
                        colors[i * size + j] = new Color((float)UNoise.Perlin(.5d+j / sizeD*255* m_Scale,.5d+i / sizeD*255* m_Scale, 0),0,0,0);
                m_Texture = new Texture2D(size, size, TextureFormat.R8, true);
                m_Texture.SetPixels(colors);
                m_Texture.Apply();
            }
            TEditor_GUIScope_Horizontal.NextLine(2,256);
            EditorGUI.DrawPreviewTexture(TEditor_GUIScope_Horizontal.NextRect(0,256), m_Texture);
            TEditor_GUIScope_Horizontal.NextLine(2, 20);
            if(GUI.Button( TEditor_GUIScope_Horizontal.NextRect(0,80),"Export"))
            {
                if(EUCommon.SaveFilePath(out string filePath,"png", "CustomNoise"))
                    EUCommon.CreateOrReplaceFile(filePath,m_Texture.EncodeToPNG());
            }
        }

    }

}