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
        public static bool HasGlobal => sVolumes.Count > 0;
        public static PostProcessGlobalVolume GlobalVolume => sVolumes[0];
        private static readonly List<PostProcessGlobalVolume> sVolumes = new List<PostProcessGlobalVolume>();

        private void OnValidate()
        {
            Sort();
        }

        private void OnEnable()
        {
            sVolumes.Add(this);
            Sort();
        }

        private void OnDisable()
        {
            sVolumes.Remove(this);
        }

        static void Sort()
        {
            sVolumes.Sort((a,b)=>b.m_Priority-a.m_Priority);
        }
    }

}