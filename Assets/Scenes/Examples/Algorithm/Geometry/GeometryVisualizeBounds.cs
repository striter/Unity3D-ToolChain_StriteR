using Runtime.Geometry;
using Runtime.Geometry.Validation;
using Unity.Mathematics;
using UnityEngine;
using Gizmos = UnityEngine.Gizmos;

namespace Examples.Algorithm.Geometry
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
        
        [Button]
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

        public GBox box;
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            Gizmos.matrix = transform.localToWorldMatrix;
            UBounds.GetBoundingBox(boundingBoxRandomPoints).DrawGizmos();
            if(boundingBoxRandomPoints!=null)
                foreach (var points in boundingBoxRandomPoints)
                    Gizmos.DrawWireSphere(points,.02f);

            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Translate(-Vector3.right * 1.5f);
            UBounds.GetBoundingSphere(boundingSpherePoints).DrawGizmos();
            if (boundingSpherePoints != null)
            {
                foreach (var points in boundingSpherePoints)
                    Gizmos.DrawWireSphere(points,.02f);
            }

            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Translate(-Vector3.right * 3f);
            Gizmos.color = Color.red;
            boundingSphere1.DrawGizmos();
            Gizmos.color = Color.blue;
            boundingSphere2.DrawGizmos();
            Gizmos.color = Color.white;
            GSphere.Minmax(boundingSphere1,boundingSphere2).DrawGizmos();
            
            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Translate(Vector3.back * 5f);
            UBounds.GetBoundingPolygon(boundingPolygonPoints).DrawGizmos();
            if (boundingPolygonPoints != null)
            {
                foreach (var points in boundingPolygonPoints)
                    Gizmos.DrawSphere(points.to3xz(),.02f);
            }
            
        }
    }
}