using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BoundingCollisionTest
{
    public class GeometryTest_Point : MonoBehaviour
    {
        public GSpaceData m_Ray;
        public Vector3 m_Point;

        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawSphere(m_Point, .1f);
            Gizmos_Extend.DrawArrow(m_Ray.m_Position,m_Ray.m_Direction,1f,.1f);
            Gizmos.color = Color.blue;

            Ray ray = new Ray(m_Ray.m_Position, m_Ray.m_Direction);
            Gizmos.DrawSphere( ray.GetPoint(UGeometry.PointRayProjection(m_Point, ray)),.1f);
        }

    }

}