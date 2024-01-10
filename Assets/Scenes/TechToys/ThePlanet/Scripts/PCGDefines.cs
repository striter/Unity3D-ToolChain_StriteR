using System.Collections.Generic;
using Runtime.Geometry;
using Runtime.Geometry.Explicit;
using Unity.Mathematics;
using UnityEngine;

namespace TechToys.ThePlanet
{
    public enum EQuadGeometry
    {
        Full,
        Half,
    }

    public static class KPCG
    {
        public static class Ocean
        {
            public static float kOceanRadius ;
            public static float4 kOceanST1, kOceanST2;
            public static float kOceanAmplitude1,kOceanAmplitude2;

            public static float3 OutputOceanCoordinates(float3 _positionWSNormalized,float _time)
            {
                float2 uv = USphereExplicit.SpherePositionToUV(_positionWSNormalized);
                float3 positionWS = _positionWSNormalized * kOceanRadius;
                positionWS+=GerstnerWave(uv,kOceanST1,kOceanAmplitude1,_positionWSNormalized,_time);
                positionWS+=GerstnerWave(uv,kOceanST2,kOceanAmplitude2,_positionWSNormalized,_time);
                return positionWS;
            }
            
            public static float3 GerstnerWave(float2 _uv,float4 _waveST,float _amplitude,float3 _normal,float _time)
            {
                float2 flowUV=_uv+_time*_waveST.xy*_waveST.zw;
                float flowSin=flowUV.x*_waveST.x+flowUV.y*_waveST.y;
                float spherical=flowSin*math.PI;
                float sinFlow = math.sin(spherical);
                return _normal*sinFlow*_amplitude;
            }
        }
        
        
        public const float kGridSize = 50f;
        public const float kUnitSize = 1.5f;
        
        public static Vector3 GetCornerPosition(this PCGVertex _vertex, byte _height) =>  _vertex.m_Position + _vertex.m_Normal * (.5f + _height)*kUnitSize*2;
        public static Vector3 GetVoxelPosition(this PCGQuad _quad,byte _height) => _quad.position + _quad.m_ShapeWS.normal*_height*kUnitSize*2;
        
        public static bool TryDownward(this PCGID _src, out PCGID _dst)
        {
            _dst = default;
            if (_src.height == byte.MinValue)
                return false;
            _dst = new PCGID(_src.location, UByte.BackOne(_src.height));
            return true;
        }
        public static bool TryUpward(this PCGID _src, out PCGID _dst)
        {
            _dst = default;
            if (_src.height == byte.MaxValue)
                return false;
            _dst= new PCGID(_src.location,UByte.ForwardOne(_src.height));
            return true;
        }
        public static bool TryUpward(this PCGID _src, out PCGID _dst,int _index)
        {
            _dst = default;
            var dstHeight = _src.height + _index;
            if (dstHeight >= byte.MaxValue)
                return false;
            _dst= new PCGID(_src.location, (byte)dstHeight);
            return true;
        }

        private static readonly List<PCGID> kIdentityHelpers=new List<PCGID>();
        public static IList<PCGID> IterateAdjacentCorners(this PCGVertex _vertex,byte _height)
        {
            kIdentityHelpers.Clear();
            var srcID = new PCGID(_vertex.m_Identity, _height);
            if (srcID.TryDownward(out var lowerID))
                kIdentityHelpers.Add(lowerID);
            foreach (var vertex in _vertex.IterateNearbyVertices())
                kIdentityHelpers.Add(new PCGID(vertex.m_Identity,_height));
            if (srcID.TryUpward(out var upperID))
                kIdentityHelpers.Add(upperID);
            return kIdentityHelpers;
        }
        public static IList<PCGID> IterateNearbyCorners(this PCGVertex _vertex, byte _height)
        {
            kIdentityHelpers.Clear();
            var nearbyVertices = _vertex.IterateNearbyVertices();
            var nearbyVertexCount = nearbyVertices.Count;
            for(int i=0;i<nearbyVertexCount;i++)
                kIdentityHelpers.Add(new PCGID(nearbyVertices[i].m_Identity,_height));
            return kIdentityHelpers;
        }
        public static IList<PCGID> IterateIntervalCorners(this PCGVertex _vertex, byte _height)
        {
            kIdentityHelpers.Clear();
            var intervalVertices = _vertex.IterateIntervalVertices();
            var intervalVerticesCount = intervalVertices.Count;
            for(int i=0;i<intervalVerticesCount;i++)
                kIdentityHelpers.Add(new PCGID(intervalVertices[i].m_Identity,_height));
            return kIdentityHelpers;
        }
        
        public static IList<PCGID> IterateRelativeVoxels(this PCGVertex _vertex,byte _height)
        {
            kIdentityHelpers.Clear();
            var nextHeight = UByte.ForwardOne(_height);
            foreach (var quad in _vertex.m_NearbyQuads)
            {
                var quadID = quad.m_Identity;
                kIdentityHelpers.Add(new PCGID(quadID, _height));
                kIdentityHelpers.Add(new PCGID(quadID, nextHeight));
            }
            return kIdentityHelpers;
        }
    }
}