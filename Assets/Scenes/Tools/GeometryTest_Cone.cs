using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BoundingCollisionTest
{
    public class GeometryTest_Cone : MonoBehaviour
    {
        public GHeightCone m_Data;
        public GRay m_Ray;
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.grey;
            Gizmos.matrix = transform.localToWorldMatrix;
            float rayDistance = 5f;
            Vector2 distances= UGeometry.RayConeDistance(m_Data,m_Ray);
            if (distances.x>=0)
            {
                Gizmos.color = Color.blue;
                rayDistance = distances.x;
                Gizmos.DrawSphere(m_Ray.GetPoint(rayDistance),.1f);
                Gizmos.color = Color.green;
            }
            if (distances.y >= 0)
            {
                Gizmos.color = Color.red;
                rayDistance = distances.y;
                Gizmos.DrawSphere(m_Ray.GetPoint(rayDistance), .1f);
                Gizmos.color = Color.green;
            }
            Gizmos_Extend.DrawCone(m_Data);
            Gizmos.DrawLine(m_Ray.origin, m_Ray.GetPoint(rayDistance));
        }
    }
}
