using System;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using Geometry.Pixel;
using Geometry.Voxel;
using GridTest;
using LinqExtentions;
using ObjectPool;
using ObjectPoolStatic;
using Procedural;
using Procedural.Hexagon;
using Procedural.Hexagon.Geometry;
using Procedural.Tile;
using UnityEngine;

namespace ConvexGrid
{
    public class GridQuad : PoolBehaviour<HexCoord>
    {
        public ConvexQuad m_Quad { get; private set; }
        public GQuad m_ShapeOS { get;private set; }
        private HexCoord m_ForwardVertex;
        private HexCoord m_RightVertex;
        private HexCoord m_BottomVertex;
        private HexCoord m_LeftVertex;
        public IEnumerable<HexCoord> GetNearbyVertices()
        {
            yield return m_ForwardVertex;
            yield return m_RightVertex;
            yield return m_BottomVertex;
            yield return m_LeftVertex;
        }
        public HexCoord GetNearbyVertex(ETileDirection _direction)
        {
            switch (_direction)
            {
                default: throw new Exception("Invalid Direction Found:" + _direction);
                case ETileDirection.Forward: return m_ForwardVertex;
                case ETileDirection.Right: return m_RightVertex;
                case ETileDirection.Bottom: return m_BottomVertex;
                case ETileDirection.Left: return m_LeftVertex;
            }
        }
        
        private HexCoord m_ForwardQuad;
        private HexCoord m_RightQuad;
        private HexCoord m_BottomQuad;
        private HexCoord m_LeftQuad;
        public IEnumerable<HexCoord> GetNearbyQuads()
        {
            yield return m_ForwardQuad;
            yield return m_RightQuad;
            yield return m_BottomQuad;
            yield return m_LeftQuad;
        }
        public HexCoord GetRelativeQuadIdentity(ETileDirection _direction)
        {
            switch (_direction)
            {
                default: throw new Exception("Invalid Direction Found:" + _direction);
                case ETileDirection.Forward: return m_ForwardQuad;
                case ETileDirection.Right: return m_RightQuad;
                case ETileDirection.Bottom: return m_BottomQuad;
                case ETileDirection.Left: return m_LeftQuad;
            }
        }

        private static readonly Coord[] offsets = new Coord[4];
        private static readonly List<(int index, float rad)> radHelper = new List<(int index, float rad)>(4);
        private static readonly List<ConvexQuad> availableQuads = new List<ConvexQuad>(4);
        public GridQuad Init(ConvexQuad _quad)
        {
            m_Quad = _quad;
            radHelper.Clear();
            for (int i = 0; i < 4; i++)
            {
                offsets[i] = m_Quad.m_CoordQuad[i] - m_Quad.m_CoordCenter;
                radHelper.Add((i,UMath.GetRadClockWise(Vector2.up,offsets[i])));
            }
            radHelper.Sort((a, b) =>  a.rad > b.rad?1:-1 );
            m_ForwardVertex= m_Quad.m_HexQuad[radHelper[0].index];
            m_RightVertex = m_Quad.m_HexQuad[radHelper[1].index];
            m_BottomVertex = m_Quad.m_HexQuad[radHelper[2].index];
            m_LeftVertex = m_Quad.m_HexQuad[radHelper[3].index];
            Quaternion rotation = Quaternion.Euler(0, radHelper[0].rad * UMath.Rad2Deg, 0);
            transform.SetPositionAndRotation( m_Quad.m_CoordCenter.ToPosition() , rotation);
            var inverseRotation = Quaternion.Inverse(rotation);
            m_ShapeOS = new GQuad(inverseRotation*offsets[0].ToPosition(),inverseRotation*offsets[1].ToPosition(),inverseRotation*offsets[2].ToPosition(),inverseRotation*offsets[3].ToPosition());
            
            availableQuads.Clear();
            availableQuads.AddRange(m_Quad.m_Vertices[0].m_NearbyQuads.Extend(m_Quad.m_Vertices[2].m_NearbyQuads).Collect(quad =>quad.m_Identity!=m_Quad.m_Identity&&quad.m_HexQuad.MatchVertexCount(m_Quad.m_HexQuad) == 2));
            if(availableQuads.Count!=4)
                throw new Exception("Invalid Nearby Quads Count:"+availableQuads.Count);

            radHelper.Clear();
            for(int i=0;i<4;i++)
                radHelper.Add((i,UMath.GetRadClockWise(Vector2.up,availableQuads[i].m_CoordCenter-m_Quad.m_CoordCenter)));    
            radHelper.Sort((a,b)=>a.rad>b.rad?1:-1);
            m_ForwardQuad= availableQuads[radHelper[0].index].m_Identity;
            m_RightQuad = availableQuads[radHelper[1].index].m_Identity;
            m_BottomQuad = availableQuads[radHelper[2].index].m_Identity;
            m_LeftQuad = availableQuads[radHelper[3].index].m_Identity;
            return this;
        }
        public override void OnPoolRecycle()
        {
            m_Quad = null;
        }
    }
}