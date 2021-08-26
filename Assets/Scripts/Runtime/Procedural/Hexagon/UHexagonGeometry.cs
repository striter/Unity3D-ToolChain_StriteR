using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Procedural.Hexagon.Geometry
{
    public static class UHexagonGeometry
    {
        public static (PHexCube m01, PHexCube m12, PHexCube m20, PHexCube m012) GetTriangleMidVertices(this HexTriangle _triangle)
        {
            var v0 = _triangle.vertex0;
            var v1 = _triangle.vertex1;
            var v2 = _triangle.vertex2;
            
            return ((v0 + v1) / 2, (v1 + v2) / 2, (v2 + v0) / 2, (v0 + v1 + v2) / 3);
        }
        public static (PHexCube m01, PHexCube m12, PHexCube m23, PHexCube m30,PHexCube m0123) GetQuadMidVertices(this HexQuad _quad)
        {
            var v0 = _quad.vertex0;
            var v1 = _quad.vertex1;
            var v2 = _quad.vertex2;
            var v3 = _quad.vertex3;
            return ((v0 + v1) / 2, (v1 + v2) / 2, (v2 + v3)/2,(v3+v0)/2,(v0+v1+v2+v3)/4);
        }

        public static int MatchVertexCount(this HexTriangle _triangle1,HexTriangle _triangle2)
        {
            return _triangle1.vertices.Collect(k => _triangle2.vertices.Contains(k)).Count() ;
        }

        public static HexQuad CombineTriangle(HexTriangle _triangle1,HexTriangle _triangle2)
        {
            var mid = _triangle1.vertices.Collect(p => _triangle2.vertices.Contains(p)).ToArray();
            var diff1 = _triangle1.vertices.Find(p => !mid.Contains(p));
            var diff2 = _triangle2.vertices.Find(p => !mid.Contains(p));
            return new HexQuad(diff1,mid[0],diff2,mid[1]);
        }
    }
}