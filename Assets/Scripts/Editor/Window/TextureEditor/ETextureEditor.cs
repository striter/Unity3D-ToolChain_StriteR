using System;
using System.Collections.Generic;
using System.Linq;
using OPhysics;
using UnityEngine;
using UnityEngine.Serialization;

namespace UnityEditor.Extensions.TextureEditor
{
    public interface ITextureEditor
    {
        void OnEnable(SerializedProperty _parentProperty);
        void OnDisable();
        void OnGUI();
        bool IsValidTexture(out int width,out int height,out TextureFormat _format);
        Texture2D GetTextureOutput();
    }

    public class ETextureEditor : EditorWindow
    {
        enum ETextureExportType
        {
            PNG,
            JPG,
            TGA,
            EXR,
        }

        enum EEditorMode
        {
            ChannelMixer = 0,
            ChannelModifier = 1,
            ChannelAppender = 2,
        }

        [SerializeField] private ChannelCombiner combiner = new ChannelCombiner();
        [SerializeField] private ChannelModifier modifier = new ChannelModifier();
        [SerializeField] private ChannelAppender appender = new ChannelAppender();
        
        ETextureExportType m_TextureExportType= ETextureExportType.TGA;

        private EEditorMode m_EditorMode;
        
        Texture2D m_DisplayTexture;

        private EColorVisualize m_ColorVisualize= EColorVisualize.RGBA;

        private List<ITextureEditor> m_Editors;
        private SerializedObject m_SerializedObject;
        private void OnEnable()
        {
            m_SerializedObject = new SerializedObject(this);
            m_Editors = new List<ITextureEditor>()
            {
                combiner, 
                modifier,
                appender,
            };
            combiner.OnEnable(m_SerializedObject.FindProperty(nameof(combiner)));
            modifier.OnEnable(m_SerializedObject.FindProperty(nameof(modifier)));
            appender.OnEnable(m_SerializedObject.FindProperty(nameof(appender)));
        }

        private void OnDisable()
        {
            m_SerializedObject = null;
            foreach (var editor in m_Editors)
                editor.OnDisable();

            if (m_DisplayTexture)
                GameObject.DestroyImmediate(m_DisplayTexture);
            m_DisplayTexture = null;
        }
        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            
            HorizontalScope.Begin(5,5,18);
            EditorGUI.LabelField(HorizontalScope.NextRect(0, 80),"Editor Mode:",UEGUIStyle_Window.m_TitleLabel);
            m_EditorMode = (EEditorMode)EditorGUI.EnumPopup(HorizontalScope.NextRect(5,100),m_EditorMode);

            var textureEditor = m_Editors[(int)m_EditorMode];
            textureEditor.OnGUI();
            
            var valid = textureEditor.IsValidTexture(out var width,out var height,out var format);
            HorizontalScope.NextLine(2, 18);
            EditorGUI.LabelField(HorizontalScope.NextRect(0, 120),valid ? "Output Settings:" : "Invalid Input.",UEGUIStyle_Window.m_TitleLabel);
            if (valid)
            {
                HorizontalScope.NextLine(2, 18);
                
                
                EditorGUI.LabelField(HorizontalScope.NextRect(0, 60), "Visualize:");
                m_ColorVisualize = (EColorVisualize)EditorGUI.EnumPopup(HorizontalScope.NextRect(5,40),m_ColorVisualize);
                if (EditorGUI.EndChangeCheck())
                {
                    m_SerializedObject.ApplyModifiedProperties();
                    if (m_DisplayTexture) GameObject.DestroyImmediate(m_DisplayTexture);
                    m_DisplayTexture = null;
                }
                
                if (GUI.Button(HorizontalScope.NextRect(20, 80), "Preview"))
                {
                    if (m_DisplayTexture) GameObject.DestroyImmediate(m_DisplayTexture);
                    m_DisplayTexture = new Texture2D(width, height, format, true);
                    Color[] colors = textureEditor.GetTextureOutput().GetPixels();
                    for (int i = 0; i < colors.Length; i++)
                        colors[i] = m_ColorVisualize.FilterGreyScale(colors[i]);
                    m_DisplayTexture.SetPixels(colors);
                    m_DisplayTexture.Apply();
                }
                
                if (m_DisplayTexture != null)
                {
                    HorizontalScope.NextLine(2, 18);
                    EditorGUI.LabelField(HorizontalScope.NextRect(0, 60), "Display:", UEGUIStyle_Window.m_TitleLabel);
                    HorizontalScope.NextLine(2, 18);
                    
                    HorizontalScope.NextLine(2, 256);
                    Rect textureRect = HorizontalScope.NextRect(0, 256);
                    GUI.DrawTexture(textureRect, EditorGUIUtility.whiteTexture);
                    GUI.DrawTexture(textureRect.Collapse(Vector2.one * 10f), m_DisplayTexture);
                    HorizontalScope.NextLine(2, 20);
                }

                
                HorizontalScope.NextLine(2, 18);
                EditorGUI.LabelField(HorizontalScope.NextRect(5, 50), "Export:", UEGUIStyle_Window.m_TitleLabel);
                m_TextureExportType =(ETextureExportType) EditorGUI.EnumPopup(HorizontalScope.NextRect(0,50), m_TextureExportType);
                if (GUI.Button(HorizontalScope.NextRect(20, 80), "Export"))
                {
                    var exportTexture = textureEditor.GetTextureOutput();
                    ExportTexture(exportTexture,exportTexture.name,m_TextureExportType);
                }

                return;
            }

            if (EditorGUI.EndChangeCheck())
            {
                m_SerializedObject.ApplyModifiedProperties();
            }
        }
        
        static void ExportTexture(Texture2D _exportTexture,string _name,ETextureExportType _exportType)
        {
            string extend = "";
            switch(_exportType)
            {
                default:throw new Exception("Invalid Type:"+_exportType);
                case ETextureExportType.JPG:extend = "jpg";break;
                case ETextureExportType.PNG:extend = "png";break;
                case ETextureExportType.TGA:extend = "tga";break;
                case ETextureExportType.EXR:extend = "exr";break;
            }

            if (!UEAsset.SaveFilePath(out string filePath, extend, _name + "_M"))
                return;
            
            byte[] bytes = null; 
            switch(_exportType)
            {
                case ETextureExportType.TGA:bytes = _exportTexture.EncodeToTGA();break;
                case ETextureExportType.EXR: bytes = _exportTexture.EncodeToEXR(); break;
                case ETextureExportType.JPG: bytes = _exportTexture.EncodeToJPG(); break;
                case ETextureExportType.PNG: bytes = _exportTexture.EncodeToPNG(); break;
            }
            UEAsset.CreateOrReplaceFile(filePath,bytes);
        }
    }
}