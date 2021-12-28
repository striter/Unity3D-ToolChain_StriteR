using System;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace Boids
{
    [Serializable]
    public struct BoidsStartleConfig
    {
        public float speed;
        public RangeFloat reaction;
        public RangeFloat duration;
        public float damping;
        public string animName;
    }

    [Serializable]
    public struct BoidsFlyingConfig
    {
        public float speed;
        public float maintainHeight;
        public float heightDamping;
        public float boderRange;
        public float borderDamping;
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
    public struct BoidsHoverLandingConfig
    {
        public float speed;
        public float damping;
        public string anim;
    }

    [Serializable]
    public struct BoidsLandingConfig
    {
        public RangeFloat duration;
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
        [Header("Move")] 
        public float moveSpeed;
        public string moveAnim;
        public RangeFloat moveDuration;

        [Header("Rotate")]
        public RangeFloat rotateCooldown;
        public RangeFloat rotateSpeed;
        public RangeFloat rotateDuration;
        
        public RangeFloat alertDuration;
        public RangeFloat idleDuration;
        public RangeFloat relaxDuration;
        [Range(0f,1f)]public float relaxedMovingPossibility;
        public string alertAnim;
        public string idleAnim;
        public string relaxAnim;
    }
    [Serializable]
    public struct BoidsIdleConfig
    {
        public string anim;
    }
}