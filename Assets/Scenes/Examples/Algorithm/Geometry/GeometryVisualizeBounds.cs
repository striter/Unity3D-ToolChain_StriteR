using Geometry;
using Geometry.PointSet;
using Unity.Mathematics;
using UnityEngine;

namespace Examples.Algorithm.Geometry
{
    public class GeometryVisualizeBounds : MonoBehaviour
    {
        public float m_SinValue;
        
        [Header("Box")]
        public float3[] boundingBoxRandomPoints;
        public GBox boudingBox = default;

        [Header("Sphere")]
        public float3[] boundingSpherePoints;
        public GSphere boundingSphere;

        private void OnValidate()
        {
            if (boundingBoxRandomPoints != null && boundingBoxRandomPoints.Length > 0)
            {
                for (int i = 0; i < boundingBoxRandomPoints.Length; i++)
                    boundingBoxRandomPoints[i] = URandom.RandomSphere();
                boudingBox = UBounds.GetBoundingBox(boundingBoxRandomPoints);
            }

            if (boundingSpherePoints != null && boundingSpherePoints.Length > 0)
            {
                for (int i = 0; i < boundingSpherePoints.Length; i++)
                    boundingSpherePoints[i] = URandom.RandomSphere();
                boundingSphere = UBounds.GetBoundingSphere(boundingSpherePoints);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            boudingBox.DrawGizmos();
            if(boundingBoxRandomPoints!=null)
                foreach (var points in boundingBoxRandomPoints)
                    Gizmos.DrawWireSphere(points,.02f);

            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Translate(-Vector3.right * 1.5f);
            boundingSphere.DrawGizmos();
            if (boundingSpherePoints != null)
            {
                foreach (var points in boundingSpherePoints)
                    Gizmos.DrawWireSphere(points,.02f);
            }
        }
    }
}