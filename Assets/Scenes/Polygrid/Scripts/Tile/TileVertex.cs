using System;
using System.Collections.Generic;
using LinqExtentions;
using TPool;
using Procedural;
using Procedural.Hexagon;
using UnityEngine;

namespace PolyGrid.Tile
{
    public class TileVertex : PoolBehaviour<HexCoord>
    {
        public PolyVertex m_Vertex { get; private set; }
        public Mesh m_CornerMesh { get; private set; }
        public readonly List<(Vector3 position,HexCoord _vertex)> m_RelativeCornerDirections=new List<(Vector3 position, HexCoord _vertex)>();
        public override void OnPoolInit(Action<HexCoord> _DoRecycle)
        {
            base.OnPoolInit(_DoRecycle);
            m_CornerMesh = new Mesh() { hideFlags = HideFlags.HideAndDontSave};
        }

        public void Init(PolyVertex _vertex)
        {
            m_Vertex = _vertex;
            m_CornerMesh.name = $"GridVertex: {m_Vertex.m_Identity}";
            _vertex.ConstructLocalMesh(m_CornerMesh,EQuadGeometry.Half,EVoxelGeometry.VoxelTight,out Vector3 positionWS,true,true);
            m_RelativeCornerDirections.Clear();
            foreach (var tuple in _vertex.m_NearbyQuads.LoopIndex())
            {
                var quad = tuple.value;
                var index = tuple.index;
                var indexes = _vertex.GetQuadVertsCW(index);
                var destIndex = indexes[1];
                var srcIndex = indexes[0];
                var center = (( quad.m_CoordQuad[destIndex]-quad.m_CoordQuad[srcIndex]) / 2).ToPosition();
                m_RelativeCornerDirections.Add((center,quad.m_HexQuad[destIndex]));
            }
            transform.position = positionWS;
        }

        
        public override void OnPoolRecycle()
        {
            m_Vertex = null;
            m_CornerMesh.Clear();
        }
    }
}