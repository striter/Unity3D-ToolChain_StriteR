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
        private void OnEnable()
        {
            sVolumes.Add(this);
        }

        private void OnDisable()
        {
            sVolumes.Remove(this);
        }
    }

}