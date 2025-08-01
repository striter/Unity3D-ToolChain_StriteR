using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Runtime.Pool;
using Unity.Mathematics;
using UnityEngine;
using Gizmos = UnityEngine.Gizmos;

namespace Examples.Algorithm.MathematicsVisualize
{
    public class GeometryVisualizeBounds : MonoBehaviour
    {
        [Header("Box")]
        public float3[] boundingBoxRandomPoints;

        [Header("Sphere")]
        public float3[] boundingSpherePoints;

        public GSphere boundingSphere1;
        public GSphere boundingSphere2;

        [Header("Polygon")] public float2[] boundingPolygonPoints;
        [Range(0f,1f)] public float alphaShapeThreshold = .5f;
        
        [InspectorButton(true)]
        private void RandomPoints()
        {
            if (boundingBoxRandomPoints is { Length: > 0 })
            {
                for (int i = 0; i < boundingBoxRandomPoints.Length; i++)
                    boundingBoxRandomPoints[i] = URandom.RandomSphere();
            }

            if (boundingSpherePoints is { Length: > 0 })
            {
                for (int i = 0; i < boundingSpherePoints.Length; i++)
                    boundingSpherePoints[i] = URandom.RandomSphere();
            }

            if (boundingPolygonPoints is { Length: > 0 })
            {
                for(int i = 0; i < boundingPolygonPoints.Length; i++)
                    boundingPolygonPoints[i] = URandom.Random2DSphere();
            }
        }

        private const float kPadding = 3f;
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            Gizmos.matrix = transform.localToWorldMatrix;
            GBox.GetBoundingBox(boundingBoxRandomPoints).DrawGizmos();
            if(boundingBoxRandomPoints!=null)
                foreach (var points in boundingBoxRandomPoints)
                    Gizmos.DrawWireSphere(points,.02f);

            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Translate(Vector3.right * kPadding);
            GSphere.GetBoundingSphere(boundingSpherePoints).DrawGizmos();
            if (boundingSpherePoints != null)
            {
                foreach (var points in boundingSpherePoints)
                    Gizmos.DrawWireSphere(points,.02f);
            }
            
            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Translate(Vector3.right * kPadding * 2);
            Gizmos.color = Color.red;
            boundingSphere1.DrawGizmos();
            Gizmos.color = Color.blue;
            boundingSphere2.DrawGizmos();
            Gizmos.color = Color.white;
            GSphere.Minmax(boundingSphere1,boundingSphere2).DrawGizmos();

            if (boundingPolygonPoints == null)
                return;
            
            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Translate(Vector3.forward * kPadding);
            G2Polygon.ConvexHull(boundingPolygonPoints).DrawGizmos();
            foreach (var points in boundingPolygonPoints)
                Gizmos.DrawSphere(points.to3xz(),.02f);
            UGizmos.DrawString("Convex Hull");
            
            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Translate(Vector3.forward * kPadding + Vector3.right * kPadding * 1);
            G2Polygon.GrahamScan(boundingPolygonPoints).DrawGizmos();
            foreach (var points in boundingPolygonPoints)
                Gizmos.DrawSphere(points.to3xz(),.02f);
            UGizmos.DrawString("Graham Scan");
            
            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Translate(Vector3.forward * kPadding + Vector3.right * kPadding * 2);
            G2Polygon.QuickHull(boundingPolygonPoints).DrawGizmos();
            foreach (var points in boundingPolygonPoints)
                Gizmos.DrawSphere(points.to3xz(),.02f);
            UGizmos.DrawString("Quick Hull");
            
            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Translate(Vector3.forward * kPadding * 2);
            G2Polygon.ConcaveHull(boundingPolygonPoints).DrawGizmos();
            foreach (var points in boundingPolygonPoints)
                Gizmos.DrawSphere(points.to3xz(),.02f);
            UGizmos.DrawString("Concave Hull");
            
            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Translate(Vector3.forward * kPadding * 2 + Vector3.right * kPadding * 1);
            var triangles = PoolList<PTriangle>.Empty(nameof(GeometryVisualizeBounds));
            Gizmos.color = Color.blue.SetA(.2f);
            UTriangulation.Triangulation(boundingPolygonPoints,ref triangles);
            triangles.Select(p=>new G2Triangle(boundingPolygonPoints,p)).Traversal(p=>p.DrawGizmos());
            var graph = G2Graph.FromTriangles(boundingPolygonPoints,triangles);
            graph.DrawGizmos();
            Gizmos.color = Color.white;
            G2Polygon.AlphaShape(boundingPolygonPoints,alphaShapeThreshold).DrawGizmos();
            UGizmos.DrawString("Alpha Shape");
        }
    }
}