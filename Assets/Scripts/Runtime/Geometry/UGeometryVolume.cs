using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace Geometry
{
    public static class UGeometryVolume
    {
        public static bool IsPointInside(this GBox _box, Vector3 _point)=> 
            _point.x >= _box.min.x && _point.x <= _box.max.x && 
            _point.y >= _box.min.y && _point.y <= _box.max.y && 
            _point.z >= _box.min.z && _point.z <= _box.max.z;
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

        public static Bounds ToBounds(this GBox _box)
        {
            return new Bounds(_box.center, _box.size);
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
                case ECubeFacing.B:return (EQubeCorner.DB,EQubeCorner.DL,EQubeCorner.TL,EQubeCorner.TB);
                case ECubeFacing.L:return (EQubeCorner.DL,EQubeCorner.DF,EQubeCorner.TF,EQubeCorner.TL);
                case ECubeFacing.F:return (EQubeCorner.DF,EQubeCorner.DR,EQubeCorner.TR,EQubeCorner.TF);
                case ECubeFacing.R:return (EQubeCorner.DR,EQubeCorner.DB,EQubeCorner.TB,EQubeCorner.TR);
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
                    yield return (EQubeCorner.DL, ECubeFacing.L);
                    yield return (EQubeCorner.DR, ECubeFacing.F);
                    break;
                case EQubeCorner.DL:
                    yield return (EQubeCorner.TL, ECubeFacing.T);
                    yield return (EQubeCorner.DF, ECubeFacing.F);
                    yield return (EQubeCorner.DB, ECubeFacing.R);
                    break;
                case EQubeCorner.DF: 
                    yield return (EQubeCorner.TF, ECubeFacing.T);
                    yield return (EQubeCorner.DR, ECubeFacing.R);
                    yield return (EQubeCorner.DL, ECubeFacing.B);
                    break;
                case EQubeCorner.DR:
                    yield return (EQubeCorner.TR, ECubeFacing.T);
                    yield return (EQubeCorner.DB, ECubeFacing.B);
                    yield return (EQubeCorner.DF, ECubeFacing.L);
                    break;
                case EQubeCorner.TB: 
                    yield return (EQubeCorner.DB, ECubeFacing.D);
                    yield return (EQubeCorner.TL, ECubeFacing.L);
                    yield return (EQubeCorner.TR, ECubeFacing.F);
                    break;
                case EQubeCorner.TL: 
                    yield return (EQubeCorner.DL, ECubeFacing.D);
                    yield return (EQubeCorner.TF, ECubeFacing.F);
                    yield return (EQubeCorner.TB, ECubeFacing.R);
                    break;
                case EQubeCorner.TF: 
                    yield return (EQubeCorner.DF, ECubeFacing.D);
                    yield return (EQubeCorner.TR, ECubeFacing.R);
                    yield return (EQubeCorner.TL, ECubeFacing.B);
                    break;
                case EQubeCorner.TR: 
                    yield return (EQubeCorner.DR, ECubeFacing.D);
                    yield return (EQubeCorner.TB, ECubeFacing.B);
                    yield return (EQubeCorner.TF, ECubeFacing.L);
                    break;
            }
        }

        static readonly Dictionary<ECubeFacing, EQubeCorner[]> kFacingCorners = new Dictionary<ECubeFacing, EQubeCorner[]>()
            {
                { ECubeFacing.D, new[] { EQubeCorner.DB, EQubeCorner.DL, EQubeCorner.DF, EQubeCorner.DR } },
                { ECubeFacing.T, new[] { EQubeCorner.TB, EQubeCorner.TL, EQubeCorner.TF, EQubeCorner.TR } },
                { ECubeFacing.B,new []{ EQubeCorner.DB, EQubeCorner.TB, EQubeCorner.DL, EQubeCorner.TL}},
                { ECubeFacing.L,new []{ EQubeCorner.DL, EQubeCorner.TL, EQubeCorner.DF,EQubeCorner.TF }},
                { ECubeFacing.F,new []{ EQubeCorner.DF, EQubeCorner.TF,EQubeCorner.DR,EQubeCorner.TR }},
                { ECubeFacing.R,new []{ EQubeCorner.DR, EQubeCorner.TR, EQubeCorner.DB, EQubeCorner.TB}}
            };

        public static Quad<T> GetSideFacing<T>(this CubeSides<T> _sides) => new Quad<T>(_sides.fBL,_sides.fLF,_sides.fFR,_sides.fRB);
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
                case ECubeFacing.B: return 0;
                case ECubeFacing.L: return 1;
                case ECubeFacing.F: return 2;
                case ECubeFacing.R: return 3;
                case ECubeFacing.T: return 4;
                case ECubeFacing.D: return 5;
            }
        }      
        public static ECubeFacing Opposite(this ECubeFacing _facing)
        {
            switch (_facing)
            {
                default: throw new InvalidEnumArgumentException();
                case ECubeFacing.B: return ECubeFacing.F;
                case ECubeFacing.L: return ECubeFacing.R;
                case ECubeFacing.F: return ECubeFacing.B;
                case ECubeFacing.R: return ECubeFacing.L;
                case ECubeFacing.T: return ECubeFacing.D;
                case ECubeFacing.D: return ECubeFacing.T;
            }
        } 
        
        public static ECubeFacing IndexToFacing(int _index)
        {
            switch (_index)
            {
                default:throw new IndexOutOfRangeException();
                case 0:return ECubeFacing.B;
                case 1:return ECubeFacing.L;
                case 2:return ECubeFacing.F;
                case 3:return ECubeFacing.R;
                case 4:return ECubeFacing.T;
                case 5:return ECubeFacing.D;
            }
        }

        public static Int3 GetCubeOffset(ECubeFacing _facing)
        {
            switch (_facing)
            {
                default: throw new InvalidEnumArgumentException();
                case ECubeFacing.B: return Int3.kBack;
                case ECubeFacing.L: return Int3.kLeft;
                case ECubeFacing.F: return Int3.kForward;
                case ECubeFacing.R: return Int3.kRight;
                case ECubeFacing.T: return Int3.kUp;
                case ECubeFacing.D: return Int3.kDown;
            }
        }
        
        public static void GetFacingQuadGeometry(ECubeFacing _facing,out float3 b,out float3 l,out float3 f,out float3 r,out half3 n,out half4 t)
        {
            switch (_facing)
            {
                default: throw new Exception("Invalid facing");
                case ECubeFacing.B: { b = KQube.kUnitQubeCentered[0]; l = KQube.kUnitQubeCentered[1]; f = KQube.kUnitQubeCentered[5]; r = KQube.kUnitQubeCentered[4]; n = (half3)new float3(0f,0f,-1f);t = (half4)new float4(1f,0f,0f,1f);}break;
                case ECubeFacing.L: { b = KQube.kUnitQubeCentered[1]; l = KQube.kUnitQubeCentered[2]; f = KQube.kUnitQubeCentered[6]; r = KQube.kUnitQubeCentered[5]; n = (half3)new float3(-1f,0f,0f);t = (half4)new float4(0f,0f,1f,1f);}break;
                case ECubeFacing.F: { b = KQube.kUnitQubeCentered[2]; l = KQube.kUnitQubeCentered[3]; f = KQube.kUnitQubeCentered[7]; r = KQube.kUnitQubeCentered[6]; n = (half3)new float3(0f,0f,1f);t = (half4)new float4(-1f,0f,0f,1f); }break;
                case ECubeFacing.R: { b = KQube.kUnitQubeCentered[3]; l = KQube.kUnitQubeCentered[0]; f = KQube.kUnitQubeCentered[4]; r = KQube.kUnitQubeCentered[7]; n = (half3)new float3(1f,0f,0f);t = (half4)new float4(0f,0f,-1f,1f); }break;
                case ECubeFacing.T: { b = KQube.kUnitQubeCentered[4]; l = KQube.kUnitQubeCentered[5]; f = KQube.kUnitQubeCentered[6]; r = KQube.kUnitQubeCentered[7]; n = (half3)new float3(0f,1f,0f);t = (half4)new float4(1f,0f,0f,1f);}break;
                case ECubeFacing.D: { b = KQube.kUnitQubeCentered[3]; l = KQube.kUnitQubeCentered[2]; f = KQube.kUnitQubeCentered[1]; r = KQube.kUnitQubeCentered[0]; n = (half3)new float3(0f,-1f,0f);t = (half4)new float4(-1f,0f,0f,1f); }break;
            }
        }
    }
}
