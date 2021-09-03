using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geometry
{
    public static class UQuad
    {
        public static T GetBaryCenter<T>(this IQuad<T> _quad) where T:struct
        {
            dynamic vertex0 = _quad.vertex0;
            dynamic vertex1 = _quad.vertex1;
            dynamic vertex2 = _quad.vertex2;
            dynamic vertex3 = _quad.vertex3;
            return (vertex0+vertex1+vertex2+vertex3)/4;
        }
        
        public static (T m01, T m12, T m23, T m30,T m0123) GetQuadMidVertices<T>(this IQuad<T> _quad) where T:struct
        {
            dynamic v0 = _quad.vertex0;
            dynamic v1 = _quad.vertex1;
            dynamic v2 = _quad.vertex2;
            dynamic v3 = _quad.vertex3;
            return ((v0 + v1) / 2, (v1 + v2) / 2, (v2 + v3)/2,(v3+v0)/2,(v0+v1+v2+v3)/4);
        }

        public static bool IsPointInside<T> (this IQuad<T> _quad,T _point) where T:struct
        { 
            dynamic A = _quad.vertex0;
            dynamic B = _quad.vertex1;
            dynamic C = _quad.vertex2;
            dynamic D = _quad.vertex3;
            dynamic point = _point;
            dynamic x = point.x;
            dynamic y = point.y;
            int a = (int)Mathf.Sign((B.x - A.x) * (y - A.y) - (B.y - A.y) * (x - A.x));
            int b = (int)Mathf.Sign((C.x - B.x) * (y - B.y) - (C.y - B.y) * (x - B.x));
            int c = (int)Mathf.Sign((D.x - C.x) * (y - C.y) - (D.y - C.y) * (x - C.x));
            int d = (int)Mathf.Sign((A.x - D.x) * (y - D.y) - (A.y - D.y) * (x - D.x));
            return Mathf.Abs( a + b + c + d) == 4;
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

        
        public static (Vector3 u, Vector3 v) GetUVDirection<T>(this ITriangle<T> _triangle) where T:struct
        {
            dynamic vertex0 = _triangle.vertex0;
            dynamic vertex1 = _triangle.vertex1;
            dynamic vertex2 = _triangle.vertex2;
            return (vertex1-vertex0,vertex2-vertex0);
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
            // var a=_triangle[0];
            // var b=_triangle[1];
            // var c = _triangle[2];
            for (int i = 0; i < 3; i++)
            {
                if (_element.Equals(_triangle[i]))
                    return true;
            }
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