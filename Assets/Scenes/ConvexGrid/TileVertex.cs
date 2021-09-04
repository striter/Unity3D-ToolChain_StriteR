using System.Collections;
using System.Collections.Generic;
using ObjectPool;
using Procedural.Hexagon;
using UnityEngine;

namespace ConvexGrid
{
    public class TileVertex : APoolMono<HexCoord>
    {
        public ConvexVertex m_Vertex { get; private set; }
        public void Init(ConvexVertex _vertex)
        {
            m_Vertex = _vertex;
        }

        public override void OnPoolRecycle()
        {
            m_Vertex = null;
        }

    }
}