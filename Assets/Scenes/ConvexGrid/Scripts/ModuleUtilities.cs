using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Geometry.Voxel;
using UnityEngine;

namespace ConvexGrid
{
    public static class UModule
    {
        public static readonly float unityLength = Mathf.Sqrt(2f);
        public static readonly float halfUnityLength = unityLength / 2f;
        public static readonly GQuad unitQuad = new GQuad( Vector3.back*unityLength, Vector3.left*unityLength ,Vector3.forward*unityLength, Vector3.right*unityLength);
        public static readonly GQuad halfQuad = unitQuad.Shrink(.5f);
        public static readonly GQuad halfQuadFwd = halfQuad + Vector3.forward * halfUnityLength;
        public static readonly GQube unitQube = unitQuad.ConvertToQube(ConvexGridHelper.m_TileHeight, .5f);
        public static readonly GQube halfQube = halfQuad.ConvertToQube(ConvexGridHelper.m_TileHeightHalf, .5f);
        public static readonly GQube halfQubeFwd = halfQuadFwd.ConvertToQube(ConvexGridHelper.m_TileHeightHalf, .5f);
        static readonly Quaternion[] m_QuadRotations = {Quaternion.Euler(0f,180f,0f),Quaternion.Euler(0f,270f,0f),Quaternion.Euler(0f,0f,0f),Quaternion.Euler(0f,90f,0f)};

        static Quaternion GetModuleMeshRotation(int _qubeIndex)=> m_QuadRotations[_qubeIndex%4];

        private static readonly Vector3 moduleMeshDownSide =- ConvexGridHelper.m_TileHeightHalf / 2f;
        private static readonly Vector3 moduleMeshTopSide = ConvexGridHelper.m_TileHeightHalf / 2f;
        static Vector3 GetModuleMeshOffset(int _qubeIndex)
        {
            if (_qubeIndex < 4)
                return moduleMeshDownSide;
            return moduleMeshTopSide;
        }

        public static Matrix4x4[] LocalToModuleMatrix = new[]
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
    }
}