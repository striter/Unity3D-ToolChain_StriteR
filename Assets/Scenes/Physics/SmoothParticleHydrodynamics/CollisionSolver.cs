using System;
using System.Collections.Generic;
using Runtime.Geometry;
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
    [Serializable]
    public struct CollisionSolver
    {
        public IList<ISurface> m_Surfaces;
        public float m_RestitutionCoefficient;
        // public static readonly CollisionSolverPlane kDefault = new() { m_Surfaces = GMesh.kDefault, m_RestitutionCoefficient = .1f };
        
        public void ResolveCollision(float _radius, List<float3> _positions, List<float3> velocities)
        {
            ColliderQueryResult colliderPoint;
        }

        ColliderQueryResult GetClosestPoint(GPlane _plane,IList<float3> _positions,IList<float3> _velocities)
        {
            ColliderQueryResult result = default;
            var maxDistance = float.MaxValue;
            for (var i = _positions.Count - 1; i-- >= 0;)
            {
                var point = _positions[i];
                var distance = _plane.SDF(point);
                if (distance >= maxDistance)
                    continue;
                result.velocity = _velocities[i];
                // result.normal = 
            }

            return result;
        }
    }
}