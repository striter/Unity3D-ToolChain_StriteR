using System;
using UnityEngine;

namespace UnityEditor.Extensions
{

    [Serializable]
    public class FTextureGeneratorMaterial : ITextureGenerator
    {
        [DefaultAsset("Assets/Shaders/TextureOutput/ColorPalette.mat")] public Material material;
        public bool Valid => material != null;
        public static readonly FTextureGeneratorMaterial kDefault = new();
        private RenderTexture m_RenderTexture;
        
        public void Setup(TextureGeneratorData _data)
        {
            m_RenderTexture = _data.RenderTexture(m_RenderTexture);
        }

        public void Preview(Rect _rect) => EditorGUI.DrawPreviewTexture(_rect, m_RenderTexture, material);
        
        public void Dispose()
        {
            if (m_RenderTexture != null)
                RenderTexture.ReleaseTemporary(m_RenderTexture);
            m_RenderTexture = null;
        }

        public void Output()
        {
            RenderTexture.active = m_RenderTexture;
            Graphics.Blit(Texture2D.whiteTexture, m_RenderTexture, material);
            var texture = new Texture2D(m_RenderTexture.width, m_RenderTexture.height, TextureFormat.ARGB32, false);
            texture.ReadPixels(new Rect(0, 0, m_RenderTexture.width, m_RenderTexture.height), 0, 0);
            texture.Apply();
            if (UEAsset.SaveFilePath(out string filePath, "png", $"{material.name}_Output"))
                UEAsset.CreateOrReplaceFile<Texture2D>(filePath, texture.EncodeToPNG());
            GameObject.DestroyImmediate(texture);
            RenderTexture.active = null;
        }
    }
}