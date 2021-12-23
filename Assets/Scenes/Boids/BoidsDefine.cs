using System;
using UnityEngine;

namespace Boids
{
    public enum EBoidHovering
    {
        AroundHeight,
        Disorder,
    }

    [Serializable]
    public struct BoidsConfig
    {
        public float speed;
        public BoidFlockingConfig flockingConfig;
        public BoidHoveringConfig hoveringConfig;
        public bool evade;
        [MFoldout(nameof(evade),true)] public BoidEvadeConfig evadeConfig;

        public static readonly BoidsConfig Default = new BoidsConfig()
        {
            speed=3f,
            flockingConfig = new BoidFlockingConfig(){alignmentDamping = .1f,cohesionDamping = .1f,separateDamping = .3f,sqrSeparationDistance=2.25f,sqrVisualizeRange=4f},
            hoveringConfig = new BoidHoveringConfig(){damping = .4f,distance = 7f,height = 2f,type = EBoidHovering.AroundHeight},
            evadeConfig = new BoidEvadeConfig(){evadeDamping = 5f,evadeDistance = 20f}
        };
    }

    [Serializable]
    public struct BoidHoveringConfig
    {
        public float damping;
        public EBoidHovering type;
        public float distance;
        [MFoldout(nameof(type),EBoidHovering.AroundHeight)]public float height;
    }

    [Serializable]
    public struct BoidFlockingConfig
    {
        public float sqrVisualizeRange;
        public float sqrSeparationDistance;
        public float separateDamping;
        public float cohesionDamping;
        public float alignmentDamping;
    }

    [Serializable]
    public struct BoidEvadeConfig
    {
        public float evadeDistance;
        public float evadeDamping;
    }
}