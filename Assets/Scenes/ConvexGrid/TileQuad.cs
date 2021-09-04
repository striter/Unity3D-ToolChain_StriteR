using System;
using System.Collections;
using System.Collections.Generic;
using Geometry;
using Geometry.Voxel;
using ObjectPool;
using ObjectPoolStatic;
using Procedural;
using Procedural.Hexagon;
using Procedural.Hexagon.Geometry;
using UnityEngine;

namespace ConvexGrid
{
    public class TileQuad : APoolMono<HexCoord>
    {
        public ConvexQuad m_Quad { get; private set; }
        public float m_Angle { get; private set; }
        public readonly List<HexCoord> m_RelativeVertexCoords = new List<HexCoord>();
        public bool m_Selected { get; private set; }
        public TileQuad Init(ConvexQuad _quad)
        {
            m_Quad = _quad;
            m_Selected = false;
            return this;
        }

        public void Refresh()
        {
            m_RelativeVertexCoords.Clear();
            var list = TSPoolList<(int index, float rad)>.Spawn();
            var center = m_Quad.m_CoordQuad.GetBaryCenter();
            for (int i = 0; i < 4; i++)
                list.Add((i,UMath.GetRadClockWise(Vector2.up,m_Quad.m_CoordQuad[i]-center)));
            list.Sort((a, b) =>  a.rad > b.rad?1:-1 );
            foreach (var tuple in list)
                m_RelativeVertexCoords.Add(m_Quad.m_HexQuad[tuple.index]);
            transform.SetPositionAndRotation( center.ToWorld() , Quaternion.Euler(0,list[0].rad*UMath.Rad2Deg,0));
            TSPoolList<(int index, float rad)>.Recycle(list);
        }

        public override void OnPoolRecycle()
        {
            m_Selected = false;
            m_Quad = null;
        }

    }

}