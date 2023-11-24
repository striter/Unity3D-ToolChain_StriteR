using System;
using System.Collections.Generic;
using System.Linq;
using OPhysics;
using UnityEngine;
namespace UnityEditor.Extensions.TextureEditor
{
    public interface ITextureEditor
    {
        void OnEnable(SerializedProperty _parentProperty);
        void OnDisable();
        void OnGUI();
        bool IsValidTexture();
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
        }

        [SerializeField] private ChannelMixer mixer = new ChannelMixer();
        [SerializeField] private ChannelModifier modifier = new ChannelModifier();
        
        ETextureExportType m_TextureExportType= ETextureExportType.TGA;

        private EEditorMode m_EditorMode;
        
        Texture2D m_TargetTexture;
        Texture2D m_DisplayTexture;

        private EColorVisualize m_ColorVisualize= EColorVisualize.RGBA;

        private List<ITextureEditor> m_Editors;
        private SerializedObject m_SerializedObject;
        private void OnEnable()
        {
            m_Editors = new List<ITextureEditor>()
            {
                mixer, 
                modifier
            };
            m_SerializedObject = new SerializedObject(this);
            foreach (var editor in m_Editors)
                editor.OnEnable(m_SerializedObject.FindProperty(nameof(mixer)));
        }

        private void OnDisable()
        {
            m_SerializedObject = null;
            foreach (var editor in m_Editors)
                editor.OnDisable();

            if (m_DisplayTexture)
                GameObject.DestroyImmediate(m_DisplayTexture);
            m_DisplayTexture = null;
            if (m_TargetTexture)
                GameObject.DestroyImmediate(m_TargetTexture);
            m_TargetTexture = null;
        }
        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            
            HorizontalScope.Begin(5,5,18);
            EditorGUI.LabelField(HorizontalScope.NextRect(0, 80),"Editor Mode:",UEGUIStyle_Window.m_TitleLabel);
            m_EditorMode = (EEditorMode)EditorGUI.EnumPopup(HorizontalScope.NextRect(5,100),m_EditorMode);

            HorizontalScope.NextLine(2,20);
            var textureEditor = m_Editors[(int)m_EditorMode];
            textureEditor.OnGUI();
            if (EditorGUI.EndChangeCheck())
            {
                if(m_TargetTexture)
                    GameObject.DestroyImmediate(m_TargetTexture);
                
                m_TargetTexture = textureEditor.IsValidTexture() ? textureEditor.GetTextureOutput() : null;
                UpdatePreviewTexture();
                m_SerializedObject.ApplyModifiedProperties();
            }
            
            if (!m_TargetTexture)
                return;
            EditorGUI.BeginChangeCheck();

            HorizontalScope.NextLine(2, 18);
            EditorGUI.LabelField(HorizontalScope.NextRect(0, 60), "Display:", UEGUIStyle_Window.m_TitleLabel);
            HorizontalScope.NextLine(2, 18);
            EditorGUI.LabelField(HorizontalScope.NextRect(0, 60), "Visualize:");
            m_ColorVisualize = (EColorVisualize)EditorGUI.EnumPopup(HorizontalScope.NextRect(5,40),m_ColorVisualize);
                
            HorizontalScope.NextLine(2, 256);
            Rect textureRect = HorizontalScope.NextRect(0, 256);
            GUI.DrawTexture(textureRect, EditorGUIUtility.whiteTexture);
            GUI.DrawTexture(textureRect.Collapse(Vector2.one * 10f), m_DisplayTexture);
            HorizontalScope.NextLine(2, 20);
        
            HorizontalScope.NextLine(2, 18);
            EditorGUI.LabelField(HorizontalScope.NextRect(5, 50), "Export:", UEGUIStyle_Window.m_TitleLabel);
            m_TextureExportType =(ETextureExportType) EditorGUI.EnumPopup(HorizontalScope.NextRect(0,50), m_TextureExportType);
            
            if (GUI.Button(HorizontalScope.NextRect(20, 80), "Export"))
                ExportTexture(m_TargetTexture,m_TargetTexture.name,m_TextureExportType);

            if (EditorGUI.EndChangeCheck())
                UpdateDisplayTexture();
        }
        
        void UpdatePreviewTexture()
        {
            if (m_TargetTexture == null) return;

            if (m_DisplayTexture!=null && m_TargetTexture.width == m_DisplayTexture.width && m_TargetTexture.height == m_DisplayTexture.height)
                return;
            
            if (m_DisplayTexture) GameObject.DestroyImmediate(m_DisplayTexture);
            m_DisplayTexture = new Texture2D(m_TargetTexture.width, m_TargetTexture.height, m_TargetTexture.format, true);
        }
        
        void UpdateDisplayTexture()
        {
            Color[] colors = m_TargetTexture.GetPixels();
            for (int i = 0; i < colors.Length; i++)
                colors[i] = m_ColorVisualize.FilterGreyScale(colors[i]);
            m_DisplayTexture.SetPixels(colors);
            m_DisplayTexture.Apply();
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