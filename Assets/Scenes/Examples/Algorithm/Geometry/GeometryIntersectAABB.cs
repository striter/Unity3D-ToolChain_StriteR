using System.Collections;
using System.Collections.Generic;
using Geometry;
using Geometry.Validation;
using Unity.Mathematics;
using UnityEngine;
namespace Examples.Algorithm.Geometry
{
    public class GeometryIntersectAABB : MonoBehaviour
    {
        public GBox m_Box;
        public GRay m_Ray;

        public GBox m_CollisionBox1;
        public GBox m_CollisionBox2;
#if UNITY_EDITOR
        private float totalTime = 0;
        private void OnDrawGizmos()
        {
            totalTime += UTime.deltaTime;
            
            Gizmos.matrix = transform.localToWorldMatrix;
            bool intersect = UGeometryValidation.Ray.Intersect(m_Ray,m_Box);
            Gizmos.color = intersect ? Color.green : Color.grey;
            m_Box.DrawGizmos();
            
            Vector2 distances = UGeometryValidation.Ray.Distances(m_Ray,m_Box);
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

            float delta = math.sin(totalTime * kmath.kPI * 2f / 4);
            var collisionBox1 = m_CollisionBox1.Move(delta*.5f);
            var collisionBox2 = m_CollisionBox2.Move(-delta*.5f);
            
            Gizmos.color = UGeometryValidation.AABB.AABBIntersection(m_Box,collisionBox1) ? Color.green:Color.red;
            collisionBox1.DrawGizmos();
            Gizmos.color = UGeometryValidation.AABB.AABBIntersection(m_Box,collisionBox2) ? Color.green:Color.red;
            collisionBox2.DrawGizmos();

            Gizmos.color = intersect ? Color.white:Color.grey;
            m_Ray.ToLine(distances.x + distances.y).DrawGizmos(); 
        }
#endif
    }
}