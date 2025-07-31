using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;
using Gizmos = UnityEngine.Gizmos;

namespace Examples.Algorithm.MathematicsVisualize
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
        [Header("Plane & Point")]
        public GCoordinates m_Axis = GCoordinates.kDefault;
        public float3 m_AxisPoint;
        public GRay m_AxisRay = new GRay(float3.zero, kfloat3.down);

        [Header("2D")]
        public G2Line m_Line2D1 = new G2Line(kfloat2.left, kfloat2.right);
        public G2Line m_Line2D2 = new G2Line(kfloat2.down, kfloat2.up);
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(m_Point, .1f);
            float rayPointProjection = m_Ray.Projection(m_Point);
            m_Ray.ToLine(rayPointProjection).DrawGizmos();
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(m_Ray.GetPoint(rayPointProjection),.1f);

            Gizmos.color = Color.white;
            var lineRayProjections = m_Ray1.Projection(m_Line1);
            m_Line1.DrawGizmos();
            m_Ray1.ToLine(lineRayProjections.x).DrawGizmos();
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(m_Ray1.GetPoint(lineRayProjections.x), .1f);
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(m_Line1.GetPoint(lineRayProjections.y), .1f);

            Gizmos.color = Color.white;
            var rayrayProjections = m_Ray20.Projection(m_Ray21);
            m_Ray20.ToLine(rayrayProjections.x).DrawGizmos();
            m_Ray21.ToLine(rayrayProjections.y).DrawGizmos();
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(m_Ray20.GetPoint(rayrayProjections.x), .1f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(m_Ray21.GetPoint(rayrayProjections.y), .1f);

            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Translate(kfloat3.right * 6f);
            m_Axis.DrawGizmos();
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(m_AxisPoint,.05f);
            var uv = m_Axis.GetUV(m_AxisPoint);
            var point = m_Axis.GetPoint(uv);
            UGizmos.DrawLinesConcat(m_Axis.GetPoint(kfloat2.zero),m_Axis.GetPoint(kfloat2.up),m_Axis.GetPoint(kfloat2.one),m_Axis.GetPoint(kfloat2.right));
            UGizmos.DrawString(uv.ToString(),point);
            Gizmos.DrawWireSphere(point,.05f);
            
            m_AxisRay.DrawGizmos();
            Gizmos.DrawWireSphere(m_AxisRay.GetPoint(m_Axis.IntersectDistance(m_AxisRay)),.025f);
            
            

            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Translate(kfloat3.right * 9f);
            Gizmos.color = Color.white;
            var projection = m_Line2D1.Projection(m_Line2D2);
            
            var intersect = m_Line2D1.Intersect(m_Line2D2);
            Gizmos.color = intersect ? Color.green : Color.white;
            m_Line2D1.DrawGizmos();
            m_Line2D2.DrawGizmos();
            Gizmos.color = Color.red.SetA(.5f);
            Gizmos.DrawWireSphere(((G2Ray)m_Line2D1).GetPoint(projection.x).to3xz(),.05f);
            Gizmos.color = Color.green.SetA(.5f);
            Gizmos.DrawWireSphere(((G2Ray)m_Line2D2).GetPoint(projection.y).to3xz(),.05f);
        }
#endif
    }
}