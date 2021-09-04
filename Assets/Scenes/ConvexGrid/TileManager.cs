using System.Collections.Generic;
using System.Linq;
using Geometry;
using LinqExtentions;
using ObjectPool;
using Procedural.Hexagon;
using Procedural.Hexagon.Geometry;
using UnityEngine;

namespace ConvexGrid
{

    public class TileManager : MonoBehaviour,IConvexGridControl
    {
        private TObjectPoolMono<HexCoord, TileVertex> m_Verticies;
        private TObjectPoolMono<HexCoord,TileQuad> m_Quads;
        public void Init(Transform _transform)
        {
            m_Verticies = new TObjectPoolMono<HexCoord, TileVertex>(transform.Find("Tile/Vertex/Item"));
            m_Quads = new TObjectPoolMono<HexCoord,TileQuad>(_transform.Find("Tile/Quad/Item"));
        }


        public void Clear()
        {
            m_Quads.Clear();
            m_Verticies.Clear();
        }
        
        public void Tick(float _deltaTime)
        {
        }

        public void OnSelectVertex(ConvexVertex _vertex)
        {
            if (!m_Verticies.Contains(_vertex.m_Hex))
                m_Verticies.Spawn(_vertex.m_Hex).Init(_vertex);
            else
                m_Verticies.Recycle(_vertex.m_Hex);

            GenerateQuads();
        }

        public void OnAreaConstruct(ConvexArea _area)
        {
            GenerateQuads();
        }
        
        void GenerateQuads()
        {
            foreach (var quad in m_Quads.Collect(p=>p.m_Quad.m_HexQuad.All(k=>!m_Verticies.Contains(k))).ToArray())
                m_Quads.Recycle(quad.m_Identity);
            foreach (var vertex in m_Verticies)
            {
                foreach (var quad in vertex.m_Vertex.m_RelativeQuads)
                {
                    if (!m_Quads.Contains(quad.m_Identity))
                        m_Quads.Spawn(quad.m_Identity).Init(quad);
                }
            }
            
            foreach (var grid in m_Quads)
                grid.Refresh();
        }

        private void OnDrawGizmos()
        {
            if (m_Quads == null)
                return;
            Gizmos.color = Color.white;
            foreach (var vertex in m_Verticies)
                Gizmos.DrawSphere(vertex.m_Vertex.m_Coord.ToWorld(),.3f);

            foreach (var quad in m_Quads)
            {
                Gizmos.color = Color.white;
                Gizmos.matrix = quad.transform.localToWorldMatrix;
                Gizmos.DrawWireCube( Vector3.zero,Vector3.one*ConvexGridHelper.m_TileHeight/2f);
                Gizmos.DrawLine(Vector3.zero,Vector3.forward);
        
                Gizmos.matrix=Matrix4x4.identity;
                foreach (var coordTuple in quad.m_RelativeVertexCoords.LoopIndex())
                {
                    Gizmos.color = URender.IndexToColor(coordTuple.index);
                    if(m_Verticies.Contains(coordTuple.value))
                        Gizmos.DrawLine(quad.transform.position,m_Verticies[coordTuple.value].m_Vertex.m_Coord.ToWorld());
                }
            }
        }
    }
}