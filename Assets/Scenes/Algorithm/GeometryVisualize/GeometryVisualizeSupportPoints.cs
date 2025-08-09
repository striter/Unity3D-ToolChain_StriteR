using System.Linq;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using System.Linq.Extensions;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Examples.Algorithm.GeometryVisualize
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
            var shapes = new IGeometry2[] { box + kfloat2.up * math.sin(UTime.time), sphere+ kfloat2.up * math.sin(UTime.time * 2), polygon ,triangle};
            foreach (var shape in shapes)
            {
                Gizmos.color = shapes.Exclude(shape).Any(p=> p.Intersect(shape)) ? Color.yellow : Color.white;
                shape.DrawGizmos();
                UGizmos.DrawArrow(shape.Origin.to3xz(), supportPointDirection.to3xz(), .5f, .1f);
                Gizmos.DrawWireSphere(shape.GetSupportPoint(supportPointDirection).to3xz(), .1f);
            }

            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Translate(Vector3.back*3f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(Vector3.zero, .1f);
            var finalCircle = sumCircle + kfloat2.right * math.sin(UTime.time);
            var finalPolygon = sumPolygon + sumPolygonOffset;
            Gizmos.color = finalPolygon.Intersect( finalCircle) ? Color.green : Color.white;
            finalCircle.DrawGizmos();
            finalPolygon.DrawGizmos();

            Gizmos.color = KColor.kChocolate;
            GJK.Sum(finalPolygon,finalCircle).DrawGizmos();
            Gizmos.color = KColor.kHotPink;
            GJK.Difference(finalPolygon,finalCircle).DrawGizmos();

            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Translate(Vector3.back * 6f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(Vector3.zero, .05f);
            var finalSphere3D = sphere3D + kfloat3.right * math.sin(UTime.time);
            var finalTriangle = Matrix4x4.Rotate(quaternion.Euler(kfloat3.right * math.sin(UTime.time)  * kmath.kPI2)) * triangle3d;
            
            Gizmos.color = finalTriangle.Intersect(finalSphere3D) ? Color.green : Color.white;
            finalTriangle.DrawGizmos();
            finalSphere3D.DrawGizmos();
            Gizmos.color = KColor.kHotPink;
            UGizmos.DrawPoints(GJK.Difference(finalTriangle, finalSphere3D, 256),.01f);
        }
    }
}