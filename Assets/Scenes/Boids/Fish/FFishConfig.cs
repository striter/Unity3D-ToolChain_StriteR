using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Boids.Fish
{
    [Serializable]
    public struct FFishConfig
    {
        public BoidsHoveringConfig hovering;
        public BoidsFlockingConfig flocking;
    }
}
