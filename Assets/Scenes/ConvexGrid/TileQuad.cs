using System;
using System.Collections.Generic;
using Geometry;
using Geometry.Voxel;
using LinqExtentions;
using TPool;
using Procedural;
using Procedural.Hexagon;
using Procedural.Hexagon.Geometry;
using Procedural.Tile;
using UnityEngine;

namespace ConvexGrid
{
    public class TileQuad : PoolBehaviour<HexCoord>
    {
        public ConvexQuad m_Quad { get; private set; }
        public GQuad m_OrientedShapeOS { get;private set; }
        public HexQuad m_NearbyVertsCW { get; private set; }
        public HexQuad m_NearbyQuadsCW { get; private set; }

        private static readonly Coord[] offsets = new Coord[4];
        private static readonly List<(int index, float rad)> radHelper = new List<(int index, float rad)>(4);
        private static readonly List<ConvexQuad> availableQuads = new List<ConvexQuad>(4);
        public TileQuad Init(ConvexQuad _quad)
        {
            m_Quad = _quad;
            radHelper.Clear();
            for (int i = 0; i < 4; i++)
            {
                offsets[i] = m_Quad.m_CoordQuad[i] - m_Quad.m_CoordCenter;
                radHelper.Add((i,UMath.GetRadClockWise(Vector2.up,offsets[i])));
            }
            radHelper.Sort((a, b) =>  a.rad > b.rad?1:-1 );
            m_NearbyVertsCW = new HexQuad( m_Quad.m_HexQuad[radHelper[0].index], m_Quad.m_HexQuad[ radHelper[1].index],
                m_Quad.m_HexQuad[ radHelper[2].index], m_Quad.m_HexQuad[radHelper[3].index]);
            
            Quaternion rotation = Quaternion.Euler(0, radHelper[0].rad * UMath.Rad2Deg, 0);
            transform.SetPositionAndRotation( m_Quad.m_CoordCenter.ToPosition() , rotation);
            var inverseRotation = Quaternion.Inverse(rotation);
            m_OrientedShapeOS = new GQuad(inverseRotation*offsets[ radHelper[0].index].ToPosition(),inverseRotation*offsets[ radHelper[1].index].ToPosition(),
                inverseRotation*offsets[ radHelper[2].index].ToPosition(),inverseRotation*offsets[ radHelper[3].index].ToPosition());
            
            availableQuads.Clear();
            availableQuads.AddRange(m_Quad.m_Vertices[0].m_NearbyQuads.Extend(m_Quad.m_Vertices[2].m_NearbyQuads).Collect(quad =>quad.m_Identity!=m_Quad.m_Identity&&quad.m_HexQuad.MatchVertexCount(m_Quad.m_HexQuad) == 2));
            if(availableQuads.Count!=4)
                throw new Exception("Invalid Nearby Quads Count:"+availableQuads.Count);

            radHelper.Clear();
            for(int i=0;i<4;i++)
                radHelper.Add((i,UMath.GetRadClockWise(Vector2.up,availableQuads[i].m_CoordCenter-m_Quad.m_CoordCenter)));    
            radHelper.Sort((a,b)=>a.rad>b.rad?1:-1);
            m_NearbyQuadsCW = new HexQuad(availableQuads[radHelper[0].index].m_Identity,availableQuads[radHelper[1].index].m_Identity,
                availableQuads[radHelper[2].index].m_Identity,availableQuads[radHelper[3].index].m_Identity);
            return this;
        }
        public override void OnPoolRecycle()
        {
            m_Quad = null;
        }
    }
}