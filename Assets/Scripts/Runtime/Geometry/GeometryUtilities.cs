using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Geometry
{
    public static class UGeometry
    {
        public static Vector3 GetBaryCenter(this GQuad _quad)
        {
            var vertex0 = _quad.B;
            var vertex1 = _quad.L;
            var vertex2 = _quad.F;
            var vertex3 = _quad.R;
            return (vertex0+vertex1+vertex2+vertex3)/4;
        }

        public static Vector2 GetPoint(this Quad<Vector2> _quad, float _u,float _v)=>UMath.BilinearLerp(_quad.vB, _quad.vL, _quad.vF, _quad.vR, _u,_v);
        public static float GetPoint(this Quad<float> _quad, float _u,float _v)=>UMath.BilinearLerp(_quad.vB, _quad.vL, _quad.vF, _quad.vR, _u,_v);
        
        public static Quad<Vector3> Resize(this Quad<Vector3> _quad, float _shrinkScale) 
        {
            var vertex0 = _quad.vB;
            var vertex1 = _quad.vL;
            var vertex2 = _quad.vF;
            var vertex3 = _quad.vR;
            _quad.vB = vertex0 * _shrinkScale;
            _quad.vL = vertex1 * _shrinkScale;
            _quad.vF = vertex2 * _shrinkScale;
            _quad.vR = vertex3 * _shrinkScale;
            return _quad;
        }
        
        public static Qube<Vector3> Resize(this Qube<Vector3> _qube, float _shrinkScale)
        {
            var db = _qube.vDB;
            var dl = _qube.vDL;
            var df = _qube.vDF;
            var dr = _qube.vDR;
            var tb = _qube.vTB;
            var tl = _qube.vTL;
            var tf = _qube.vTF;
            var tr = _qube.vTR;
            _qube.vDB = db * _shrinkScale;
            _qube.vDL = dl * _shrinkScale;
            _qube.vDF = df * _shrinkScale;
            _qube.vDR = dr * _shrinkScale;
            _qube.vTB = tb * _shrinkScale;
            _qube.vTL = tl * _shrinkScale;
            _qube.vTF = tf * _shrinkScale;
            _qube.vTR = tr * _shrinkScale;
            return _qube;
        }

        
        public static (Vector3 vBL, Vector3 vLF, Vector3 vFR, Vector3 vRB, Vector3 vC) GetQuadMidVertices(this IQuad<Vector3> _quad)
        {
            var vB = _quad.B;
            var vL = _quad.L;
            var vF = _quad.F;
            var vR = _quad.R;
            return ((vB + vL) / 2, (vL + vF) / 2, (vF + vR)/2,(vR+vB)/2,(vB+vL+vF+vR)/4);
        }

        public static IEnumerable<Quad<Vector3>> SplitToQuads(this IQuad<Vector3> _quad,bool _insideOut)
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
                yield return new Quad<Vector3>(vC, vRB, vB, vBL);    //B
                yield return new Quad<Vector3>(vC, vBL, vL, vLF);    //L
                yield return new Quad<Vector3>(vC, vLF, vF, vFR);    //F
                yield return new Quad<Vector3>(vC, vFR, vR, vRB);    //R
            }
            else   //Forwarded 
            {
                yield return new Quad<Vector3>(vB,vBL,vC,vRB);   //B
                yield return new Quad<Vector3>(vBL,vL,vLF,vC);   //L
                yield return new Quad<Vector3>(vC,vLF,vF,vFR);   //F
                yield return new Quad<Vector3>(vRB,vC,vFR,vR);   //R
            }
        }
        
        public static IEnumerable<Quad<Vector3>> SplitTopDownQuads(this GQuad _quad)
        {
            var vB = _quad.B;
            var vL = _quad.L;
            var vF = _quad.F;
            var vR = _quad.R;
            var midTuple = _quad.GetQuadMidVertices();
                
            var vLF = midTuple.vLF;
            var vRB = midTuple.vRB;
            
            yield return new Quad<Vector3>(vB,vL,vLF,vRB);
            yield return new Quad<Vector3>(vRB,vLF,vF,vR);
        }

        public static Qube<Vector3> ExpandToQube<T>(this T _quad, Vector3 _expand, float _baryCenter = 0) where T : IQuad<Vector3>
        {
            var expand = _expand * (1 - _baryCenter);
            var shrink = _expand * _baryCenter;

            return new Qube<Vector3>(_quad.B - shrink, _quad.L - shrink, _quad.F - shrink, _quad.R - shrink,
                             _quad.B + expand, _quad.L + expand, _quad.F + expand, _quad.R + expand);
        }
        public static IEnumerable<Qube<Vector3>> SplitToQubes(this Quad<Vector3> _quad, Vector3 _halfSize, bool insideOut)
        {
            var quads = _quad.SplitToQuads(insideOut).ToArray();
            foreach (var quad in quads)
                yield return new Quad<Vector3>(quad.vB, quad.vL, quad.vF, quad.vR).ExpandToQube(_halfSize, 1f);
            foreach (var quad in quads)
                yield return new Quad<Vector3>(quad.vB, quad.vL, quad.vF, quad.vR).ExpandToQube(_halfSize, 0f);
        }

        public static Quad<T> GetQuad<T>(this CubeSides<T> _sides) => new Quad<T>(_sides.fBL, _sides.fLF, _sides.fFR, _sides.fRB);

        public static void FillFacingQuadTriangle(this Qube<Vector3> _qube, ECubeFacing _facing, List<Vector3> _vertices, List<int> _indices,
            List<Vector2> _uvs, List<Vector3> _normals, List<Color> _colors = null, Color _color = default)
        {
            new GQuad(_qube.GetFacingCornersCW(_facing)).FillQuadTriangle(_vertices, _indices, _uvs, _normals, _colors, _color);
        }
        public static void FillFacingSplitQuadTriangle(this Qube<Vector3> _qube, ECubeFacing _facing, List<Vector3> _vertices, List<int> _indices,
            List<Vector2> _uvs, List<Vector3> _normals, List<Color> _colors = null, Color _color = default)
        {
            foreach (var quad in new GQuad(_qube.GetFacingCornersCW(_facing)).SplitToQuads(true))
                new GQuad(quad.B, quad.L, quad.F, quad.R).FillQuadTriangle(_vertices, _indices, _uvs, _normals, _colors, _color);
        }
        public static void FillTopDownQuadTriangle(this Qube<Vector3> _qube, ECubeFacing _facing, List<Vector3> _vertices, List<int> _indices,
            List<Vector2> _uvs, List<Vector3> _normals, List<Color> _colors = null, Color _color = default)
        {
            foreach (var quad in new GQuad(_qube.GetFacingCornersCW(_facing)).SplitTopDownQuads())
                new GQuad(quad.B, quad.L, quad.F, quad.R).FillQuadTriangle(_vertices, _indices, _uvs, _normals, _colors, _color);
        }
        
    }
}