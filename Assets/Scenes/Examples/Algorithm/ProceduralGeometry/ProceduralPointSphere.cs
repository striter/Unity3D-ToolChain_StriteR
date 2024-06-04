using Runtime.Geometry.Explicit;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Examples.Algorithm.Procedural
{
    public class ProceduralPointSphere : MonoBehaviour
    {
        [Clamp(0,128)] public int kAxixResolution = 12;
        [Clamp(0,128)] public int kPolygonRhombusCount = 4;
        [Clamp(0,128)] public int kUVSphereResolution = 12 * 12 * 3;
        [Clamp(0,128)] public int kFibonacciResolution = 12 * 12 * 3;
        
        private void OnDrawGizmos()
        {
            float r = kUVSphereResolution;
            Gizmos.matrix = transform.localToWorldMatrix;
            for (int i = 0; i <= kUVSphereResolution ; i ++)
                for(int j = 0 ; j <= kUVSphereResolution*2 ; j++)
                    Gizmos.DrawSphere(USphereExplicit.UV.GetPoint( new float2( i / r , j/r*2)),.02f);
            UGizmos.DrawString(Vector3.zero,"UV Sphere");
            
            r = kAxixResolution;
            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Translate(Vector3.right*3f);

            for (int k = 0; k < UCubeExplicit.kCubeFacingAxisCount; k++)
            {
                var axis = UCubeExplicit.GetFacingAxis(k);
                for(int i = 0 ; i <= kAxixResolution ; i ++)
                for(int j = 0 ; j <= kAxixResolution ; j++)
                    Gizmos.DrawSphere(USphereExplicit.CubeToSpherePosition(axis.GetPoint(new float2( i / r , j/ r))),.02f);
            }
            UGizmos.DrawString(Vector3.zero,"Cube Sphere");
            
            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Translate(Vector3.right*6f);
            for (int k = 0; k < kPolygonRhombusCount; k++)
            {
                var axis = UCubeExplicit.GetOctahedronRhombusAxis(k,kPolygonRhombusCount);
                for(int i = 0 ; i <= kAxixResolution ; i ++)
                for(int j = 0 ; j <= kAxixResolution ; j++)
                    Gizmos.DrawSphere(USphereExplicit.Polygon.GetPoint(new float2( i / r , j/ r),axis,false),.02f);
            }
            UGizmos.DrawString(Vector3.zero,"Poly Sphere");
            
            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Translate(Vector3.right*9f);
            for (int k = 0; k < kPolygonRhombusCount; k++)
            {
                var axis = UCubeExplicit.GetOctahedronRhombusAxis(k,kPolygonRhombusCount);
                for(int i = 0 ; i <= kAxixResolution ; i ++)
                for(int j = 0 ; j <= kAxixResolution ; j++)
                    Gizmos.DrawSphere(USphereExplicit.Polygon.GetPoint(new float2( i / r , j/ r),axis,true),.02f);
            }
            UGizmos.DrawString(Vector3.zero,"Poly Sphere Geodesic");
            
            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Translate(Vector3.right*12f);
            for (int i = 0; i < kFibonacciResolution; i++)
            {
                Gizmos.color = Color.Lerp(Color.white,KColor.kOrange,(float)i/kFibonacciResolution);
                Gizmos.DrawSphere(ULowDiscrepancySequences.FibonacciSphere(i,kFibonacciResolution),.02f);
            }
            
            UGizmos.DrawString(Vector3.zero,"Fibonacci Sphere");
        }
    }

}
