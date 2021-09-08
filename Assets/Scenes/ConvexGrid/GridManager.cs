using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using LinqExtentions;
using TPool;
using TPoolStatic;
using Procedural;
using Procedural.Hexagon;
using Procedural.Hexagon.Geometry;
using Unity.Mathematics;
using UnityEngine;

namespace ConvexGrid
{
    public struct GridPile:IEquatable<GridPile>
    {
        public HexCoord gridID;
        public byte height;

        public GridPile(HexCoord _gridID, byte _height)
        {
            gridID = _gridID;
            height = _height;
        }

        public override string ToString() => gridID.ToString() +" "+ height;

        public bool Equals(GridPile other)=> gridID.Equals(other.gridID) && height == other.height;

        public override bool Equals(object obj)=> obj is GridPile other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (gridID.GetHashCode() * 397) ^ height.GetHashCode();
            }
        }
    }
    class PilePool<T> : IEnumerable<T> where T:PoolBehaviour<GridPile>
    {
        private readonly Dictionary<HexCoord, List<byte>> m_Corners = new Dictionary<HexCoord, List<byte>>();
        readonly TObjectPoolMono<GridPile,T> m_Pool;

        public PilePool(Transform _transform)
        {
            m_Pool = new TObjectPoolMono<GridPile, T>(_transform);
        }
        public bool Contains(HexCoord _coord,byte _height)
        {
            if (!m_Corners.ContainsKey(_coord))
                return false;
            return m_Corners[_coord].Contains(_height);
        }
        public T Spawn(HexCoord _coord,byte _height)
        {
            T item = m_Pool.Spawn( new GridPile(_coord,_height));
            AddVertex(_coord);
            m_Corners[_coord].Add(_height);
            return item;
        }

        public T Recycle(HexCoord _coord,byte _height)
        {
            T item = m_Pool.Recycle(new GridPile(_coord,_height));
            m_Corners[_coord].Remove(_height);
            RemoveVertex(_coord);
            return item;
        }

        public byte Count(HexCoord _location)
        {
            if (!m_Corners.ContainsKey(_location))
                return 0;
            return (byte)m_Corners[_location].Count;
        }

        public byte Max(HexCoord _location)
        {
            if (!m_Corners.ContainsKey(_location))
                return 0;
            return m_Corners[_location].Max();
        }
        void AddVertex(HexCoord _vertex)
        {
            if (m_Corners.ContainsKey(_vertex))
                return;
            m_Corners.Add(_vertex,TSPoolList<byte>.Spawn());
        }

        void RemoveVertex(HexCoord _vertex)
        {
            if (m_Corners[_vertex].Count != 0)
                return;
            TSPoolList<byte>.Recycle(m_Corners[_vertex]);
            m_Corners.Remove(_vertex);
        }


        public void Clear()
        {
            foreach (var vertex in m_Corners.Keys)
                TSPoolList<byte>.Recycle(m_Corners[vertex]);
            m_Corners.Clear();
            m_Pool.Clear();
        }
        public IEnumerator<T> GetEnumerator() => m_Pool.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()=> GetEnumerator();
    }
    
    public class GridManager : MonoBehaviour,IConvexGridControl
    {
        private TObjectPoolMono<HexCoord, GridVertex> m_GridVertices;
        private TObjectPoolMono<HexCoord,GridQuad> m_GridQuads;
        private PilePool<GridCorner> m_Corners;
        private PilePool<GridVoxel> m_Voxels;
        public void Init(Transform _transform)
        {
            m_GridVertices = new TObjectPoolMono<HexCoord, GridVertex>(_transform.Find("Grid/Vertex/Item"));
            m_GridQuads = new TObjectPoolMono<HexCoord,GridQuad>(_transform.Find("Grid/Quad/Item"));
            m_Corners = new PilePool<GridCorner>(_transform.Find("Grid/Corner/Item"));
            m_Voxels = new PilePool<GridVoxel>(_transform.Find("Grid/Voxel/Item"));
        }

        public void Clear()
        {
            m_GridQuads.Clear();
            m_GridVertices.Clear();
            m_Corners.Clear();
            m_Voxels.Clear();
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

        void FillCorner(HexCoord _coord,byte _height)
        {
            if (m_Corners.Contains(_coord,_height))
                return;
            var vertex = m_GridVertices[_coord];
            var corner = m_Corners.Spawn(_coord,_height).Init(vertex);
        }

        void FillVoxels(HexCoord _quadID)
        {
            var quad = m_GridQuads[_quadID];
            var maxHeight = quad.m_Quad.m_Vertices.Max(p => m_Corners.Max(p.m_Hex));
            for (byte i = 0; i <= maxHeight; i++)
            {
                if (m_Voxels.Contains(_quadID,i))
                   continue;
                m_Voxels.Spawn(_quadID,i).Init(m_GridQuads[_quadID]);
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
        
        void RemoveCorner(HexCoord _coord,byte _height)
        {
            if (!m_Corners.Contains(_coord,_height))
                return;
            var corner=m_Corners.Recycle(_coord,_height);
        }

        void RemoveVoxels(HexCoord _quadID)
        {
            var quad = m_GridQuads[_quadID];
            var maxHeight = quad.m_Quad.m_Vertices.Max(p => m_Corners.Count(p.m_Hex));
            var srcHeight = m_Voxels.Max(_quadID);
            for (var i = maxHeight ;i <= srcHeight; i++)
                m_Voxels.Recycle(_quadID, i);
        }
        
        public void Tick(float _deltaTime)
        {
        }

        public void OnSelectVertex(ConvexVertex _vertex, byte _height, bool _construct)
        {
            bool contains = m_Corners.Contains(_vertex.m_Hex, _height);
            if (_construct&&!contains)
            {
                FillVertex(_vertex);
                foreach (var convexQuad in _vertex.m_NearbyQuads)
                    FillQuad(convexQuad);
                FillCorner(_vertex.m_Hex,_height);
                foreach (var quad in _vertex.m_NearbyQuads)
                    FillVoxels(quad.m_Identity);
            }
            
            if(!_construct&&contains)
            {
                RemoveCorner(_vertex.m_Hex,_height);
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
                    
                    Gizmos_Extend.DrawLines(quad.m_ShapeOS.ConstructIteratorArray());
                    // Gizmos.DrawLine(Vector3.up,Vector3.up+Vector3.forward);

                    Gizmos.matrix = Matrix4x4.identity;
                    if(m_RelativeVertexGizmos)
                        foreach (var coordTuple in quad.GetNearbyVertices().LoopIndex())
                        {
                            Gizmos.color = URender.IndexToColor(coordTuple.index);
                            if(m_GridVertices.Contains(coordTuple.value))
                                Gizmos.DrawLine(quad.transform.position,m_GridVertices[coordTuple.value].m_Vertex.m_Coord.ToPosition());
                        }

                    if(m_RelativeQuadGizmos)
                        foreach (var quadTuple in quad.GetNearbyQuads().LoopIndex())
                        {
                            Gizmos.color = URender.IndexToColor(quadTuple.index);
                            if(m_GridQuads.Contains(quadTuple.value))
                                Gizmos.DrawLine(quad.transform.position,(quad.transform.position+ m_GridQuads[quadTuple.value].m_Quad.m_CoordCenter.ToPosition())/2f);
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
                    Gizmos.matrix = voxel.transform.localToWorldMatrix;
                    Gizmos.DrawWireCube(Vector3.zero,Vector3.one);
                }
            }
        }
        #endregion
        #endif
    }
}