using System.Collections;
using System.Collections.Generic;
using Geometry.Voxel;
using UnityEngine;
namespace ExampleScenes.Rendering.GeometryVisualize
{
    public class GeometryTest_Point : MonoBehaviour
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
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(m_Point, .1f);
            float distances= UGeometryIntersect.PointRayProjection(m_Point, m_Ray);
            Handles_Extend.DrawLine(m_Ray.ToLine(distances));
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(m_Ray.GetPoint(distances),.1f);

            Gizmos.color = Color.white;
            Vector2 lineRayDistances = UGeometryIntersect.LineRayProjection(m_Line1, m_Ray1);
            Gizmos_Extend.DrawLine(m_Line1);
            Gizmos_Extend.DrawLine(m_Ray1.ToLine(lineRayDistances.y));
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(m_Line1.GetPoint(lineRayDistances.x), .1f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(m_Ray1.GetPoint(lineRayDistances.y), .1f);

            Gizmos.color = Color.white;
            Vector2 rayrayDistances = UGeometryIntersect.RayRayProjection(m_Ray20, m_Ray21);
            Gizmos_Extend.DrawLine(m_Ray20.ToLine(rayrayDistances.x));
            Gizmos_Extend.DrawLine(m_Ray21.ToLine(rayrayDistances.y));
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(m_Ray20.GetPoint(rayrayDistances.x), .1f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(m_Ray21.GetPoint(rayrayDistances.y), .1f);
        }
#endif
    }
}