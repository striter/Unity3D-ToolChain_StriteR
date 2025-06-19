using System;
using Procedural.Tile;
using Unity.Mathematics;
using UnityEngine;

namespace UnityEditor.Extensions
{

    public enum EGradientMode
    {
        Gradient,
        ColorPalette
    }

    [Serializable]
    public struct GradientTextureData
    {
        public EGradientMode mode;
        [Foldout(nameof(mode),EGradientMode.Gradient)] public Gradient gradient;
        [Foldout(nameof(mode),EGradientMode.ColorPalette)] public ColorPalette colorPalette;
        public static GradientTextureData kDefault => new GradientTextureData { mode = EGradientMode.ColorPalette, gradient = new Gradient(), colorPalette = ColorPalette.kDefault };
        public Color Evaluate(float _value) => mode == EGradientMode.Gradient ? gradient.Evaluate(_value) : colorPalette.Evaluate(_value);
    }

    [Serializable]
    public class FGradientTextureGenerator : ITextureGenerator
    {
        public GradientTextureData m_Data = GradientTextureData.kDefault;
        public bool Valid => true;

        void Apply(ref FTextureHelper helper)
        {
            var texture = helper.texture;
            var colors = helper.colors;
            for (var i = 0; i < texture.width; i++)
            {
                var normalize = (float)i / texture.width;
                var color = m_Data.Evaluate(normalize);
                for (var j = 0; j < texture.height; j++)
                {
                    var index = new int2(i, j).ToIndex(texture.width);
                    colors[index] = color;
                }
            }
            
            texture.SetPixels(colors);
            texture.Apply();
        }
        
        public void Preview(Rect _rect, ref FTextureHelper helper)
        {
            Apply(ref helper);
            EditorGUI.DrawPreviewTexture(_rect,helper.texture);
        }

        public Texture2D Output(ref FTextureHelper helper)
        {
            Apply(ref helper);
            if (UEAsset.SaveFilePath(out string filePath, "png", $"Gradient"))
                return UEAsset.CreateOrReplaceFile<Texture2D>(filePath, helper.texture.EncodeToPNG());
            return null;
        }

        public void Dispose()
        {
        }
    }
}