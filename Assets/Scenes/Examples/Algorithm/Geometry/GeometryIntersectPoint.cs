using Geometry;
using Geometry.Validation;
using Unity.Mathematics;
using UnityEngine;
namespace Examples.Algorithm.Geometry
{
    public class GeometryIntersectPoint : MonoBehaviour
    {
        [Header("Ray & Point")]
        public GRay m_Ray;
        public Vector3 m_Point;
        [Header("Ray & Line")]
        public GRay m_Ray1;
        public GLine m_Line1;
        [Header("Ray & Ray")]
        public GRay m_Ray20;
        public GRay m_Ray21;

        [Header("Point & Triangle")] 
        public float3 m_Point3;
        public GTriangle m_Triangle;
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(m_Point, .1f);
            float distances= UGeometryValidation.Ray.Projection(m_Ray, m_Point);
            Gizmos_Extend.DrawGizmos(m_Ray.ToLine(distances));
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(m_Ray.GetPoint(distances),.1f);

            Gizmos.color = Color.white;
            var lineRayDistances = UGeometryValidation.Ray.Projection(m_Ray1,m_Line1);
            Gizmos_Extend.DrawGizmos(m_Line1);
            Gizmos_Extend.DrawGizmos(m_Ray1.ToLine(lineRayDistances.y));
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(m_Line1.GetPoint(lineRayDistances.x), .1f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(m_Ray1.GetPoint(lineRayDistances.y), .1f);

            Gizmos.color = Color.white;
            var rayrayDistances = UGeometryValidation.Ray.Projection(m_Ray20, m_Ray21);
            Gizmos_Extend.DrawGizmos(m_Ray20.ToLine(rayrayDistances.x));
            Gizmos_Extend.DrawGizmos(m_Ray21.ToLine(rayrayDistances.y));
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(m_Ray20.GetPoint(rayrayDistances.x), .1f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(m_Ray21.GetPoint(rayrayDistances.y), .1f);
            
            var distances 
        }
#endif
    }
}