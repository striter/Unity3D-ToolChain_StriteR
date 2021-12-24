using UnityEngine;
namespace Boids.Bird
{
    public enum EBirdBehaviour
    {
        Invalid=-1,
        
        Startling=0,
        Flying=1,
        
        Hovering=2,
        Landing=3,
        Perching=5,
    }

    [CreateAssetMenu(fileName = "Bird Config",menuName = "Config/Boids/Bird")]
    public class FBirdConfig:ScriptableObject
    {
        [Header("Base")]
        public BoidsStartleConfig startleConfig;
        public BoidsFlyingConfig flyingConfig;
        public BoidsHoveringConfig hoveringConfig;
        public BoidsLandingConfig landConfig;
        public BoidsPerchConfig perchConfig;
        
        [Header("Additional")]
        public BoidsFlockingConfig flockingConfig;
        public BoidsEvadeConfig evadeConfig;
    }
}