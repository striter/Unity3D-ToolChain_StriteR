using System.Collections;
using System.Collections.Generic;
using TPhysics;
using UnityEngine;

namespace BoundingCollisionTest
{
    public class BoundingsCollisionTest_P : MonoBehaviour
    {
        public Vector3 m_PlanePosition;
        public Vector3 m_PlaneDirection;
        public Vector3 m_PlaneNormal;
        public Vector3 m_RayOrigin;
        public Vector3 m_RayDirection;

        private void OnDrawGizmos()
        {
            float distance = Physics_Extend.PlaneRayDistance(m_PlanePosition, m_PlaneDirection, m_PlaneNormal, m_RayOrigin, m_RayDirection);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.white;
            Gizmos.DrawLine(m_RayOrigin, m_RayOrigin + m_RayDirection * 5f);
            float planeSize = 3f;
            if (distance >= 0)
            {
                Vector3 hitPoint = m_RayOrigin + m_RayDirection * distance;
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(hitPoint, .1f);
                hitPoint.y = 0;
                planeSize = Mathf.Max(3f, hitPoint.magnitude * 2);
            }

            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.TRS(m_PlanePosition, Quaternion.LookRotation(m_PlaneDirection, m_PlaneNormal), Vector3.one);
            Gizmos.color = distance >= 0 ? Color.green : Color.grey;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(planeSize, 0, planeSize));
        }
    }
}