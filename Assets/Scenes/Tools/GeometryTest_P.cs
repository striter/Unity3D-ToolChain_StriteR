using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoundingCollisionTest
{
    public class GeometryTest_P : MonoBehaviour
    {
        public Vector3 m_PlaneNormal;
        public float m_PlaneDistance;
        public Vector3 m_RayOrigin;
        public Vector3 m_RayDirection;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            float distance = UBoundingCollision.PlaneRayDistance(m_PlaneNormal,m_PlaneDistance, m_RayOrigin, m_RayDirection);

            Gizmos.matrix = transform.localToWorldMatrix;
            Vector3 planeSize = new Vector3(1,0,1);
            float rayDistance = 1f;
            bool intersect = distance >= 0;
            Gizmos.DrawWireSphere(m_RayOrigin, .05f);
            if (intersect)
            {
                Vector3 hitPoint = m_RayOrigin + m_RayDirection * distance;
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(hitPoint, .05f);
                rayDistance = hitPoint.magnitude;
                planeSize = new Vector3(Mathf.Max(1,Mathf.Abs( hitPoint.x)*2),0,Mathf.Max(1,Mathf.Abs(hitPoint.z)*2));
            }

            Gizmos.color = intersect? Color.white:Color.grey;
            Gizmos.DrawLine(m_RayOrigin, m_RayOrigin + m_RayDirection.normalized * rayDistance);
            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.TRS(m_PlaneNormal* m_PlaneDistance, Quaternion.LookRotation(Vector3.forward, m_PlaneNormal), Vector3.one);
            Gizmos.color = intersect ? Color.green : Color.grey;
            Gizmos_Extend.DrawArrow(Vector3.zero,Quaternion.LookRotation(Vector3.up), .3f, .1f);
            Gizmos.DrawWireCube(Vector3.zero, planeSize);
        }
#endif
    }
}