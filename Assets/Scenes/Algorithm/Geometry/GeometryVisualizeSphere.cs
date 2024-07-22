using Runtime.Geometry.Explicit;
using Unity.Mathematics;
using UnityEngine;

namespace Examples.Algorithm.Procedural
{
    public class GeometryVisualizeSphere : MonoBehaviour
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
            for (int j = 0; j <= kUVSphereResolution * 2; j++)
            {
                var uv = new float2(i / r, j / r);
                Gizmos.color = (Color.red * uv.x + Color.green * uv.y).SetA(1f);
                Gizmos.DrawSphere(USphereExplicit.UV.Cube( uv),.02f);
            }
            UGizmos.DrawString(Vector3.zero,"UV");
            
            Gizmos.matrix *= Matrix4x4.Translate(Vector3.right*3f);
            for (int i = 0; i <= kUVSphereResolution ; i ++)
            for (int j = 0; j <= kUVSphereResolution; j++)
            {
                var uv = new float2(i / r, j / r);
                Gizmos.color = (Color.red * uv.x + Color.green * uv.y).SetA(1f);
                Gizmos.DrawSphere(USphereExplicit.UV.Octahedral(uv),.02f);
            }
            UGizmos.DrawString(Vector3.zero,"Octahedral");
            
            
            Gizmos.matrix *= Matrix4x4.Translate(Vector3.right*3f);
            for (int i = 0; i <= kUVSphereResolution ; i ++)
            for (int j = 0; j <= kUVSphereResolution; j++)
            {
                var uv = new float2(i / r, j / r);
                Gizmos.color = Color.red * uv.x + Color.green * uv.y;
                Gizmos.DrawSphere(USphereExplicit.UV.ConcentricOctahedral(uv),.02f);
            }
            UGizmos.DrawString(Vector3.zero,"Concentric Octahedral");

            r = kAxixResolution;
            Gizmos.matrix *= Matrix4x4.Translate(Vector3.right*3f);

            for (int k = 0; k < UCubeExplicit.kCubeFacingAxisCount; k++)
            {
                Gizmos.color = UColor.IndexToColor(k);
                var axis = UCubeExplicit.GetFacingAxis(k);
                for(int i = 0 ; i <= kAxixResolution ; i ++)
                for(int j = 0 ; j <= kAxixResolution ; j++)
                    Gizmos.DrawSphere(USphereExplicit.CubeToSpherePosition(axis.GetPoint(new float2( i / r , j/ r))),.02f);
            }
            UGizmos.DrawString(Vector3.zero,"Cube");
            
            Gizmos.matrix *= Matrix4x4.Translate(Vector3.right*3f);
            for (int k = 0; k < kPolygonRhombusCount; k++)
            {
                Gizmos.color = UColor.IndexToColor(k);
                var axis = UCubeExplicit.GetOctahedronRhombusAxis(k,kPolygonRhombusCount);
                for(int i = 0 ; i <= kAxixResolution ; i ++)
                for(int j = 0 ; j <= kAxixResolution ; j++)
                    Gizmos.DrawSphere(USphereExplicit.Polygon.GetPoint(new float2( i / r , j/ r),axis,false),.02f);
            }
            UGizmos.DrawString(Vector3.zero,"Poly");
            
            Gizmos.matrix *= Matrix4x4.Translate(Vector3.right*3f);
            for (int k = 0; k < kPolygonRhombusCount; k++)
            {
                Gizmos.color = UColor.IndexToColor(k);
                var axis = UCubeExplicit.GetOctahedronRhombusAxis(k,kPolygonRhombusCount);
                for(int i = 0 ; i <= kAxixResolution ; i ++)
                for(int j = 0 ; j <= kAxixResolution ; j++)
                    Gizmos.DrawSphere(USphereExplicit.Polygon.GetPoint(new float2( i / r , j/ r),axis,true),.02f);
            }
            UGizmos.DrawString(Vector3.zero,"Poly Geodesic");
            
            Gizmos.matrix *= Matrix4x4.Translate(Vector3.right*3f);
            for (int i = 0; i < kFibonacciResolution; i++)
            {
                Gizmos.color = Color.Lerp(Color.white,KColor.kOrange,(float)i/kFibonacciResolution);
                Gizmos.DrawSphere(USphereExplicit.LowDiscrepancySequences.Fibonacci(i,kFibonacciResolution),.02f);
            }
            UGizmos.DrawString(Vector3.zero,"Fibonacci");
        }
    }

}
