using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rendering.PostProcess
{
    [ExecuteInEditMode]
    public class PostProcessGlobalVolume : MonoBehaviour
    {
        public static readonly List<PostProcessGlobalVolume> sVolumes = new List<PostProcessGlobalVolume>();

        public IPostProcessBehaviour[] m_PostProcesses { get; private set; }
        private void OnEnable()
        {
            m_PostProcesses = GetComponents<IPostProcessBehaviour>();
            sVolumes.Add(this);
        }

        private void OnDisable()
        {
            m_PostProcesses = null;
            sVolumes.Remove(this);
        }
    }

}