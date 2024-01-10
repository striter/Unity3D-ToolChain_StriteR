using System.Collections.Generic;
using Runtime.Geometry;
using Unity.Mathematics;
using UnityEngine;

namespace TechToys.ThePlanet
{
    public static class UGrid
    {
        public static TrapezoidQuad ConstructGeometry(this TrapezoidQuad _quad, int[] _indexes,EQuadGeometry _geometry)
        {
            var v0 = _quad.positions[_indexes[0]];
            var v1 = _quad.positions[_indexes[1]];
            var v2 = _quad.positions[_indexes[2]];
            var v3 = _quad.positions[_indexes[3]];
            
            var n0 = _quad.normals[_indexes[0]];
            var n1 = _quad.normals[_indexes[1]];
            var n2 = _quad.normals[_indexes[2]];
            var n3 = _quad.normals[_indexes[3]];
            
            if (_geometry == EQuadGeometry.Half)
            {
                var v01 = (v0 + v1) / 2;
                var vC =  (v0+v1+v2+v3)/4;
                var v30 = (v3 + v0) / 2;

                var n01 = ((n0 + n1) / 2).normalize();
                var nC =  ((n0 + n1 + n2 + n3)/4).normalize();
                var n30 = ((n3 + n0) / 2).normalize();

                return new TrapezoidQuad(new Quad<float3>(v0, v01, vC, v30),new Quad<float3>(n0,n01,nC,n30));
            }
            return new TrapezoidQuad(new Quad<float3>(v0, v1, v2, v3),new Quad<float3>(n0,n1,n2,n3));
        }
        
        public static IEnumerable<Vector3> ExpandToQube(this TrapezoidQuad _quad,float3 _center,int _unitHeight,float _baryCenter = .5f)
        {
            float expand = (_unitHeight + 1f - _baryCenter) * KPCG.kUnitSize*2;
            float shrink = (_unitHeight - _baryCenter) * KPCG.kUnitSize*2;

            yield return _quad.positions.vB + shrink * _quad.normals.vB -_center;
            yield return _quad.positions.vL + shrink * _quad.normals.vL -_center;
            yield return _quad.positions.vF + shrink * _quad.normals.vF -_center;
            yield return _quad.positions.vR + shrink * _quad.normals.vR -_center;
            yield return _quad.positions.vB + expand * _quad.normals.vB -_center;
            yield return _quad.positions.vL + expand * _quad.normals.vL -_center;
            yield return _quad.positions.vF + expand * _quad.normals.vF -_center;
            yield return _quad.positions.vR + expand * _quad.normals.vR -_center;
        }

        public static TrapezoidQuad Expand(this TrapezoidQuad _quad, float _height)
        {
            var offset = _quad.normal * _height;
            return new TrapezoidQuad(_quad.positions.Convert((i,p)=>p+_quad.normals[i]*_height-offset),_quad.normals);
        }
        
        public static IEnumerable<TrapezoidQuad> SplitToQuads(this TrapezoidQuad _quad,bool _insideOut,float _height = 0)
        {
            var vB = _quad.positions.B;
            var vL = _quad.positions.L;
            var vF = _quad.positions.F;
            var vR = _quad.positions.R;
            var midPositions = _quad.positions.GetQuadMidVertices();
            var vBL = midPositions.vBL;
            var vLF = midPositions.vLF;
            var vFR = midPositions.vFR;
            var vRB = midPositions.vRB;
            var vC = midPositions.vC;
            
            var nB = _quad.normals.B;
            var nL = _quad.normals.L;
            var nF = _quad.normals.F;
            var nR = _quad.normals.R;
            var midNormals = _quad.normals.GetQuadMidVertices();
            var nBL = midNormals.vBL.normalize();
            var nLF = midNormals.vLF.normalize();
            var nFR = midNormals.vFR.normalize();
            var nRB = midNormals.vRB.normalize();
            var nC = midNormals.vC.normalize();

            var offset = _quad.normal * _height;
            if (_insideOut) 
            {
                yield return new TrapezoidQuad(new Quad<float3>(vC, vRB, vB, vBL),new Quad<float3>(nC, nRB, nB, nBL),_height,offset);    //B
                yield return new TrapezoidQuad(new Quad<float3>(vC, vBL, vL, vLF),new Quad<float3>(nC, nBL, nL, nLF),_height,offset);    //L
                yield return new TrapezoidQuad(new Quad<float3>(vC, vLF, vF, vFR),new Quad<float3>(nC, vLF, nF, nFR),_height,offset);    //F
                yield return new TrapezoidQuad(new Quad<float3>(vC, vFR, vR, vRB),new Quad<float3>(nC, nFR, nR, nRB),_height,offset);    //R
            }
            else   //Forwarded 
            {
                yield return new TrapezoidQuad(new Quad<float3>(vB,vBL,vC,vRB),new Quad<float3>(nB,nBL,nC,nRB),_height,offset);   //B
                yield return new TrapezoidQuad(new Quad<float3>(vBL,vL,vLF,vC),new Quad<float3>(nBL,nL,nLF,nC),_height,offset);   //L
                yield return new TrapezoidQuad(new Quad<float3>(vC,vLF,vF,vFR),new Quad<float3>(nC,nLF,nF,nFR),_height,offset);   //F
                yield return new TrapezoidQuad(new Quad<float3>(vRB,vC,vFR,vR),new Quad<float3>(nRB,nC,nFR,nR),_height,offset);   //R
            }
        }
    }
    
}