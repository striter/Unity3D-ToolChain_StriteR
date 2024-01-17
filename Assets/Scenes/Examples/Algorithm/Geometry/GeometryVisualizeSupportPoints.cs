using System.Linq;
using Runtime.Geometry;
using Runtime.Geometry.Validation;
using Unity.Mathematics;
using UnityEngine;

namespace Examples.Algorithm.Geometry
{
    public class GeometryVisualizeSupportPoints : MonoBehaviour
    {
        [Header("Intersect")]
        public G2Box box = G2Box.kDefault;
        public G2Polygon polygon = G2Polygon.kDefault;
        public G2Circle sphere = G2Circle.kOne;
        public G2Triangle triangle = G2Triangle.kDefault;
        [PostNormalize] public float2 supportPointDirection = kfloat2.up;
        
        [Header("Sum")]
        public G2Circle sumCircle = G2Circle.kOne;
        public G2Polygon sumPolygon = G2Polygon.kDefault;
        public float2 sumPolygonOffset = float2.zero;
        
        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            int index = 0;
            var shapes = new IShape2D[] { box, sphere, polygon ,triangle};
            foreach (var shape in shapes)
            {
                Gizmos.color = index++ != 0 && shapes.Exclude(shape).Any(p=> p.Intersect(shape)) ? Color.yellow : Color.white;
                shape.DrawGizmos();
                UGizmos.DrawArrow(shape.Center.to3xz(), supportPointDirection.to3xz(), .5f, .1f);
                Gizmos.DrawWireSphere(shape.GetSupportPoint(supportPointDirection).to3xz(), .1f);
            }

            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Translate(Vector3.back*3f);
            var finalPolygon = sumPolygon + sumPolygonOffset;
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(Vector3.zero, .1f);
            Gizmos.color = GJKAlgorithm.Intersect(sumCircle, finalPolygon) ? Color.green : Color.white;
            sumCircle.DrawGizmos();
            finalPolygon.DrawGizmos();

            Gizmos.color = KColor.kChocolate;
            GJKAlgorithm.Sum(sumCircle,finalPolygon).DrawGizmos();
            Gizmos.color = KColor.kHotPink;
            GJKAlgorithm.Difference(sumCircle,finalPolygon).DrawGizmos();
        }
    }
}