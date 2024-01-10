using System;
using Runtime.Geometry;
using Runtime.Geometry.Validation;
using UnityEngine;
using Gizmos = UnityEngine.Gizmos;

namespace Examples.Algorithm.Geometry
{
    public class GeometryIntersectTriangle : MonoBehaviour
    {
        [Serializable]
        public struct TriangleRayIntersection
        {
            public GTriangle triangle;
            public GRay ray;
            public bool rayDirectionCheck;
            public bool planeDirectionCheck;
        }

        public TriangleRayIntersection[] visualizations;
        
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            int index = 0;
            foreach (var visualization in visualizations)
            {
                Gizmos.color = Color.green;
                Gizmos.matrix = Matrix4x4.Translate(Vector3.right * index++) * transform.localToWorldMatrix;

                var triangle = visualization.triangle;
                var ray = visualization.ray;
                Gizmos.DrawLine(triangle[0], triangle[1]);
                Gizmos.DrawLine(triangle[1], triangle[2]);
                Gizmos.DrawLine(triangle[2], triangle[0]);
                
                if(visualization.planeDirectionCheck)
                    UGizmos.DrawArrow(triangle.GetBarycenter(), Quaternion.LookRotation(triangle.normal), .5f, .1f);

                float distance = 2f;
                if(UGeometry.Intersect(triangle,ray, visualization.rayDirectionCheck,visualization.planeDirectionCheck,out float rayDistance))
                {
                    distance = rayDistance;
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(visualization.ray.GetPoint(distance), .05f);
                }
                Gizmos.color = Color.white;
                if (visualization.rayDirectionCheck)
                    UGizmos.DrawArrow(ray.origin,Quaternion.LookRotation(ray.direction),.5f,.1f);
                Gizmos.DrawLine(ray.origin, ray.GetPoint(distance));
            }
        }
#endif
    }
}