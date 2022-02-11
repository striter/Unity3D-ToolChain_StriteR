using System.Collections;
using System.Collections.Generic;
using Geometry;
using Geometry.Voxel;
using UnityEngine;
namespace ExampleScenes.Algorithm.Geometry
{
    public class GeometryVisualize_AABB : MonoBehaviour
    {
        public GBox m_Box;
        public GRay m_Ray;

        public GBox m_CollisionBox1;
        public GBox m_CollisionBox2;
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            bool intersect = UGeometryIntersect.RayAABBIntersect(m_Box,m_Ray);
            Gizmos.color = intersect ? Color.green : Color.grey;
            m_Box.DrawGizmos();
            
            Vector2 distances = UGeometryIntersect.RayAABBDistance(m_Box,m_Ray);
            if (distances.y > 0)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(m_Ray.GetPoint(distances.y + distances.x), .1f);
                if (distances.x > 0)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawSphere(m_Ray.GetPoint( distances.x), .1f);
                }
            }

            Gizmos.color = m_Box.AABBIntersection(m_CollisionBox1) ? Color.green:Color.red;
            m_CollisionBox1.DrawGizmos();
            Gizmos.color = m_Box.AABBIntersection(m_CollisionBox2) ? Color.green:Color.red;
            m_CollisionBox2.DrawGizmos();

            Gizmos.color = intersect ? Color.white:Color.grey;
            m_Ray.ToLine(distances.x + distances.y).DrawGizmos(); 
        }
#endif
    }
}