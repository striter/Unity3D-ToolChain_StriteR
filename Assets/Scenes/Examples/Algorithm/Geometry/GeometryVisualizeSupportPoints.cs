using System.Linq;
using Runtime.Geometry;
using Runtime.Geometry.Validation;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

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
        
        [Header("3D")]
        public GTriangle triangle3d = GTriangle.kDefault;
        public GSphere sphere3D = GSphere.kDefault;
        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            var shapes = new IShape2D[] { box, sphere, polygon ,triangle};
            foreach (var shape in shapes)
            {
                Gizmos.color = shapes.Exclude(shape).Any(p=> p.Intersect(shape)) ? Color.yellow : Color.white;
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
            GJKAlgorithm._2D.Sum(sumCircle,finalPolygon).DrawGizmos();
            Gizmos.color = KColor.kHotPink;
            GJKAlgorithm._2D.Difference(sumCircle,finalPolygon).DrawGizmos();


            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Translate(Vector3.back * 6f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(Vector3.zero, .05f);
            Gizmos.color = GJKAlgorithm.Intersect(triangle3d, sphere3D) ? Color.green : Color.white;
            triangle3d.DrawGizmos();
            sphere3D.DrawGizmos();
            Gizmos.color = KColor.kHotPink;
            GJKAlgorithm._3D.Difference(triangle3d,sphere3D,256).DrawGizmos();
        }
    }
}