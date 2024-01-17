using System;
using System.Linq;
using Runtime.Geometry;
using Runtime.Geometry.Validation;
using UnityEngine;

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
        
        public GQuad kQQuad = GQuad.kDefault;
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            int index = 0;
            foreach (var visualization in visualizations.Concat(kQQuad.GetTriangles().Select(p=>new TriangleRayIntersection(){triangle = p,ray = visualizations[0].ray,rayDirectionCheck = true,planeDirectionCheck = true})))
            {
                Gizmos.color = Color.white;
                Gizmos.matrix = Matrix4x4.Translate(Vector3.right * index++) * transform.localToWorldMatrix;

                var triangle = visualization.triangle;
                var ray = visualization.ray;
                Gizmos.DrawLine(triangle[0], triangle[1]);
                Gizmos.DrawLine(triangle[1], triangle[2]);
                Gizmos.DrawLine(triangle[2], triangle[0]);
                
                if(visualization.planeDirectionCheck)
                    UGizmos.DrawArrow(triangle.GetBarycenter(), Quaternion.LookRotation(triangle.normal), .5f, .1f);

                float distance = 2f;
                var intersect = ray.Intersect(triangle, out var rayDistance,visualization.rayDirectionCheck,visualization.planeDirectionCheck);
                if(intersect)
                {
                    distance = rayDistance;
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(visualization.ray.GetPoint(distance), .05f);
                }
                Gizmos.color =  intersect ? Color.green : Color.white;
                if (visualization.rayDirectionCheck)
                    UGizmos.DrawArrow(ray.origin,Quaternion.LookRotation(ray.direction),.5f,.1f);
                Gizmos.DrawLine(ray.origin, ray.GetPoint(distance));
            }
        }
#endif
    }
}