using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TPhysics;
public class BoundingsCollisionTest_BS : MonoBehaviour
{
    public Vector3 m_RayOrigin = Vector3.up*5;
    public Vector3 m_RayDirection = Vector3.down;
    public Vector3 m_BoundingSphereOrigin = Vector3.zero;
    public float m_BoundingSphereRadius = 2f;

    private void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Vector3 direction = m_RayDirection.normalized;
        Gizmos.color = Color.grey;
        Gizmos.DrawWireSphere(m_BoundingSphereOrigin,m_BoundingSphereRadius);
        Vector2 distances = Physics_Extend.BSRayDistance( m_BoundingSphereOrigin, m_BoundingSphereRadius, m_RayOrigin, direction);
        Gizmos.color= Color.green;
        Gizmos.DrawRay(m_RayOrigin, direction * 100f );

        if (distances.x >0)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(m_RayOrigin+ direction * distances.x,.1f);
        }
        if (distances.y >0)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(m_RayOrigin + direction * distances.y,.1f);
        }
    }

}
