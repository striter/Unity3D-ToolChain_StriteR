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
        Perching=4,
    }

    [CreateAssetMenu(fileName = "Bird Config",menuName = "Config/Boids/Bird")]
    public class FBirdConfig:ScriptableObject
    {
        [Header("Flying")]
        public BoidsStartleConfig startleConfig;
        public BoidsFlyingConfig flyingConfig;
        public BoidsHoveringConfig hoveringConfig;
        public RangeFloat hoveringDuration;
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