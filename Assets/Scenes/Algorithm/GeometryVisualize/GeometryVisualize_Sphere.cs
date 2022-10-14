using Geometry;
using Geometry.Voxel;
using UnityEngine;
namespace ExampleScenes.Algorithm.Geometry
{
    public class GeometryVisualize_Sphere : MonoBehaviour
    {
        public GSphere m_Sphere;
        public GRay m_SRay;

        public GEllipsoid m_Ellipsoid;
        public GRay m_ERay;
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;

            bool intersect = UGeometryIntersect.RayBSIntersect(m_Sphere,m_SRay);
            Gizmos.color = intersect ? Color.green : Color.grey;
            Gizmos.DrawWireSphere(m_Sphere.center,m_Sphere.radius);
            Vector2 distances = UGeometryIntersect.RayBSDistance(m_Sphere, m_SRay);

            float rayDistance = 2f;
            if (distances.x >= 0)
            {
                Gizmos.color = Color.blue;
                rayDistance = distances.x;
                Gizmos.DrawSphere(m_SRay.GetPoint(distances.x), .1f);
            }
            if (distances.y >= 0)
            {
                Gizmos.color = Color.red;
                rayDistance = distances.y;
                Gizmos.DrawSphere(m_SRay.GetPoint(distances.y), .1f);
            }
            Gizmos.color = Color.white;
            m_SRay.ToLine(rayDistance).DrawGizmos();

            rayDistance = 2f;
            intersect = UGeometryIntersect.RayBEIntersect(m_Ellipsoid,m_ERay);
            Gizmos.color = intersect ? Color.green : Color.grey;
            m_Ellipsoid.DrawGizmos();
            distances = UGeometryIntersect.RayBEDistance(m_Ellipsoid, m_ERay);
            if (distances.x >= 0)
            {
                Gizmos.color = Color.blue;
                rayDistance = distances.x;
                Gizmos.DrawSphere(m_ERay.GetPoint(distances.x), .1f);
            }
            if (distances.y >= 0)
            {
                Gizmos.color = Color.red;
                rayDistance = distances.y;
                Gizmos.DrawSphere(m_ERay.GetPoint(distances.y), .1f);
            }
            Gizmos.color = Color.white;
            m_ERay.ToLine(rayDistance).DrawGizmos();
            
        }
#endif
    }
}