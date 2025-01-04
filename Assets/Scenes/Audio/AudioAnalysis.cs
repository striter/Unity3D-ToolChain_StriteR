using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Runtime.SignalProcessing;
using TPool;
using Unity.Mathematics;
using UnityEngine;
[RequireComponent(typeof(AudioSource))]
public class AudioAnalysis : MonoBehaviour
{
    public enum EVisualize
    {
        Output,
        Spectrum,
        Band,
    }

    public AudioSource m_Audio;
    public EVisualize m_Visualize = EVisualize.Spectrum;
    [MFoldout(nameof(m_Visualize),EVisualize.Spectrum)] public bool m_CustomFourier = false;
    private float[] m_VisualizeData;
    private Damper[] m_Dampers;
    private float[] m_SampleData;
    public FFTWindow m_Window;
    [Range(6,13)]public int m_SpectrumPow = 1;
    public float m_SpectrumMultiplier = 1000;
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

        var size =  umath.pow(2,m_SpectrumPow);
        var visualizeCount = m_SpectrumPow;
        switch (m_Visualize)
        {
            case EVisualize.Output:
            case EVisualize.Spectrum:
            {
                visualizeCount = size;
            }
                break;
        }
        m_SampleData = new float[size];
        m_VisualizeData = new float[visualizeCount];
        m_Dampers = new Damper[visualizeCount].Remake(p => m_Damper);
        m_TransformPool.Clear();
        for (var i = 0; i < visualizeCount; i++)
        {
            var rad = i * kmath.kPI2 / visualizeCount;
            umath.sincos_fast(rad, out var s, out var c);
            var visualizeTransform = m_TransformPool.Spawn();
            visualizeTransform.localPosition = (kfloat3.up * s + kfloat3.right * c) * 100f;
            visualizeTransform.localRotation = math.mul(quaternion.LookRotation(visualizeTransform.localPosition.normalized, kfloat3.up) , quaternion.Euler(-90f * kmath.kDeg2Rad,0f,0f));
        }
    }

    void Update()
    {
        switch (m_Visualize)
        {
            case EVisualize.Output:
                m_Audio.GetOutputData(m_SampleData,0);
                m_VisualizeData.Remake((index, srcValue) => m_SampleData[index]);
                break;
            case EVisualize.Spectrum:
                if (m_CustomFourier)
                {
                    m_Audio.GetOutputData(m_SampleData,0);
                    Fourier.DFT(m_SampleData.Remake((index,p)=>p * Audio.Hanning(index,m_SampleData.Length)),m_SampleData.Length).Select(p=> p.magnitude() ).FillArray(m_VisualizeData);
                }
                else
                {
                    m_Audio.GetSpectrumData(m_SampleData, 0,m_Window);
                    m_VisualizeData.Remake((index, srcValue) => m_SampleData[index]);
                }
                break;
            case EVisualize.Band:
                m_Audio.GetSpectrumData(m_SampleData,0,m_Window);
                var count = 0;
                float average = 0;
                for (var i = 0; i < m_SpectrumPow; i++)
                {
                    var sampleCount = (int)Mathf.Pow(2, i);
                    for(var j = 0; j < sampleCount; j++)
                    {
                        average += m_SampleData[count] * (count + 1);
                        count++;
                    }
                    average /= count;
                    m_VisualizeData[i] = average;
                }
                break;
        }
        
        var deltaTime = Time.deltaTime;
        for (var i = 0; i < m_TransformPool.Count; i++)
        {
            var data = m_VisualizeData[i] * m_SpectrumMultiplier;
            
            if(m_Dampers[i].value.x < data)
                m_Dampers[i].Initialize(data);
            m_TransformPool.Get(i).localScale = new Vector3(1f,m_Dampers[i].Tick(deltaTime, data),1f);
        }
        
    }
}
