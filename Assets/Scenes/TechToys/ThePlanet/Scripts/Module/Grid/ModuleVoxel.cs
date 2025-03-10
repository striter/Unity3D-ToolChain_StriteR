using System;
using System.Collections.Generic;
using System.Linq.Extensions;
using Runtime.Geometry;
using MeshFragment;
using TechToys.ThePlanet.Module.BOIDS;
using TechToys.ThePlanet.Module.Cluster;
using TPool;
using UnityEngine;

namespace TechToys.ThePlanet.Module
{
    public class ModuleVoxel : PoolBehaviour<PCGID>,IVoxel
    {
        private Action<Transform, List<IMeshFragment>> populateMesh;
        public PCGID Identity => identity;
        private byte Height => identity.height;
        public IQuad m_Quad { get; private set; }
        public Qube<ICorner> m_Corners { get; private set; }

        //Some of cluster data,should? be moved elsewhere
        public Dictionary<int, byte> m_TypedCluster { get; } = new Dictionary<int, byte>();
        public TrapezoidQuad m_ShapeOS { get; private set; }
        public TrapezoidQuad[] m_ClusterQuads { get; private set; } = new TrapezoidQuad[4];
        public Qube<byte> m_ClusterUnitBaseBytes { get; private set; }
        public Qube<byte> m_ClusterUnitAvailableBytes { get; private set; }
        
        public CubeSides<PCGID> m_CubeSides { get; private set; }
        public ECubeFacing m_CubeSidesExists { get; private set; }

        public ModuleVoxel Init(IQuad _srcQuad)
        {
            m_Quad = _srcQuad;
            transform.SetPositionAndRotation(m_Quad.Quad.GetVoxelPosition(identity.height), m_Quad.transform.rotation);
            return this;
        }

        public override void OnPoolRecycle()
        {
            base.OnPoolRecycle();
            m_Quad = null;
            m_Corners = default;
            m_ClusterUnitBaseBytes = default;
            m_ClusterUnitAvailableBytes = default;
            m_CubeSides = default;
            m_CubeSidesExists = default;
            m_TypedCluster.Clear();
        }

        public void RefreshRelations(PilePool<ModuleCorner> _corners,PilePool<ModuleVoxel> _voxels)
        {
            m_CubeSides = new CubeSides<PCGID>(new PCGID(m_Quad.m_NearbyQuadCW[0], Height),new PCGID(m_Quad.m_NearbyQuadCW[1], Height),new PCGID(m_Quad.m_NearbyQuadCW[2], Height),
                new PCGID(m_Quad.m_NearbyQuadCW[3], Height), new PCGID(m_Quad.Identity,UByte.ForwardOne( Height)), new PCGID(m_Quad.Identity, UByte.BackOne(Height))) ;
            m_CubeSidesExists = 0;
            m_ShapeOS = m_Quad.m_ShapeOS.Expand(identity.height*KPCG.kUnitSize*2);
            m_Quad.m_ShapeOS.SplitToQuads(false,identity.height*KPCG.kUnitSize*2).Fill(m_ClusterQuads);
            for (int i = 0; i < UEnum.GetEnums<ECubeFacing>().Length; i++)
            {
                var facing = UEnum.GetEnums<ECubeFacing>()[i];
                if (_voxels.Contains(m_CubeSides[facing]))
                    m_CubeSidesExists |= facing;
            }

            var voxelIDs = this.CollectVoxelCorners();
            var corners = new Qube<ICorner>(null);
            for (int i = 0; i < 8; i++)
            {
                var voxelID = voxelIDs[i];
                corners[i] = _corners.Contains(voxelID) ? _corners[voxelID] : null;
            }
            m_Corners = corners;
            //Recreate Cluster Data
            m_TypedCluster.Clear();
            Qube<bool> cornerValidMask = KQube.kFalse;
            for (int i = 0; i < 8; i++)
            {
                var corner = m_Corners[i];
                bool validCorner = corner != null;
                cornerValidMask[i] = validCorner;
                if(!validCorner)
                    continue;

                var type = corner.m_Type;
                if (m_TypedCluster.ContainsKey(type))
                    continue;
            
                m_TypedCluster.Add(corner.m_Type,m_Corners.CreateVoxelClusterByte(type,DModule.Collection[type].m_ClusterType));
            }

            var clusterByte = KQube.kMinByte;
            var clusterMask = KQube.kMinByte;
            var cornerValidByte = cornerValidMask.ToByte();
            for(int i=0;i<8;i++)
            {
                var corner = m_Corners[i];
                bool validCorner = corner != null;
                clusterByte[i] = validCorner ? UModuleByte.kByteQubeIndexer[m_TypedCluster[corner.m_Type]][i] : Byte.MinValue;
                clusterMask[i] = validCorner ? UModuleByte.kByteQubeIndexerFilled[cornerValidByte][i] : Byte.MinValue;
            }
            m_ClusterUnitBaseBytes = clusterByte;
            m_ClusterUnitAvailableBytes = clusterMask;
        }

    }
}