using System.Linq;
using Runtime.Geometry;
using Runtime.Geometry.Validation;
using Unity.Mathematics;
using UnityEngine;

namespace Examples.Algorithm.Geometry
{
    public class GeometryVisualizeSAT : MonoBehaviour
    {
        [Header("2D")]
        public G2Polygon m_Polygon2 = G2Polygon.kDefault;
        public G2Triangle m_Triangle2 = G2Triangle.kDefault;
        public G2Box m_Box2 = G2Box.kDefault;
        
        [Header("3D")]
        public GBox m_Box = GBox.kDefault;
        public GTriangle m_Triangle = GTriangle.kDefault;
        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            var convex2D = new IConvex2D[] { m_Polygon2,m_Triangle2, m_Box2 + kfloat2.up * math.sin(UTime.time) };
            foreach (var convex in convex2D)
            {
                Gizmos.color = convex2D.Exclude(convex).Any(p => p.Intersect(convex)) ? Color.yellow : Color.white;
                convex.DrawGizmos();
            }
            
            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Translate(Vector3.back * 3f);
            var convex3D = new IConvex3D[] { m_Box + kfloat3.up * math.sin(UTime.time), m_Triangle };
            foreach (var convex in convex3D)
            {
                Gizmos.color = convex3D.Exclude(convex).Any(p => p.Intersect(convex)) ? Color.yellow : Color.white;
                convex.DrawGizmos();
            }
        }
    }

}
