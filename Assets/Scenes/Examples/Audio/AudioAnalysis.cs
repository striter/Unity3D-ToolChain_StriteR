using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(AudioSource))]
public class AudioAnalysis : MonoBehaviour
{
    public AudioSource m_Audio;
    public float m_Time;
    private const int kSize = 512;
    private readonly float[] m_SpectrumData = new float[kSize];
    private Transform[] m_Transforms = new Transform[kSize];
    private void Awake()
    {
        m_Audio = GetComponent<AudioSource>();
        m_Audio.time = m_Time;
        for (int i = 0; i < kSize; i++)
        {
            m_Transforms[i] = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            m_Transforms[i].SetParent(transform);
            m_Transforms[i].localPosition = new Vector3(((i-kSize/2)+.5f)*2f, 0f, 0f);
        }
            
    }

    void Update()
    {
        var frequency = m_Audio.clip.frequency;
        m_Audio.GetSpectrumData(m_SpectrumData,0,FFTWindow.BlackmanHarris);
        for (int i = 0; i < kSize; i++)
        {
            m_Transforms[i].localScale = new Vector3(1f,m_SpectrumData[i]*100f,1f);
        }
    }
}
