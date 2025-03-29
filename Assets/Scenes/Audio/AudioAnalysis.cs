using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using System.Numerics;
using Runtime.Geometry;
using Runtime.Geometry.Curves.Spline;
using Runtime.SignalProcessing;
using TPool;
using Unity.Mathematics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

[RequireComponent(typeof(AudioSource))]
public class AudioAnalysis : MonoBehaviour
{
    public AudioSource m_Audio;
    public EAnalysis m_Analysis = EAnalysis.Spectrum;
    [Foldout(nameof(m_Analysis), EAnalysis.Output)] public FOutputAnalysis m_OutputAnalysis = new();
    [Foldout(nameof(m_Analysis), EAnalysis.Spectrum)] public FSpectrumAnalysis m_SpectrumAnalysis = new();
    [Foldout(nameof(m_Analysis), EAnalysis.CustomSpectrum)] public FSpectrumAnalysisHanning m_CustomSpectrumAnalysis = new();
    [Foldout(nameof(m_Analysis), EAnalysis.Band)] public FBandAnalysis m_BandAnalysis = new();
    private IAudioAnalysis AnalysisCore => m_Analysis switch {
        EAnalysis.Output => m_OutputAnalysis,
        EAnalysis.Spectrum => m_SpectrumAnalysis,
        EAnalysis.CustomSpectrum => m_CustomSpectrumAnalysis,
        EAnalysis.Band => m_BandAnalysis,
        _ => throw new ArgumentOutOfRangeException(nameof(m_Analysis), m_Analysis, null)
    };
    
    
    public EVisualize m_Visualize = EVisualize.Line;
    [Foldout(nameof(m_Visualize), EVisualize.Line)] public FVisualizeLine m_LineVisualize = new(); 
    [Foldout(nameof(m_Visualize), EVisualize.Circle)] public FVisualizeCircle m_CircleVisualize = new();
    private IAudioVisualize VisualizeCore => m_Visualize switch {
        EVisualize.Line => m_LineVisualize,
        EVisualize.Circle => m_CircleVisualize,
        _ => throw new ArgumentOutOfRangeException(nameof(m_Visualize), m_Visualize, null)
    };
    
    private ObjectPoolTransform m_TransformPool;
    
    private void Awake()
    {
        m_Audio = GetComponent<AudioSource>();

        m_TransformPool = new ObjectPoolTransform(transform.Find("Element"));
        if(RenderSettings.skybox)
            RenderSettings.skybox = new Material(RenderSettings.skybox){hideFlags = HideFlags.HideAndDontSave};
        OnValidate();
    }

    private void OnDestroy()
    {
        if(RenderSettings.skybox)
            GameObject.Destroy(RenderSettings.skybox);
    }

    private void OnValidate()
    {
        if (m_TransformPool == null)
            return;

        var size = AnalysisCore.Initialize(m_Audio);
        m_TransformPool.Clear();
        VisualizeCore.Init(m_TransformPool, size);
    }

    void Update()
    {
        var deltaTime = Time.deltaTime;
        var elements = AnalysisCore.Tick(m_Audio, deltaTime);
        VisualizeCore.Tick(deltaTime,m_TransformPool, elements);
    }

    [InspectorButton]
    public void Play(float _normalizedTime = 0f)
    {
        if (m_Audio == null)
            return;
        m_Audio.time = _normalizedTime * m_Audio.clip.length;
    }
    
    public enum EAnalysis
    {
        Output,
        Spectrum,
        CustomSpectrum,
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

        private float[] m_SpecturmSample;
        public int Initialize(AudioSource _source)
        {
            var size = umath.pow(2,m_SpectrumPow);
            m_SpecturmSample = new float[size];
            return size;
        }

        public IEnumerable<float2> Tick(AudioSource _source, float _deltaTime)
        {
            if(_source.volume == 0)
                yield break;

            _source.GetSpectrumData(m_SpecturmSample, 0,m_Window);
            foreach (var value in m_SpecturmSample)
                yield return new float2(value  * m_SpectrumPow / _source.volume,value) ;
        }
    }

    //https://github.com/mduff1/Fourier/blob/main/Assets/Scripts/AudioAnalyzer.cs
    [Serializable]
    public class FSpectrumAnalysisHanning : IAudioAnalysis
    {
        [Range(6,12)]public int m_SpectrumPow = 8;
        private float[] m_OutputSample;
        public bool m_DFT = false;
        private cfloat2[] m_FrequencySample;  public int Initialize(AudioSource _source)
        {
            var size = umath.pow(2,m_SpectrumPow);
            m_OutputSample = new float[size * 2];
            m_FrequencySample = new cfloat2[size * 2];
            return size;
        }

