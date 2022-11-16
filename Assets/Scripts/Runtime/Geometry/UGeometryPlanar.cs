using System;
using System.Collections.Generic;
using UnityEngine;

namespace Geometry
{
    public static partial class UQuad
    {
        public static EQuadCorner IndexToCorner(int _index)
        {
            switch (_index)
            {
                default: throw new Exception("Invalid Corner Index:"+_index);
                case 0: return EQuadCorner.B;
                case 1: return EQuadCorner.L;
                case 2: return EQuadCorner.F;
                case 3: return EQuadCorner.R;
            }
        }
        
        public static int CornerToIndex(this EQuadCorner corner)
        {
            switch (corner)
            {
                default: throw new Exception("Invalid Corner:"+corner);
                case EQuadCorner.B: return 0;
                case EQuadCorner.L: return 1;
                case EQuadCorner.F: return 2;
                case EQuadCorner.R: return 3;
            }
        }

        public static (EQuadCorner i0, EQuadCorner i1) GetRelativeVertIndexesCW(this EQuadFacing _facing)
        {
            switch (_facing)
            {
                default: throw new Exception("Invalid Face:"+_facing);
                case EQuadFacing.BL: return (EQuadCorner.B,EQuadCorner.L);
                case EQuadFacing.LF: return (EQuadCorner.L,EQuadCorner.F);
                case EQuadFacing.FR: return (EQuadCorner.F,EQuadCorner.R);
                case EQuadFacing.RB: return (EQuadCorner.R,EQuadCorner.B);
            }
        }
        
        public static (T v0, T v1) ToRelativeVertex<T>(this EQuadFacing _patch, Quad<T> _quad)
        {
            var patch = GetRelativeVertIndexesCW(_patch);
            return (_quad[patch.i0], _quad[patch.i1]);
        }

        public static Vector2 GetUV(this Quad<Vector2>  _quad, Vector2 _position)
        {
            return UMath.InvBilinearLerp(_quad.vB,_quad.vL,_quad.vF,_quad.vR,_position);
        }

        
        public static Vector2 GetPoint_Dynamic<T>(this Quad<T> _quad, float _u,float _v)
        {
            dynamic B = _quad.B;
            dynamic L = _quad.L;
            dynamic F = _quad.F;
            dynamic R = _quad.R;

            return new Vector2(
                UMath.BilinearLerp(B.x, L.x, F.x, R.x, _u, _v),
                UMath.BilinearLerp(B.y, L.y, F.y, R.y, _u, _v)
            );
        }

        
        public static Quad<T> Resize_Dynamic<T>(this Quad<T> _quad, float _shrinkScale) 
        {
            dynamic vertex0 = _quad.vB;
            dynamic vertex1 = _quad.vL;
            dynamic vertex2 = _quad.vF;
            dynamic vertex3 = _quad.vR;
            _quad.vB = vertex0 * _shrinkScale;
            _quad.vL = vertex1 * _shrinkScale;
            _quad.vF = vertex2 * _shrinkScale;
            _quad.vR = vertex3 * _shrinkScale;
            return _quad;
        }

        public static T GetBaryCenter_Dynamic<T>(this Quad<T> _quad)
        {
            dynamic vertex0 = _quad.vB;
            dynamic vertex1 = _quad.vL;
            dynamic vertex2 = _quad.vF;
            dynamic vertex3 = _quad.vR;
            return (vertex0+vertex1+vertex2+vertex3)/4;
        }
        
        public static (Y vBL, Y vLF, Y vFR, Y vRB, Y vC) GetQuadMidVertices_Dynamic<T,Y>(this T _quad) where T:IQuad<Y> 
        {
            dynamic vB = _quad.B;
            dynamic vL = _quad.L;
            dynamic vF = _quad.F;
            dynamic vR = _quad.R;
            return ((vB + vL) / 2, (vL + vF) / 2, (vF + vR)/2,(vR+vB)/2,(vB+vL+vF+vR)/4);
        }

