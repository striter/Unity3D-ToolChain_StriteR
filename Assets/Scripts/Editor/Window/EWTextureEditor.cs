using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace TEditor
{
    public class TextureEditor : EditorWindow
    {
        interface ITextureEditor
        {
            void Disable();
            bool OnGUIExportValid(ref Texture2D _targetTexture, Action DisplayNotify);
        }

        enum enum_TextureEditorMode
        {
            ChannelModifer,
            ChannelMixer,
        }
        enum enum_TextureExportType
        {
            PNG,
            JPG,
            TGA,
            EXR,
        }


        enum_TextureEditorMode m_EditorMode= enum_TextureEditorMode.ChannelModifer;
        enum_TextureExportType m_TextureExportType= enum_TextureExportType.TGA;

        Dictionary<enum_TextureEditorMode, ITextureEditor> m_Editors = new Dictionary<enum_TextureEditorMode, ITextureEditor>()
        { { enum_TextureEditorMode.ChannelModifer, new TE_ChannelModifier() },{enum_TextureEditorMode.ChannelMixer,new TE_ChannelMixer() }  };

        Texture2D m_TargetTexture;
        Texture2D m_DisplayTexture;

        public enum_ColorVisualize m_ColorVisualize= enum_ColorVisualize.RGBA;
        private void OnDisable()
        {
            foreach (var editor in m_Editors.Values)
                editor.Disable();

            if (m_DisplayTexture)
                GameObject.DestroyImmediate(m_DisplayTexture);
            m_DisplayTexture = null;
            if (m_TargetTexture)
                GameObject.DestroyImmediate(m_TargetTexture);
            m_TargetTexture = null;
        }
        private void OnGUI()
        {
            HorizontalScope.Begin(5,5,18);
            EditorGUI.LabelField(HorizontalScope.NextRect(0, 80),"Editor Mode:",UEGUIStyle_Window.m_TitleLabel);
            m_EditorMode = (enum_TextureEditorMode)EditorGUI.EnumPopup(HorizontalScope.NextRect(5,100),m_EditorMode);

            HorizontalScope.NextLine(2,20);
            var textureEditor = m_Editors[m_EditorMode];
            if (!textureEditor.OnGUIExportValid(ref m_TargetTexture, UpdateTexture))
                return;

            EditorGUI.BeginChangeCheck();
            HorizontalScope.NextLine(2, 18);
            EditorGUI.LabelField(HorizontalScope.NextRect(0, 60), "Display:", UEGUIStyle_Window.m_TitleLabel);
            HorizontalScope.NextLine(2, 18);
            EditorGUI.LabelField(HorizontalScope.NextRect(0, 60), "Visualize:");
            m_ColorVisualize= (enum_ColorVisualize)EditorGUI.EnumPopup(HorizontalScope.NextRect(5,40),m_ColorVisualize);

            if (EditorGUI.EndChangeCheck())
                UpdateDisplayTexture();

            HorizontalScope.NextLine(2, 256);
            Rect textureRect = HorizontalScope.NextRect(0, 256);
            GUI.DrawTexture(textureRect, EditorGUIUtility.whiteTexture);
            GUI.DrawTexture(textureRect.Collapse(Vector2.one * 10f), m_DisplayTexture);
            HorizontalScope.NextLine(2, 20);

            HorizontalScope.NextLine(2, 18);
            EditorGUI.LabelField(HorizontalScope.NextRect(5, 50), "Export:", UEGUIStyle_Window.m_TitleLabel);
            m_TextureExportType =(enum_TextureExportType) EditorGUI.EnumPopup(HorizontalScope.NextRect(0,50), m_TextureExportType);
            if (GUI.Button(HorizontalScope.NextRect(20, 80), "Export"))
                ExportTexture(m_TargetTexture,m_TargetTexture.name,m_TextureExportType);
        }


        void UpdateTexture()
        {
            if (m_DisplayTexture)
                GameObject.DestroyImmediate(m_DisplayTexture);
            m_DisplayTexture = new Texture2D(m_TargetTexture.width, m_TargetTexture.height, m_TargetTexture.format, true);
            UpdateDisplayTexture();
        }
        
        void UpdateDisplayTexture()
        {
            Color[] colors = m_TargetTexture.GetPixels();
            for (int i = 0; i < colors.Length; i++)
                colors[i] = m_ColorVisualize.FilterGreyScale(colors[i]);
            m_DisplayTexture.SetPixels(colors);
            m_DisplayTexture.Apply();
        }


        static void ExportTexture(Texture2D _saveTexture,string _name,enum_TextureExportType _exportType)
        {
            string extend = "";
            switch(_exportType)
            {
                default:throw new Exception("Invalid Export Type:"+_exportType);
                case enum_TextureExportType.JPG:extend = "jpg";break;
                case enum_TextureExportType.PNG:extend = "png";break;
                case enum_TextureExportType.TGA:extend = "tga";break;
                case enum_TextureExportType.EXR:extend = "exr";break;
            }


            if (!UEAsset.SaveFilePath(out string filePath, extend, _name + "_M"))
                return;
            byte[] bytes = null; 
            switch(_exportType)
            {
                case enum_TextureExportType.TGA:bytes = _saveTexture.EncodeToTGA();break;
                case enum_TextureExportType.EXR: bytes = _saveTexture.EncodeToEXR(); break;
                case enum_TextureExportType.JPG: bytes = _saveTexture.EncodeToJPG(); break;
                case enum_TextureExportType.PNG: bytes = _saveTexture.EncodeToPNG(); break;
            }
            UEAsset.CreateOrReplaceFile(filePath,bytes);
        }
        class TE_ChannelModifier : ITextureEditor
        {
            Texture2D m_ModifyTexture;
            enum_ColorVisualize m_ChannelModify = enum_ColorVisualize.None;
            public void Disable()
            {
                m_ModifyTexture = null;
            }
            public bool OnGUIExportValid(ref Texture2D _targetTexture, Action DisplayNotify)
            {
                EditorGUI.LabelField(HorizontalScope.NextRect(0, 60), "Texture:");
                EditorGUI.BeginChangeCheck();
                m_ModifyTexture = (Texture2D)EditorGUI.ObjectField(HorizontalScope.NextRect(5, 65), m_ModifyTexture, typeof(Texture2D), false);

                if (m_ModifyTexture == null || !m_ModifyTexture.isReadable)
                {
                    HorizontalScope.NextLine(2, 20);
                    EditorGUI.LabelField(HorizontalScope.NextRect(0, 240), "Select Readable Texture To Begin", UEGUIStyle_Window.m_ErrorLabel);
                    return false;
                }
                if (EditorGUI.EndChangeCheck())
                {
                    _targetTexture = new Texture2D(m_ModifyTexture.width, m_ModifyTexture.height, TextureFormat.RGBA32, true);
                    ResetModifyTexture(_targetTexture);
                    DisplayNotify();
                }

                HorizontalScope.NextLine(2, 20);
                EditorGUI.LabelField(HorizontalScope.NextRect(5, 60), "Modify:", UEGUIStyle_Window.m_TitleLabel);
                m_ChannelModify = (enum_ColorVisualize)EditorGUI.EnumPopup(HorizontalScope.NextRect(5, 120), m_ChannelModify);

                if (m_ChannelModify != enum_ColorVisualize.None)
                {
                    HorizontalScope.NextLine(2, 20);
                    if (GUI.Button(HorizontalScope.NextRect(10, 60), "Reverse"))
                    {
                        DoColorModify(_targetTexture, m_ChannelModify, value => 1 - value);
                        DisplayNotify();
                    }
                    if (GUI.Button(HorizontalScope.NextRect(10, 60), "Fill"))
                    {
                        DoColorModify(_targetTexture, m_ChannelModify, value => 1);
                        DisplayNotify();
                    }
                    if (GUI.Button(HorizontalScope.NextRect(10, 60), "Clear"))
                    {
                        DoColorModify(_targetTexture, m_ChannelModify, value => 0);
                        DisplayNotify();
                    }
                }

                if (GUI.Button(HorizontalScope.NextRect(5, 80), "Reset"))
                {
                    ResetModifyTexture(_targetTexture);
                    DisplayNotify();
                }
                return true;
            }

            void ResetModifyTexture(Texture2D _targetTexture)
            {
                _targetTexture.SetPixels(m_ModifyTexture.GetPixels());
                _targetTexture.Apply();

            }

            static void DoColorModify(Texture2D _target, enum_ColorVisualize _color, Func<float, float> _OnEachValue)
            {
                Color[] colors = _target.GetPixels();
                for (int i = 0; i < colors.Length; i++)
                {
                    switch (_color)
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
        class TE_ChannelMixer : ITextureEditor
        {
            Texture2D m_R, m_G, m_B, m_A;
            float m_RDefault = 1f, m_GDefault = 1f, m_BDefault = 1f, m_ADefault = 1f;
            public void Disable()
            {
                m_R = null;
                m_A = null;
                m_G = null;
                m_A = null;
            }
            bool ValidTexture(Texture2D _texture) => _texture != null && _texture.isReadable;
            bool ValidCheck(out int width, out int height, out bool rValid, out bool gValid, out bool bValid, out bool aValid)
            {
                rValid = ValidTexture(m_R);
                gValid = ValidTexture(m_G);
                bValid = ValidTexture(m_B);
                aValid = ValidTexture(m_A);

                int maxWidth = Mathf.Max(m_R ? m_R.width : 0, m_G ? m_G.width : 0, m_B ? m_B.width : 0, m_A ? m_A.width : 0);
                int maxHeight = Mathf.Max(m_R ? m_R.height : 0, m_G ? m_G.height : 0, m_B ? m_B.height : 0, m_A ? m_A.height : 0);
                width = maxWidth;
                height = maxHeight;

                IEnumerable<Texture2D> AllTextures()
                {
                    yield return m_R;
                    yield return m_G;
                    yield return m_B;
                    yield return m_A;
                }

                bool verifyChannels = !AllTextures().Any(p => p != null && (p.width != maxWidth || p.height != maxHeight));
                bool validChannel = rValid || gValid || bValid || aValid;
                if (!verifyChannels || !validChannel)
                    return false;
                return true;
            }
            public bool OnGUIExportValid(ref Texture2D _targetTexture, Action DisplayNotify)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUI.LabelField(HorizontalScope.NextRect(0, 20), "R:");
                m_R = (Texture2D)EditorGUI.ObjectField(HorizontalScope.NextRect(5, 65), m_R, typeof(Texture2D), false);
                if (!ValidTexture(m_R))
                    m_RDefault = EditorGUI.Slider(HorizontalScope.NextRect(5, 150), m_RDefault, 0f, 1f);
                HorizontalScope.NextLine(2, 20);
                EditorGUI.LabelField(HorizontalScope.NextRect(0, 20), "G:");
                m_G = (Texture2D)EditorGUI.ObjectField(HorizontalScope.NextRect(5, 65), m_G, typeof(Texture2D), false);
                if (!ValidTexture(m_G))
                    m_GDefault = EditorGUI.Slider(HorizontalScope.NextRect(5, 150), m_GDefault, 0f, 1f);
                HorizontalScope.NextLine(2, 20);
                EditorGUI.LabelField(HorizontalScope.NextRect(0, 20), "B:");
                m_B = (Texture2D)EditorGUI.ObjectField(HorizontalScope.NextRect(5, 65), m_B, typeof(Texture2D), false);
                if (!ValidTexture(m_B))
                    m_BDefault = EditorGUI.Slider(HorizontalScope.NextRect(5, 150), m_BDefault, 0f, 1f);
                HorizontalScope.NextLine(2, 20);
                EditorGUI.LabelField(HorizontalScope.NextRect(0, 20), "A:");
                m_A = (Texture2D)EditorGUI.ObjectField(HorizontalScope.NextRect(5, 65), m_A, typeof(Texture2D), false);
                if (!ValidTexture(m_A))
                    m_ADefault = EditorGUI.Slider(HorizontalScope.NextRect(5, 150), m_ADefault, 0f, 1f);

                bool validTextures = ValidCheck(out int maxWidth, out int maxHeight, out bool rValid, out bool gValid, out bool bValid, out bool aValid);
                if (!validTextures)
                    return false;

                if (EditorGUI.EndChangeCheck())
                {
                    Color[] rPixels = rValid ? m_R.GetPixels() : null;
                    Color[] gPixels = gValid ? m_G.GetPixels() : null;
                    Color[] bPixels = bValid ? m_B.GetPixels() : null;
                    Color[] aPixels = aValid ? m_A.GetPixels() : null;

                    Color[] mix = new Color[maxWidth * maxHeight];
                    for (int i = 0; i < maxWidth * maxHeight; i++)
                        mix[i] = new Color(
                            rValid ? rPixels[i].r : m_RDefault,
                            gValid ? gPixels[i].r : m_GDefault,
                            bValid ? bPixels[i].r : m_BDefault,
                            aValid ? aPixels[i].r : m_ADefault);

                    _targetTexture = new Texture2D(maxWidth, maxHeight, TextureFormat.RGBA32, true);
                    _targetTexture.SetPixels(mix);
                    _targetTexture.Apply();
                    DisplayNotify();
                }
                return true;
            }
        }
    }
}