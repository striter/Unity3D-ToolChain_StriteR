using System;
using Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEditor.Extensions
{
    [Serializable]
    public class FTextureGeneratorMaterial : ITextureGenerator
    {
        [DefaultAsset("Assets/Shaders/Baking/Gradient/ColorPalette.mat")] public Material m_Material;
        public bool Valid => m_Material != null;
        public static readonly FTextureGeneratorMaterial kDefault = new();
        public void Preview(Rect _rect,ref FTextureHelper helper)
        {
            var m_RenderTexture = helper.renderTexture;
            RenderTexture.active = m_RenderTexture;
            GL.Clear(true, true, Color.clear); // clear the render texture
            Graphics.Blit(Texture2D.whiteTexture, m_RenderTexture, m_Material,0);
            RenderTexture.active = null;
            EditorGUI.DrawTextureTransparent(_rect, m_RenderTexture);
        }

        public void Output(ref FTextureHelper helper)
        {
            if (!UEAsset.SaveFilePath(out var filePath, "png", $"{m_Material.name}_Output"))
                return;
            
            var renderTexture = helper.renderTexture;
            var texture = helper.texture;
            RenderTexture.active = renderTexture;
            texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture.Apply();
            UEAsset.CreateOrReplaceFile<Texture2D>(filePath, texture.EncodeToPNG());
            RenderTexture.active = null;
        }

        public void Dispose()
        {
        }
    }
}