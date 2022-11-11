using System;
using Unity.Burst;
using Unity.Jobs;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityEditor.Extensions
{
    public enum ENoiseType
    {
        Value,
        Perlin,
        Simplex,
        VoronoiUnit,
        VoronoiDistance,
    }
        
    public enum ENoiseSample
    {
        Unit,
        _01,
        Absolute,
    }

    [Serializable]
    public struct NoiseTextureInput
    {
        [Clamp(2,13)] public int sizePower;
        public FilterMode filterMode;
        [Clamp(1e-6f,1024f)] public float scale;
        public ENoiseType noiseType;
        public ENoiseSample noiseSample;
        public bool octave;
        [MFoldout(nameof(octave),true)] [Range( 2, 7)] public int octaveCount;
        public static readonly NoiseTextureInput kDefault = new NoiseTextureInput() {sizePower = 9,filterMode = FilterMode.Bilinear,scale = 5f,noiseType = ENoiseType.Value,noiseSample = ENoiseSample._01,octave = false,octaveCount = 3};
        
        public static float GetNoiseOctave(float _x, float _y,float _scale, ENoiseType _noiseType, int _octaveCount)
        {
            float value = 0;
            float frequency = 1;
            float amplitude = .6f;
            for (int i = 0; i < _octaveCount; i++)
            {
                value += GetNoise(_x * frequency, _y * frequency, _scale, _noiseType) * amplitude;
                amplitude *= .5f;
                frequency *= 2;
            }
            return value;
        }

        public static float GetNoise(float _noiseX,float _noiseY,float _scale, ENoiseType _noiseType)
        {
            float noise = 0;
            _noiseX *= _scale;
            _noiseY *= _scale;
            switch (_noiseType)
            {
                case ENoiseType.Value: noise = Noise.Value.Unit1f2(Mathf.Floor(_noiseX) , Mathf.Floor( _noiseY) ); break;
                case ENoiseType.Perlin: noise = Noise.Perlin.Unit1f3( _noiseX ,_noiseY, 0); break;
                case ENoiseType.Simplex: noise = Noise.Simplex.Unit1f2(_noiseX , _noiseY ); break;
                case ENoiseType.VoronoiUnit: noise = Noise.Voronoi.Unit2f2(_noiseX, _noiseY).y; break;
                case ENoiseType.VoronoiDistance: noise = Noise.Voronoi.Unit2f2(_noiseX, _noiseY).x;break;
            }
            return noise;
        }

        static bool NoiseSampleSupported(ENoiseType _type)
        {
            switch (_type)
            {
                default:
                    return false;
                case ENoiseType.Simplex:
                case ENoiseType.Perlin:
                    return true;
            }
        }
        
        public Texture2D Output(Texture2D _texture)
        {
            int size = UMath.Pow(2, sizePower);
            
            if(_texture==null)
                _texture = new Texture2D(size, size, TextureFormat.ARGB32, true);
            if (_texture.width != size)
            {
                Object.DestroyImmediate(_texture);
                _texture = new Texture2D(size, size, TextureFormat.ARGB32, true);
            }
            
            
            Color[] colors = new Color[size * size];
            float sizeF = size;
            for (int i = 0; i < size; i++)
            for (int j = 0; j < size; j++)
            {
                float noiseX = j / sizeF;
                float noiseY = i / sizeF;
                float noise = octave ? GetNoiseOctave(noiseX, noiseY, scale, noiseType, octaveCount) : GetNoise(noiseX, noiseY, scale, noiseType);
                if (NoiseSampleSupported(noiseType))
                {
                    switch (noiseSample)
                    {
                        case ENoiseSample.Absolute: noise = Mathf.Abs(noise); break;
                        case ENoiseSample._01: noise = noise / 2f + .5f; break;
                    }
                }
                colors[i * size + j] = new Color(noise, noise, noise, 1);
            }

            _texture.filterMode = filterMode;
            _texture.SetPixels( colors);
            _texture.Apply();
            return _texture;
        }
    }
    
    public class EWNoiseTextureGenerator : EditorWindow
    {
        [SerializeField] public NoiseTextureInput m_Input = NoiseTextureInput.kDefault;
        private SerializedObject m_SerializedWindow;
        SerializedProperty m_InputProperty;
        void OnEnable()
        {
            m_SerializedWindow = new SerializedObject(this);
            m_InputProperty = m_SerializedWindow.FindProperty(nameof(m_Input));
            m_Texture = m_Input.Output(m_Texture);
        }
        void OnDisable()
        {
            m_InputProperty.Dispose();
        }

        Texture2D m_Texture;
        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(m_InputProperty);
            HorizontalScope.Begin(5,5,EditorGUI.GetPropertyHeight(m_InputProperty,true));
            
            if (EditorGUI.EndChangeCheck())
            {
                m_SerializedWindow.ApplyModifiedPropertiesWithoutUndo();
                m_Texture = m_Input.Output(m_Texture);
                Undo.RecordObject(this, "Noise Generator Change");
            }
            
            HorizontalScope.NextLine(0f,256);
            Rect textureRect = HorizontalScope.NextRect(0, 256);
            GUI.DrawTexture(textureRect, EditorGUIUtility.whiteTexture);
            GUI.DrawTexture(textureRect.Collapse(Vector2.one*10f), m_Texture);
            HorizontalScope.NextLine(2, 20);
            if (GUI.Button(HorizontalScope.NextRect(0, 80), "Export"))
            {
                if (UEAsset.SaveFilePath(out string filePath, "png", "m_NoiseType.ToString()"))
                    UEAsset.CreateOrReplaceFile(filePath, m_Texture.EncodeToPNG());
            }
        }
    }
}