using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geometry
{
    
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