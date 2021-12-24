using System;
using UnityEngine;

namespace Boids
{
    [Serializable]
    public struct BoidsStartleConfig
    {
        public float speed;
        public RangeFloat duration;
        public float damping;
        public string animName;
    }

    [Serializable]
    public struct BoidsFlyingConfig
    {
        public float speed;
        public float damping;
        public float maintainHeight;
        public float sqrBorder;
        public RangeFloat tiringDuration;
        public string flyAnim;
        public string glideAnim;
    }
    
    [Serializable]
    public struct BoidsHoveringConfig
    {
        public float speed;
        public RangeFloat duration;
        public float damping;
        public float distance;
        public float height;
        public string flyAnim;
        public string glideAnim;
    }
    
    [Serializable]
    public struct BoidsFloatingConfig
    {
        public float speed;
        public float damping;
        public float sqrRadius;
        public string anim;
    }
    
    [Serializable]
    public struct BoidsLandingConfig
    {
        public float speed;
        public float distanceBias;
        public float damping;
        public string anim;
    }
    
    [Serializable]
    public struct BoidsFlockingConfig
    {
        public float sqrVisualizeRange;
        public float sqrSeparationDistance;
        public float separateDamping;
        public float cohesionDamping;
        public float alignmentDamping;
    }

    [Serializable]
    public struct BoidsEvadeConfig
    {
        public float evadeDistance;
        public float evadeDamping;
    }

    [Serializable]
    public struct BoidsPerchConfig
    {
        public string moveAnim;
        public string standAnim;
        public string idleAnim;
        public string stopAnim;
    }
    [Serializable]
    public struct BoidsIdleConfig
    {
        public string anim;
    }
}