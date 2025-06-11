using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace UnityEditor.Extensions
{
    public enum EResolution
    {
        _1 = 1,
        _2 = 2,
        _4 = 4,
        _8 = 8,
        _16 = 16,
        _32 = 32,
        _64 = 64,
        _128 = 128,
        _256 = 256,
        _512 = 512,
        _1024 = 1024,
        _2048 = 2048,
        _4096 = 4096,
    }
    
    [Serializable]
    public struct FTextureGenerateData
    {
        public bool 
        [DefaultAsset("Assets/Shaders/TextureOutput/ColorPalette.mat")] public Material material;
        public EResolution resolutionX;
        public EResolution resolutionY;
        public FilterMode filterMode;
        public static readonly FTextureGenerateData kDefault = new(){material = null, resolutionX = EResolution._256, resolutionY = EResolution._256, filterMode = FilterMode.Point};
        public bool Valid => material != null;
        public RenderTexture Output(RenderTexture _texture)
        {
            if (_texture != null && (_texture.width != (int)resolutionX || _texture.height != (int)resolutionY))
            {
                RenderTexture.ReleaseTemporary(_texture);
                _texture = null;
            }

            if (_texture == null)
                _texture = RenderTexture.GetTemporary((int)resolutionX, (int)resolutionY, 0, RenderTextureFormat.ARGB32);

            _texture.filterMode = filterMode;
            return _texture;
        }
    }
    
    public class EWTextureGenerator : EditorWindow
    {
        private RenderTexture m_RenderTexture;
        [SerializeField] private FTextureGenerateData m_Data = FTextureGenerateData.kDefault;
        private SerializedProperty m_DataProperty;
        private SerializedObject m_SerializedWindow;
        private const float kTextureCollapse = 6f;
        private const float kTexturePadding = 10f;
        private void OnEnable()
        {
            m_SerializedWindow = new SerializedObject(this);
            m_DataProperty = m_SerializedWindow.FindProperty(nameof(m_Data));
        }

        private void OnDisable()
        {
            if (m_RenderTexture != null)
                RenderTexture.ReleaseTemporary(m_RenderTexture);
            m_RenderTexture = null;
        }

        private void OnGUI()
        {
            EditorGUILayout.PropertyField(m_DataProperty);
            var propertyHeight = EditorGUI.GetPropertyHeight(m_DataProperty, true);
            HorizontalScope.Begin(5,5,propertyHeight,Screen.width);
            if (EditorGUI.EndChangeCheck())
            {
                m_SerializedWindow.ApplyModifiedPropertiesWithoutUndo();
                m_RenderTexture = m_Data.Output(m_RenderTexture);
                Undo.RecordObject(this, "Noise Generator Change");
            }

            if (!m_Data.Valid)
                return;
            
            var aspect = (float)m_RenderTexture.height / m_RenderTexture.width;
            var width = math.min(position.width,(position.height - propertyHeight - 20) / aspect) - kTexturePadding;
            HorizontalScope.NextLine(0f,aspect * width);
            var textureRect = HorizontalScope.NextRect(kTexturePadding / 2, width);
            GUI.DrawTexture(textureRect,Texture2D.whiteTexture);
            EditorGUI.DrawPreviewTexture(textureRect.Collapse(Vector2.one*kTextureCollapse,Vector2.one * .5f), m_RenderTexture, m_Data.material);
            HorizontalScope.NextLine(2, 20);
            if (GUI.Button(HorizontalScope.NextRect(0, 80), "Export"))
            {
                RenderTexture.active = m_RenderTexture;
                Graphics.Blit(Texture2D.whiteTexture, m_RenderTexture, m_Data.material);
                var texture = new Texture2D(m_RenderTexture.width, m_RenderTexture.height, TextureFormat.ARGB32, false);
                texture.ReadPixels(new Rect(0, 0, m_RenderTexture.width, m_RenderTexture.height), 0, 0);
                texture.Apply();
                if (UEAsset.SaveFilePath(out string filePath, "png", $"{m_Data.material.name}_Output"))
                    UEAsset.CreateOrReplaceFile<Texture2D>(filePath, texture.EncodeToPNG());
                GameObject.DestroyImmediate(texture);
                RenderTexture.active = null;
            }
        }
    }
}