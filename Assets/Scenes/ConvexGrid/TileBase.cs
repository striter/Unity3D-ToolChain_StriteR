using System;
using System.Collections;
using System.Collections.Generic;
using Geometry;
using Geometry.Voxel;
using ObjectPool;
using Procedural.Hexagon.Geometry;
using UnityEngine;

namespace ConvexGrid
{
    public class TileBase : APoolMono<HexQuad>
    {
        public ConvexQuad m_Quad { get; private set; }
        public readonly List<ConvexVertex> m_NearbyVertex = new List<ConvexVertex>();
        public bool m_Selected { get; private set; }
        public TileBase Init(ConvexQuad _quad)
        {
            m_Quad = _quad;
            m_Selected = false;
            m_Quad.m_CoordQuad.GetBaryCenter().ToWorld();
            return this;
        }

        public void AddConvexRelation(ConvexVertex _vertex) => m_NearbyVertex.Add(_vertex);

        public override void OnPoolRecycle()
        {
            m_NearbyVertex.Clear();
            m_Selected = false;
            m_Quad = null;
        }

        private void OnDrawGizmos()
        {
            if (m_Quad == null)
                return;

            var center = m_Quad.m_CoordQuad.GetBaryCenter().ToWorld() + Vector3.up * ConvexGridHelper.m_TileHeight / 2f;
            Gizmos.DrawWireCube( center,Vector3.one*ConvexGridHelper.m_TileHeight/2f);
            foreach (ConvexVertex vertex in m_NearbyVertex)
                Gizmos.DrawLine(center,vertex.m_Coord.ToWorld());
        }
    }

}