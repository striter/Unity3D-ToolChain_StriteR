using Geometry;
using Geometry.Validation;
using UnityEngine;
namespace Examples.Algorithm.Geometry
{
    public class GeometryIntersectCone : MonoBehaviour
    {
        public GHeightCone m_Data;
        public GRay m_Ray;
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.grey;
            Gizmos.matrix = transform.localToWorldMatrix;
            float rayDistance = 1f;
            Vector2 distances= UGeometryValidation.Ray.Distances(m_Ray,m_Data);
            if (distances.x>=0)
            {
                Gizmos.color = Color.blue;
                rayDistance = distances.x;
                Gizmos.DrawSphere(m_Ray.GetPoint(rayDistance),.1f);
                Gizmos.color = Color.green;
            }
            if (distances.y >= 0)
            {
                Gizmos.color = Color.red;
                rayDistance = distances.y;
                Gizmos.DrawSphere(m_Ray.GetPoint(rayDistance), .1f);
                Gizmos.color = Color.green;
            }
            Gizmos_Extend.DrawGizmos(m_Data);
            Gizmos.color = Color.white;
            Gizmos_Extend.DrawGizmos(m_Ray.ToLine(rayDistance));
        }
#endif
    }
}
