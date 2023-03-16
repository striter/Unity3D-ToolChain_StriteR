using System;
using System.Collections.Generic;
using System.ComponentModel;
using Geometry;
using Procedural;
using UnityEngine;

namespace TechToys.ThePlanet.Module.Cluster
{
    public enum EClusterType
    {
        Invalid=-1,
        Vanilla,
        Surface,
        Foundation,
    }
    
    [Flags]
    public enum EClusterStatus
    {
        Invalid=-1,
        Common=2<<0,
        Base=2<<1,
        Spike=2<<2,
        Rooftop=2<<3,
        
        Surface = 3<<5,
        Foundation = 3<<6,
    }

    public static class DModuleCluster
    {
        public static Vector3 ObjectToOrientedVertex(Vector3 _vertexOS)
        { 
            var uv=KQuad.k2SquareCentered.GetUV(new Vector2(_vertexOS.x,_vertexOS.z));
            return new Vector3(uv.x, _vertexOS.y,uv.y);
        }

        public static Vector3 ModuleToObjectVertex(TrapezoidQuad _quads,int _orientation,Vector3 _positionUS,int _offset=0)
        {
            float u = _positionUS.x, v = _positionUS.z;

            u -= 0.5f;
            v -= 0.5f;
            KRotation.kRotate2DCW[_orientation].Multiply(u,v,out var x,out var z);
            u = x;
            v = z;

            u += 0.5f;
            v += 0.5f;
            return _quads.GetPoint(u, v, (_positionUS.y+_offset) * DPCG.kUnitSize);
        }

        public static byte CreateVoxelClusterByte(this Qube<ICorner> _corners,int _clusterIndex,EClusterType _clusterType)
        {
            var clusterByte = KQube.kFalse;
            for (int i = 0; i < 8; i++)
                clusterByte[i] = DModule.IsCornerAdjacent(_clusterIndex, _corners[i]?.m_Type ?? -1);

            return clusterByte.ToByte();
        }

        public static Qube<EClusterStatus> CollectClusterStatus(ModuleClusterContainer _container,Dictionary<PCGID,ModuleClusterCorner> _corners)
        {
            var voxelCorners = _container.m_Voxel.m_Corners;
            Qube<ModuleClusterCorner> clusterCorners = default; //Qube<ModuleClusterCorner>.Convert(voxelCorners, _p => _p==null?null: _corners[_p.Identity]);
            for (int i = 0; i < 8; i++)
            {
                var corner = voxelCorners[i];
                clusterCorners[i] = corner == null ? null: _corners[corner.Identity];
            }
            
            Qube<EClusterStatus> destStatuses = default;
            for (int i = 0; i < destStatuses.Length; i++)
            {
                var dstStatus = EClusterStatus.Invalid;
                var voxelCorner = clusterCorners[i];
                if (voxelCorner!=null)
                {
                    var corner = _corners[voxelCorner.Identity];
                    dstStatus=corner.m_Status;
                    var dstType = corner.Type;
                    switch (dstStatus)
                    {
                        case EClusterStatus.Spike:
                        case EClusterStatus.Rooftop:
                        {
                            if (i >= 4)
                                dstStatus = EClusterStatus.Common;
                        }
                        break;
                    }
                }
                destStatuses[i] = dstStatus;
            }

            return destStatuses;
        }
        public static bool IsValidClusterUnit(EClusterStatus _status,byte _srcByte)
        {
            Qube<bool> corners = default;
            corners.SetByteElement(_srcByte);
            
            bool valid = _srcByte != byte.MinValue && _srcByte != byte.MaxValue;
            
            switch (_status)
            {
                default: throw new InvalidEnumArgumentException();
                case EClusterStatus.Common:
                    break;
                case EClusterStatus.Rooftop:
                {
                    valid &= !ECubeFacing.T.FacingCorners().Any(_p => corners[_p]);
                }
                    break;
                case EClusterStatus.Spike:
                {
                    valid &= !(ECubeFacing.T.FacingCorners().Any(_p => corners[_p]) ||
                               ECubeFacing.D.FacingCorners().Count(_p => corners[_p]) != 1);
                }
                    break;
                case EClusterStatus.Base:
                {
                    if (ECubeFacing.T.FacingCorners().All(_p => !corners[_p])
                        || ECubeFacing.D.FacingCorners().All(_p => !corners[_p]))
                        return valid;

                    if (ECubeFacing.D.FacingCorners().Count(_p => corners[_p]) <
                        ECubeFacing.T.FacingCorners().Count(_p => corners[_p]))
                        return false;
                }                
                    break;
                case EClusterStatus.Foundation:
                {
                    valid &= !ECubeFacing.T.FacingCorners().Any(_p => corners[_p]);
                }
                    break;
                case EClusterStatus.Surface:
                {
                    valid &= !ECubeFacing.D.FacingCorners().Any(_p => corners[_p]);
                }
                    break;

            }
            return valid;
        }
        
