using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TPoolStatic;
using UnityEngine;

namespace Geometry.Voxel
{
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

        public static EQubeCorner FlooredDiagonalCorner(this EQubeCorner _corner)
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

        public static Y GetQubeCorner<T, Y>(this T _qube, int _corner) where T : IQube<Y> where Y : struct
        {
            switch (_corner)
            {
                default: throw new IndexOutOfRangeException();
                case 0: return _qube.vDB;
                case 1: return _qube.vDL;
                case 2: return _qube.vDF;
                case 3: return _qube.vDR;
                case 4: return _qube.vTB;
                case 5: return _qube.vTL;
                case 6: return _qube.vTF;
                case 7: return _qube.vTR;
            }
        }
        public static IEnumerator<Y> GetEnumerator<T,Y>(this T _qube) where T : IQube<Y> where Y : struct
        {
               yield return _qube.vDB;
               yield return _qube.vDL;
               yield return _qube.vDF;
               yield return _qube.vDR;
               yield return _qube.vTB;
               yield return _qube.vTL;
               yield return _qube.vTF;
               yield return _qube.vTR;
        }
        public static Y GetQubeCorner<T,Y>(this T _qube,EQubeCorner _corner)  where T:IQube<Y> where Y:struct
        {
            switch (_corner)
            {
                default: throw new IndexOutOfRangeException();
                case EQubeCorner.DB: return _qube.vDB;
                case EQubeCorner.DL: return _qube.vDL;
                case EQubeCorner.DF: return _qube.vDF;
                case EQubeCorner.DR: return _qube.vDR;
                case EQubeCorner.TB: return _qube.vTB;
                case EQubeCorner.TL: return _qube.vTL;
                case EQubeCorner.TF: return _qube.vTF;
                case EQubeCorner.TR: return _qube.vTR;
            }
        }
        public static void SetCorner<T,Y>(ref this T _qube, EQubeCorner _corner,Y _value) where T : struct, IQube<Y> where Y : struct =>_qube.SetCorner(_corner.CornerToIndex(),_value);
        public static void SetCorner<T,Y>(ref this T _qube, int _index,Y _value) where T : struct, IQube<Y> where Y : struct
        {
            switch (_index)
            {
                default: throw new IndexOutOfRangeException();
                case 0: _qube.vDB = _value; break;
                case 1: _qube.vDL = _value; break;
                case 2: _qube.vDF = _value; break;
                case 3: _qube.vDR = _value; break;
                case 4: _qube.vTB = _value; break;
                case 5: _qube.vTL = _value; break;
                case 6: _qube.vTF = _value; break;
                case 7: _qube.vTR = _value; break;
            }
        }


