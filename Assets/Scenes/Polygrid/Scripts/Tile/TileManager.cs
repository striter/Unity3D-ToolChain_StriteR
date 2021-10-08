using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LinqExtension;
using TPool;
using TPoolStatic;
using Procedural;
using Procedural.Hexagon;
using UnityEngine;

namespace PolyGrid.Tile
{
    public class TileManager : MonoBehaviour,IPolyGridControl
    {
        private TObjectPoolMono<HexCoord, TileVertex> m_GridVertices;
        private TObjectPoolMono<HexCoord,TileQuad> m_GridQuads;
        private PilePool<TileCorner> m_Corners;
        private PilePool<TileVoxel> m_Voxels;
        public void Init(Transform _transform)
        {
            m_GridVertices = new TObjectPoolMono<HexCoord, TileVertex>(_transform.Find("Grid/Vertex/Item"));
            m_GridQuads = new TObjectPoolMono<HexCoord,TileQuad>(_transform.Find("Grid/Quad/Item"));
            m_Corners = new PilePool<TileCorner>(_transform.Find("Grid/Corner/Item"));
            m_Voxels = new PilePool<TileVoxel>(_transform.Find("Grid/Voxel/Item"));
        }

        public void Clear()
        {
            m_GridQuads.Clear();
            m_GridVertices.Clear();
            m_Corners.Clear();
            m_Voxels.Clear();
        }

        public void CornerConstruction( PolyVertex _vertex, byte _height,Action<PolyVertex> _vertexSpawn,Action<PolyQuad> _quadSpawn, Action<ICorner> _cornerSpawn,Action<IVoxel> _moduleSpawn)
        {
            var corner = new PileID(_vertex.m_Identity, _height);
            if (m_Corners.Contains(corner))
                return;
            
            FillVertex(_vertex,_vertexSpawn);
            FillQuads(_vertex,_quadSpawn);
            
            FillCorner(corner,_cornerSpawn);
            FillVoxels(_vertex,_moduleSpawn);
            
            RefreshCornerRelations(_vertex,_height);
            RefreshVoxelRelations(_vertex);
        }
        
        public void CornerDeconstruction(PolyVertex _vertex, byte _height,Action<HexCoord> _vertexRecycle,Action<HexCoord> _quadRecycle,Action<PileID> _cornerRecycle,Action<PileID> _moduleRecycle)
        {
            var corner = new PileID(_vertex.m_Identity, _height);
            if (!m_Corners.Contains(corner))
                return;
            
            RemoveCorner(corner,_cornerRecycle);
            RemoveVoxels(_vertex,_moduleRecycle);
            
            RemoveVertex(_vertex,_vertexRecycle);
            RemoveQuads(_vertex,_quadRecycle);
            
            RefreshCornerRelations(_vertex,_height);
            RefreshVoxelRelations(_vertex);
        }

        byte GetMaxCornerHeight(HexCoord _quadID)
        {
            var maxHeight = byte.MinValue;
            for (int i = 0; i < m_GridQuads[_quadID].m_NearbyVertsCW.Length; i++)
            {
                var vertexID = m_GridQuads[_quadID].m_NearbyVertsCW[i];
                if (m_Corners.Contains(vertexID))
                    maxHeight = Math.Max(maxHeight, UByte.ForwardOne( m_Corners.Max(vertexID)));
            }
            return maxHeight;
        }
        
        void FillVertex(PolyVertex _vertex,Action<PolyVertex> _vertexSpawn)
        {
            var vertexID = _vertex.m_Identity;
            if (m_GridVertices.Contains(vertexID))
                return;
            m_GridVertices.Spawn(vertexID).Init(_vertex);
            _vertexSpawn(_vertex);
        }

        void FillQuads(PolyVertex _vertex,Action<PolyQuad> _quadSpawn)
        {
            foreach (var quad in _vertex.m_NearbyQuads)
            {
                var quadID = quad.m_Identity;
                if (m_GridQuads.Contains(quadID))
                    continue;
                m_GridQuads.Spawn(quadID).Init(quad);
                _quadSpawn(quad);
            }
        }

        void FillCorner(PileID _cornerID,Action<ICorner> _cornerSpawn)
        {
            if (m_Corners.Contains(_cornerID))
                return;
            var vertex = m_GridVertices[_cornerID.location];
            var corner=m_Corners.Spawn(_cornerID).Init(vertex);
            _cornerSpawn(corner);
        }

