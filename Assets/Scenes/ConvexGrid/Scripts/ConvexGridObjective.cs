using System.Collections;
using System.Collections.Generic;
using Geometry;
using Procedural;
using Procedural.Hexagon;
using Procedural.Hexagon.Area;
using Procedural.Hexagon.Geometry;
using UnityEngine;

namespace ConvexGrid
{
    public class ConvexVertex
    {
        public HexCoord m_Hex;
        public Coord m_Coord;
        public readonly List<ConvexQuad> m_NearbyQuads = new List<ConvexQuad>();
        private readonly int[] m_NearbyQuadsStartIndex = new int[6];
        private static readonly int[] s_QuadIndexHelper = new int[4];
        public int[] GetQuadVertsCW(int _index)
        {
            int offset = m_NearbyQuadsStartIndex[_index];
            s_QuadIndexHelper[0] = offset;
            s_QuadIndexHelper[1] = (offset + 1) % 4;
            s_QuadIndexHelper[2] = (offset + 2) % 4;
            s_QuadIndexHelper[3] = (offset + 3) % 4;
            return s_QuadIndexHelper;
        }
        public void AddNearbyQuads(ConvexQuad _quad)
        {
            m_NearbyQuadsStartIndex[m_NearbyQuads.Count] = _quad.m_HexQuad.FindIndex(p => p == m_Hex);
            m_NearbyQuads.Add(_quad);
        }
    }
    public class ConvexQuad
    {
        public HexCoord m_Identity => m_HexQuad.identity;
        public HexQuad m_HexQuad { get; private set; }
        public CoordQuad m_CoordQuad { get; private set; }
        public Coord m_CoordCenter { get; private set; }
        public readonly ConvexVertex[] m_Vertices = new ConvexVertex[4];
        public ConvexQuad(HexQuad _hexQuad,Dictionary<HexCoord,ConvexVertex> _vertices)
        {
            m_HexQuad = _hexQuad;
            m_CoordQuad=new CoordQuad(
                _vertices[m_HexQuad.vB].m_Coord,
                _vertices[m_HexQuad.vL].m_Coord,
                _vertices[m_HexQuad.vF].m_Coord,
                _vertices[m_HexQuad.vR].m_Coord);
            m_Vertices[0] = _vertices[m_HexQuad.vB];
            m_Vertices[1] = _vertices[m_HexQuad.vL];
            m_Vertices[2] = _vertices[m_HexQuad.vF];
            m_Vertices[3] = _vertices[m_HexQuad.vR];
            m_CoordCenter = m_CoordQuad.GetBaryCenter();
        }
    }
    public class ConvexArea
    {
        public HexagonArea m_Identity;
        public readonly List<ConvexQuad> m_Quads = new List<ConvexQuad>();
        public readonly List<ConvexVertex> m_Vertices = new List<ConvexVertex>();
        public ConvexArea(HexagonArea identity)
        {
            m_Identity = identity;
        }
    }
}