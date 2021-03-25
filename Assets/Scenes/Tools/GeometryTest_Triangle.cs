using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoundingCollisionTest
{
    public class GeometryTest_Triangle : MonoBehaviour
    {
        public DirectedTriangle Triangle=new DirectedTriangle( new Vector3(0, 0, 1), new Vector3(1, 0, -1), new Vector3(-1, 0, -1) );
        public Vector3 m_Origin=new Vector3(0,2,0),m_Direction= new Vector3(-.1f,-1,.1f);
        public bool m_RayDirectionCheck = true;
        public bool m_PlaneDirectionCheck = true;
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawLine(Triangle[0], Triangle[1]);
            Gizmos.DrawLine(Triangle[1], Triangle[2]);
            Gizmos.DrawLine(Triangle[2], Triangle[0]);
            if(m_PlaneDirectionCheck)
                Gizmos_Extend.DrawArrow(Triangle.GetUVPoint(Vector2.one * .25f), Quaternion.LookRotation(Triangle.m_Normal), .5f, .1f);

            Ray ray = new Ray(m_Origin, m_Direction.normalized);
            float distance = 2f;
            if( UBoundingCollision.RayDirectedTriangleIntersect(Triangle,ray, m_RayDirectionCheck,m_PlaneDirectionCheck,out float rayDistance))
            {
                distance = rayDistance;
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(ray.GetPoint(distance), .05f);
            }
            Gizmos.color = Color.white;
            if (m_RayDirectionCheck)
                Gizmos_Extend.DrawArrow(ray.origin,Quaternion.LookRotation(ray.direction),.5f,.1f);
            Gizmos.DrawLine(ray.origin,ray.GetPoint(distance));
        }
#endif
    }
}