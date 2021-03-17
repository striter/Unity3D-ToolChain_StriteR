using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UAlthogrim;
namespace TEditor
{
    using static UEGUI;
    public class EWNoiseGenerator : EditorWindow
    {
        const int C_MinSizePower = 3;
        const int C_MaxSizePower = 10;
        int m_SizePower=5;
        float m_Scale = 1;
        Texture2D m_Texture;
        private void OnGUI()
        {
            HorizontalScope.Begin(5,5,20);
            EditorGUI.BeginChangeCheck();
            EditorGUI.LabelField(HorizontalScope.NextRect(0, 60),  "Size:", UEGUIStyle_Window.m_TitleLabel);
            m_SizePower = EditorGUI.IntSlider(HorizontalScope.NextRect( 5, 120), m_SizePower, C_MinSizePower, C_MaxSizePower);
            int size = Mathf.RoundToInt(Mathf.Pow(2, m_SizePower));
            EditorGUI.LabelField(HorizontalScope.NextRect(5,40), size.ToString());
            HorizontalScope.NextLine(2,20);
            EditorGUI.LabelField(HorizontalScope.NextRect(0, 60), "Scale:", UEGUIStyle_Window.m_TitleLabel);
            m_Scale = EditorGUI.Slider(HorizontalScope.NextRect(5, 120), m_Scale,1f,10f);
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
            HorizontalScope.NextLine(2,256);
            EditorGUI.DrawPreviewTexture(HorizontalScope.NextRect(0,256), m_Texture);
            HorizontalScope.NextLine(2, 20);
            if(GUI.Button( HorizontalScope.NextRect(0,80),"Export"))
            {
                if(UECommon.SaveFilePath(out string filePath,"png", "CustomNoise"))
                    UECommon.CreateOrReplaceFile(filePath,m_Texture.EncodeToPNG());
            }
        }

    }

}