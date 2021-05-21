using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BoundingCollisionTest
{
    public class GeometryTest_AABB : MonoBehaviour
    {
        public GBox m_Box;
        public GRay m_Ray;
        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            bool intersect = UGeometry.RayAABBIntersect(m_Box,m_Ray);
            Gizmos.color = intersect ? Color.green : Color.grey;
            Gizmos.DrawWireCube(m_Box.center,m_Box.Size);

            Vector2 distances = UGeometry.RayAABBDistance(m_Box,m_Ray);
            if (distances.y > 0)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(m_Ray.GetPoint(distances.y + distances.x), .1f);
                if (distances.x > 0)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawSphere(m_Ray.GetPoint( distances.x), .1f);
                }
            }

            Gizmos.color = intersect ? Color.white:Color.grey;
            Gizmos.DrawRay(m_Ray.origin, m_Ray.direction * Mathf.Max(1f, distances.x+distances.y));
        }


    }
}