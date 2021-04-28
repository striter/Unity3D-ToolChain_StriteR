using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace TEditor
{
    public class EWTextureEditor : EditorWindow
    {
        Texture2D m_SrcTexture;
        Texture2D m_ModifyTexture;
        Texture2D m_DisplayTexture;

        bool m_R, m_B, m_G, m_A;
        enum_ColorVisualize m_ColorModify = enum_ColorVisualize.None;
        private void OnDisable()
        {
            m_R = true;
            m_G = true;
            m_B = true;
            m_A = true;
            m_SrcTexture = null;
        }
        private void OnGUI()
        {
            UEGUI.HorizontalScope.Begin(5,5,30);
            EditorGUI.LabelField(UEGUI.HorizontalScope.NextRect(0, 60), "Texture:", UEGUIStyle_Window.m_TitleLabel);
            EditorGUI.BeginChangeCheck();
            m_SrcTexture = (Texture2D)EditorGUI.ObjectField(UEGUI.HorizontalScope.NextRect(5,65), m_SrcTexture,typeof(Texture2D),false);

            if(m_SrcTexture==null||!m_SrcTexture.isReadable)
            {
                UEGUI.HorizontalScope.NextLine(2, 20);
                EditorGUI.LabelField(UEGUI.HorizontalScope.NextRect(0,240),"Select Readable Texture To Begin",UEGUIStyle_Window.m_TitleLabel);
                return;
            }
            if (EditorGUI.EndChangeCheck())
                UpdateTexture();

            EditorGUI.BeginChangeCheck();
            UEGUI.HorizontalScope.NextLine(2, 20);
            EditorGUI.LabelField(UEGUI.HorizontalScope.NextRect(0, 60),"Display:",UEGUIStyle_Window.m_TitleLabel);
            EditorGUI.LabelField(UEGUI.HorizontalScope.NextRect(5, 15), "R:");
            m_R = EditorGUI.Toggle(UEGUI.HorizontalScope.NextRect(2, 20), m_R);
            EditorGUI.LabelField(UEGUI.HorizontalScope.NextRect(5, 15), "G:");
            m_G = EditorGUI.Toggle(UEGUI.HorizontalScope.NextRect(2, 20), m_G);
            EditorGUI.LabelField(UEGUI.HorizontalScope.NextRect(5, 15), "B:");
            m_B = EditorGUI.Toggle(UEGUI.HorizontalScope.NextRect(2, 20), m_B);
            EditorGUI.LabelField(UEGUI.HorizontalScope.NextRect(5, 15), "A:");
            m_A = EditorGUI.Toggle(UEGUI.HorizontalScope.NextRect(2, 20), m_A);
            if(GUI.Button(UEGUI.HorizontalScope.NextRect(10,60),"All"))
            {
                m_R = true;
                m_G = true;
                m_B = true;
                m_A = true;
            }
            if(EditorGUI.EndChangeCheck())
                UpdateDisplayTexture();

            UEGUI.HorizontalScope.NextLine(2, 256);
            Rect textureRect = UEGUI.HorizontalScope.NextRect(0, 256);
            GUI.DrawTexture(textureRect, EditorGUIUtility.whiteTexture);
            GUI.DrawTexture(textureRect.Collapse(Vector2.one * 10f), m_DisplayTexture);

            UEGUI.HorizontalScope.NextLine(2, 20);
            EditorGUI.LabelField(UEGUI.HorizontalScope.NextRect(5, 60), "Modify:",UEGUIStyle_Window.m_TitleLabel);
            m_ColorModify = (enum_ColorVisualize)EditorGUI.EnumPopup(UEGUI.HorizontalScope.NextRect(5, 120), m_ColorModify);

            if(m_ColorModify!= enum_ColorVisualize.None)
            {
                UEGUI.HorizontalScope.NextLine(2, 20);
                if (GUI.Button(UEGUI.HorizontalScope.NextRect(10, 60), "Reverse"))
                {
                    DoColorModify(m_ModifyTexture, m_ColorModify, value => 1 - value);
                    UpdateDisplayTexture();
                }
                if (GUI.Button(UEGUI.HorizontalScope.NextRect(10, 60), "Fill"))
                {
                    DoColorModify(m_ModifyTexture, m_ColorModify, value => 1);
                    UpdateDisplayTexture();
                }
                if (GUI.Button(UEGUI.HorizontalScope.NextRect(10, 60), "Clear"))
                {
                    DoColorModify(m_ModifyTexture, m_ColorModify, value => 0);
                    UpdateDisplayTexture();
                }
            }
            UEGUI.HorizontalScope.NextLine(2, 20);
            EditorGUI.LabelField(UEGUI.HorizontalScope.NextRect(0, 60), "Finalize", UEGUIStyle_Window.m_TitleLabel);
            UEGUI.HorizontalScope.NextLine(2, 20);
            if (GUI.Button(UEGUI.HorizontalScope.NextRect(5, 80), "Reset"))
                UpdateTexture();
            if (GUI.Button(UEGUI.HorizontalScope.NextRect(20, 80), "Export"))
            {
                if (UEAsset.SaveFilePath(out string filePath, "png", m_SrcTexture.name+"_M"))
                    UEAsset.CreateOrReplaceFile(filePath, m_ModifyTexture.EncodeToPNG());
            }
        }


        void UpdateTexture()
        {
            m_ModifyTexture = new Texture2D(m_SrcTexture.width, m_SrcTexture.height, TextureFormat.RGBA32, true);
            m_ModifyTexture.SetPixels(m_SrcTexture.GetPixels());
            m_DisplayTexture = new Texture2D(m_SrcTexture.width, m_SrcTexture.height, TextureFormat.RGBA32, true);
            UpdateDisplayTexture();
        }
        void UpdateDisplayTexture()
        {
            Color[] colors = m_ModifyTexture.GetPixels();
            for (int i = 0; i < colors.Length; i++)
            {
                float alpha = m_A ? colors[i].a : 1;
                colors[i] = new Color(m_R ? colors[i].r* alpha : 0, m_G ? colors[i].g* alpha : 0, m_B ? colors[i].b* alpha : 0, 1);
            }
            m_DisplayTexture.SetPixels(colors);
            m_DisplayTexture.Apply();
        }

        static void DoColorModify(Texture2D _target, enum_ColorVisualize _color,Func<float,float> _OnEachValue)
        {
            Color[] colors = _target.GetPixels();
            for(int i=0;i<colors.Length;i++)
            {
                switch(_color)
                {
                    case enum_ColorVisualize.RGBA: colors[i] = new Color(_OnEachValue(colors[i].r), _OnEachValue(colors[i].g), _OnEachValue(colors[i].b), _OnEachValue(colors[i].a)); break;
                    case enum_ColorVisualize.RGB: colors[i] = new Color(_OnEachValue(colors[i].r), _OnEachValue(colors[i].g), _OnEachValue(colors[i].b), colors[i].a); break;
                    case enum_ColorVisualize.R: colors[i] = new Color(_OnEachValue(colors[i].r), colors[i].g, colors[i].b, colors[i].a); break;
                    case enum_ColorVisualize.G: colors[i] = new Color(colors[i].r, _OnEachValue(colors[i].g), colors[i].b, colors[i].a); break;
                    case enum_ColorVisualize.B: colors[i] = new Color(colors[i].r, colors[i].g, _OnEachValue(colors[i].b), colors[i].a); break;
                    case enum_ColorVisualize.A: colors[i] = new Color(colors[i].r, colors[i].g, colors[i].b, _OnEachValue(colors[i].a)); break;
                }
            }
            _target.SetPixels(colors);
            _target.Apply();
        }
    }
}