using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geometry
{
    
    public static class UTriangle
    {
        public static Triangle<Y> Convert<T,Y>(this ITriangle<T> _srcQuad, Func<T, Y> _convert) where T:struct where Y:struct =>
            new Triangle<Y>(_convert(_srcQuad.V0), _convert(_srcQuad.V1), _convert(_srcQuad.V2));
        
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