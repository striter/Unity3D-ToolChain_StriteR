using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Object = System.Object;

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
    public struct NoiseTextureData
    {
        
        [Clamp(1e-6f,1024f)] public float scale;
        public ENoiseType noiseType;
        [Foldout(nameof(noiseType),ENoiseType.Simplex,ENoiseType.Perlin)]public ENoiseSample noiseSample;
        public bool octave;
        [Foldout(nameof(octave),true)] [Range( 2, 7)] public int octaveCount;
        public static readonly NoiseTextureData kDefault = new NoiseTextureData() {scale = 5f,noiseType = ENoiseType.Value,noiseSample = ENoiseSample._01,octave = false,octaveCount = 3};

    }
    
    [Serializable]
    public class FNoiseTextureGenerator : ITextureGenerator
    {
        public NoiseTextureData m_Data;
        private Texture2D m_Texture;
        public static FNoiseTextureGenerator kDefault = new() { m_Data = NoiseTextureData.kDefault };
        public void Setup(TextureGeneratorData _data) => m_Texture = _data.Texture2D(m_Texture);
        public bool Valid => true;

        void Apply()
        {
            var colors = m_Texture.GetPixelData<Color32>(0);
            var size = new int2(m_Texture.width, m_Texture.height);
            new NoiseTextureJob(){ input = m_Data,size=size,colors = colors}.ScheduleParallel(size.x*size.y,size.y,default).Complete();
            m_Texture.SetPixelData(colors,0);
            m_Texture.Apply();
            colors.Dispose();
        }
        public void Output()
        {
            Apply();
            if (UEAsset.SaveFilePath(out string filePath, "png", $"{m_Data.noiseType}_{m_Data.noiseSample}"))
                UEAsset.CreateOrReplaceFile<Texture2D>(filePath, m_Texture.EncodeToPNG());
        }
        public void Preview(Rect _rect)
        {
            Apply();
            EditorGUI.DrawPreviewTexture(_rect, m_Texture);
        }

        public void Dispose()
        {
            if(m_Texture != null)
                GameObject.DestroyImmediate(m_Texture);
            m_Texture = null;
        }
    }

    [BurstCompile(FloatPrecision.Standard,FloatMode.Fast,CompileSynchronously = true)]
    public struct NoiseTextureJob : IJobFor
    {
        public NoiseTextureData input;
        public NativeArray<Color32> colors;
        public int2 size;
        
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
            var u = index % size.x;
            var v = index / size.y;
            
            var noiseX = u / (float)size.x;
            var noiseY = v / (float)size.y;
            var noise = input.octave ? GetNoiseOctave(noiseX, noiseY, input.scale, input.noiseType, input.octaveCount) : GetNoise(noiseX, noiseY, input.scale, input.noiseType);
            if (NoiseSampleSupported())
            {
                noise = input.noiseSample switch
                {
                    ENoiseSample.Absolute => Mathf.Abs(noise),
                    ENoiseSample._01 => noise / 2f + .5f,
                    _ => noise
                };
            }

            var noiseByte = (byte)(noise * byte.MaxValue);
            colors[index] = new Color32(byte.MaxValue,noiseByte,noiseByte,noiseByte);       //ARGB
        }
    }
}