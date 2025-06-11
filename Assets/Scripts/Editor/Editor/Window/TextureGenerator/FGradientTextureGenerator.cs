using System;
using Procedural.Tile;
using Unity.Mathematics;
using UnityEngine;

namespace UnityEditor.Extensions
{
    
    [Serializable]
    public class FGradientTextureGenerator : ITextureGenerator
    {
        public Gradient m_Gradient = new Gradient();
        public bool Valid => true;
        private Texture2D m_Texture;
        private Color[] m_Colors;
        public void Setup(TextureGeneratorData _data)
        {
            m_Texture = _data.Texture2D(m_Texture);
            m_Colors = _data.Colors(m_Colors);
        }

        void Apply()
        {
            for (var i = 0; i < m_Texture.width; i++)
            {
                var normalize = (float)i / m_Texture.width;
                var color = m_Gradient.Evaluate(normalize);
                for (var j = 0; j < m_Texture.height; j++)
                {
                    var index = new int2(i, j).ToIndex(m_Texture.width);
                    m_Colors[index] = color;
                }
            }
            
            m_Texture.SetPixels(m_Colors);
            m_Texture.Apply();
        }
        
        public void Preview(Rect _rect)
        {
            Apply();
            EditorGUI.DrawPreviewTexture(_rect,m_Texture);
        }

        public void Output()
        {
            Apply();
            if (UEAsset.SaveFilePath(out string filePath, "png", $"Gradient"))
                UEAsset.CreateOrReplaceFile<Texture2D>(filePath, m_Texture.EncodeToPNG());
        }

        public void Dispose()
        {
            if (m_Texture != null)
                GameObject.DestroyImmediate(m_Texture);
            m_Texture = null;
        }
    }
}