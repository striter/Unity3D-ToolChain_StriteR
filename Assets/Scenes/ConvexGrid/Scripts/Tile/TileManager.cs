using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LinqExtentions;
using TPool;
using TPoolStatic;
using Procedural;
using Procedural.Hexagon;
using UnityEngine;

namespace ConvexGrid
{
    public class TileManager : MonoBehaviour,IConvexGridControl
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

        public void OnSelectVertex(ConvexVertex _vertex, byte _height)
        {
        }

        public void OnAreaConstruct(ConvexArea _area)
        {
        }

        public void CornerConstruction( ConvexVertex _vertex, byte _height,Action<IModuleCollector> _moduleSpawn)
        {
            var corner = new PileID(_vertex.m_Hex, _height);
            if (m_Corners.Contains(corner))
                return;
            
            FillVertex(_vertex);
            FillQuads(_vertex);
            
            FillCorner(corner);
            FillVoxels(_vertex,_moduleSpawn);
            RefreshVoxels(_vertex);
        }

        public void CornerDeconstruction(ConvexVertex _vertex, byte _height,Action<PileID> _moduleRecycle)
        {
            var corner = new PileID(_vertex.m_Hex, _height);
            if (!m_Corners.Contains(corner))
                return;
            
            RemoveCorner(corner);
            RemoveVoxels(_vertex,_moduleRecycle);
            RefreshVoxels(_vertex);
            
            RemoveVertex(_vertex);
            RemoveQuads(_vertex);
        }

