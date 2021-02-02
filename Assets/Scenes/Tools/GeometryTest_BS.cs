using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TPhysics;
namespace BoundingCollisionTest
{
    public class GeometryTest_BS : MonoBehaviour
    {
        public Vector3 m_RayOrigin = Vector3.up * 5;
        public Vector3 m_RayDirection = Vector3.down;
        public Vector3 m_BoundingSphereOrigin = Vector3.zero;
        public float m_BoundingSphereRadius = 2f;

        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Vector3 direction = m_RayDirection.normalized;
            bool intersect = Physics_Extend.BSRayIntersect(m_BoundingSphereOrigin, m_BoundingSphereRadius, m_RayOrigin, direction);
            Gizmos.color = intersect ? Color.green : Color.grey;
            Gizmos.DrawWireSphere(m_BoundingSphereOrigin, m_BoundingSphereRadius);
            Vector2 distances = Physics_Extend.BSRayDistance(m_BoundingSphereOrigin, m_BoundingSphereRadius, m_RayOrigin, direction);

            Gizmos.color = intersect ? Color.white:Color.grey;
            Gizmos.DrawRay(m_RayOrigin, direction * Mathf.Max(1f,distances.x+distances.y));

            if (distances.x >= 0)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(m_RayOrigin + direction * distances.x, .1f);
            }
            if (distances.y >= 0)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(m_RayOrigin + direction * (distances.x+distances.y), .1f);
            }
        }

    }
}