        void FillVoxels(PolyVertex _vertex,Action<IVoxel> _voxelSpawn)
        {
            foreach (var quadID in _vertex.m_NearbyQuads.Select(p=>p.m_Identity))
            {
                var maxHeight = GetMaxCornerHeight(quadID);
                for (byte i = 0; i <= maxHeight; i++)
                {
                    var voxelID = new PileID(quadID, i);
                    if(m_Voxels.Contains(voxelID))
                        continue; 
                    _voxelSpawn(m_Voxels.Spawn(voxelID).Init(m_GridQuads[quadID]));
                }
            }
        }
        
        void RemoveVertex(PolyVertex _vertex,Action<HexCoord> _vertexRecycle)
        {
            var vertexID = _vertex.m_Identity;
            if (m_Corners.Contains(vertexID)||!m_GridVertices.Contains(vertexID))
                return;
            m_GridVertices.Recycle(vertexID);
            _vertexRecycle(vertexID);
        }

        void RemoveQuads(PolyVertex _vertex,Action<HexCoord> _quadRecycle)
        {
            foreach (var quadID in _vertex.m_NearbyQuads.Select(p => p.m_Identity))
            {
                if (m_Voxels.Contains(quadID)||!m_GridQuads.Contains(quadID))
                    continue;
                m_GridQuads.Recycle(quadID);
                _quadRecycle(quadID);
            }
        }
        
        void RemoveCorner(PileID _cornerID,Action<PileID> _cornerRecycle)
        {
            if (!m_Corners.Contains(_cornerID))
                return;
            m_Corners.Recycle(_cornerID);
            _cornerRecycle(_cornerID);
        }

        void RemoveVoxels(PolyVertex _vertex,Action<PileID> _voxelRecycle)
        {
            foreach (var _quadID in _vertex.m_NearbyQuads.Select(p => p.m_Identity))
            {
                var maxHeight = GetMaxCornerHeight(_quadID);
                maxHeight = maxHeight == byte.MinValue ? byte.MinValue : UByte.ForwardOne(maxHeight);
                var srcHeight = m_Voxels.Max(_quadID);
                for (var i = maxHeight; i <= srcHeight; i++)
                {
                    var voxelID = new PileID(_quadID, i);
                    m_Voxels.Recycle(voxelID);
                    _voxelRecycle(voxelID);
                }
            }
        }

        void RefreshCornerRelations(PolyVertex _vertex,byte _height)
        {
            foreach (var cornerID in _vertex.AllNearbyCorner(_height).Extend(new PileID(_vertex.m_Identity,_height)))
            {
                 if(!m_Corners.Contains(cornerID))
                     continue;
                 m_Corners[cornerID].RefreshRelations(m_Corners,m_Voxels);
            }
        }
        
        void RefreshVoxelRelations(PolyVertex _vertex)
        {
            var quadRefreshing = TSPoolList<HexCoord>.Spawn();
            
            foreach (var _quadID in _vertex.m_NearbyQuads.Select(p => p.m_Identity))
            {
                quadRefreshing.Add(_quadID);
                if(!m_GridQuads.Contains(_quadID))
                    continue;
                var quad = m_GridQuads[_quadID];
                quadRefreshing.TryAdd(quad.m_NearbyQuadsCW.B);
                quadRefreshing.TryAdd(quad.m_NearbyQuadsCW.L);
                quadRefreshing.TryAdd(quad.m_NearbyQuadsCW.F);
                quadRefreshing.TryAdd(quad.m_NearbyQuadsCW.R);
            }

            foreach (var _quadID in quadRefreshing)
            {
                if (!m_Voxels.Contains(_quadID))
                   continue;
                var maxHeight = GetMaxCornerHeight(_quadID);
                for (byte i = 0; i <= maxHeight; i++)
                    m_Voxels[new PileID(_quadID, i)].RefreshRelations(m_Corners,m_Voxels);
            }
            
            TSPoolList<HexCoord>.Recycle(quadRefreshing);
        }

        public void Tick(float _deltaTime)
        {
        }

