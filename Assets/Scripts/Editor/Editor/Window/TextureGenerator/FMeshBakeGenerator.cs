using System;
using UnityEngine;

namespace UnityEditor.Extensions
{
    [Serializable]
    public class FMeshBakeGenerator : ITextureGenerator
    {       
        [DefaultAsset("Assets/Shaders/Baking/Mesh/ModelCurvature.mat")] public Material m_Material;
        [DefaultAsset("Assets/ArtPreset/Meshes/InfiniteScan/ScanHead.fbx")] public Mesh m_BakeMesh;
        public Color m_BackgroundColor = Color.clear;
        public bool Valid => m_Material != null && m_BakeMesh != null;
        void Apply(RenderTexture _texture)
        {
            RenderTexture.active = _texture;
            GL.Clear(true, true, m_BackgroundColor); // clear the render texture
            m_Material.SetPass(0);
            Graphics.DrawMeshNow(m_BakeMesh, Matrix4x4.identity);
            RenderTexture.active = null;
        }
        public void Preview(Rect _rect, ref FTextureHelper _helper)
        {
            var renderTexture = _helper.renderTexture;
            Apply(renderTexture);
            EditorGUI.DrawTextureTransparent(_rect, renderTexture);
        }

        public Texture2D Output(ref FTextureHelper _helper)
        {
            if (!UEAsset.SaveFilePath(out var filePath, "png", $"{m_Material.name}_Output"))
                return null;
            
            var renderTexture = _helper.renderTexture;
            Apply(renderTexture);
            var texture = _helper.texture;
            RenderTexture.active = renderTexture;
            texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture.Apply();
            RenderTexture.active = null;
            return UEAsset.CreateOrReplaceFile<Texture2D>(filePath, texture.EncodeToPNG());
        }

        public void Dispose()
        {
        }
    }
}