using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BoundingCollisionTest
{
    public class GeometryTest_Cone : MonoBehaviour
    {
        public GHeightCone m_Data;
        public GSpaceData m_Ray;
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.grey;
            Gizmos.matrix = transform.localToWorldMatrix;
            float rayDistance = 5f;
            Ray ray = new Ray(m_Ray.m_Position, m_Ray.m_Direction);
            Vector2 distances= UGeometry.RayConeDistance(m_Data,ray);
            if (distances.x>=0)
            {
                Gizmos.color = Color.blue;
                rayDistance = distances.x;
                Gizmos.DrawSphere( ray.GetPoint(rayDistance),.1f);
                Gizmos.color = Color.green;
            }
            if (distances.y >= 0)
            {
                Gizmos.color = Color.red;
                rayDistance = distances.y;
                Gizmos.DrawSphere(ray.GetPoint(rayDistance), .1f);
                Gizmos.color = Color.green;
            }
            Gizmos_Extend.DrawCone(m_Data);
            Gizmos.DrawLine(m_Ray.m_Position, ray.GetPoint(rayDistance));
        }
    }
}
