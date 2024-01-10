using Runtime.Geometry;
using Runtime.Geometry.Validation;
using Unity.Mathematics;
using UnityEngine;
using Gizmos = UnityEngine.Gizmos;

namespace Examples.Algorithm.Geometry
{
    public class GeometryIntersectSphere : MonoBehaviour
    {
        public GSphere m_Sphere;
        public GRay m_SRay;

        public GEllipsoid m_Ellipsoid;
        public GRay m_ERay;
        
        public G2Circle m_Circle;
        public float2[] m_CirclePoint;
#if UNITY_EDITOR
        private float time;
        private void OnDrawGizmos()
        {
            time += UTime.deltaTime;
            Gizmos.matrix = transform.localToWorldMatrix;

            //Sphere
            var intersect = UGeometry.Intersect(m_SRay,m_Sphere);
            Gizmos.color = intersect ? Color.green : Color.grey;
            Gizmos.DrawWireSphere(m_Sphere.center,m_Sphere.radius);
            var distances = UGeometry.Distance(m_SRay,m_Sphere);

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

            //Ellipsoid
            rayDistance = 2f;
            intersect = UGeometry.Intersect(m_ERay,m_Ellipsoid);
            Gizmos.color = intersect ? Color.green : Color.grey;
            m_Ellipsoid.DrawGizmos();
            distances = UGeometry.Distance(m_ERay,m_Ellipsoid);
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
            
            //Circle
            m_Circle.DrawGizmos();
            if (m_CirclePoint != null)
            {
                var index = 0;
                foreach (var point in m_CirclePoint)
                {
                    float2 deltaPosition = math.sin(time * 2 * math.PI * UNoise.Value.Unit1f1((float)index++/m_CirclePoint.Length));
                    
                    var curPoint = point + m_Circle.center + deltaPosition;

                    bool contains = m_Circle.Contains(curPoint);
                    Gizmos.color = contains ? Color.green : Color.red;
                    Gizmos.DrawWireSphere(curPoint.to3xz(),.02f);
                }
            }
        }
#endif
    }
}