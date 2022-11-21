using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace PCG.Module.BOIDS
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
        public Vector3 origin;
        public float speed;
        public float height;
        public float heightDamping;
        public RangeFloat tiringDuration;
        public string flyAnim;
        public string glideAnim;
        
    #if UNITY_EDITOR
        public void DrawGizmos()
        {
            Handles.matrix=Matrix4x4.identity;
            Handles_Extend.DrawWireSphere(origin,Vector3.up,height);
        }
    #endif
    }
    

    public enum EBoidsFloatingConstrain
    {
        Spherical,
        Box,
    }
    
    [Serializable]
    public struct BoidsFloatingConfig
    {
        public EBoidsFloatingConstrain constrain;
        public float speed;
        public float damping;
        [MFoldout(nameof(constrain),EBoidsFloatingConstrain.Spherical)]public float sqrRadius;
        [MFoldout(nameof(constrain),EBoidsFloatingConstrain.Box)] public Vector3 boxSize;
        public string anim;
    }
    
    
    [Serializable]
    public struct BoidsPreLandingConfig
    {
        public float speed;
        public float damping;
        public float distance;
        public float height;
        public string anim;
    }
    
    [Serializable]
    public struct BoidsLandingConfig
    {
        public float speed;
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
    public struct BoidsFollowingConfig
    {
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
        [Header("Idle")]
        public RangeFloat idleDuration;
        public string idleAnim;
        
        [Header("Relax")]
        public RangeFloat relaxDuration;
        public string relaxAnim;
        
        [Header("Move")] 
        public RangeFloat moveDuration;
        public string moveAnim;

        [Header("Alert")]
        public RangeFloat alertDuration;
        [Range(0f,1f)]public float alertMovePossibility;
        public string alertAnim;
        
        [Header("Rotate")]
        public RangeFloat rotateCooldown;
        public RangeFloat rotateSpeed;
        public RangeFloat rotateDuration;
    }
    [Serializable]
    public struct BoidsIdleConfig
    {
        public string anim;
    }
}