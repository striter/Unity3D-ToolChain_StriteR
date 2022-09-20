using System;
using System.Collections.Generic;
using System.ComponentModel;
using Geometry.Voxel;
using UnityEngine;

namespace Geometry
{
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

        public static Vector2 GetUV(this Quad<Vector2>  _quad, Vector2 _position)
        {
            return UMath.InvBilinearLerp(_quad.vB,_quad.vL,_quad.vF,_quad.vR,_position);
        }

        
        public static Vector2 GetPoint_Dynamic<T>(this Quad<T> _quad, float _u,float _v)
        {
            dynamic B = _quad.B;
            dynamic L = _quad.L;
            dynamic F = _quad.F;
            dynamic R = _quad.R;

            return new Vector2(
                UMath.BilinearLerp(B.x, L.x, F.x, R.x, _u, _v),
                UMath.BilinearLerp(B.y, L.y, F.y, R.y, _u, _v)
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
        

        public static IEnumerable<Triangle<T>> SplitToTriangle<T>(this IQuad<T> splitQuad, T v0, T v1)  where T:struct
        {
            for (int i = 0; i < 4; i++)
            {
                if (splitQuad[i].Equals(v0))
                {
                    yield return new Triangle<T>(splitQuad[i], splitQuad[(i + 1) % 4], splitQuad[(i + 2) % 4]);
                }

                if (splitQuad[i].Equals(v1))
                {
                    yield return new Triangle<T>(splitQuad[i], splitQuad[(i + 1) % 4], splitQuad[(i + 2) % 4]);
                }
            }
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
            
            return new Quad<T>(_triangle1[diff1],_triangle1[(diff1+1)%3],_triangle2[diff2],_triangle1[(diff1+2)%3]);
        }
    }
    
    public static class UQube
    {
        public static int CornerToIndex(this EQubeCorner _corners)
        {
            switch (_corners)
            {
                default: throw new Exception("Invalid Corner:"+_corners);
                case EQubeCorner.DB: return 0;
                case EQubeCorner.DL: return 1;
                case EQubeCorner.DF: return 2;
                case EQubeCorner.DR: return 3;
                case EQubeCorner.TB: return 4;
                case EQubeCorner.TL: return 5;
                case EQubeCorner.TF: return 6;
                case EQubeCorner.TR: return 7;
            }
        } 
        
        public static EQubeCorner IndexToCorner(int _index)
        {
            switch (_index)
            {
                default: throw new Exception("Invalid Corner:"+_index);
                case 0:return EQubeCorner.DB; 
                case 1:return EQubeCorner.DL;
                case 2:return EQubeCorner.DF;
                case 3:return EQubeCorner.DR;
                case 4:return EQubeCorner.TB;
                case 5:return EQubeCorner.TL;
                case 6:return EQubeCorner.TF;
                case 7:return EQubeCorner.TR;
            }
        }

        public static bool IsTopFloor(this EQubeCorner _corner)=> _corner.CornerToIndex() >= 4;
        
        public static EQubeCorner FlipFloor(this EQubeCorner _corner)
        {
            var _index = _corner.CornerToIndex();
            _index += _corner.IsTopFloor() ? -4 : 4;
            return IndexToCorner(_index);
        }

        public static EQubeCorner NextCornerFlooredCW(this EQubeCorner _corner,int _step)
        {
            var _index = _corner.CornerToIndex();
            var baseIndex = _corner.IsTopFloor() ? 4:0;
            _index -= baseIndex;
            _index += _step;
            _index %= 4;
            return IndexToCorner(baseIndex+_index);
        }

        public static EQubeCorner DiagonalCorner(this EQubeCorner _corner)
        {
            switch (_corner)
            {
                default: throw new Exception("Invalid Corner:"+_corner);
                case EQubeCorner.DB: return EQubeCorner.TF;
                case EQubeCorner.DL: return EQubeCorner.TR;
                case EQubeCorner.DF: return EQubeCorner.TB;
                case EQubeCorner.DR: return EQubeCorner.TL;
                case EQubeCorner.TB: return EQubeCorner.DF;
                case EQubeCorner.TL: return EQubeCorner.DR;
                case EQubeCorner.TF: return EQubeCorner.DB;
                case EQubeCorner.TR: return EQubeCorner.DL;
            }
        }

        public static EQubeCorner HorizontalDiagonalCorner(this EQubeCorner _corner)
        {
            switch (_corner)
            {
                default: throw new Exception("Invalid Corner:"+_corner);
                case EQubeCorner.DB: return EQubeCorner.DF;
                case EQubeCorner.DL: return EQubeCorner.DR;
                case EQubeCorner.DF: return EQubeCorner.DB;
                case EQubeCorner.DR: return EQubeCorner.DL;
                case EQubeCorner.TB: return EQubeCorner.TF;
                case EQubeCorner.TL: return EQubeCorner.TR;
                case EQubeCorner.TF: return EQubeCorner.TB;
                case EQubeCorner.TR: return EQubeCorner.TL;
            }
        }
        public static IEnumerable<(EQubeCorner _qube, EQubeCorner _adjactileCorner1, EQubeCorner _adjactileCorner2)> NearbyValidCornerQube(this EQubeCorner _srcCorner)
        {
            var flip = _srcCorner.FlipFloor();
            var qube0 =  _srcCorner.HorizontalDiagonalCorner();
            yield return (qube0,_srcCorner,_srcCorner.FlipFloor());

            var qube1 = flip.NextCornerFlooredCW(1);
            yield return (qube1,_srcCorner,_srcCorner.NextCornerFlooredCW(3));
            
            var qube2 = flip.NextCornerFlooredCW(3);
            yield return (qube2,_srcCorner,_srcCorner.NextCornerFlooredCW(1));
        }
        
        public static Qube<T> Resize_Dynamic<T>(this Qube<T> _qube, float _shrinkScale) where  T: struct
        {
            dynamic db = _qube.vDB;
            dynamic dl = _qube.vDL;
            dynamic df = _qube.vDF;
            dynamic dr = _qube.vDR;
            dynamic tb = _qube.vTB;
            dynamic tl = _qube.vTL;
            dynamic tf = _qube.vTF;
            dynamic tr = _qube.vTR;
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

        public static (Quad<T> _downQuad, Quad<T> _topQuad) SplitTopDownQuads<T>(this Qube<T> _qube)where T:struct
        {
            Quad<T> downQuad = new Quad<T>( _qube.vDB,_qube.vDL,_qube.vDF,_qube.vDR);;
            Quad<T> topQuad = new Quad<T>(_qube.vTB,_qube.vTL,_qube.vTF,_qube.vTR);
            return (downQuad, topQuad);
        }
        public static Qube<T> RotateYawCW<T>(this Qube<T> _qube,int _90DegMult) where T:struct
        {
            var quads = _qube.SplitTopDownQuads<T>();
            var top = quads._topQuad.RotateYawCW(_90DegMult);
            var down = quads._downQuad.RotateYawCW(_90DegMult);
            return new Qube<T>(down,top);
        }

        public static Qube<T> MirrorLR<T>(this Qube<T> _qube) where T:struct
        {
            var quads = _qube.SplitTopDownQuads<T>();
            return new Qube<T>(quads._downQuad.MirrorLR(),quads._topQuad.MirrorLR());
        }
        
        public static Qube<byte> SplitByteQubes(this Qube<bool> _qube,bool _fillHorizontalDiagonal)
        {
            Qube<bool>[] splitQubes = new Qube<bool>[8];
            for (int i = 0; i < 8; i++)
            {
                splitQubes[i] = default;
                splitQubes[i].SetByteElement(_qube[i]?byte.MaxValue:byte.MinValue);
            }

            foreach (var corner in UEnum.GetEnums<EQubeCorner>())
            {
                if(_qube[corner])
                    continue;

                var diagonal = corner.DiagonalCorner();
                
                if (_qube[diagonal])
                    splitQubes[diagonal.CornerToIndex()][corner]=false;

                foreach (var tuple in corner.NearbyValidQubeFacing())
                {
                    var qube = tuple._cornerQube;
                    var facing = tuple._facingDir;
                    if(!_qube[qube])
                        continue;
                    var qubeIndex = qube.CornerToIndex();
                    foreach (var facingCorner in facing.Opposite().FacingCorners())
                        splitQubes[qubeIndex][facingCorner] = false;
                }
                
                foreach (var tuple in corner.NearbyValidCornerQube())
                {
                    var qube = tuple._qube;
                    if(!_qube[qube])
                        continue;
                    splitQubes[qube.CornerToIndex()][tuple._adjactileCorner1]=false;
                    splitQubes[qube.CornerToIndex()][tuple._adjactileCorner2]=false;
                }
            }

            if (_fillHorizontalDiagonal)
            {
                int bottomValidCount = 0;
                int topValidCount = 0;
                for (int i = 0; i < 8; i++)
                {
                    if (!_qube[i])
                        continue;
                    if (i < 4)
                        bottomValidCount += 1;
                    else
                        topValidCount += 1;
                }

                for (int i = 0; i < 8; i++)
                {
                    if (i < 4)
                    {
                        if(bottomValidCount%2!=0)
                            continue;
                    }
                    else
                    {
                        if (topValidCount % 2 != 0)
                            continue;
                    }
                    
                    if(!_qube[i])
                        continue;
                    
                    var horizontalDiagonal = IndexToCorner(i).HorizontalDiagonalCorner();
                    if (!_qube[horizontalDiagonal])
                        continue;

                    splitQubes[i][horizontalDiagonal] = true;
                    splitQubes[horizontalDiagonal.CornerToIndex()][i] = true;
                }
            }

            Qube<byte> byteQube = new Qube<byte>(splitQubes[0].ToByte(),splitQubes[1].ToByte(),splitQubes[2].ToByte(),splitQubes[3].ToByte(),
                splitQubes[4].ToByte(),splitQubes[5].ToByte(),splitQubes[6].ToByte(),splitQubes[7].ToByte());
            return byteQube;
        }
        

        public static void SetByteElement(ref this Qube<bool> _qube, byte _byte)
        {
            for (int i = 0; i < 8; i++)
                _qube[i] = UByte.PosValid(_byte,i);
        }

        public static Qube<bool> ToQube(this byte _byte)
        {
            Qube<bool> qube = default;
            qube.SetByteElement(_byte);
            return qube;
        }
        public static byte ToByte(this Qube<bool> _qube)
        {
            return UByte.ToByte(_qube[0],_qube[1],_qube[2],_qube[3],
                _qube[4],_qube[5],_qube[6],_qube[7]);
        }
        
        public static Qube<bool> And(this Qube<bool> _srcQube,Qube<bool> _dstQube)=> Qube<bool>.Convert(_srcQube,(index,value)=>value&&_dstQube[index]);
    }

    public static class UCubeFacing
    {
        public static (EQubeCorner v0, EQubeCorner v1, EQubeCorner v2, EQubeCorner v3) GetRelativeVertsCW(this ECubeFacing _facing)
        {
            switch (_facing)
            {
                default: throw new Exception("Invalid Face:"+_facing);
                case ECubeFacing.BL:return (EQubeCorner.DB,EQubeCorner.DL,EQubeCorner.TL,EQubeCorner.TB);
                case ECubeFacing.LF:return (EQubeCorner.DL,EQubeCorner.DF,EQubeCorner.TF,EQubeCorner.TL);
                case ECubeFacing.FR:return (EQubeCorner.DF,EQubeCorner.DR,EQubeCorner.TR,EQubeCorner.TF);
                case ECubeFacing.RB:return (EQubeCorner.DR,EQubeCorner.DB,EQubeCorner.TB,EQubeCorner.TR);
                case ECubeFacing.T:return (EQubeCorner.TB,EQubeCorner.TL,EQubeCorner.TF,EQubeCorner.TR);
                case ECubeFacing.D:return (EQubeCorner.DB,EQubeCorner.DR,EQubeCorner.DF,EQubeCorner.DL);
            }
        }
        public static IEnumerable<(EQubeCorner _cornerQube, ECubeFacing _facingDir)> NearbyValidQubeFacing(this EQubeCorner _srcCorner)
        {
            switch (_srcCorner)
            {
                default: throw new IndexOutOfRangeException();
                
                case EQubeCorner.DB:
                    yield return (EQubeCorner.TB, ECubeFacing.T);
                    yield return (EQubeCorner.DL, ECubeFacing.LF);
                    yield return (EQubeCorner.DR, ECubeFacing.FR);
                    break;
                case EQubeCorner.DL:
                    yield return (EQubeCorner.TL, ECubeFacing.T);
                    yield return (EQubeCorner.DF, ECubeFacing.FR);
                    yield return (EQubeCorner.DB, ECubeFacing.RB);
                    break;
                case EQubeCorner.DF: 
                    yield return (EQubeCorner.TF, ECubeFacing.T);
                    yield return (EQubeCorner.DR, ECubeFacing.RB);
                    yield return (EQubeCorner.DL, ECubeFacing.BL);
                    break;
                case EQubeCorner.DR:
                    yield return (EQubeCorner.TR, ECubeFacing.T);
                    yield return (EQubeCorner.DB, ECubeFacing.BL);
                    yield return (EQubeCorner.DF, ECubeFacing.LF);
                    break;
                case EQubeCorner.TB: 
                    yield return (EQubeCorner.DB, ECubeFacing.D);
                    yield return (EQubeCorner.TL, ECubeFacing.LF);
                    yield return (EQubeCorner.TR, ECubeFacing.FR);
                    break;
                case EQubeCorner.TL: 
                    yield return (EQubeCorner.DL, ECubeFacing.D);
                    yield return (EQubeCorner.TF, ECubeFacing.FR);
                    yield return (EQubeCorner.TB, ECubeFacing.RB);
                    break;
                case EQubeCorner.TF: 
                    yield return (EQubeCorner.DF, ECubeFacing.D);
                    yield return (EQubeCorner.TR, ECubeFacing.RB);
                    yield return (EQubeCorner.TL, ECubeFacing.BL);
                    break;
                case EQubeCorner.TR: 
                    yield return (EQubeCorner.DR, ECubeFacing.D);
                    yield return (EQubeCorner.TB, ECubeFacing.BL);
                    yield return (EQubeCorner.TF, ECubeFacing.LF);
                    break;
            }
        }

        static readonly Dictionary<ECubeFacing, EQubeCorner[]> kFacingCorners = new Dictionary<ECubeFacing, EQubeCorner[]>()
            {
                { ECubeFacing.D, new[] { EQubeCorner.DB, EQubeCorner.DL, EQubeCorner.DF, EQubeCorner.DR } },
                { ECubeFacing.T, new[] { EQubeCorner.TB, EQubeCorner.TL, EQubeCorner.TF, EQubeCorner.TR } },
                { ECubeFacing.BL,new []{ EQubeCorner.DB, EQubeCorner.TB, EQubeCorner.DL, EQubeCorner.TL}},
                { ECubeFacing.LF,new []{ EQubeCorner.DL, EQubeCorner.TL, EQubeCorner.DF,EQubeCorner.TF }},
                { ECubeFacing.FR,new []{ EQubeCorner.DF, EQubeCorner.TF,EQubeCorner.DR,EQubeCorner.TR }},
                { ECubeFacing.RB,new []{ EQubeCorner.DR, EQubeCorner.TR, EQubeCorner.DB, EQubeCorner.TB}}
            };

        public static Quad<T> GetSideFacing<T>(this CubeFacing<T> _facing) => new Quad<T>(_facing.fBL,_facing.fLF,_facing.fFR,_facing.fRB);
        public static EQubeCorner[] FacingCorners(this ECubeFacing _facing) => kFacingCorners[_facing];

        public static (T v0, T v1, T v2, T v3) GetFacingCornersCW<T>(this Qube<T> _qube, ECubeFacing _facing) where T : struct
        {
            var corners = _facing.GetRelativeVertsCW();
            return (_qube[corners.v0],_qube[corners.v1],_qube[corners.v2],_qube[corners.v3] );
        }
        
        public static int FacingToIndex(this ECubeFacing _facing)
        {
            switch (_facing)
            {
                default: throw new InvalidEnumArgumentException();
                case ECubeFacing.BL: return 0;
                case ECubeFacing.LF: return 1;
                case ECubeFacing.FR: return 2;
                case ECubeFacing.RB: return 3;
                case ECubeFacing.T: return 4;
                case ECubeFacing.D: return 5;
            }
        }      
        public static ECubeFacing Opposite(this ECubeFacing _facing)
        {
            switch (_facing)
            {
                default: throw new InvalidEnumArgumentException();
                case ECubeFacing.BL: return ECubeFacing.FR;
                case ECubeFacing.LF: return ECubeFacing.RB;
                case ECubeFacing.FR: return ECubeFacing.BL;
                case ECubeFacing.RB: return ECubeFacing.LF;
                case ECubeFacing.T: return ECubeFacing.D;
                case ECubeFacing.D: return ECubeFacing.T;
            }
        } 
        
        public static ECubeFacing IndexToFacing(int _index)
        {
            switch (_index)
            {
                default:throw new IndexOutOfRangeException();
                case 0:return ECubeFacing.BL;
                case 1:return ECubeFacing.LF;
                case 2:return ECubeFacing.FR;
                case 3:return ECubeFacing.RB;
                case 4:return ECubeFacing.T;
                case 5:return ECubeFacing.D;
            }
        }
    }
}