using System.Collections;
using System.Collections.Generic;
using Geometry;
using Geometry.Validation;
using Unity.Mathematics;
using UnityEngine;

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
            float distance = UGeometry.Distance.Eval(m_Ray,m_Plane);

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
            UGizmos.DrawGizmos(m_Ray.ToLine(rayDistance));
            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.TRS(m_Plane.position, Quaternion.LookRotation(Vector3.forward, m_Plane.normal), Vector3.one);
            Gizmos.color = intersect ? Color.green : Color.grey;
            UGizmos.DrawArrow(Vector3.zero,Quaternion.LookRotation(Vector3.up), .3f, .1f);
            Gizmos.DrawWireCube(Vector3.zero, planeSize);

            Gizmos.color = Color.white;
            Gizmos.matrix = transform.localToWorldMatrix;
            float projection = UGeometry.Projection.Eval(m_Point,m_Plane);
            Gizmos.DrawLine(m_Point,m_Point-projection*m_Plane.normal);
        }
#endif
    }
}