        public IEnumerable<PileID> CollectAvailableModules(ConvexVertex _vertex,byte _height)
        {
            foreach (var quad in _vertex.m_NearbyQuads)
            {
                var quadID = quad.m_Identity;
                var curPile = new PileID(quadID, _height);
                var nextPile = new PileID(quadID,UByte.ForwardOne(_height));
                if (m_Voxels.Contains(curPile))
                    yield return curPile;
                if (m_Voxels.Contains(nextPile))
                    yield return nextPile;
            }
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
        
        void FillVertex(ConvexVertex _vertexData)
        {
            var vertexID = _vertexData.m_Hex;
            if (m_GridVertices.Contains(vertexID))
                return;
            m_GridVertices.Spawn(vertexID).Init(_vertexData);
        }

        void FillQuads(ConvexVertex _vertex)
        {
            foreach (var quad in _vertex.m_NearbyQuads)
            {
                var quadID = quad.m_Identity;
                if (m_GridQuads.Contains(quadID))
                    continue;
                m_GridQuads.Spawn(quadID).Init(quad);
            }
        }

        void FillCorner(PileID _cornerID)
        {
            if (m_Corners.Contains(_cornerID))
                return;
            var vertex = m_GridVertices[_cornerID.gridID];
            m_Corners.Spawn(_cornerID).Init(vertex);
        }

        void FillVoxels(ConvexVertex _vertex,Action<IModuleCollector> _spawn)
        {
            foreach (var quadID in _vertex.m_NearbyQuads.Select(p=>p.m_Identity))
            {
                var maxHeight = GetMaxCornerHeight(quadID);
                for (byte i = 0; i <= maxHeight; i++)
                {
                    var voxelID = new PileID(quadID, i);
                    if(m_Voxels.Contains(voxelID))
                        continue; 
                    _spawn(m_Voxels.Spawn(voxelID).Init(m_GridQuads[quadID]));
                }
            }
        }
        
        void RemoveVertex(ConvexVertex _vertex)
        {
            var vertexID = _vertex.m_Hex;
            if (m_Corners.Contains(vertexID)||!m_GridVertices.Contains(vertexID))
                return;
            m_GridVertices.Recycle(vertexID);
        }

        void RemoveQuads(ConvexVertex _vertex)
        {
            foreach (var quadID in _vertex.m_NearbyQuads.Select(p => p.m_Identity))
            {
                if (m_Voxels.Contains(quadID)||!m_GridQuads.Contains(quadID))
                    continue;
                m_GridQuads.Recycle(quadID);
            }
        }
        
        void RemoveCorner(PileID _cornerID)
        {
            if (!m_Corners.Contains(_cornerID))
                return;
            var corner=m_Corners.Recycle(_cornerID);
        }

        void RemoveVoxels(ConvexVertex _vertex,Action<PileID> _recycle)
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
                    _recycle(voxelID);
                }
            }
        }

        void RefreshVoxels(ConvexVertex _vertex)
        {
            var quadRefreshing = TSPoolList<HexCoord>.Spawn(_vertex.m_NearbyQuads.Count*3);
            
            foreach (var _quadID in _vertex.m_NearbyQuads.Select(p => p.m_Identity))
            {
                quadRefreshing.Add(_quadID);
                if(!m_GridQuads.Contains(_quadID))
                    continue;
                var quad = m_GridQuads[_quadID];
                quadRefreshing.TryAdd(quad.m_NearbyQuadsCW.vB);
                quadRefreshing.TryAdd(quad.m_NearbyQuadsCW.vL);
                quadRefreshing.TryAdd(quad.m_NearbyQuadsCW.vF);
                quadRefreshing.TryAdd(quad.m_NearbyQuadsCW.vR);
            }

            foreach (var _quadID in quadRefreshing)
            {
                if (!m_Voxels.Contains(_quadID))
                   continue;
                var maxHeight = GetMaxCornerHeight(_quadID);
                for (byte i = 0; i <= maxHeight; i++)
                    m_Voxels.Get(new PileID(_quadID, i)).RefreshRelations(m_Corners,m_Voxels);
            }
            
            TSPoolList<HexCoord>.Recycle(quadRefreshing);
        }

        public void Tick(float _deltaTime)
        {
        }

        #if UNITY_EDITOR
        #region Gizmos
        [Header("Gizmos")] 
        public bool m_VertexGizmos;
        public bool m_QuadGizmos;
        [MFoldout(nameof(m_QuadGizmos),true)] public bool m_RelativeQuadGizmos;
        [MFoldout(nameof(m_QuadGizmos),true)] public bool m_RelativeVertexGizmos;
        public bool m_CornerGizmos;
        [MFoldout(nameof(m_CornerGizmos), true)] public bool m_CornerMeshGizmos;
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
                                Gizmos.DrawLine(quad.transform.position,m_GridVertices[quad.m_NearbyVertsCW[i]].m_Vertex.m_Coord.ToPosition());
                        }
                    

                    if(m_RelativeQuadGizmos)
                        for(int i=0;i<quad.m_NearbyQuadsCW.Length;i++)
                        {
                            Gizmos.color = UColor.IndexToColor(i);
                            if(m_GridQuads.Contains(quad.m_NearbyQuadsCW[i]))
                                Gizmos.DrawLine(quad.transform.position,(quad.transform.position+ m_GridQuads[quad.m_NearbyQuadsCW[i]].m_Quad.m_CoordCenter.ToPosition())/2f);
                        }
                }
            }

            if (m_CornerGizmos&&m_Corners!=null)
            {
                Gizmos.color = Color.green;
                foreach (var corner in m_Corners)
                {
                    Gizmos.matrix = corner.transform.localToWorldMatrix;
                    Gizmos.DrawWireSphere(Vector3.zero,.5f);
                    if(m_CornerMeshGizmos)
                        Gizmos.DrawWireMesh(corner.m_BaseVertex.m_CornerMesh);
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
                            if (voxel.m_CornerRelation[i])
                                Gizmos.DrawLine(voxel.transform.position,m_Corners.Get(voxel.GetCornerID(i)).transform.position);
                        }
                    }

                    if (m_VoxelSideRelations)
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            Gizmos.color = UColor.IndexToColor(i);
                            if(voxel.m_SideRelation[i])
                                Gizmos.DrawLine(voxel.transform.position,(voxel.transform.position+m_Voxels.Get(voxel.GetFacingID(i)).transform.position)/2);
                        }
                    }
                }
            }
        }
        #endregion
        #endif
    }
}