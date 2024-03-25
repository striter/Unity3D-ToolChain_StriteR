using System;
using Runtime.Geometry;
using Unity.Mathematics;
using UnityEngine;

namespace CameraController.Component
{

    [Serializable]
    public class FControllerCollision
    {
        [Range(0, 1f)] public float m_CollisionMin = 0f;
        [Range(0,.5f)] public float m_CollisionForward = .1f;
        
        [Header("Collider")]
        [CullingMask] public int m_CullingMask = int.MinValue;
        [Range(0,.5f)] public float m_ColliderRadius = .1f;
        private RaycastHit m_RaycastHit;

        bool CalculateDistance(GRay ray,float distance,out float hitDistance)
        {
            hitDistance = default;
            if (m_CullingMask > 0 && Physics.SphereCast(ray, m_ColliderRadius, out var hit, distance, m_CullingMask))
            {
                hitDistance =  hit.distance;
                return true;
            }

            return false;
        }

        public float Evaluate(GRay ray, float distance) => CalculateDistance(ray, distance, out var hitDistance)
            ? math.max(m_CollisionMin, hitDistance - m_CollisionForward) : distance;
        
        public void DrawGizmos(GRay ray,float distance)
        {
            Gizmos.color = Color.blue;
            if (Physics.SphereCast(ray, m_ColliderRadius,out m_RaycastHit, distance, m_CullingMask))
            {
                distance = m_RaycastHit.distance - m_CollisionForward;
                Gizmos.color = Color.green;
            }
            Gizmos.DrawLine(ray.origin,ray.GetPoint(distance));
        }
    }

}