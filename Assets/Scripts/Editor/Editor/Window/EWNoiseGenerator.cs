using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
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
        [Clamp(1e-6f,1024f)] public float scale;
        public FilterMode filterMode;
        public ENoiseType noiseType;
        public ENoiseSample noiseSample;
        public bool octave;
        [MFoldout(nameof(octave),true)] [Range( 2, 7)] public int octaveCount;
        public static readonly NoiseTextureInput kDefault = new NoiseTextureInput() {sizePower = 9,filterMode = FilterMode.Bilinear,scale = 5f,noiseType = ENoiseType.Value,noiseSample = ENoiseSample._01,octave = false,octaveCount = 3};

        public Texture2D Output(Texture2D _texture)
        {
            int size = umath.pow(2, sizePower);

            if (_texture != null && _texture.width != size)
            {
                Object.DestroyImmediate(_texture);
                _texture = null;
            }
            if(_texture==null)
                _texture = new Texture2D(size, size, TextureFormat.ARGB32, 0,true);

            var colors = _texture.GetPixelData<Color32>(0);
            new NoiseTextureJob(){ input = this,size=size,colors = colors}.ScheduleParallel(size*size,size,default).Complete();

            _texture.filterMode = filterMode;
            _texture.SetPixelData(colors,0);
            _texture.Apply();
            colors.Dispose();
            return _texture;
        }
    }

    [BurstCompile(FloatPrecision.Standard,FloatMode.Fast,CompileSynchronously = true)]
    public struct NoiseTextureJob : IJobFor
    {
        public NoiseTextureInput input;
        public NativeArray<Color32> colors;
        public int size;
        
        float GetNoiseOctave(float _x, float _y,float _scale, ENoiseType _noiseType, int _octaveCount)
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

        float GetNoise(float _noiseX,float _noiseY,float _scale, ENoiseType _noiseType)
        {
            float noise = 0;
            _noiseX *= _scale;
            _noiseY *= _scale;
            switch (_noiseType)
            {
                case ENoiseType.Value: noise = Noise.Value.Unit1f2(Mathf.Floor(_noiseX) , Mathf.Floor( _noiseY) ); break;
                case ENoiseType.Perlin: noise = Noise.Perlin.Unit1f3( _noiseX ,_noiseY, 0); break;
                case ENoiseType.Simplex: noise = Noise.Simplex.Unit1f2(_noiseX - .5f , _noiseY - .5f); break;
                case ENoiseType.VoronoiUnit: noise = Noise.Voronoi.Unit2f2(_noiseX - .5f, _noiseY - .5f).y; break;
                case ENoiseType.VoronoiDistance: noise = Noise.Voronoi.Unit2f2(_noiseX - .5f, _noiseY - .5f).x;break;
            }
            return noise;
        }

        bool NoiseSampleSupported()
        {
            switch (input.noiseType)
            {
                default:
                    return false;
                case ENoiseType.Simplex:
                case ENoiseType.Perlin:
                    return true;
            }
        }

        public void Execute(int index)
        {
            int u = index % size;
            int v = index / size;
            
            float sizeF = size;
            float noiseX = u / sizeF;
            float noiseY = v / sizeF;
            float noise = input.octave ? GetNoiseOctave(noiseX, noiseY, input.scale, input.noiseType, input.octaveCount) : GetNoise(noiseX, noiseY, input.scale, input.noiseType);
            if (NoiseSampleSupported())
            {
                switch (input.noiseSample)
                {
                    case ENoiseSample.Absolute: noise = Mathf.Abs(noise); break;
                    case ENoiseSample._01: noise = noise / 2f + .5f; break;
                }
            }

            var noiseByte = (byte)(noise * byte.MaxValue);
            colors[index] = new Color32(byte.MaxValue,noiseByte,noiseByte,noiseByte);       //ARGB
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
            HorizontalScope.Begin(5,5,EditorGUI.GetPropertyHeight(m_InputProperty,true),Screen.width);
            
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
                if (UEAsset.SaveFilePath(out string filePath, "png", m_Input.noiseType.ToString()))
                    UEAsset.CreateOrReplaceFile<Texture2D>(filePath, m_Texture.EncodeToPNG());
            }
        }
    }
}