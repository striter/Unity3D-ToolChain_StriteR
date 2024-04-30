using System;
using Runtime.Geometry;
using UnityEngine;

namespace CameraController.Component
{

    [Serializable]
    public class FControllerCollision
    {
        [Range(0, 2f)] public float m_CollisionMin = 0f;
        [Range(0,.5f)] public float m_CollisionForward = .1f;

        [Header("Collider")] 
        [CullingMask] public int m_CullingMask = int.MinValue;
        [Range(0,.5f)] public float m_ColliderRadius = .1f;

        [Header("Debug")]
        private RaycastHit m_RaycastHit;

        bool CalculateDistance(GRay ray,float distance,out float hitDistance)
        {
            hitDistance = default;
            if (m_CullingMask != 0 && Physics.SphereCast(ray, m_ColliderRadius, out var hit, distance, m_CullingMask))
            {
                hitDistance =  hit.distance;
                return true;
            }

            return false;
        }

        public float Evaluate(Camera _camera, GRay ray, float distance)
        {
            return CalculateDistance(ray.Forward(m_CollisionMin), distance - m_CollisionMin, out var hitDistance) ? hitDistance : distance;
        }
        public void DrawGizmos(Camera _camera, GRay ray,float distance)
        {
            ray = ray.Forward(m_CollisionMin);
            if (m_CullingMask != 0 && Physics.SphereCast(ray, m_ColliderRadius,out m_RaycastHit, distance, m_CullingMask))
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(ray.origin,ray.GetPoint(m_RaycastHit.distance - m_CollisionForward));
            } 
            
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(ray.origin,ray.GetPoint(distance));
        }
    }

}