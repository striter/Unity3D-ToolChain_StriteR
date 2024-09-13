using Runtime.Geometry;
using Unity.Mathematics;
using UnityEngine;

namespace Procedural
{
    public static class UProcedural
    {
        static Matrix4x4 TransformMatrix = Matrix4x4.identity;
        static Matrix4x4 InvTransformMatrix = Matrix4x4.identity;

        public static void InitMatrix(Matrix4x4 _transformMatrix,Matrix4x4 _invTransformMatrix, float _scale)
        {
            TransformMatrix = _transformMatrix * Matrix4x4.Scale(_scale * Vector3.one);
            InvTransformMatrix = Matrix4x4.Scale(Vector3.one / _scale)*_invTransformMatrix ;
        }
        
        public static float3 ToPosition(this Coord _pixel)
        {
            return TransformMatrix.MultiplyPoint(new float3(_pixel.x, 0, _pixel.y));
        }

        public static Coord ToCoord(this Vector3 _world)
        {
            _world = InvTransformMatrix.MultiplyPoint(_world);
            return new Coord(_world.x,  _world.z);
        }
        
        public static Coord Lerp(this Coord _src, Coord _dst, float _value)
        {
            return _src + (_dst-_src) * _value;
        }
        

        public static Coord GetPoint(this Quad<Coord> _quad, float _u,float _v)
        {
            return new Coord(
                umath.bilinearLerp(_quad.B.x, _quad.L.x, _quad.F.x, _quad.R.x, _u, _v),
                umath.bilinearLerp(_quad.B.y, _quad.L.y, _quad.F.y, _quad.R.y, _u, _v)
            );
        }
        
        public static bool IsPointInside(this IQuad<Coord> _quad,Coord _point) 
        { 
            var A = _quad.B;
            var B = _quad.L;
            var C = _quad.F;
            var D = _quad.R;
            var point = _point;
            var x = point.x;
            var y = point.y;
            int a = (int)Mathf.Sign((B.x - A.x) * (y - A.y) - (B.y - A.y) * (x - A.x));
            int b = (int)Mathf.Sign((C.x - B.x) * (y - B.y) - (C.y - B.y) * (x - B.x));
            int c = (int)Mathf.Sign((D.x - C.x) * (y - C.y) - (D.y - C.y) * (x - C.x));
            int d = (int)Mathf.Sign((A.x - D.x) * (y - D.y) - (A.y - D.y) * (x - D.x));
            return Mathf.Abs( a + b + c + d) == 4;
        }

        public static bool IsPointInsideOrOnSegment(this IQuad<Coord> _quad, Coord _point)
        {
            var A = _quad.B;
            var B = _quad.L;
            var C = _quad.F;
            var D = _quad.R;
            var point = _point;
            var x = point.x;
            var y = point.y;
            int a = (int)Mathf.Sign((B.x - A.x) * (y - A.y) - (B.y - A.y) * (x - A.x));
            int b = (int)Mathf.Sign((C.x - B.x) * (y - B.y) - (C.y - B.y) * (x - B.x));
            int c = (int)Mathf.Sign((D.x - C.x) * (y - C.y) - (D.y - C.y) * (x - C.x));
            int d = (int)Mathf.Sign((A.x - D.x) * (y - D.y) - (A.y - D.y) * (x - D.x));
            if (Mathf.Abs(a + b + c + d) == 4)
                return true;

            bool onSegment(Coord Pi, Coord Pj, Coord Q)
            {
                if ((Q.x - Pi.x) * (Pj.y - Pi.y) == (Pj.x - Pi.x) * (Q.y - Pi.y)
                   && Mathf.Min(Pi.x, Pj.x) <= Q.x && Q.x <= Mathf.Max(Pi.x, Pj.x)
                   && Mathf.Min(Pi.y, Pj.y) <= Q.y && Q.y <= Mathf.Max(Pi.y, Pj.y))
                    return true;
                else
                    return false;
            }
            if (onSegment(A, B, _point))
                return true;
            if (onSegment(B, C, _point))
                return true;
            if (onSegment(C, D, _point))
                return true;
            if (onSegment(D, A, _point))
                return true;
            return false;
        }
    }

}