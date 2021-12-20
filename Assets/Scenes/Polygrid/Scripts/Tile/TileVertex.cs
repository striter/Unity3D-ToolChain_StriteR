using TPool;
using Procedural;
using Procedural.Hexagon;

namespace PolyGrid.Tile
{
    public class TileVertex : PoolBehaviour<HexCoord>
    {
        public PolyVertex m_Vertex { get; private set; }

        public void Init(PolyVertex _vertex)
        {
            m_Vertex = _vertex;
            transform.position = _vertex.m_Coord.ToPosition();
        }

        public override void OnPoolRecycle()
        {
            m_Vertex = null;
        }
    }
}