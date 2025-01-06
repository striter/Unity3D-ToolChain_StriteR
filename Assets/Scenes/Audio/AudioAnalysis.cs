using System;
using System.Collections.Generic;
using System.Linq.Extensions;
using Runtime.Geometry;
using Runtime.Geometry.Curves.Spline;
using TPool;
using Unity.Mathematics;
using UnityEngine;
[RequireComponent(typeof(AudioSource))]
public class AudioAnalysis : MonoBehaviour
{
    public AudioSource m_Audio;
    public EAnalysis m_Analysis = EAnalysis.Spectrum;
    [MFoldout(nameof(m_Analysis), EAnalysis.Output)] public FOutputAnalysis m_OutputAnalysis = new();
    [MFoldout(nameof(m_Analysis), EAnalysis.Spectrum)] public FSpectrumAnalysis m_SpectrumAnalysis = new();
    [MFoldout(nameof(m_Analysis), EAnalysis.Band)] public FBandAnalysis m_BandAnalysis = new();
    private IAudioAnalysis AnalysisCore => m_Analysis switch {
        EAnalysis.Output => m_OutputAnalysis,
        EAnalysis.Spectrum => m_SpectrumAnalysis,
        EAnalysis.Band => m_BandAnalysis,
        _ => throw new ArgumentOutOfRangeException(nameof(m_Analysis), m_Analysis, null)
    };
    
    
    public EVisualize m_Visualize = EVisualize.Line;
    [MFoldout(nameof(m_Visualize), EVisualize.Line)] public FVisualizeLine m_LineVisualize = new(); 
    [MFoldout(nameof(m_Visualize), EVisualize.Circle)] public FVisualizeCircle m_CircleVisualize = new();
    private IAudioVisualize VisualizeCore => m_Visualize switch {
        EVisualize.Line => m_LineVisualize,
        EVisualize.Circle => m_CircleVisualize,
        _ => throw new ArgumentOutOfRangeException(nameof(m_Visualize), m_Visualize, null)
    };
    
    public Damper m_Damper = Damper.kDefault;
    private ObjectPoolTransform m_TransformPool;
    
    private void Awake()
    {
        m_Audio = GetComponent<AudioSource>();

        m_TransformPool = new ObjectPoolTransform(transform.Find("Element"));
        OnValidate();
    }

    private void OnValidate()
    {
        if (m_TransformPool == null)
            return;

        var size = AnalysisCore.Initialize(m_Audio);
        m_TransformPool.Clear();
        VisualizeCore.Init(m_TransformPool, size,m_Damper);
    }

    void Update()
    {
        var deltaTime = Time.deltaTime;
        var elements = AnalysisCore.Tick(m_Audio, deltaTime);
        VisualizeCore.Tick(deltaTime,m_TransformPool, elements);
    }
    
    public enum EAnalysis
    {
        Output,
        Spectrum,
        Band,
    }
    private interface IAudioAnalysis
    {
        public int Initialize(AudioSource _source);
        public IEnumerable<float2> Tick(AudioSource _source,float _deltaTime);
    }

    [Serializable]
    public class FOutputAnalysis : IAudioAnalysis
    {
        [Range(6,13)]public int m_SpectrumPow = 8;
        public float m_OutputMultiplier = 100;
        private float[] m_SampleData;
        public int Initialize(AudioSource _source)
        {
            var size = umath.pow(2,m_SpectrumPow);
            m_SampleData = new float[size];
            return size;
        }

        public IEnumerable<float2> Tick(AudioSource _source, float _deltaTime)
        {
            if(_source.volume == 0)
                yield break;
            
            _source.GetOutputData(m_SampleData, 0);
            foreach (var value in m_SampleData)
                yield return new float2(value * m_OutputMultiplier / _source.volume,value);
        }
    }

    [Serializable]
    public class FSpectrumAnalysis : IAudioAnalysis
    {
        public FFTWindow m_Window = FFTWindow.BlackmanHarris;
        [Range(6,13)]public int m_SpectrumPow = 8;
        public float m_SpectrumMultiplier = 100;

        private float[] m_SampleData;
        public int Initialize(AudioSource _source)
        {
            var size = umath.pow(2,m_SpectrumPow);
            m_SampleData = new float[size];
            return size;
        }

        public IEnumerable<float2> Tick(AudioSource _source, float _deltaTime)
        {
            if(_source.volume == 0)
                yield break;
            
            _source.GetSpectrumData(m_SampleData, 0,m_Window);
            foreach (var value in m_SampleData)
                yield return new float2(value * m_SpectrumMultiplier * m_SpectrumPow / _source.volume,value) ;
        }
    }

