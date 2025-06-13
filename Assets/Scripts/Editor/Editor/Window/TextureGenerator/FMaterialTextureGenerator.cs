using System;
using UnityEngine;

namespace UnityEditor.Extensions
{

    [Serializable]
    public class FTextureGeneratorMaterial : ITextureGenerator
    {
        [DefaultAsset("Assets/Shaders/TextureOutput/TextureOutput_ColorPalette.mat")] public Material material;
        public bool Valid => material != null;
        public static readonly FTextureGeneratorMaterial kDefault = new();
        private RenderTexture m_RenderTexture;
        private Texture2D m_Texture;
        public void Setup(TextureGeneratorData _data)
        {
            m_RenderTexture = _data.RenderTexture(m_RenderTexture,false);
            m_Texture = _data.Texture2D(m_Texture,false);
        }

        public void Preview(Rect _rect)
        {
            EditorGUI.DrawPreviewTexture(_rect, Texture2D.whiteTexture,material);
        }
        
        public void Dispose()
        {
            if (m_RenderTexture != null)
                RenderTexture.ReleaseTemporary(m_RenderTexture);
            m_RenderTexture = null;
            
            if (m_Texture != null)
                GameObject.DestroyImmediate(m_Texture);
            m_Texture = null;
        }

        public void Output()
        {
            if (!UEAsset.SaveFilePath(out var filePath, "png", $"{material.name}_Output"))
                return;
            
            RenderTexture.active = m_RenderTexture;
            Graphics.Blit(Texture2D.whiteTexture, m_RenderTexture, material,0);
            m_Texture.ReadPixels(new Rect(0, 0, m_RenderTexture.width, m_RenderTexture.height), 0, 0);
            m_Texture.Apply();
            UEAsset.CreateOrReplaceFile<Texture2D>(filePath, m_Texture.EncodeToPNG());
            RenderTexture.active = null;
        }
    }
}