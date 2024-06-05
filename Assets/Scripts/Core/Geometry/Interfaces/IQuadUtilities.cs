using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry
{
    public class KQuad
    {
        public static readonly Quad<float2> kUV = new Quad<float2>(
             new float2(0,0),
             new float2(0,1),
             new float2(1,1),
             new float2(1,0)
            );
        public static readonly Quad<bool> kFalse = new Quad<bool>(false, false, false, false);
        public static readonly Quad<bool> kTrue = new Quad<bool>(true, true, true, true);
        
        public static readonly Quad<float3> k3SquareCentered = new Quad<float3>( Vector3.right+Vector3.back,Vector3.back+Vector3.left, Vector3.left+Vector3.forward ,Vector3.forward+Vector3.right).Resize(.5f);
        public static readonly Quad<float3> k3SquareBottomLeft = new Quad<float3>(Vector3.zero,Vector3.forward,Vector3.forward+Vector3.right,Vector3.right);
        public static readonly Quad<float3> k3SquareCentered45Deg = new Quad<float3>(Vector3.back,Vector3.left,Vector3.forward,Vector3.right);
        
        public static readonly Quad<float2> k2SquareCentered = k3SquareCentered.Convert(p=>p.xz);
    }
    
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

        public static float2 GetUV(this Quad<float2>  _quad, float2 _position) => umath.invBilinearLerp(_quad.vB,_quad.vL,_quad.vF,_quad.vR,_position);
        public static float2 GetPoint(this Quad<float2> _quad, float _u,float _v)=>umath.bilinearLerp(_quad.vB, _quad.vL, _quad.vF, _quad.vR, _u,_v);
        public static float GetPoint(this Quad<float> _quad, float _u,float _v)=>umath.bilinearLerp(_quad.vB, _quad.vL, _quad.vF, _quad.vR, _u,_v);
        
        
        public static Quad<float3> Resize(this Quad<float3> _quad, float _shrinkScale) 
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
        
        public static Qube<float3> Resize(this Qube<float3> _qube, float _shrinkScale)
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
        
        public static (float3 vBL, float3 vLF, float3 vFR, float3 vRB, float3 vC) GetQuadMidVertices(this IQuad<float3> _quad)
        {
            var vB = _quad.B;
            var vL = _quad.L;
            var vF = _quad.F;
            var vR = _quad.R;
            return ((vB + vL) / 2, (vL + vF) / 2, (vF + vR)/2,(vR+vB)/2,(vB+vL+vF+vR)/4);
        }

        public static IEnumerable<Quad<float3>> SplitToQuads(this IQuad<float3> _quad,bool _insideOut)
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
                yield return new Quad<float3>(vC, vRB, vB, vBL);    //B
                yield return new Quad<float3>(vC, vBL, vL, vLF);    //L
                yield return new Quad<float3>(vC, vLF, vF, vFR);    //F
                yield return new Quad<float3>(vC, vFR, vR, vRB);    //R
            }
            else   //Forwarded 
            {
                yield return new Quad<float3>(vB,vBL,vC,vRB);   //B
                yield return new Quad<float3>(vBL,vL,vLF,vC);   //L
                yield return new Quad<float3>(vC,vLF,vF,vFR);   //F
                yield return new Quad<float3>(vRB,vC,vFR,vR);   //R
            }
        }
        
        public static IEnumerable<Quad<float3>> SplitTopDownQuads(this Quad<float3> _quad)
        {
            var vB = _quad.B;
            var vL = _quad.L;
            var vF = _quad.F;
            var vR = _quad.R;
            var midTuple = _quad.GetQuadMidVertices();
                
            var vLF = midTuple.vLF;
            var vRB = midTuple.vRB;
            
            yield return new Quad<float3>(vB,vL,vLF,vRB);
            yield return new Quad<float3>(vRB,vLF,vF,vR);
        }

        public static Qube<float3> ExpandToQube<T>(this T _quad, float3 _expand, float _baryCenter = 0) where T : IQuad<float3>
        {
            var expand = _expand * (1 - _baryCenter);
            var shrink = _expand * _baryCenter;

            return new Qube<float3>(_quad.B - shrink, _quad.L - shrink, _quad.F - shrink, _quad.R - shrink,
                             _quad.B + expand, _quad.L + expand, _quad.F + expand, _quad.R + expand);
        }
        
        public static IEnumerable<Qube<float3>> SplitToQubes(this Quad<float3> _quad, Vector3 _halfSize, bool insideOut)
        {
            var quads = _quad.SplitToQuads(insideOut).ToArray();
            foreach (var quad in quads)
                yield return new Quad<float3>(quad.vB, quad.vL, quad.vF, quad.vR).ExpandToQube(_halfSize, 1f);
            foreach (var quad in quads)
                yield return new Quad<float3>(quad.vB, quad.vL, quad.vF, quad.vR).ExpandToQube(_halfSize, 0f);
        }

        public static float3 GetBaryCenter(this IQuad<float3> _quad)
        {
            var vertex0 = _quad.B;
            var vertex1 = _quad.L;
            var vertex2 = _quad.F;
            var vertex3 = _quad.R;
            return (vertex0+vertex1+vertex2+vertex3)/4;
        }
        
        public static Vector2 GetPoint_Dynamic<T>(this Quad<T> _quad, float _u,float _v)
        {
            dynamic B = _quad.B;
            dynamic L = _quad.L;
            dynamic F = _quad.F;
            dynamic R = _quad.R;

            return new Vector2(
                umath.bilinearLerp( B.x,  L.x, F.x,  R.x, _u, _v),
                umath.bilinearLerp(  B.y, L.y, F.y, R.y, _u, _v)
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

        public static Quad<T> Shrink_Dynamic<T>(this Quad<T> _quad, float _shrinkScale,T _center)
        {
            dynamic center = _center;
            dynamic vertex0 = _quad.vB - center;
            dynamic vertex1 = _quad.vL - center;
            dynamic vertex2 = _quad.vF - center;
            dynamic vertex3 = _quad.vR - center;
            _quad.vB = _center + vertex0 * _shrinkScale;
            _quad.vL = _center + vertex1 * _shrinkScale;
            _quad.vF = _center + vertex2 * _shrinkScale;
            _quad.vR = _center + vertex3 * _shrinkScale;
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

        public static Quad<float3> Shrink(this IQuad<float3>  _quad, float _scale)
        {
            var center = _quad.GetBaryCenter();
            return new Quad<float3>(
                center + (_quad.B - center)*_scale,
                center + (_quad.L - center)*_scale,
                center + (_quad.F - center)*_scale,
                center + (_quad.R - center)*_scale 
                );
        }
        
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
}