        #if UNITY_EDITOR
        #region Gizmos
        [Header("Gizmos")] 
        [Header("2 Dimension")]
        public bool m_VertexGizmos;
        public bool m_QuadGizmos;
        [MFoldout(nameof(m_QuadGizmos),true)] public bool m_RelativeQuadGizmos;
        [MFoldout(nameof(m_QuadGizmos),true)] public bool m_RelativeVertexGizmos;
        [Header("3 Dimension")]
        public bool m_CornerGizmos;
        [MFoldout(nameof(m_CornerGizmos), true)] public bool m_CornerSideRelations;
        [MFoldout(nameof(m_CornerGizmos), true)] public bool m_CornerVoxelRelations;
        public bool m_VoxelGizmos;
        [MFoldout(nameof(m_VoxelGizmos),true)] public bool m_VoxelCornerRelations;
        [MFoldout(nameof(m_VoxelGizmos),true)] public bool m_VoxelSideRelations;
        private void OnDrawGizmos()
        {
            if (m_VertexGizmos && m_GridVertices != null) 
            {
                Gizmos.color = Color.cyan;
                foreach (var vertex in m_GridVertices)
                    Gizmos.DrawWireSphere(vertex.m_Vertex.m_Coord.ToPosition(),.3f);
            }

            if (m_QuadGizmos&&m_GridQuads!=null)
            {
                foreach (var quad in m_GridQuads)
                {
                    Gizmos.color = Color.white;
                    Gizmos.matrix = quad.transform.localToWorldMatrix;
                    
                    Gizmos_Extend.DrawLinesConcat(quad.m_QuadShapeLS.Iterate());
                    // Gizmos.DrawLine(Vector3.up,Vector3.up+Vector3.forward);

                    Gizmos.matrix = Matrix4x4.identity;
                    if(m_RelativeVertexGizmos)
                        for(int i=0;i<quad.m_NearbyVertsCW.Length;i++)
                        {
                            Gizmos.color = UColor.IndexToColor(i);
                            if(m_GridVertices.Contains(quad.m_NearbyVertsCW[i]))
                                Gizmos_Extend.DrawLine(quad.transform.position,m_GridVertices[quad.m_NearbyVertsCW[i]].m_Vertex.m_Coord.ToPosition(),.8f);
                        }
                    

                    if(m_RelativeQuadGizmos)
                        for(int i=0;i<quad.m_NearbyQuadsCW.Length;i++)
                        {
                            Gizmos.color = UColor.IndexToColor(i);
                            if(m_GridQuads.Contains(quad.m_NearbyQuadsCW[i]))
                                Gizmos_Extend.DrawLine(quad.transform.position,m_GridQuads[quad.m_NearbyQuadsCW[i]].m_Quad.m_CoordCenter.ToPosition(),.4f);
                        }
                }
            }

            if (m_CornerGizmos&&m_Corners!=null)
            {
                
                foreach (var corner in m_Corners)
                {
                    
                    Gizmos.color = Color.cyan;
                    Gizmos.matrix = corner.transform.localToWorldMatrix;
                    Gizmos.DrawWireSphere(Vector3.zero,.5f);
                    
                    if (m_CornerSideRelations)
                    {
                        Gizmos.matrix = Matrix4x4.identity;
                        Gizmos.color = Color.green;
                        foreach (var cornerID in corner.NearbyCorners)
                            Gizmos_Extend.DrawLine(corner.transform.position, m_Corners[cornerID].transform.position , .4f);
                    }

                    if(m_CornerVoxelRelations)
                    {
                        Gizmos.matrix = Matrix4x4.identity;
                        Gizmos.color = Color.yellow;
                        foreach (var voxel in corner.NearbyVoxels)
                            Gizmos_Extend.DrawLine(corner.transform.position,m_Voxels[voxel].transform.position,.8f);
                    }
                }
            }

            if (m_VoxelGizmos&&m_Voxels!=null)
            {
                Gizmos.color = Color.white;
                foreach (var voxel in m_Voxels)
                {
                    Gizmos.color = Color.white;
                    Gizmos.matrix = voxel.transform.localToWorldMatrix;
                    Gizmos.DrawSphere(Vector3.zero,.3f);
                    Gizmos.matrix = Matrix4x4.identity;
                    
                    if (m_VoxelCornerRelations)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            Gizmos.color = UColor.IndexToColor(i%4);
                            if (voxel.CornerRelations[i])
                                Gizmos_Extend.DrawLine(voxel.transform.position,m_Corners[voxel.QubeCorners[i]].transform.position,.8f);
                        }
                    }
                    
                    if (m_VoxelSideRelations)
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            Gizmos.color = UColor.IndexToColor(i);
                            if(voxel.SideRelations[i])
                                Gizmos_Extend.DrawLine(voxel.transform.position,m_Voxels[voxel.CubeSides[i]].transform.position,.4f);
                        }
                    }
                }
            }
        }
        #endregion
        #endif
    }
}