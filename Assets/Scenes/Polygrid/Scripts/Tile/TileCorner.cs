using System.Collections.Generic;
using TPool;
using UnityEngine;

namespace PolyGrid.Tile
{
    public class TileCorner : PoolBehaviour<PolyID>,ICorner
    {
        private byte m_Height => m_PoolID.height;
        private TileVertex m_Vertex { get; set; }
        private readonly List<PolyID> m_NearbyValidCorners = new List<PolyID>();
        private readonly List<PolyID> m_NearbyValidVoxels = new List<PolyID>();

        public TileCorner Init(TileVertex _vertex)
        {
            m_Vertex = _vertex;
            transform.SetPositionAndRotation(_vertex.transform.position+DPolyGrid.GetCornerHeight(m_Height),_vertex.transform.rotation);
            return this;
        }

        public void RefreshRelations(PilePool<TileCorner> _corners,PilePool<TileVoxel> _voxels)
        {
            m_NearbyValidCorners.Clear();
            m_NearbyValidVoxels.Clear();
            
            void AddCorner(PilePool<TileCorner> _corners,PolyID _cornerID)
            {
                if (!_corners.Contains(_cornerID))
                    return;
                m_NearbyValidCorners.Add(_cornerID);
            }

            void AddQuad(PilePool<TileVoxel> _voxels, PolyID _voxelID)
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
        public PolyID Identity => m_PoolID;
        public List<PolyID> NearbyCorners => m_NearbyValidCorners;
        public List<PolyID> NearbyVoxels => m_NearbyValidVoxels;
    }
}