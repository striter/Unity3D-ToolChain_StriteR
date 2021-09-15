using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using Geometry.Pixel;
using Geometry.Voxel;
using Procedural;
using UnityEngine;

namespace ConvexGrid
{
    public static class UModule
    {
        public static readonly float unityLength = Mathf.Sqrt(2f);
        public static readonly float halfUnityLength = unityLength / 2f;
        
        public static readonly GQuad unitGQuad = new GQuad( Vector3.back*unityLength, Vector3.left*unityLength ,Vector3.forward*unityLength, Vector3.right*unityLength);
        public static readonly GQuad halfGQuad = unitGQuad.Shrink<GQuad,Vector3>(.5f);
        public static readonly GQuad halfGQuadFwd = halfGQuad + Vector3.forward * halfUnityLength;

        public static readonly G2Quad unitG2Quad = unitGQuad.ConvertToG2Quad(p=>p.ToCoord());
        public static readonly G2Quad halfG2Quad = halfGQuad.ConvertToG2Quad(p=>p.ToCoord());
        public static readonly G2Quad halfG2QuadFwd = halfGQuadFwd.ConvertToG2Quad(p=>p.ToCoord());
        
        public static readonly GQube unitQube = unitGQuad.ExpandToQUbe(ConvexGridHelper.m_TileHeightVector, .5f);
        public static readonly GQube halfQube = halfGQuad.ExpandToQUbe(ConvexGridHelper.m_TileHeightHalfVector, 0f);
        public static readonly GQube halfQubeFwd = halfGQuadFwd.ExpandToQUbe(ConvexGridHelper.m_TileHeightHalfVector, 0f);
        
        static readonly Quaternion[] m_QuadRotations = {Quaternion.Euler(0f,180f,0f),Quaternion.Euler(0f,270f,0f),Quaternion.Euler(0f,0f,0f),Quaternion.Euler(0f,90f,0f)};

        public static Quaternion GetModuleMeshRotation(int _qubeIndex)=> m_QuadRotations[_qubeIndex%4];

        private static readonly Vector3 moduleCenterDownSide = - ConvexGridHelper.m_TileHeightHalfVector;
        private static readonly Vector3 moduleCenterTopSide = Vector3.zero;
        public static Vector3 GetModuleMeshOffset(int _qubeIndex)
        {
            if (_qubeIndex < 4)
                return moduleCenterDownSide;
            return moduleCenterTopSide;
        }

        public static readonly Matrix4x4[] LocalToModuleMatrix = new[]
        {
            Matrix4x4.TRS(GetModuleMeshOffset(0), GetModuleMeshRotation(0), Vector3.one),
            Matrix4x4.TRS(GetModuleMeshOffset(1), GetModuleMeshRotation(1), Vector3.one),
            Matrix4x4.TRS(GetModuleMeshOffset(2), GetModuleMeshRotation(2), Vector3.one),
            Matrix4x4.TRS(GetModuleMeshOffset(3), GetModuleMeshRotation(3), Vector3.one),
            Matrix4x4.TRS(GetModuleMeshOffset(4), GetModuleMeshRotation(4), Vector3.one),
            Matrix4x4.TRS(GetModuleMeshOffset(5), GetModuleMeshRotation(5), Vector3.one),
            Matrix4x4.TRS(GetModuleMeshOffset(6), GetModuleMeshRotation(6), Vector3.one),
            Matrix4x4.TRS(GetModuleMeshOffset(7), GetModuleMeshRotation(7), Vector3.one),
        };

        public static Vector3 RemapModuleVertex(Vector3 _srcVertexOS, ref CoordQuad _moduleShapeLS)
        {
            var uv = halfG2QuadFwd.GetUV(_srcVertexOS.ToCoord());
            var vec2 = _moduleShapeLS.GetPoint<CoordQuad,Coord>(uv);
            return new Vector3(vec2.x,_srcVertexOS.y,vec2.y);
        }
    }
}