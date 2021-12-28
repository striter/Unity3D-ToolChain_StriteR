using UnityEngine;
namespace Boids.Bird
{
    public enum EBirdBehaviour
    {
        Invalid=-1,
        
        Startling=0,
        Flying=1,
        
        Hovering=2,
        TryLanding=3,
        Landing=4,
        Perching=5,
    }

    [CreateAssetMenu(fileName = "Bird Config",menuName = "Config/Boids/Bird")]
    public class FBirdConfig:ScriptableObject
    {
        [Header("Flying")]
        public BoidsStartleConfig startleConfig;
        public BoidsFlyingConfig flyingConfig;
        public BoidsHoveringConfig hoveringConfig;
        public BoidsTryLandingConfig tryLandingConfig;
        public BoidsHoverLandingConfig landConfig;
        [Header("_Additional")]
        public BoidsFlockingConfig flockingConfig;
        public BoidsEvadeConfig evadeConfig;

        [Header("Perching")]
        public BoidsPerchConfig perchConfig;
        [Header("_Additional")]
        public BoidsFlockingConfig perchFlocking;
    }
}