        private static readonly EClusterStatus[] kVanillaStatus = {EClusterStatus.Base, EClusterStatus.Common, EClusterStatus.Rooftop, EClusterStatus.Spike};
        private static readonly EClusterStatus[] kSurfaceStatus = {EClusterStatus.Surface};
        private static readonly EClusterStatus[] kFoundationStatus = {EClusterStatus.Foundation};
        
        public static EClusterStatus[] GetPredefinedStatus(EClusterType _type)
        {
            switch (_type)
            {
                default: throw new InvalidEnumArgumentException();
                case EClusterType.Vanilla: return kVanillaStatus;
                case EClusterType.Surface: return kSurfaceStatus;
                case EClusterType.Foundation: return kFoundationStatus;
            }
        }
        
        public static EClusterStatus CollectCornerStatus(this ModuleClusterCorner _srcCorner, EClusterType _type, byte _minValue,byte _maxValue, Dictionary<PCGID,ModuleClusterCorner> _corners)
        {
            ValidateHorizonAdjacent(_srcCorner,_corners, out var horizontalAdjacent, out var horizonLined);

            switch (_type)
            {
                default: throw new InvalidEnumArgumentException();
                case EClusterType.Vanilla:
                {
                    var cornerID = _srcCorner.Identity;
                    if (cornerID.height == _minValue)
                        return EClusterStatus.Base;
            
                    if (cornerID.height == _maxValue)
                        return EClusterStatus.Rooftop;

                    var topAdjacent=cornerID.TryUpward(out var upperID)&& _corners.TryGetValue(upperID,out var upperCorner)&&DModule.IsCornerAdjacent( upperCorner.Type,_srcCorner.Type);
                    var bottomAdjacent=cornerID.TryDownward(out var lowerID)&& _corners.TryGetValue(lowerID,out var lowerCorner)&&DModule.IsCornerAdjacent( lowerCorner.Type,_srcCorner.Type);

                    if (bottomAdjacent && !topAdjacent && horizontalAdjacent==0)
                        return EClusterStatus.Spike;
                
                    return EClusterStatus.Common;
                }
                case EClusterType.Surface: return EClusterStatus.Surface;
                case EClusterType.Foundation: return EClusterStatus.Foundation;
            }
        }

        static void ValidateHorizonAdjacent(this ModuleClusterCorner _srcCorner, Dictionary<PCGID,ModuleClusterCorner> _corners,out int _nearbyCount,out bool _lined)
        {
            var cornerID = _srcCorner.Identity;
            _nearbyCount=0;
            _lined = true;
            bool lastAvailable = false;
            
            foreach (var corner in _srcCorner.Vertex.Vertex.IterateNearbyCorners(cornerID.height))
            {
                if (_corners.TryGetValue(corner, out var nearbyCorner) && DModule.IsCornerAdjacent(nearbyCorner.Type, _srcCorner.Type))
                {
                    lastAvailable = false;
                    continue;
                }

                if (lastAvailable)
                    _lined = false;
                
                lastAvailable = true;
                _nearbyCount++;
            }

            _lined &= _nearbyCount == 2;
        }
    }
}