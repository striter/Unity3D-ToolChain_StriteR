using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rendering.PostProcess
{
    [ExecuteInEditMode]
    public class PostProcessGlobalVolume : MonoBehaviour
    {        
        public int m_Priority=0;
        public static List<PostProcessGlobalVolume> kVolumes { get; private set; }= new List<PostProcessGlobalVolume>();
        private void OnValidate()
        {
            Sort();
        }

        private void OnEnable()
        {
            kVolumes.Add(this);
            Sort();
        }

        private void OnDisable()
        {
            kVolumes.Remove(this);
        }

        static void Sort()
        {
            kVolumes.Sort((a,b)=>b.m_Priority-a.m_Priority);
        }
    }

}