using Geometry.Voxel;
using UnityEngine;

namespace BoundingCollisionTest
{
    public class GeometryTest_Triangle : MonoBehaviour
    {
        public GTriangle Triangle=new GTriangle( new Vector3(0, 0, 1), new Vector3(1, 0, -1), new Vector3(-1, 0, -1) );
        public GRay m_Ray = new GRay(new Vector3(0, 2, 0), new Vector3(-.1f, -1, .1f));
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
                Gizmos_Extend.DrawArrow(Triangle.GetUVPoint(.25f,.25f), Quaternion.LookRotation(Triangle.normal), .5f, .1f);

            float distance = 2f;
            if( UGeometryVoxel.RayDirectedTriangleIntersect(Triangle,m_Ray, m_RayDirectionCheck,m_PlaneDirectionCheck,out float rayDistance))
            {
                distance = rayDistance;
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(m_Ray.GetPoint(distance), .05f);
            }
            Gizmos.color = Color.white;
            if (m_RayDirectionCheck)
                Gizmos_Extend.DrawArrow(m_Ray.origin,Quaternion.LookRotation(m_Ray.direction),.5f,.1f);
            Gizmos.DrawLine(m_Ray.origin, m_Ray.GetPoint(distance));
        }
#endif
    }
}