        public IEnumerable<float2> Tick(AudioSource _source, float _deltaTime)
        {
            if(_source.volume == 0)
                yield break;

            var N = m_OutputSample.Length;
            _source.GetOutputData(m_OutputSample, 0);
            for(var i = 0; i < N; i++)
                m_FrequencySample[i] = new cfloat2(m_OutputSample[i] * UAudio.Hanning(i,N), 0);

            if (m_DFT)
                Fourier.DFT(m_FrequencySample,N).FillList(UList.Empty<cfloat2>()).FillArray(m_FrequencySample);
            else
                Fourier.FFT(m_FrequencySample);
            
            for (var i = 0; i < N/2; i++)
            {
                var output = 0f;
                if (i == 0)
                    output = (m_FrequencySample[i].abs() + m_FrequencySample[i + 1].abs());
                else if (i == N - 1)
                    output = (m_FrequencySample[i].abs() + m_FrequencySample[i - 1].abs());
                else
                    output = (m_FrequencySample[i - 1].abs() + m_FrequencySample[i].abs() + m_FrequencySample[i + 1].abs());

                output /= N;
                output *= m_SpectrumPow;
                output /= _source.volume;
                yield return output;
            }
        }
        
    }
    
    [Serializable]
    public class FBandAnalysis : IAudioAnalysis
    {
        public FFTWindow m_Window = FFTWindow.BlackmanHarris;
        [Range(6,13)]public int m_SpectrumPow = 8;

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
                int sampleCount = (int)Mathf.Pow(2, i)*2;
                int count = 0;
                for (int j=0; j<sampleCount; j++) {
 
                    average += m_SampleData[count]* ( count+1);
                    count++;
                }
 
                average /= count;
                yield return new float2(average,average);
            }
        }
    }

    public enum EVisualize
    {
        Line,
        Circle,
    }

    public interface IAudioVisualize
    {
        void Init(ObjectPoolTransform _elements,int _size);
        void Tick(float _deltaTime,ObjectPoolTransform _elements,IEnumerable<float2> _values);
    }

    [Serializable]
    public class FVisualizeLine : IAudioVisualize
    {
        public float m_Width = 100f;
        private Damper[] m_Dampers;
        public Damper m_Damper = Damper.kDefault;
        public void Init(ObjectPoolTransform _elements, int _size)
        {
            m_Dampers = new Damper[_size].Remake(_ => m_Damper);
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

        [Range(0,1)]public float m_ValueMultiplier = 0.5f;
        public ColorPalette m_ColorPalette = ColorPalette.kDefault;
        public Damper m_Damper = Damper.kDefault;
        public Damper m_ColorDamper = Damper.kDefault;
        
        private Damper[] m_Dampers;
        public void Init(ObjectPoolTransform _elements, int _size)
        {
            m_Dampers = new Damper[_size].Remake(p => m_Damper);
            for (var i = 0; i < _size; i++)
                 _elements.Spawn().GetComponentInChildren<Renderer>().material.color = m_ColorPalette.Evaluate(i / (float)_size);
        }

        public void Tick(float _deltaTime,ObjectPoolTransform _elements, IEnumerable<float2> _values)
        {
            var size = _elements.Count;
            var highest = float.MinValue;
            foreach (var (i,value) in _values.LoopIndex())
            {
                var output = value.x;
                highest = math.max(highest, output);

                if(m_Dampers[i].value.x < output)
                    m_Dampers[i].Initialize(output);
                
                var indexNormalized = i / (float)size + m_Offset;
                var visualizeTransform = _elements.Get(i);
                
                umath.sincos_fast(indexNormalized * kmath.kPI2, out var s, out var c);
                var direction = kfloat3.up * s + kfloat3.right * c;
                var dampedValue = m_Dampers[i].Tick(_deltaTime, output * m_Radius * m_ValueMultiplier);
                visualizeTransform.localPosition = direction * m_Radius + (kfloat3.up * -s + kfloat3.right * -c ) * dampedValue / 2;
                visualizeTransform.localRotation = math.mul(quaternion.LookRotation(visualizeTransform.localPosition.normalized, kfloat3.up) , quaternion.Euler(
                    -90f * kmath.kDeg2Rad, 0f, 0f));
                visualizeTransform.localScale = new Vector3(1f,dampedValue,1f);
            }

            var colorValue = m_ColorDamper.Tick(_deltaTime, highest);
            for (var i = 0; i < _elements.Count; i++)
            {
                var visualizeTransform = _elements.Get(i);
                visualizeTransform.GetComponentInChildren<Renderer>().material.color = m_ColorPalette.Evaluate((colorValue + i / (float)size) % 1f).SetA(colorValue);
            }
            
            RenderSettings.skybox.SetFloat("_Exposure",m_ColorDamper.value.x);
        }
    }

}
