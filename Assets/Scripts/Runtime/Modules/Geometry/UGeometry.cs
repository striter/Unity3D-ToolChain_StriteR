using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Geometry.Voxel;
using UnityEngine;

namespace Geometry
{
    public static class UQuad
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
        public static Y GetVertex<T,Y>(this T _quad, int _corner) where T:IQuad<Y> where Y : struct
        {   
            return GetVertex<T,Y>(_quad, IndexToCorner(_corner));
        }
        public static Y GetVertex<T,Y>(this T _quad, EQuadCorner _corner) where T:IQuad<Y> where Y : struct
        {
            switch (_corner)
            {
                default: throw new Exception("Invalid Corner:"+_corner);
                case EQuadCorner.B: return _quad.vB;
                case EQuadCorner.L: return _quad.vL;
                case EQuadCorner.F: return _quad.vF;
                case EQuadCorner.R: return _quad.vR;
            }
        }

        public static (T v0, T v1) ToRelativeVertex<T>(this EQuadFacing _patch, IQuad<T> _quad) where T : struct
        {
            var patch = GetRelativeVertIndexesCW(_patch);
            return (_quad[patch.i0], _quad[patch.i1]);
        }
        
        public static T GetBaryCenter<T>(this IQuad<T> _quad) where T:struct
        {
            dynamic vertex0 = _quad.vB;
            dynamic vertex1 = _quad.vL;
            dynamic vertex2 = _quad.vF;
            dynamic vertex3 = _quad.vR;
            return (vertex0+vertex1+vertex2+vertex3)/4;
        }
        
        public static (T vBL, T vLF, T vFR, T vRB, T vC) GetQuadMidVertices<T>(this IQuad<T> _quad) where T:struct
        {
            dynamic vB = _quad.vB;
            dynamic vL = _quad.vL;
            dynamic vF = _quad.vF;
            dynamic vR = _quad.vR;
            return ((vB + vL) / 2, (vL + vF) / 2, (vF + vR)/2,(vR+vB)/2,(vB+vL+vF+vR)/4);
        }

        public static IEnumerable<(Y vB,Y vL,Y vF,Y vR)> SplitToQuads<T,Y>(this T _quad) where T:IQuad<Y> where Y:struct
        {
            var vB = _quad.vB;
            var vL = _quad.vL;
            var vF = _quad.vF;
            var vR = _quad.vR;
            var midTuple = _quad.GetQuadMidVertices();
                
            var vBL = midTuple.vBL;
            var vLF = midTuple.vLF;
            var vFR = midTuple.vFR;
            var vRB = midTuple.vRB;
            var vC = midTuple.vC;
                
            yield return (vB,vBL,vC,vRB);   //B
            yield return (vBL,vL,vLF,vC);   //L
            yield return (vC,vLF,vF,vFR);   //F
            yield return (vRB,vC,vFR,vR);   //R
        }
        
        public static bool IsPointInside<T> (this IQuad<T> _quad,T _point) where T:struct
        { 
            dynamic A = _quad.vB;
            dynamic B = _quad.vL;
            dynamic C = _quad.vF;
            dynamic D = _quad.vR;
            dynamic point = _point;
            dynamic x = point.x;
            dynamic y = point.y;
            int a = (int)Mathf.Sign((B.x - A.x) * (y - A.y) - (B.y - A.y) * (x - A.x));
            int b = (int)Mathf.Sign((C.x - B.x) * (y - B.y) - (C.y - B.y) * (x - B.x));
            int c = (int)Mathf.Sign((D.x - C.x) * (y - C.y) - (D.y - C.y) * (x - C.x));
            int d = (int)Mathf.Sign((A.x - D.x) * (y - D.y) - (A.y - D.y) * (x - D.x));
            return Mathf.Abs( a + b + c + d) == 4;
        }

        public static bool Contains<T>(this IQuad<T> _quad, T _element) where T : struct,IEquatable<T>
        {
            for (int i = 0; i < 4; i++)
                if (_element.Equals(_quad[i]))
                    return true;
            return false;
        }
        public static int MatchVertexCount<T>(this IQuad<T> _quad1,IQuad<T> _quad2) where T:struct,IEquatable<T>
        {
            int index=0;
            for(int i=0;i<4;i++)
            {
                var vertex = _quad1[i];
                if (_quad2.Contains(vertex))
                    index++;
            }
            return index;
        }

        public static int NearestPointIndex<T>(this IQuad<T> _quad, T _point) where T:struct
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
    }
    
    public static class UTriangle
    {
        public static (T m01, T m12, T m20, T m012) GetMiddleVertices<T>(this ITriangle<T> _triangle) where T:struct
        {
            dynamic v0 = _triangle.vertex0;
            dynamic v1 = _triangle.vertex1;
            dynamic v2 = _triangle.vertex2;
            return ((v0 + v1) / 2, (v1 + v2) / 2, (v2 + v0) / 2, (v0 + v1 + v2) / 3);
        }

        public static T GetBaryCenter<T>(this ITriangle<T> _triangle) where T:struct
        {
            dynamic vertex0 = _triangle.vertex0;
            dynamic vertex1 = _triangle.vertex1;
            dynamic vertex2 = _triangle.vertex2;
            return (vertex0+vertex1+vertex2)/3;
        }

        public static bool Contains<T>(this ITriangle<T> _triangle, T _element) where T : struct,IEquatable<T>
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
                if (_triangle2.Contains(vertex))
                    index++;
            }
            return index;
        }

        public static IEnumerable<(T v0, T v1, T v2,T v3)> SplitToQuads<Y,T>(this Y splitTriangle) where Y:ITriangle<T> where T:struct
        {
            var index0 = splitTriangle.vertex0;
            var index1 = splitTriangle.vertex1;
            var index2 = splitTriangle.vertex2;
            
            var midTuple = splitTriangle.GetMiddleVertices();
            var index01 = midTuple.m01;
            var index12 = midTuple.m12;
            var index20 = midTuple.m20;
            var index012 = midTuple.m012;
                
            yield return (index0,index01,index012,index20);
            yield return (index01, index1, index12, index012);
            yield return (index20, index012, index12, index2);
        }
        public static (T v1,T v2,T v3,T v4) CombineTriangle<T>(this ITriangle<T> _triangle1,ITriangle<T> _triangle2) where T:struct,IEquatable<T>
        {
            int diff1=0;
            for (; diff1 < 3; diff1++)
            {
                var element= _triangle1[diff1];
                if (!_triangle2.Contains(element))
                    break;
            }
            
            int diff2=0;
            for (; diff2 < 3; diff2++)
            {
                var element= _triangle2[diff2];
                if (!_triangle1.Contains(element))
                    break;
            }
            
            return (_triangle1[diff1],_triangle1[(diff1+1)%3],_triangle2[diff2],_triangle1[(diff1+2)%3]);
        }
    }
}