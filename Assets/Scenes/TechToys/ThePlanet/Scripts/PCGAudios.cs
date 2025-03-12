using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using TPool;
using TObjectPool;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Pool;

namespace TechToys.ThePlanet
{
    public static class KPCGAudios
    {
        public static readonly string kClick = "IClick";
        public static readonly string kClick2 = "IClick2";
        public static readonly string kPositive = "IPositive";
    }
    
    public class PCGAudios : MonoBehaviour,IPCGControl
    {
        public static PCGAudios Instance;
        public AudioClip[] m_Clips;
        private AudioSource m_BG;
        private ObjectPoolClass<int, AudioInstance> m_Pool;
        private float m_Time;
        
        private Counter m_AudioPitcher = new Counter(.3f);
        private int m_PitchCounter = 0;
        private string m_CurPitching;
 
        public void Init()
        {
            Instance = this;
            m_BG = transform.Find("BG").GetComponent<AudioSource>();
            m_Pool = new ObjectPoolClass<int, AudioInstance>(transform.Find("AudioElements/Item"));
        }

        public void Tick(float _deltaTime)
        {
            m_Time += _deltaTime;
            TSPoolList<int>.Spawn(out var recycleList);
            recycleList.AddRange(m_Pool.Collect(p=>p.m_EndTime < m_Time).Select(p=>p.identity));
            recycleList.Traversal(p=>m_Pool.Recycle(p));
            TSPoolList<int>.Recycle(recycleList);

            if (m_AudioPitcher.TickTrigger(_deltaTime))
                m_PitchCounter = 0;
        }

        public void Clear()
        {
            m_Time = 0f;
        }

        public void Dispose()
        {
            Instance = null;
        }

        public static void SetBGVolume(float _normalized)
        {
            Instance.m_BG.volume = math.lerp(.4f,0.2f,_normalized);
        }

        public void InternalPlay(string _name)
        {
            var clip = m_Clips.Find(p => p.name == _name);
            if (!clip)
                throw new Exception("Invalid clip name");

            if (m_AudioPitcher.Playing)
                m_PitchCounter++;
            if (m_CurPitching != _name)
            {
                m_PitchCounter = 0;
                m_CurPitching = _name;
            }

            m_AudioPitcher.Replay();
            
            m_Pool.Spawn().Play(clip,m_Time,umath.lerp(1f,2f,(m_PitchCounter/5f).saturate()));
        }
        public static void Play(string _name) => Instance.InternalPlay(_name);
        
        private class AudioInstance : APoolTransform<int>
        {
            public float m_EndTime { get; private set; }
            private AudioSource m_AudiosSource;
            public AudioInstance(Transform _transform):base(_transform)
            {
                m_AudiosSource = _transform.GetComponent<AudioSource>();
            }

            public void Play(AudioClip _clip,float _curTime,float _pitch)
            {
                m_AudiosSource.clip = _clip;
                m_AudiosSource.pitch = _pitch;
                m_AudiosSource.Play();
                m_EndTime = _curTime + _clip.length;
            }
        }
    }
}
