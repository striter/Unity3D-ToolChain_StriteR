using TPool;
using Procedural.Hexagon;
using UnityEngine;

namespace ConvexGrid
{
    public class GridVoxel : PoolBehaviour<GridPile>
    {
        public GridQuad m_Quad { get; private set; }
        public GridVoxel Init(GridQuad _srcQuad)
        {
            m_Quad = _srcQuad;
            transform.SetParent(m_Quad.transform);
            transform.localPosition = ConvexGridHelper.GetVoxelHeight(m_PoolID);
            transform.localRotation = Quaternion.identity;
            return this;
        }

    }
}