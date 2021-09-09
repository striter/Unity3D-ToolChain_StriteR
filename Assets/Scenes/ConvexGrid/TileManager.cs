using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LinqExtentions;
using TPool;
using TPoolStatic;
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

        byte GetMaxCornerHeight(HexCoord _quadID)
        {
            var maxHeight = byte.MinValue;
            for (int i = 0; i < m_GridQuads[_quadID].m_NearbyVertsCW.Length; i++)
            {
                var vertex = m_GridQuads[_quadID].m_NearbyVertsCW[i];
                if (m_Corners.Count(vertex) != 0)
                    maxHeight = Math.Max(maxHeight, UByte.ForwardOne( m_Corners.Max(vertex)));
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

        void FillQuad(ConvexQuad _quadData)
        {
            var quadID = _quadData.m_Identity;
            if (m_GridQuads.Contains(quadID))
                return;
            m_GridQuads.Spawn(quadID).Init(_quadData);
        }

        void FillCorner(PileID _cornerID)
        {
            if (m_Corners.Contains(_cornerID))
                return;
            var vertex = m_GridVertices[_cornerID.gridID];
            var corner = m_Corners.Spawn(_cornerID).Init(vertex);
        }

        void FillVoxels(HexCoord _quadID)
        {
            var maxHeight = GetMaxCornerHeight(_quadID);
            Debug.Log(maxHeight);
            for (byte i = 0; i <= maxHeight; i++)
            {
                var cornerID = new PileID(_quadID, i);
                TileVoxel voxel = m_Voxels.Contains(cornerID) ? m_Voxels.Get(cornerID): m_Voxels.Spawn(cornerID).Init(m_GridQuads[_quadID]);
                voxel.RefreshRelations(m_Corners);
            }
        }
        
        void RemoveVertex(HexCoord _vertex)
        {
            if (m_Corners.Count(_vertex) != 0)
                return;
            
            if (!m_GridVertices.Contains(_vertex))
                return;
            m_GridVertices.Recycle(_vertex);
        }

        void RemoveQuad(HexCoord _quadIdentity)
        {
            if (m_Voxels.Count(_quadIdentity) != 0)
                return;
            if (!m_GridQuads.Contains(_quadIdentity))
                return;
            m_GridQuads.Recycle(_quadIdentity);
        }
        
        void RemoveCorner(PileID _cornerID)
        {
            if (!m_Corners.Contains(_cornerID))
                return;
            var corner=m_Corners.Recycle(_cornerID);
        }

        void RemoveVoxels(HexCoord _quadID)
        {
            var maxHeight = GetMaxCornerHeight(_quadID);
            Debug.Log(maxHeight);
            maxHeight = maxHeight == byte.MinValue ? byte.MinValue : UByte.ForwardOne(maxHeight);
            var srcHeight = m_Voxels.Max(_quadID);
            Debug.Log(maxHeight+" "+srcHeight);
            for (var i = maxHeight; i <= srcHeight; i++)
                m_Voxels.Recycle(new PileID(_quadID, i));
        }
        
        public void Tick(float _deltaTime)
        {
        }

        public void OnSelectVertex(ConvexVertex _vertex, byte _height, bool _construct)
        {
            var corner = new PileID(_vertex.m_Hex, _height);
            bool contains = m_Corners.Contains(corner);
            if (_construct&&!contains)
            {
                FillVertex(_vertex);
                foreach (var convexQuad in _vertex.m_NearbyQuads)
                    FillQuad(convexQuad);
                FillCorner(corner);
                foreach (var quad in _vertex.m_NearbyQuads)
                    FillVoxels(quad.m_Identity);
            }
            
            if(!_construct&&contains)
            {
                RemoveCorner(corner);
                foreach (var quad in _vertex.m_NearbyQuads)
                    RemoveVoxels(quad.m_Identity);
                RemoveVertex(_vertex.m_Hex);
                foreach (var quad in _vertex.m_NearbyQuads)
                    RemoveQuad(quad.m_Identity);
            }
        }

        public void OnAreaConstruct(ConvexArea _area)
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
        [MFoldout(nameof(m_VoxelGizmos),true)]
        public bool m_VoxelRelations;
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
                    
                    Gizmos_Extend.DrawLines(quad.m_ShapeOS.Iterate());
                    // Gizmos.DrawLine(Vector3.up,Vector3.up+Vector3.forward);

                    Gizmos.matrix = Matrix4x4.identity;
                    if(m_RelativeVertexGizmos)
                        for(int i=0;i<quad.m_NearbyVertsCW.Length;i++)
                        {
                            Gizmos.color = URender.IndexToColor(i);
                            if(m_GridVertices.Contains(quad.m_NearbyVertsCW[i]))
                                Gizmos.DrawLine(quad.transform.position,m_GridVertices[quad.m_NearbyVertsCW[i]].m_Vertex.m_Coord.ToPosition());
                        }
                    

                    if(m_RelativeQuadGizmos)
                        for(int i=0;i<quad.m_NearbyQuadsCW.Length;i++)
                        {
                            Gizmos.color = URender.IndexToColor(i);
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
                    Gizmos.DrawWireCube(Vector3.zero,Vector3.one);
                    Gizmos.matrix = Matrix4x4.identity;
                    if (m_VoxelRelations)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            Gizmos.color = URender.IndexToColor(i%4);
                            if (voxel.m_CornerRelations[i])
                            {
                                var cornerID = voxel.GetCornerID(i);
                                if(!m_Corners.Contains(cornerID))
                                    continue;
                                Gizmos.DrawLine(voxel.transform.position,m_Corners.Get(cornerID).transform.position);
                            }
                        }
                    }
                }
            }
        }
        #endregion
        #endif
    }
}