        public static IEnumerable<Quad<Y>> SplitToQuads_Dynamic<T,Y>(this T _quad,bool _insideOut) where T:IQuad<Y> 
        {
            var vB = _quad.B;
            var vL = _quad.L;
            var vF = _quad.F;
            var vR = _quad.R;
            var midTuple = _quad.GetQuadMidVertices_Dynamic<T,Y>();
                
            var vBL = midTuple.vBL;
            var vLF = midTuple.vLF;
            var vFR = midTuple.vFR;
            var vRB = midTuple.vRB;
            var vC = midTuple.vC;

            if (_insideOut) 
            {
                yield return new Quad<Y>(vC, vRB, vB, vBL);    //B
                yield return new Quad<Y>(vC, vBL, vL, vLF);    //L
                yield return new Quad<Y>(vC, vLF, vF, vFR);    //F
                yield return new Quad<Y>(vC, vFR, vR, vRB);    //R
            }
            else   //Forwarded 
            {
                yield return new Quad<Y>(vB,vBL,vC,vRB);   //B
                yield return new Quad<Y>(vBL,vL,vLF,vC);   //L
                yield return new Quad<Y>(vC,vLF,vF,vFR);   //F
                yield return new Quad<Y>(vRB,vC,vFR,vR);   //R
            }
        }

        
        public static IEnumerable<Quad<Y>> SplitTopDownQuads_Dynamic<T, Y>(this T _quad) where T : IQuad<Y>
        {
            var vB = _quad.B;
            var vL = _quad.L;
            var vF = _quad.F;
            var vR = _quad.R;
            var midTuple = _quad.GetQuadMidVertices_Dynamic<T,Y>();
                
            var vLF = midTuple.vLF;
            var vRB = midTuple.vRB;
            
            yield return new Quad<Y>(vB,vL,vLF,vRB);
            yield return new Quad<Y>(vRB,vLF,vF,vR);
        }
        

        public static IEnumerable<Triangle<T>> SplitToTriangle<T>(this IQuad<T> splitQuad, T v0, T v1)  where T:struct
        {
            for (int i = 0; i < 4; i++)
            {
                if (splitQuad[i].Equals(v0))
                {
                    yield return new Triangle<T>(splitQuad[i], splitQuad[(i + 1) % 4], splitQuad[(i + 2) % 4]);
                }

                if (splitQuad[i].Equals(v1))
                {
                    yield return new Triangle<T>(splitQuad[i], splitQuad[(i + 1) % 4], splitQuad[(i + 2) % 4]);
                }
            }
        }

        public static Quad<Y> Convert<T,Y>(this IQuad<T> _srcQuad, Func<T, Y> _convert)
        {
            return new Quad<Y>(_convert(_srcQuad.B), _convert(_srcQuad.L), _convert(_srcQuad.F),
                _convert(_srcQuad.R));
        }
        
        public static Quad<Y> Convert<T,Y>(this IQuad<T> _srcQuad, Func<int,T, Y> _convert)
        {
            return new Quad<Y>(_convert(0,_srcQuad.B), _convert(1,_srcQuad.L), _convert(2,_srcQuad.F),
                _convert(3,_srcQuad.R));
        }
        
        public static bool IsPointInside_Dynamic<T> (this IQuad<T> _quad,T _point) 
        { 
            dynamic A = _quad.B;
            dynamic B = _quad.L;
            dynamic C = _quad.F;
            dynamic D = _quad.R;
            dynamic point = _point;
            dynamic x = point.x;
            dynamic y = point.y;
            int a = (int)Mathf.Sign((B.x - A.x) * (y - A.y) - (B.y - A.y) * (x - A.x));
            int b = (int)Mathf.Sign((C.x - B.x) * (y - B.y) - (C.y - B.y) * (x - B.x));
            int c = (int)Mathf.Sign((D.x - C.x) * (y - C.y) - (D.y - C.y) * (x - C.x));
            int d = (int)Mathf.Sign((A.x - D.x) * (y - D.y) - (A.y - D.y) * (x - D.x));
            return Mathf.Abs( a + b + c + d) == 4;
        }

        public static bool MatchVertex<T>(this IQuad<T> _quad, T _element) where T :IEquatable<T>
        {
            for (int i = 0; i < 4; i++)
                if (_element.Equals(_quad[i]))
                    return true;
            return false;
        }
        
        public static int MatchVertexCount<T>(this IQuad<T> _quad1,IQuad<T> _quad2) where T:IEquatable<T>
        {
            int index=0;
            for(int i=0;i<4;i++)
            {
                var vertex = _quad1[i];
                if (_quad2.MatchVertex(vertex))
                    index++;
            }
            return index;
        }

