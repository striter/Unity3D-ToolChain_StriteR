using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BoundingCollisionTest
{
    public class GeometryTest_Point : MonoBehaviour
    {
        [Header("Ray & Point")]
        public GRay m_Ray;
        public Vector3 m_Point;
        [Header("Ray & Line")]
        public GRay m_Ray1;
        public GLine m_Line1;
        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(m_Point, .1f);
            Gizmos_Extend.DrawArrow(m_Ray.origin,m_Ray.direction,1f,.1f);
            Gizmos.DrawLine(m_Ray.origin,m_Ray.GetPoint(2f));
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(m_Ray.GetPoint(UGeometry.PointRayProjection(m_Point, m_Ray)),.1f);


            Gizmos.color = Color.white;
            Gizmos_Extend.DrawArrow(m_Ray1.origin, m_Ray1.direction, 1f, .1f);
            Gizmos.DrawLine(m_Ray1.origin, m_Ray1.GetPoint(2f));
            Gizmos.DrawLine(m_Line1.origin, m_Line1.End);
            Vector2 lineRayDistances = UGeometry.LineRayProjectionDistance(m_Line1,m_Ray1);
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(m_Line1.GetPoint(lineRayDistances.x), .1f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(m_Ray1.GetPoint(lineRayDistances.y), .1f);
        }
    }
}