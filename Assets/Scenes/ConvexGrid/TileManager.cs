using System.Collections.Generic;
using System.Linq;
using ObjectPool;
using Procedural.Hexagon;
using Procedural.Hexagon.Geometry;
using UnityEngine;

namespace ConvexGrid
{

    public class TileManager : MonoBehaviour,IConvexGridControl
    {
        private readonly Dictionary<HexCoord, ConvexVertex> m_VertexSelected=new Dictionary<HexCoord, ConvexVertex>();
        private TObjectPoolMono<HexQuad,TileBase> m_TileController;
        public void Init(Transform _transform)
        {
            m_TileController = new TObjectPoolMono<HexQuad,TileBase>(_transform.Find("TileContainer/TileBase"));
        }


        public void Clear()
        {
            m_TileController.Clear();
            m_VertexSelected.Clear();
        }
        
        public void Tick(float _deltaTime)
        {
        }

        public void OnSelectVertex(ConvexVertex _vertex)
        {
            if (m_VertexSelected.ContainsKey(_vertex.m_Hex))
                m_VertexSelected.Remove(_vertex.m_Hex);
            else
                m_VertexSelected.Add(_vertex.m_Hex, _vertex);

            UpdateTiles();
        }

        public void OnAreaConstruct(ConvexArea _area)
        {
            UpdateTiles();
        }

        void UpdateTiles()
        {
            m_TileController.Clear();
            foreach (var vertex in m_VertexSelected.Values)
            {
                foreach (ConvexQuad quad in vertex.m_RelativeQuads)
                    m_TileController.TryAddItem(quad.m_HexQuad).Init(quad);
            }
        }

        private void OnDrawGizmos()
        {
            foreach (var vertexSelect in m_VertexSelected.Values)
                Gizmos.DrawWireSphere(vertexSelect.m_Coord.ToWorld(),.5f);
        }
    }
}