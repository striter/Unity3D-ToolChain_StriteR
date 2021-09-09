using System;
using System.Collections.Generic;
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
                case EQubeCorner.BB: return 0;
                case EQubeCorner.BL: return 1;
                case EQubeCorner.BF: return 2;
                case EQubeCorner.BR: return 3;
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
                case 0:return EQubeCorner.BB; 
                case 1:return EQubeCorner.BL;
                case 2:return EQubeCorner.BF;
                case 3:return EQubeCorner.BR;
                case 4:return EQubeCorner.TB;
                case 5:return EQubeCorner.TL;
                case 6:return EQubeCorner.TF;
                case 7:return EQubeCorner.TR;
            }
        }
        public static (EQubeCorner v0, EQubeCorner v1, EQubeCorner v2, EQubeCorner v3) GetRelativeVertsCW(this ECubeFace _face)
        {
            switch (_face)
            {
                default: throw new Exception("Invalid Face:"+_face);
                case ECubeFace.BL:return (EQubeCorner.BB,EQubeCorner.BL,EQubeCorner.TL,EQubeCorner.TB);
                case ECubeFace.LF:return (EQubeCorner.BL,EQubeCorner.BF,EQubeCorner.TF,EQubeCorner.TL);
                case ECubeFace.FR:return (EQubeCorner.BF,EQubeCorner.BR,EQubeCorner.TR,EQubeCorner.TF);
                case ECubeFace.RB:return (EQubeCorner.BR,EQubeCorner.BB,EQubeCorner.TB,EQubeCorner.TR);
                case ECubeFace.T:return (EQubeCorner.TB,EQubeCorner.TL,EQubeCorner.TF,EQubeCorner.TR);
                case ECubeFace.B:return (EQubeCorner.BB,EQubeCorner.BR,EQubeCorner.BF,EQubeCorner.BL);
            }
        }

        public static T GetVertex<T>(this IQube<T> _qube, int _corner) where T : struct => GetVertex(_qube, IndexToCorner(_corner));
        public static T GetVertex<T>(this IQube<T> _qube,EQubeCorner _corner) where T:struct
        {
            switch (_corner)
            {
                default: throw new Exception("Invalid Corner:"+_corner);
                case EQubeCorner.BB: return _qube.vertBB;
                case EQubeCorner.BL: return _qube.vertBL;
                case EQubeCorner.BF: return _qube.vertBF;
                case EQubeCorner.BR: return _qube.vertBR;
                case EQubeCorner.TB: return _qube.vertTB;
                case EQubeCorner.TL: return _qube.vertTL;
                case EQubeCorner.TF: return _qube.vertTF;
                case EQubeCorner.TR: return _qube.vertTR;
            }
        }

        public static (T v0, T v1, T v2, T v3) GetVertsCW<T>(this IQube<T> _qube, ECubeFace _face) where T : struct
        {
            var corners = _face.GetRelativeVertsCW();
            return (_qube[corners.v0],_qube[corners.v1],_qube[corners.v2],_qube[corners.v3] );
        }
    }
    public static class UGeometryVoxel
    {
        public static GQube ConvertToQube(this GQuad _quad, Vector3 _offset,float _centerOffset=0)
        {
            var shrink= _offset*(1-_centerOffset);
            var expand= _offset * _centerOffset;
            
            return new GQube(_quad.vB-shrink,_quad.vL-shrink,_quad.vF-shrink,_quad.vR-shrink,
                             _quad.vB+expand,_quad.vL+expand,_quad.vF+expand,_quad.vR+expand);
        }
        
        public static void FillFaceMesh(this IQube<Vector3> _qube,ECubeFace _face,List<Vector3> _vertices,List<int> _indices,List<Vector2> _uvs,List<Vector3> _normals)
        {
            int indexOffset = _vertices.Count;
            var vertsCW = _qube.GetVertsCW(_face);
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
