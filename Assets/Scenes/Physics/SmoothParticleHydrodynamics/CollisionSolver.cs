using System.Collections.Generic;
using Unity.Mathematics;

namespace Examples.PhysicsScenes.SmoothParticleHydrodynamics
{
    public struct ColliderQueryResult
    {
        public float distance;
        public float3 point;
        public float3 normal;
        public float3 velocity;
    }
    public class CollisionSolver
    {
        public float m_RestitutionCoefficient;
        
        public void ResolveCollision(float _radius, List<float3> _positions, List<float3> velocities)
        {
            ColliderQueryResult colliderPoint;
        }

        void GetClosestPoint()
        {
            
        }
    }
}