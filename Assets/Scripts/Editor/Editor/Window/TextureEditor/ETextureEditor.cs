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

    [Serializable]
    public struct ChannelCollector : IChannelCollector
    {
        public EChannelOperation operation;
        [Foldout(nameof(operation), EChannelOperation.Constant)] [Range(0, 1)] public float constantValue;
        [MFold(nameof(operation),EChannelOperation.Constant)] public Texture2D texture;
        public EChannelOperation Operation => operation;
        public Texture2D Texture => texture;
        public float ConstantValue => constantValue;
        public Color[] PixelsResolved { get; set; }
        
        public static readonly ChannelCollector kDefault = new ChannelCollector()
        {
            texture = null,
            operation = EChannelOperation.R,
        };
    }

    public class ETextureEditor : EditorWindow
    {
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
                    if (!UEAsset.SaveFilePath(out string filePath, UTextureExport.GetExtension(m_TextureExportType), exportTexture.name + "_M"))
                        return;

                    UTextureExport.ExportTexture(exportTexture,filePath,m_TextureExportType);
                }

                return;
            }

            if (EditorGUI.EndChangeCheck())
            {
                m_SerializedObject.ApplyModifiedProperties();
            }
        }
        
    }
}