using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UAlthogrim;
namespace TEditor
{
    using static UEGUI;
    public class EWNoiseGenerator : EditorWindow
    {
        public enum enum_NoiseType
        {
            Value,
            Perlin,
            Simplex,
            VoronoiUnit,
            VoronoiDistance,
        }
        public enum enum_NoiseSample
        {
            Unit,
            _01,
            Absolute,
        }
        public static bool NoiseSampleSupported(enum_NoiseType _type)
        {
            switch(_type)
            {
                default:
                    return false;
                case enum_NoiseType.Simplex:
                case enum_NoiseType.Perlin:
                    return true;
            }
        }
        public static float GetNoise(float noiseX,float noiseY,float scale, enum_NoiseType noiseType)
        {
            float noise = 0;
            noiseX *= scale;
            noiseY *= scale;
            switch (noiseType)
            {
                case enum_NoiseType.Value: noise = UNoise.ValueUnit(Mathf.Floor(noiseX) , Mathf.Floor( noiseY) ); break;
                case enum_NoiseType.Perlin: noise = UNoise.PerlinUnit( noiseX ,noiseY, 0); break;
                case enum_NoiseType.Simplex: noise = UNoise.Simplex(noiseX , noiseY ); break;
                case enum_NoiseType.VoronoiUnit: noise = UNoise.VoronoiUnit(noiseX, noiseY).y; break;
                case enum_NoiseType.VoronoiDistance: noise = UNoise.VoronoiUnit(noiseX, noiseY).x;break;
            }
            return noise;
        }
        public static float GetNoiseOctave(float x, float y,float scale, enum_NoiseType noiseType, int octaveCount)
        {
            float value = 0;
            float frequency = 1;
            float amplitude = .6f;
            for (int i = 0; i < octaveCount; i++)
            {
                value += GetNoise(x * frequency, y * frequency, scale, noiseType) * amplitude;
                amplitude *= .5f;
                frequency *= 2;
            }
            return value;
        }

        int m_SizePower=5;
        float m_Scale = 1;
        bool m_Octave;
        int m_OctaveCount = 6;
        enum_NoiseType m_NoiseType = enum_NoiseType.Perlin;
        enum_NoiseSample m_NoiseSample = enum_NoiseSample.Unit;
        FilterMode m_FilterMode = FilterMode.Bilinear;
        Texture2D m_Texture;
        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            HorizontalScope.Begin(5, 5, 20);
            EditorGUI.LabelField(HorizontalScope.NextRect(0, 60), "Type:", UEGUIStyle_Window.m_TitleLabel);
            m_NoiseType = (enum_NoiseType)EditorGUI.EnumPopup(HorizontalScope.NextRect(5, 120), m_NoiseType);

            bool noiseSampleSupported = NoiseSampleSupported(m_NoiseType);
            if (noiseSampleSupported)
            {
                HorizontalScope.NextLine(2, 20);
                EditorGUI.LabelField(HorizontalScope.NextRect(0, 60), "Sample:", UEGUIStyle_Window.m_TitleLabel);
                m_NoiseSample = (enum_NoiseSample)EditorGUI.EnumPopup(HorizontalScope.NextRect(5, 120), m_NoiseSample);
            }
            HorizontalScope.NextLine(2, 20);
            EditorGUI.LabelField(HorizontalScope.NextRect(0, 60), "Filter:", UEGUIStyle_Window.m_TitleLabel);
            m_FilterMode = (FilterMode)EditorGUI.EnumPopup(HorizontalScope.NextRect(5, 120), m_FilterMode);
            HorizontalScope.NextLine(2, 20);
            EditorGUI.LabelField(HorizontalScope.NextRect(0, 60), "Size:", UEGUIStyle_Window.m_TitleLabel);
            m_SizePower = EditorGUI.IntSlider(HorizontalScope.NextRect(5, 120), m_SizePower, 3, 10);
            int size = Mathf.RoundToInt(Mathf.Pow(2, m_SizePower));
            EditorGUI.LabelField(HorizontalScope.NextRect(5, 40), size.ToString());
            HorizontalScope.NextLine(2, 20);
            EditorGUI.LabelField(HorizontalScope.NextRect(0, 60), "Scale:", UEGUIStyle_Window.m_TitleLabel);
            m_Scale = EditorGUI.Slider(HorizontalScope.NextRect(5, 120), m_Scale, 1f, 30f);

            HorizontalScope.NextLine(2, 20);
            EditorGUI.LabelField(HorizontalScope.NextRect(0, 60), "Octave:", UEGUIStyle_Window.m_TitleLabel);
            m_Octave = EditorGUI.Toggle(HorizontalScope.NextRect(5, 20), m_Octave);
            if (m_Octave)
            {
                HorizontalScope.NextLine(2, 20);
                EditorGUI.LabelField(HorizontalScope.NextRect(0, 60), "Count:", UEGUIStyle_Window.m_TitleLabel);
                m_OctaveCount = EditorGUI.IntSlider(HorizontalScope.NextRect(5, 120), m_OctaveCount, 2, 7);
                HorizontalScope.NextLine(2, 20);
            }

            if (!m_Texture || EditorGUI.EndChangeCheck())
            {
                float sizeF = size;
                Color[] colors = new Color[size * size];
                for (int i = 0; i < size; i++)
                    for (int j = 0; j < size; j++)
                    {
                        float noiseX = j / sizeF;
                        float noiseY = i / sizeF;
                        float noise = m_Octave ? GetNoiseOctave(noiseX, noiseY, m_Scale, m_NoiseType, m_OctaveCount) : GetNoise(noiseX, noiseY, m_Scale, m_NoiseType);
                        if (noiseSampleSupported)
                        {
                            switch (m_NoiseSample)
                            {
                                case enum_NoiseSample.Absolute: noise = Mathf.Abs(noise); break;
                                case enum_NoiseSample._01: noise = noise / 2f + .5f; break;
                            }
                        }
                        colors[i * size + j] = new Color(noise, noise, noise, 1);
                    }
                m_Texture = new Texture2D(size, size, TextureFormat.ARGB32, true) { filterMode = m_FilterMode };
                m_Texture.SetPixels(colors);
                m_Texture.Apply();
                Undo.RecordObject(this, "Noise Generator Change");
            }
            HorizontalScope.NextLine(2, 256);
            Rect textureRect = HorizontalScope.NextRect(0, 256);
            GUI.DrawTexture(textureRect, EditorGUIUtility.whiteTexture);
            GUI.DrawTexture(textureRect.Collapse(Vector2.one*10f), m_Texture);
            HorizontalScope.NextLine(2, 20);
            if (GUI.Button(HorizontalScope.NextRect(0, 80), "Export"))
            {
                if (UEAsset.SaveFilePath(out string filePath, "png", "CustomNoise_" + m_NoiseType.ToString()))
                    UEAsset.CreateOrReplaceFile(filePath, m_Texture.EncodeToPNG());
            }
        }
    }
}