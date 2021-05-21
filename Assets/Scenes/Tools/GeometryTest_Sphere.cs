using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BoundingCollisionTest
{
    public class GeometryTest_Sphere : MonoBehaviour
    {
        public GSphere m_Sphere;
        public GRay m_Ray;

        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;

            bool intersect = UGeometry.RayBSIntersect(m_Sphere,m_Ray);
            Gizmos.color = intersect ? Color.green : Color.grey;
            Gizmos.DrawWireSphere(m_Sphere.center,m_Sphere.radius);
            Vector2 distances = UGeometry.RayBSDistance(m_Sphere, m_Ray);

            Gizmos.color = intersect ? Color.white:Color.grey;
            Gizmos.DrawRay(m_Ray.origin, m_Ray.direction * Mathf.Max(1f,distances.y));

            if (distances.x >= 0)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(m_Ray.GetPoint(distances.x), .1f);
            }
            if (distances.y >= 0)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(m_Ray.GetPoint(distances.y), .1f);
            }
        }

    }
}