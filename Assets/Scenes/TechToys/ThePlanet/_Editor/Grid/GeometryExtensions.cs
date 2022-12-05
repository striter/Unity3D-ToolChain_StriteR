using System.Collections.Generic;
using Procedural;
using Procedural.Hexagon;
using Procedural.Hexagon.Geometry;
using UnityEngine;

namespace Geometry      //Some copy from original codes
{
    public static partial class UQuad
    {
        //Coord
        public static (Coord vBL, Coord vLF, Coord vFR, Coord vRB, Coord vC) GetQuadMidVertices(this Quad<Coord> _quad)
        {
            var vB = _quad.B;
            var vL = _quad.L;
            var vF = _quad.F;
            var vR = _quad.R;
            return ((vB + vL) / 2, (vL + vF) / 2, (vF + vR)/2,(vR+vB)/2,(vB+vL+vF+vR)/4);
        }

        public static IEnumerable<Quad<Coord>> SplitToQuads(this Quad<Coord> _quad,bool _insideOut)
        {
            var vB = _quad.B;
            var vL = _quad.L;
            var vF = _quad.F;
            var vR = _quad.R;
            var midTuple = _quad.GetQuadMidVertices();
                
            var vBL = midTuple.vBL;
            var vLF = midTuple.vLF;
            var vFR = midTuple.vFR;
            var vRB = midTuple.vRB;
            var vC = midTuple.vC;

            if (_insideOut) 
            {
                yield return new Quad<Coord>(vC, vRB, vB, vBL);    //B
                yield return new Quad<Coord>(vC, vBL, vL, vLF);    //L
                yield return new Quad<Coord>(vC, vLF, vF, vFR);    //F
                yield return new Quad<Coord>(vC, vFR, vR, vRB);    //R
            }
            else   //Forwarded 
            {
                yield return new Quad<Coord>(vB,vBL,vC,vRB);   //B
                yield return new Quad<Coord>(vBL,vL,vLF,vC);   //L
                yield return new Quad<Coord>(vC,vLF,vF,vFR);   //F
                yield return new Quad<Coord>(vRB,vC,vFR,vR);   //R
            }
        }

        
        //Hexcoord
        
        public static bool IsPointInside<T> (this HexQuad _quad,HexCoord _point) 
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
        
        public static (HexCoord vBL, HexCoord vLF, HexCoord vFR, HexCoord vRB, HexCoord vC) GetQuadMidVertices(this HexQuad _quad) 
        {
            var vB = _quad.B;
            var vL = _quad.L;
            var vF = _quad.F;
            var vR = _quad.R;
            return ((vB + vL) / 2, (vL + vF) / 2, (vF + vR)/2,(vR+vB)/2,(vB+vL+vF+vR)/4);
        }

        public static IEnumerable<Quad<HexCoord>> SplitToQuads(this HexQuad _quad,bool _insideOut)
        {
            var vB = _quad.B;
            var vL = _quad.L;
            var vF = _quad.F;
            var vR = _quad.R;
            var midTuple = _quad.GetQuadMidVertices();
                
            var vBL = midTuple.vBL;
            var vLF = midTuple.vLF;
            var vFR = midTuple.vFR;
            var vRB = midTuple.vRB;
            var vC = midTuple.vC;

            if (_insideOut) 
            {
                yield return new Quad<HexCoord>(vC, vRB, vB, vBL);    //B
                yield return new Quad<HexCoord>(vC, vBL, vL, vLF);    //L
                yield return new Quad<HexCoord>(vC, vLF, vF, vFR);    //F
                yield return new Quad<HexCoord>(vC, vFR, vR, vRB);    //R
            }
            else   //Forwarded 
            {
                yield return new Quad<HexCoord>(vB,vBL,vC,vRB);   //B
                yield return new Quad<HexCoord>(vBL,vL,vLF,vC);   //L
                yield return new Quad<HexCoord>(vC,vLF,vF,vFR);   //F
                yield return new Quad<HexCoord>(vRB,vC,vFR,vR);   //R
            }
        }
        
        public static (HexCoord m01, HexCoord m12, HexCoord m20, HexCoord m012) GetMiddleVertices(this HexTriangle _triangle)
        {
            var v0 = _triangle.V0;
            var v1 = _triangle.V1;
            var v2 = _triangle.V2;
            return ((v0 + v1) / 2, (v1 + v2) / 2, (v2 + v0) / 2, (v0 + v1 + v2) / 3);
        }

        public static IEnumerable<Quad<HexCoord>> SplitToQuads(this HexTriangle splitTriangle) 
        {
            var index0 = splitTriangle[0];
            var index1 = splitTriangle[1];
            var index2 = splitTriangle[2];

            var midTuple = splitTriangle.GetMiddleVertices();
            var index01 = midTuple.m01;
            var index12 = midTuple.m12;
            var index20 = midTuple.m20;
            var index012 = midTuple.m012;
                
            yield return new Quad<HexCoord>(index0,index01,index012,index20);
            yield return new Quad<HexCoord>(index01, index1, index12, index012);
            yield return new Quad<HexCoord>(index20, index012, index12, index2);
        }
    }
}