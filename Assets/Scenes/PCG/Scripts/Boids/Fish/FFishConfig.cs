using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PCG.Module.BOIDS.Fish
{
    [Serializable]
    public struct FFishConfig
    {
        public BoidsFloatingConfig floating;
        public BoidsFlockingConfig flocking;
    }
}
