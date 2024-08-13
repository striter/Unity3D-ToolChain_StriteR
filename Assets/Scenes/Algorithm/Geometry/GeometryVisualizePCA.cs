using System.Collections.Generic;
using System.Linq;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;
using Gizmos = UnityEngine.Gizmos;

namespace Examples.Algorithm.Geometry
{
    public class GeometryVisualizePCA : MonoBehaviour
    {
        [Header("3D")]
        public float3[] randomPoints;
        public EBoundsShape shape = EBoundsShape.Ellipsoid;
        [Readonly] public float3 centre;
        [Readonly] public float3 right;
        [Readonly] public float3 forward;
        [Readonly] public float3 up;

        [Header("2D")]
        public float2[] randomPoints2D;
        [Readonly] public float2 centre2D;
        [Readonly] public float2 right2D;
        [Readonly] public float2 up2D;
        
        [InspectorButton]
        void GenerateRandomPoints()
        {
            for (int i = 0; i < randomPoints.Length; i++)
                randomPoints[i] = URandom.RandomSphere();
            
            for (int i = 0; i < randomPoints2D.Length; i++)
                randomPoints2D[i] = URandom.Random2DSphere();
            OnValidate();
        }

        public enum EBoundsShape
        {
            OrientedBox,
            Sphere,
            Ellipsoid,
        }
        
        private void OnValidate()
        {
            PrincipleComponentAnalysis.Evaluate(randomPoints,out centre,out right,out up,out forward);
            PrincipleComponentAnalysis.Evaluate(randomPoints2D,out centre2D,out right2D,out up2D);
        }

        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;

            Gizmos.color = Color.white;
            foreach (var point in randomPoints)
                Gizmos.DrawSphere(point,.01f);
            
            Gizmos.matrix = transform.localToWorldMatrix*Matrix4x4.Translate(centre);
            Gizmos.DrawCube(Vector3.zero,.1f*Vector3.one);
            Gizmos.color = Color.red;
            UGizmos.DrawArrow(Vector3.zero,right,1f,.05f);
            Gizmos.color = Color.green;
            UGizmos.DrawArrow(Vector3.zero,up,1f,.05f);
            Gizmos.color = Color.blue;
            UGizmos.DrawArrow(Vector3.zero,forward,1f,.05f);

            Gizmos.color = Color.white;
            switch (shape)
            {
                case EBoundsShape.OrientedBox:
                {
                    var boxright = math.cross(up,forward);
                    var box = UGeometry.GetBoundingBoxOriented( boxright,up,forward,randomPoints);
                    Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.TRS(Vector3.zero,Quaternion.LookRotation(forward,up),Vector3.one);
                    box.DrawGizmos();
                }
                    break;
                case EBoundsShape.Ellipsoid:
                {
                    var ellipsoid = UGeometry.GetBoundingEllipsoid(randomPoints);
                    ellipsoid.DrawGizmos();
                }
                    break;
                case EBoundsShape.Sphere:
                {
                    Gizmos.matrix = transform.localToWorldMatrix;
                    var sphere = UGeometry.GetBoundingSphere(randomPoints);
                    sphere.DrawGizmos();
                }
                    break;
            }
            
            
            //2D Gizmos
            Gizmos.matrix = transform.localToWorldMatrix*Matrix4x4.Translate(centre2D.to3xz() + kfloat3.right * 5f);
            Gizmos.color = Color.white;
            foreach (var point in randomPoints2D)
                Gizmos.DrawSphere(point.to3xz(),.01f);
            Gizmos.DrawCube(Vector3.zero,.1f*Vector3.one);
            Gizmos.color = Color.red;
            UGizmos.DrawArrow(Vector3.zero,right2D.to3xz(),1f,.05f);
            Gizmos.color = Color.green;
            UGizmos.DrawArrow(Vector3.zero,up2D.to3xz(),1f,.05f);
        }
        

    }
}