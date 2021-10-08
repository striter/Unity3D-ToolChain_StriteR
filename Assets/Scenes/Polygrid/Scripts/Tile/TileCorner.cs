using System;
using System.Collections;
using System.Collections.Generic;
using LinqExtension;
using TPool;
using Procedural.Hexagon;
using UnityEngine;

namespace PolyGrid.Tile
{
    public class TileCorner : PoolBehaviour<PileID>,ICorner
    {
        private byte m_Height => m_PoolID.height;
        private TileVertex m_Vertex { get; set; }
        private readonly List<PileID> m_NearbyValidCorners = new List<PileID>();
        private readonly List<PileID> m_NearbyValidVoxels = new List<PileID>();

        public TileCorner Init(TileVertex _vertex)
        {
            m_Vertex = _vertex;
            transform.SetPositionAndRotation(_vertex.transform.position,_vertex.transform.rotation);
            return this;
        }

        public void RefreshRelations(PilePool<TileCorner> _corners,PilePool<TileVoxel> _voxels)
        {
            m_NearbyValidCorners.Clear();
            m_NearbyValidVoxels.Clear();
            
            void AddCorner(PilePool<TileCorner> _corners,PileID _cornerID)
            {
                if (!_corners.Contains(_cornerID))
                    return;
                m_NearbyValidCorners.Add(_cornerID);
            }

            void AddQuad(PilePool<TileVoxel> _voxels, PileID _voxelID)
            {
                if (!_voxels.Contains(_voxelID))
                    return;
                m_NearbyValidVoxels.Add(_voxelID);
            }

            var height = m_Height;
            foreach (var corner in m_Vertex.m_Vertex.AllNearbyCorner(height))
                AddCorner(_corners,corner);

            foreach (var quad in m_Vertex.m_Vertex.AllNearbyVoxels(height))
                AddQuad(_voxels,quad);
        }

        public PolyVertex Vertex => m_Vertex.m_Vertex;
        public Transform Transform => transform;
        public PileID Identity => m_PoolID;
        public List<PileID> NearbyCorners => m_NearbyValidCorners;
        public List<PileID> NearbyVoxels => m_NearbyValidVoxels;
    }
}