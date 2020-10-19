using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TPhysics;
public class BoundingsCollisionTest_AABB : MonoBehaviour
{
    public Vector3 m_BoxOrigin=Vector3.zero;
    public Vector3 m_BoxSize =Vector3.one;
    public Vector3 m_RayOrigin = Vector3.up;
    public Vector3 m_RayDirection = Vector3.down;
    private void OnDrawGizmos()
    {
        Vector3 direction = m_RayDirection.normalized;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.grey;
        Gizmos.DrawWireCube(m_BoxOrigin, m_BoxSize);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(m_RayOrigin, m_RayDirection * 100f);
        Bounds bound = new Bounds(m_BoxOrigin,m_BoxSize);
        Vector2 distances = Physics_Extend.AABBRayDistance(bound.min, bound.max, m_RayOrigin, direction);
        if (distances.y > 0)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(m_RayOrigin + direction * (distances.y + distances.x), .1f);
            if (distances.x > 0)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(m_RayOrigin + direction * distances.x, .1f);
            }
        }

    }


}
