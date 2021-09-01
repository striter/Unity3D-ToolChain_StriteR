using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Procedural.Hexagon.Geometry
{

    public static class UHexagonGeometry
    {
        public static (HexagonCoordC m01, HexagonCoordC m12, HexagonCoordC m20, HexagonCoordC m012) GetTriangleMidVertices(this HexTriangle _triangle)
        {
            var v0 = _triangle.vertex0;
            var v1 = _triangle.vertex1;
            var v2 = _triangle.vertex2;
            
            return ((v0 + v1) / 2, (v1 + v2) / 2, (v2 + v0) / 2, (v0 + v1 + v2) / 3);
        }
        public static (HexagonCoordC m01, HexagonCoordC m12, HexagonCoordC m23, HexagonCoordC m30,HexagonCoordC m0123) GetQuadMidVertices(this HexQuad _quad)
        {
            var v0 = _quad.vertex0;
            var v1 = _quad.vertex1;
            var v2 = _quad.vertex2;
            var v3 = _quad.vertex3;
            return ((v0 + v1) / 2, (v1 + v2) / 2, (v2 + v3)/2,(v3+v0)/2,(v0+v1+v2+v3)/4);
        }

        public static int MatchVertexCount(this HexTriangle _triangle1,HexTriangle _triangle2)
        {
            int index=0;
            for(int i=0;i<_triangle1.Length;i++)
            {
                var vertex = _triangle1.GetElement(i);
                if (_triangle2.vertex0 == vertex || _triangle2.vertex1 == vertex || _triangle2.vertex2 == vertex)
                    index++;
            }
            return index;
        }

        public static HexQuad CombineTriangle(HexTriangle _triangle1,HexTriangle _triangle2)
        {
            var diff1 = _triangle1.FindIndex(p => !_triangle2.Contains(p));
            var diff2 = _triangle2.FindIndex(p => !_triangle1.Contains(p));
            return new HexQuad(_triangle1[diff1],_triangle1[(diff1+1)%3],_triangle2[diff2],_triangle1[(diff1+2)%3]);
        }
    }
}