    [Serializable]
    public class FBandAnalysis : IAudioAnalysis
    {
        public FFTWindow m_Window = FFTWindow.BlackmanHarris;
        [Range(6,13)]public int m_SpectrumPow = 8;

        public float m_ValueMultiplier = 10f;
        private float[] m_SampleData;
        public int Initialize(AudioSource _source)
        {
            var size = umath.pow(2,m_SpectrumPow);
            m_SampleData = new float[size];
            return m_SpectrumPow;
        }

        public IEnumerable<float2> Tick(AudioSource _source, float _deltaTime)
        {
            _source.GetSpectrumData(m_SampleData,0,m_Window);
            float average = 0;
            for (var i = 0; i < m_SpectrumPow; i++)
            {
                var startIndex = i == 0 ? 0 : (int)Mathf.Pow(2, i);
                var max = 0f;
                var endIndex = (int)Mathf.Pow(2, i + 1);
                for(var j = startIndex; j < endIndex; j++)
                {
                    var sample = m_SampleData[j];
                    average += sample;
                    max = math.max(max,sample);
                }
                average /= (endIndex - startIndex);
                yield return new float2(average * m_ValueMultiplier,max);
            }
        }
    }

    public enum EVisualize
    {
        Line,
        Circle,
        Spline,
    }

    public interface IAudioVisualize
    {
        void Init(ObjectPoolTransform _elements,int _size,Damper _damperTemplate);
        void Tick(float _deltaTime,ObjectPoolTransform _elements,IEnumerable<float2> _values);
    }

    [Serializable]
    public class FVisualizeLine : IAudioVisualize
    {
        public float m_Width = 100f;
        private Damper[] m_Dampers;
        public void Init(ObjectPoolTransform _elements, int _size, Damper _damperTemplate)
        {
            m_Dampers = new Damper[_size].Remake(_ => _damperTemplate);
            for(var i = 0; i < _size; i++)
                 _elements.Spawn();
        }

        public void Tick(float _deltaTime,ObjectPoolTransform _elements, IEnumerable<float2> _values)
        {
            var size = _elements.Count;
            foreach (var (i,value) in _values.LoopIndex())
            {
                var output = value.x;
                if(m_Dampers[i].value.x < output)
                    m_Dampers[i].Initialize(output);
                
                var visualizeTransform = _elements.Get(i);
                var dampedValue = m_Dampers[i].Tick(_deltaTime, output);
                var indexNormalized = i / (float)size - .5f;
                visualizeTransform.localPosition = indexNormalized * kfloat3.right * m_Width + kfloat3.up * dampedValue / 2;
                visualizeTransform.localScale = new Vector3(1f,m_Dampers[i].Tick(_deltaTime, output),1f);
                visualizeTransform.localRotation = quaternion.identity;
            }
        }
    }

    [Serializable]
    public class FVisualizeCircle : IAudioVisualize
    {
        public float m_Radius = 100f;
        [Range(0f, 1f)] public float m_Offset = 0f;
        private Damper[] m_Dampers;
        public void Init(ObjectPoolTransform _elements, int _size,Damper _damperTemplate)
        {
            m_Dampers = new Damper[_size].Remake(p => _damperTemplate);
            for (var i = 0; i < _size; i++)
                 _elements.Spawn();
        }

        public void Tick(float _deltaTime,ObjectPoolTransform _elements, IEnumerable<float2> _values)
        {
            var size = _elements.Count;
            foreach (var (i,value) in _values.LoopIndex())
            {
                var output = value.x;
                if(m_Dampers[i].value.x < output)
                    m_Dampers[i].Initialize(output);
                var dampedValue = m_Dampers[i].Tick(_deltaTime, output);
                
                var indexNormalized = i / (float)size + m_Offset;
                var visualizeTransform = _elements.Get(i);
                
                umath.sincos_fast(indexNormalized * kmath.kPI2, out var s, out var c);
                var direction = kfloat3.up * s + kfloat3.right * c;
                visualizeTransform.localPosition = direction * m_Radius + (kfloat3.up * -s + kfloat3.right * -c )* dampedValue / 2;
                visualizeTransform.localRotation = math.mul(quaternion.LookRotation(visualizeTransform.localPosition.normalized, kfloat3.up) , quaternion.Euler(
                    -90f * kmath.kDeg2Rad, 0f, 0f));
                visualizeTransform.localScale = new Vector3(1f,dampedValue,1f);
            }
        }
    }

}
