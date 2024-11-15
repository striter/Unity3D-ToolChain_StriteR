using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Extensions;
using TPool;
using Unity.Mathematics;
using UnityEngine;
[RequireComponent(typeof(AudioSource))]
public class AudioAnalysis : MonoBehaviour
{
    public AudioSource m_Audio;
    public float m_Time;
    private float[] m_SpectrumData ;
    private Damper[] m_Dampers ;
    public FFTWindow m_Window;
    [Range(6,13)]public int m_SpectrumPow = 1;
    public float m_SpectrumMultiplier = 1000;
    public Damper m_Damp = Damper.kDefault;
    private ObjectPoolTransform m_TransformPool;
    private void Awake()
    {
        m_Audio = GetComponent<AudioSource>();
        m_Audio.time = m_Time;

        var root = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
        root.SetParent(transform);
        m_TransformPool = new ObjectPoolTransform(root.transform);
        OnValidate();
    }

    private void OnValidate()
    {
        if (m_TransformPool == null)
            return;
        var size = umath.pow(2,m_SpectrumPow);
        m_SpectrumData = new float[size];
        m_Dampers = new Damper[size].Remake(p => m_Damp);
        
        m_TransformPool.Clear();
        for (int i = 0; i < size; i++)
            m_TransformPool.Spawn().localPosition = new Vector3(((i-size/2f)+.5f)*2f, 0f, 0f);
    }

    void Update()
    {
        var deltaTime = Time.deltaTime;
        m_Audio.GetSpectrumData(m_SpectrumData,0,m_Window);
        for (int i = 0; i < m_TransformPool.Count; i++)
        {
            var data = m_SpectrumData[i] * m_SpectrumMultiplier;
            
            if(m_Dampers[i].value.x < data)
                m_Dampers[i].Initialize(data);
            m_TransformPool.Get(i).localScale = new Vector3(1f,m_Dampers[i].Tick(deltaTime, data),1f);
        }
    }
}
