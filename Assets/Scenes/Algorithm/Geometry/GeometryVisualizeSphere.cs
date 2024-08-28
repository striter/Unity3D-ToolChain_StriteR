using Runtime.Geometry.Explicit;
using Runtime.Geometry.Explicit.Sphere;
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
            var index = 0;
            float r = kUVSphereResolution;
            foreach (var mapping in UEnum.GetEnums<ESphereMapping>())
            {
                Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Translate(Vector3.right *index++*3f );
                for (int i = 0; i < kUVSphereResolution ; i ++)
                for (int j = 0; j < kUVSphereResolution; j++)
                {
                    var uv = (new float2(i , j ) + .5f) / r;
                    Gizmos.color = (Color.red * uv.x + Color.green * uv.y).SetA(1f);
                    if ((mapping.SphereToUV(mapping.UVToSphere(uv)) - uv).sqrmagnitude() > 0.0001f)
                        Gizmos.color = Color.magenta.SetA(.5f);
                    
                    Gizmos.DrawSphere(mapping.UVToSphere( uv),.02f);
                }
                
                UGizmos.DrawString(mapping.ToString(), Vector3.zero);
            }
            
            r = kAxixResolution;
            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Translate(Vector3.forward*3f);

            for (int k = 0; k < UCubeExplicit.kCubeFacingAxisCount; k++)
            {
                Gizmos.color = UColor.IndexToColor(k);
                var axis = UCubeExplicit.GetFacingAxis(k);
                for(int i = 0 ; i <= kAxixResolution ; i ++)
                for(int j = 0 ; j <= kAxixResolution ; j++)
                    Gizmos.DrawSphere(USphereExplicit.CubeToSpherePosition(axis.GetPoint(new float2( i / r , j/ r))),.02f);
            }
            UGizmos.DrawString("Cube", Vector3.zero);
            
            Gizmos.matrix *= Matrix4x4.Translate(Vector3.right*3f);
            for (int k = 0; k < kPolygonRhombusCount; k++)
            {
                Gizmos.color = UColor.IndexToColor(k);
                var axis = UCubeExplicit.GetOctahedronRhombusAxis(k,kPolygonRhombusCount);
                for(int i = 0 ; i <= kAxixResolution ; i ++)
                for(int j = 0 ; j <= kAxixResolution ; j++)
                    Gizmos.DrawSphere(Polygon.GetPoint(new float2( i / r , j/ r),axis,false),.02f);
            }
            UGizmos.DrawString("Poly", Vector3.zero);
            
            Gizmos.matrix *= Matrix4x4.Translate(Vector3.right*3f);
            for (int k = 0; k < kPolygonRhombusCount; k++)
            {
                Gizmos.color = UColor.IndexToColor(k);
                var axis = UCubeExplicit.GetOctahedronRhombusAxis(k,kPolygonRhombusCount);
                for(int i = 0 ; i <= kAxixResolution ; i ++)
                for(int j = 0 ; j <= kAxixResolution ; j++)
                    Gizmos.DrawSphere(Polygon.GetPoint(new float2( i / r , j/ r),axis,true),.02f);
            }
            UGizmos.DrawString("Poly Geodesic", Vector3.zero);
            
            Gizmos.matrix *= Matrix4x4.Translate(Vector3.right*3f);
            for (int i = 0; i < kFibonacciResolution; i++)
            {
                Gizmos.color = Color.Lerp(Color.white,KColor.kOrange,(float)i/kFibonacciResolution);
                Gizmos.DrawSphere(USphereExplicit.LowDiscrepancySequences.Fibonacci(i,kFibonacciResolution),.02f);
            }
            UGizmos.DrawString("Fibonacci", Vector3.zero);
        }
    }

}
