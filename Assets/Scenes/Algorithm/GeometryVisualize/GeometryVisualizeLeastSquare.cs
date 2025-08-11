using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;
using Gizmos = UnityEngine.Gizmos;

namespace Examples.Algorithm.GeometryVisualize
{
    public class GeometryVisualizeLeastSquare : MonoBehaviour
    {
        [Header("3D")]
        public float3[] randomPoints;
        public EBoundsShape shape = EBoundsShape.Ellipsoid;

        [Header("2D")]
        public float2[] randomPoints2D;
        
        [InspectorButton]
        void GenerateRandomPoints()
        {
            for (int i = 0; i < randomPoints.Length; i++)
                randomPoints[i] = URandom.RandomSphere();
            
            for (int i = 0; i < randomPoints2D.Length; i++)
                randomPoints2D[i] = URandom.Random2DSphere();
        }

        public enum EBoundsShape
        {
            OrientedBox,
            Sphere,
            Ellipsoid,
        }

        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;

            Gizmos.color = Color.white;
            foreach (var point in randomPoints)
                Gizmos.DrawSphere(point,.01f);
            
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.white;
            GPlane.LeastSquaresRegression(randomPoints).DrawGizmos(.5f);
            var coordinates = GCoordinates.PrincipleComponentAnalysis(randomPoints);
            coordinates.DrawGizmos();

            Gizmos.color = Color.white;
            switch (shape)
            {
                case EBoundsShape.OrientedBox:
                {
                    var box = GBox.GetBoundingBoxOriented( coordinates.right,coordinates.up,coordinates.forward,randomPoints);
                    Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.TRS(Vector3.zero,Quaternion.LookRotation(coordinates.forward,coordinates.up),Vector3.one);
                    box.DrawGizmos();
                }
                    break;
                case EBoundsShape.Ellipsoid:
                {
                    var ellipsoid = GEllipsoid.FromPoints(randomPoints);
                    ellipsoid.DrawGizmos();
                }
                    break;
                case EBoundsShape.Sphere:
                {
                    Gizmos.matrix = transform.localToWorldMatrix;
                    var sphere = GSphere.GetBoundingSphere(randomPoints);
                    sphere.DrawGizmos();
                }
                    break;
            }
            
            
            //2D Gizmos
            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Translate(kfloat3.right * 3f);
            var coordinates2D = G2Coordinates.PrincipleComponentAnalysis(randomPoints2D);
            coordinates2D.DrawGizmos();
            Gizmos.color = Color.white;
            foreach (var point in randomPoints2D)
                Gizmos.DrawSphere(point.to3xz(),.01f);
            G2Plane.LeastSquaresRegression(randomPoints2D).DrawGizmos(1f);
        }
        

    }
}