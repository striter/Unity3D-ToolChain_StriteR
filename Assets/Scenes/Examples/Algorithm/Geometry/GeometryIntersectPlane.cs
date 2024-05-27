using System.Collections;
using System.Collections.Generic;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;
using Gizmos = UnityEngine.Gizmos;

namespace Examples.Algorithm.Geometry
{
    public class GeometryIntersectPlane : MonoBehaviour
    {
        public GPlane m_Plane;
        public GRay m_Ray;

        public float3 m_Point;
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            float distance = m_Ray.Intersection(m_Plane);

            Gizmos.matrix = transform.localToWorldMatrix;
            Vector3 planeSize = new Vector3(1,0,1);
            float rayDistance = 1f;
            bool intersect = distance >= 0;
            if (intersect)
            {
                Vector3 hitPoint = m_Ray.GetPoint(distance);
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(hitPoint, .05f);
                rayDistance = distance;
                planeSize = new Vector3(Mathf.Max(1,Mathf.Abs( hitPoint.x)*2),0,Mathf.Max(1,Mathf.Abs(hitPoint.z)*2));
            }

            Gizmos.color = intersect? Color.white:Color.grey;
            m_Ray.ToLine(rayDistance).DrawGizmos();
            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.TRS(m_Plane.position, Quaternion.LookRotation(Vector3.forward, m_Plane.normal), Vector3.one);
            Gizmos.color = intersect ? Color.green : Color.grey;
            UGizmos.DrawArrow(Vector3.zero,Quaternion.LookRotation(Vector3.up), .3f, .1f);
            Gizmos.DrawWireCube(Vector3.zero, planeSize);

            Gizmos.color = Color.white;
            Gizmos.matrix = transform.localToWorldMatrix;
            distance = UGeometry.Distance(m_Plane,m_Point);
            Gizmos.DrawLine(m_Point,m_Point-distance*m_Plane.normal);
        }
#endif
    }
}