        public static IEnumerable<(EQubeCorner _qube, EQubeCorner _adjactileCorner1, EQubeCorner _adjactileCorner2)> NearbyValidCornerQube(this EQubeCorner _srcCorner)
        {
            var flip = _srcCorner.FlipFloor();
            var qube0 =  _srcCorner.FlooredDiagonalCorner();
            yield return (qube0,_srcCorner,_srcCorner.FlipFloor());

            var qube1 = flip.NextCornerFlooredCW(1);
            yield return (qube1,_srcCorner,_srcCorner.NextCornerFlooredCW(3));
            
            var qube2 = flip.NextCornerFlooredCW(3);
            yield return (qube2,_srcCorner,_srcCorner.NextCornerFlooredCW(1));
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
        public static void SetFacingCorners<T, Y>(ref this T _qube, ECubeFacing _facing, Y _value) where T : struct, IQube<Y> where Y : struct
        {
            switch (_facing)
            {
                default: throw new IndexOutOfRangeException();
                case ECubeFacing.D:
                    _qube.vDB = _value;
                    _qube.vDL = _value;
                    _qube.vDF = _value;
                    _qube.vDR = _value;
                    break;
                case ECubeFacing.T:
                    _qube.vTB = _value;
                    _qube.vTL = _value;
                    _qube.vTF = _value;
                    _qube.vTR = _value;
                    break;
                case ECubeFacing.BL:
                    _qube.vDB = _value;
                    _qube.vTB = _value;
                    _qube.vDL = _value;
                    _qube.vTL = _value;
                    break;
                case ECubeFacing.LF:
                    _qube.vDL = _value;
                    _qube.vTL = _value;
                    _qube.vDF = _value;
                    _qube.vTF = _value;
                    break;
                case ECubeFacing.FR:
                    _qube.vDF = _value;
                    _qube.vTF = _value;
                    _qube.vDR = _value;
                    _qube.vTR = _value;
                    break;
                case ECubeFacing.RB:
                    _qube.vDR = _value;
                    _qube.vTR = _value;
                    _qube.vDB = _value;
                    _qube.vTB = _value;
                    break;
            }
        }

        public static void SetQubeCorners<T, Y>(ref this T _qube,Y _db,Y _dl,Y _df,Y _dr,Y _tb,Y _tl,Y _tf,Y _tr) where T:struct,IQube<Y> where Y:struct
        {
            _qube.vDB = _db;
            _qube.vDL = _dl;
            _qube.vDF = _df;
            _qube.vDR = _dr;
            _qube.vTB = _tb;
            _qube.vTL = _tl;
            _qube.vTF = _tf;
            _qube.vTR = _tr;
        }

        public static T CombineToQube<T, Y>(IQuad<Y> _downQuad,IQuad<Y> _topQuad) where T:struct,IQube<Y> where Y:struct
        {
            T qube = default;
            qube.SetQubeCorners(
                _downQuad.vB,_downQuad.vL,_downQuad.vF,_downQuad.vR,
                _topQuad.vB, _topQuad.vL,_topQuad.vF,_topQuad.vR
            );
            return qube;
        }

        public static (Y _downQuad, Y _topQuad) SplitTopDownQuads<T,  Y,U>(this T _qube) where T:struct,IQube<U>where Y:struct,IQuad<U>  where U:struct 
        {
            Y downQuad = default;
            Y topQuad = default;
            downQuad.SetVertex( _qube.vDB,_qube.vDL,_qube.vDF,_qube.vDR);
            topQuad.SetVertex(_qube.vTB,_qube.vTL,_qube.vTF,_qube.vTR);
            return (downQuad, topQuad);
        }

        public static (T v0, T v1, T v2, T v3) GetFacingCornersCW<T>(this IQube<T> _qube, ECubeFacing _facing) where T : struct
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
        
        public static Y GetFacing<T,Y>(this T _cubeFace, int _corner) where T : ICubeFace<Y> where Y : struct => GetFacing<T,Y>(_cubeFace, IndexToFacing(_corner));
        public static Y GetFacing<T, Y>(this T _cubeFace, ECubeFacing _facing) where T : ICubeFace<Y> where Y : struct
        {
            switch (_facing)
            {
                default: throw new Exception("Invalid Facing:"+_facing);
                case ECubeFacing.BL: return _cubeFace.fBL;
                case ECubeFacing.LF: return _cubeFace.fLF;
                case ECubeFacing.FR: return _cubeFace.fFR;
                case ECubeFacing.RB: return _cubeFace.fRB;
                case ECubeFacing.T: return _cubeFace.fT;
                case ECubeFacing.D: return _cubeFace.fD;
            }
        }

        public static T Resize<T,Y>(this T _qube, float _shrinkScale) where  T: struct,IQube<Y> where Y:struct
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

        public static T RotateYawCW<T,Y,U>(this T _qube,ushort _90DegMult) where T:struct,IQube<U> where Y:struct ,IQuad<U> where U:struct
        {
            var quads = _qube.SplitTopDownQuads<T, Y,U>();
            var top = quads._topQuad.RotateYawCW<Y, U>(_90DegMult);
            var down = quads._downQuad.RotateYawCW<Y, U>(_90DegMult);
            return CombineToQube<T,U>(down,top);
        }
    }
    
