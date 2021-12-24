using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Boids.Butterfly
{
    public enum EButterFlyBehaviour
    {
        Startle,
        Floating,
        
        Landing,
        Stopped,
    }

    
    [CreateAssetMenu(fileName = "Butterfly_Config",menuName = "Config/Boids/Butterfly")]
    public class FButterflyConfig:ScriptableObject
    {
        [Header("Base")]
        public BoidsStartleConfig startleConfig;
        public BoidsFloatingConfig floatingConfig;
        public BoidsLandingConfig landConfig;
        public BoidsIdleConfig idleConfig;
        [Header("Additional")]
        public BoidsFlockingConfig flockingConfig;
    }
}