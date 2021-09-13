using System;
using System.Collections;
using System.Collections.Generic;
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

        public static Y GetVertex<T, Y>(this T _qube, int _corner) where T : IQube<Y> where Y : struct
        {
            switch (_corner)
            {
                default: throw new IndexOutOfRangeException();
                case 0: return _qube.vertDB;
                case 1: return _qube.vertDL;
                case 2: return _qube.vertDF;
                case 3: return _qube.vertDR;
                case 4: return _qube.vertTB;
                case 5: return _qube.vertTL;
                case 6: return _qube.vertTF;
                case 7: return _qube.vertTR;
            }
        }
        public static Y GetVertex<T,Y>(this T _qube,EQubeCorner _corner)  where T:IQube<Y> where Y:struct
        {
            switch (_corner)
            {
                default: throw new IndexOutOfRangeException();
                case EQubeCorner.DB: return _qube.vertDB;
                case EQubeCorner.DL: return _qube.vertDL;
                case EQubeCorner.DF: return _qube.vertDF;
                case EQubeCorner.DR: return _qube.vertDR;
                case EQubeCorner.TB: return _qube.vertTB;
                case EQubeCorner.TL: return _qube.vertTL;
                case EQubeCorner.TF: return _qube.vertTF;
                case EQubeCorner.TR: return _qube.vertTR;
            }
        }

        public static T SetByteCorners<T, Y>(this T _qube,Y _db,Y _dl,Y _df,Y _dr,Y _tb,Y _tl,Y _tf,Y _tr) where T:struct,IQube<Y> where Y:struct
        {
            _qube.vertDB = _db;
            _qube.vertDL = _dl;
            _qube.vertDF = _df;
            _qube.vertDR = _dr;
            _qube.vertTB = _tb;
            _qube.vertTL = _tl;
            _qube.vertTF = _tf;
            _qube.vertTR = _tr;
            return _qube;
        }

        public static (T v0, T v1, T v2, T v3) GetVertsCW<T>(this IQube<T> _qube, ECubeFacing _facing) where T : struct
        {
            var corners = _facing.GetRelativeVertsCW();
            return (_qube[corners.v0],_qube[corners.v1],_qube[corners.v2],_qube[corners.v3] );
        }
        
        
        public static int FacingToIndex(this ECubeFacing _facing)
        {
            switch (_facing)
            {
                default: throw new Exception("Invalid Face:"+_facing);
                case ECubeFacing.BL: return 0;
                case ECubeFacing.LF: return 1;
                case ECubeFacing.FR: return 2;
                case ECubeFacing.RB: return 3;
                case ECubeFacing.T: return 4;
                case ECubeFacing.D: return 5;
            }
        } 
        
        public static ECubeFacing IndexToFacing(int _index)
        {
            switch (_index)
            {
                default: throw new Exception("Invalid Corner:"+_index);
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
        
        public static byte ToByte(this IQube<bool> _qube)
        {
            return UByte.ToByte(_qube[0],_qube[1],_qube[2],_qube[3],
                _qube[4],_qube[5],_qube[6],_qube[7]);
        }

        public static void SetByteCorners<T>(this T _qube, byte _byte) where T:struct,IQube<bool>
        {
            _qube.SetByteCorners(UByte.PosValid(_byte,0),UByte.PosValid(_byte,1),UByte.PosValid(_byte,2),UByte.PosValid(_byte,3),
                UByte.PosValid(_byte,4),UByte.PosValid(_byte,5),UByte.PosValid(_byte,6),UByte.PosValid(_byte,7));
        }
    }
    
    public static class UGeometryVoxel
    {
        public static GQube ConvertToQube(this GQuad _quad, Vector3 _expand,float _baryCenter=0)
        {
            var shrink= _expand * (1-_baryCenter);
            var expand= _expand * _baryCenter;
            
            return new GQube(_quad.vB-shrink,_quad.vL-shrink,_quad.vF-shrink,_quad.vR-shrink,
                             _quad.vB+expand,_quad.vL+expand,_quad.vF+expand,_quad.vR+expand);
        }

        public static IEnumerable<GQube> SplitToQubes(this GQuad _quad, Vector3 _halfSize)
        {
            var quads = _quad.SplitToQuads<GQuad, Vector3>().ToArray();
            foreach (var quad in quads)
                yield return new GQuad(quad.vB,quad.vL,quad.vF,quad.vR).ConvertToQube(_halfSize,0f);
            foreach (var quad in quads)
                yield return  new GQuad(quad.vB,quad.vL,quad.vF,quad.vR).ConvertToQube(_halfSize,1f);
        }
        
        public static void FillFacingQuad(this IQube<Vector3> _qube,ECubeFacing _facing,List<Vector3> _vertices,List<int> _indices,List<Vector2> _uvs,List<Vector3> _normals)
        {
            int indexOffset = _vertices.Count;
            var vertsCW = _qube.GetVertsCW(_facing);
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
