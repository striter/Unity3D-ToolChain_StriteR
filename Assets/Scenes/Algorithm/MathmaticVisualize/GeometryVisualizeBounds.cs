using Runtime.Geometry;
using Runtime.Geometry.Extension;
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
        [Clamp(3,nameof(boundingPolygonPoints))]public int concaveHullK = 3;
        
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
            UGeometry.GetBoundingBox(boundingBoxRandomPoints).DrawGizmos();
            if(boundingBoxRandomPoints!=null)
                foreach (var points in boundingBoxRandomPoints)
                    Gizmos.DrawWireSphere(points,.02f);

            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Translate(Vector3.right * kPadding);
            UGeometry.GetBoundingSphere(boundingSpherePoints).DrawGizmos();
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
            
            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Translate(Vector3.forward * kPadding);
            G2Polygon.ConvexConstructor.GrahamScan(boundingPolygonPoints).DrawGizmos();
            if (boundingPolygonPoints != null)
            {
                foreach (var points in boundingPolygonPoints)
                    Gizmos.DrawSphere(points.to3xz(),.02f);
            }
            
            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Translate(Vector3.forward * kPadding + Vector3.right * kPadding);
            G2Polygon.ConvexConstructor.QuickHull(boundingPolygonPoints).DrawGizmos();
            if (boundingPolygonPoints != null)
            {
                foreach (var points in boundingPolygonPoints)
                    Gizmos.DrawSphere(points.to3xz(),.02f);
            }
            
            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Translate(Vector3.forward * kPadding + Vector3.right * kPadding * 2);
            G2Polygon.ConcaveHull(boundingPolygonPoints,concaveHullK).DrawGizmos();
            if (boundingPolygonPoints != null)
            {
                foreach (var points in boundingPolygonPoints)
                    Gizmos.DrawSphere(points.to3xz(),.02f);
            }
        }
    }
}