        public static int NearestPointIndex_Dynamic<T>(this Quad<T> _quad, T _point) 
        {
            float minDistance = float.MaxValue;
            int minIndex = 0;
            dynamic srcPoint = _point;
            for (int i = 0; i < 4; i++)
            {
                dynamic dstPoint = _quad[i];
                dynamic offset = srcPoint - dstPoint;
                var sqrDistance = offset.x*offset.x+offset.y*offset.y;
                if(minDistance<sqrDistance)
                    continue;
                minIndex = i;
                minDistance = sqrDistance;
            }
            return minIndex;
        }

        public static Quad<T> RotateYawCW<T>(this Quad<T> _quad,int _rotateIndex)
        {
            var b = _quad.vB;
            var l = _quad.vL;
            var f = _quad.vF;
            var r = _quad.vR;
            switch (_rotateIndex)
            {
                default: throw new IndexOutOfRangeException();
                case 0: return _quad;
                case 1: return new Quad<T>(r,b,l,f);
                case 2: return new Quad<T>(f,r,b,l);
                case 3: return new Quad<T>(l,f,r,b);
            }
        }

        public static Quad<T> MirrorLR<T>(this Quad<T> _quad) => new Quad<T>(_quad.vB,_quad.vR,_quad.vF,_quad.vL);
        
        public static void SetByteElement(ref this Quad<bool> _qube, byte _byte)
        {
            for (int i = 0; i < 4; i++)
                _qube[i] = UByte.PosValid(_byte,i);
        }
        public static byte ToByte(this Quad<bool> _qube)
        {
            return UByte.ToByte(_qube[0],_qube[1],_qube[2],_qube[3],
                false,false,false,false);
        }
    }
    
    public static class UTriangle
    {
        public static (Y m01, Y m12, Y m20, Y m012) GetMiddleVertices_Dynamic<T,Y>(this T _triangle) where T:struct,ITriangle<Y> where Y:struct
        {
            dynamic v0 = _triangle.V0;
            dynamic v1 = _triangle.V1;
            dynamic v2 = _triangle.V2;
            return ((v0 + v1) / 2, (v1 + v2) / 2, (v2 + v0) / 2, (v0 + v1 + v2) / 3);
        }

        public static T GetBaryCenter_Dynamic<T>(this Triangle<T> _triangle) where T:struct
        {
            dynamic vertex0 = _triangle.v0;
            dynamic vertex1 = _triangle.v1;
            dynamic vertex2 = _triangle.v2;
            return (vertex0+vertex1+vertex2)/3;
        }

        public static bool MatchVertex<T>(this ITriangle<T> _triangle, T _element) where T : struct,IEquatable<T>
        {
            for (int i = 0; i < 3; i++)
                if (_element.Equals(_triangle[i]))
                    return true;
            return false;
        }
        
        public static int MatchVertexCount<T>(this ITriangle<T> _triangle1,ITriangle<T> _triangle2) where T:struct,IEquatable<T>
        {
            int index=0;
            for(int i=0;i<3;i++)
            {
                var vertex = _triangle1[i];
                if (_triangle2.MatchVertex(vertex))
                    index++;
            }
            return index;
        }
        public static IEnumerable<Quad<Y>> SplitToQuads_Dynamic<T,Y>(this T splitTriangle) where T:struct,ITriangle<Y> where Y:struct
        {
            var index0 = splitTriangle[0];
            var index1 = splitTriangle[1];
            var index2 = splitTriangle[2];
            
            var midTuple = splitTriangle.GetMiddleVertices_Dynamic<T,Y>();
            var index01 = midTuple.m01;
            var index12 = midTuple.m12;
            var index20 = midTuple.m20;
            var index012 = midTuple.m012;
                
            yield return new Quad<Y>(index0,index01,index012,index20);
            yield return new Quad<Y>(index01, index1, index12, index012);
            yield return new Quad<Y>(index20, index012, index12, index2);
        }
        public static Quad<T> CombineTriangle<T>(this ITriangle<T> _triangle1,ITriangle<T> _triangle2) where T:struct,IEquatable<T>
        {
            int diff1=0;
            for (; diff1 < 3; diff1++)
            {
                var element= _triangle1[diff1];
                if (!_triangle2.MatchVertex(element))
                    break;
            }
            
            int diff2=0;
            for (; diff2 < 3; diff2++)
            {
                var element= _triangle2[diff2];
                if (!_triangle1.MatchVertex(element))
                    break;
            }
            
            return new Quad<T>(_triangle1[diff1],_triangle1[(diff1+1)%3],_triangle2[diff2],_triangle1[(diff1+2)%3]);
        }
    }
    
}