    public static class UGeometryVoxel
    {
        public static GQube ExpandToQUbe(this GQuad _quad, Vector3 _expand,float _baryCenter=0)
        {
            var expand= _expand * (1-_baryCenter);
            var shrink= _expand * _baryCenter;
            
            return new GQube(_quad.vB-shrink,_quad.vL-shrink,_quad.vF-shrink,_quad.vR-shrink,
                             _quad.vB+expand,_quad.vL+expand,_quad.vF+expand,_quad.vR+expand);
        }
        public static IEnumerable<GQube> SplitToQubes(this GQuad _quad, Vector3 _halfSize,bool insideOut)
        {
            var quads = _quad.SplitToQuads<GQuad, Vector3>(insideOut).ToArray();
            foreach (var quad in quads)
                yield return new GQuad(quad.vB,quad.vL,quad.vF,quad.vR).ExpandToQUbe(_halfSize,1f);
            foreach (var quad in quads)
                yield return  new GQuad(quad.vB,quad.vL,quad.vF,quad.vR).ExpandToQUbe(_halfSize,0f);
        }
        
        public static void FillFacingQuad(this IQube<Vector3> _qube,ECubeFacing _facing,List<Vector3> _vertices,List<int> _indices,List<Vector2> _uvs,List<Vector3> _normals)
        {
            int indexOffset = _vertices.Count;
            var vertsCW = _qube.GetFacingCornersCW(_facing);
            _vertices.Add(vertsCW.v0);
            _vertices.Add(vertsCW.v1);
            _vertices.Add(vertsCW.v2);
            _vertices.Add(vertsCW.v3);
            _indices.Add(indexOffset);
            _indices.Add(indexOffset+1);
            _indices.Add(indexOffset+2);
            _indices.Add(indexOffset+3);
            if (_uvs!=null)
            {
                _uvs.Add(URender.IndexToQuadUV(0));
                _uvs.Add(URender.IndexToQuadUV(1));
                _uvs.Add(URender.IndexToQuadUV(2));
                _uvs.Add(URender.IndexToQuadUV(3));
            }

            if (_normals!=null)
            {
                var normal = Vector3.Cross(vertsCW.v1-vertsCW.v0,vertsCW.v3-vertsCW.v0);
                for(int i=0;i<4;i++)
                    _normals.Add(normal);
            }
        }
        public static Matrix4x4 GetMirrorMatrix(this GPlane _plane)
        {
            Matrix4x4 mirrorMatrix = Matrix4x4.identity;
            mirrorMatrix.m00 = 1 - 2 * _plane.normal.x * _plane.normal.x;
            mirrorMatrix.m01 = -2 * _plane.normal.x * _plane.normal.y;
            mirrorMatrix.m02 = -2 * _plane.normal.x * _plane.normal.z;
            mirrorMatrix.m03 = 2 * _plane.normal.x * _plane.distance;
            mirrorMatrix.m10 = -2 * _plane.normal.x * _plane.normal.y;
            mirrorMatrix.m11 = 1 - 2 * _plane.normal.y * _plane.normal.y;
            mirrorMatrix.m12 = -2 * _plane.normal.y * _plane.normal.z;
            mirrorMatrix.m13 = 2 * _plane.normal.y * _plane.distance;
            mirrorMatrix.m20 = -2 * _plane.normal.x * _plane.normal.z;
            mirrorMatrix.m21 = -2 * _plane.normal.y * _plane.normal.z;
            mirrorMatrix.m22 = 1 - 2 * _plane.normal.z * _plane.normal.z;
            mirrorMatrix.m23 = 2 * _plane.normal.z * _plane.distance;
            mirrorMatrix.m30 = 0;
            mirrorMatrix.m31 = 0;
            mirrorMatrix.m32 = 0;
            mirrorMatrix.m33 = 1;
            return mirrorMatrix;
        }
    }
}
