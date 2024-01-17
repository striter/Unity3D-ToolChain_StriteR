using System.Runtime.InteropServices;
using Unity.Mathematics;
namespace Runtime.Geometry.Explicit.Shape
{
    using static math;
    
    public interface IShapeExplicit
    {
        Point4 GetPoint(int _i, float _resolution, float _invResolution);

        static float4x2 IndexTo4UV(int _i, float _resolution, float _invResolution)
        {
            float4x2 uv;
            float4 i4 = 4f * _i + new float4(0, 1, 2, 3);
            uv.c1 = floor(_invResolution * i4 + 0.00001f);
            uv.c0 = _invResolution * (i4 - _resolution * uv.c1 + 0.5f) ;
            uv.c1 = _invResolution * (uv.c1 + 0.5f);
            return uv;
        }
    }

    public struct SPlane :IShapeExplicit
    {
        public Point4 GetPoint(int _i, float _resolution, float _invResolution)
        {
            float4x2 uv = IShapeExplicit.IndexTo4UV(_i,_resolution,_invResolution);
            return new Point4() {positions = new float4x3(uv.c0-.5f, 0, uv.c1-.5f),normals = new float4x3(0,1,0)};
        }
    }

    public struct SSphere : IShapeExplicit
    {
        public Point4 GetPoint(int _i, float _resolution, float _invResolution)
        {
            float4x2 uv = IShapeExplicit.IndexTo4UV(_i,_resolution,_invResolution);
            float4x3 p;
            p.c0 = uv.c0 - 0.5f;
            p.c1 = uv.c1 - 0.5f;
            p.c2 = 0.5f - abs(p.c0) - abs(p.c1);
            float4 offset = max(-p.c2, 0f);
            p.c0 += select(-offset,offset,p.c0<0f);
            p.c1 += select(-offset,offset,p.c1<0f);
            float4 scale = 0.5f * rsqrt(p.c0 * p.c0 + p.c1 * p.c1 + p.c2 * p.c2);
            p.c0 *= scale;
            p.c1 *= scale;
            p.c2 *= scale;
            return new Point4(){positions = p,normals = p};
        }
    }

    public struct STorus : IShapeExplicit
    {
        public Point4 GetPoint(int _i, float _resolution, float _invResolution)
        {            
            float4x2 uv = IShapeExplicit.IndexTo4UV(_i,_resolution,_invResolution);
            float r1 = 0.375f;
            float r2 = 0.125f;
            float4 s = r1 + r2 * cos(kmath.kPI2 * uv.c1);
            float4x3 p;
            p.c0 = s * sin(kmath.kPI2 * uv.c0);
            p.c1 = r2 * sin(kmath.kPI2 * uv.c1);
            p.c2 = s * cos(kmath.kPI2 * uv.c0);

            float4x3 n = p;
            n.c0 -= r1 * sin(kmath.kPI2 * uv.c0);
            n.c2 -= r1 * cos(kmath.kPI2 * uv.c0);
            return new Point4(){positions = p,normals = n};
        }
    }
}
        