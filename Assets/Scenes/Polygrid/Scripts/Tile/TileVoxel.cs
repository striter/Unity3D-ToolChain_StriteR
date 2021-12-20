using System;
using Geometry;
using TPool;
using Procedural.Hexagon;
using UnityEngine;

namespace PolyGrid.Tile
{
    public class TileVoxel : PoolBehaviour<PolyID>,IVoxel
    {
        private TileQuad m_Quad { get; set; }
        private byte m_Height => m_PoolID.height;
        private Qube<PolyID> m_QubeCorners;
        private Qube<bool> m_CornerRelation;
        private CubeFacing<PolyID> m_CubeSides;
        private CubeFacing<bool> m_SideRelation;

        public TileVoxel Init(TileQuad _srcQuad)
        {
            m_Quad = _srcQuad;
            transform.SetPositionAndRotation(m_Quad.transform.position+DPolyGrid.GetVoxelHeight(m_PoolID),m_Quad.transform.rotation);
            m_QubeCorners = new Qube<PolyID>();
            for (int i = 0; i < 8; i++)
            {
                HexCoord corner = m_Quad.m_NearbyVertsCW[i % 4];
                byte height = i >= 4 ? m_Height : UByte.BackOne(m_Height);
                m_QubeCorners[i]= new PolyID(corner,height);
            }
            return this;
        }

        public override void OnPoolRecycle()
        {
            base.OnPoolRecycle();
            m_Quad = null;
        }

        PolyID GetFacingID(int _index)
        {
            var _facing = UCubeFacing.IndexToFacing(_index);
            switch (_facing)
            {
                default: throw new Exception("Invalid Facing:" + _facing);
                case ECubeFacing.BL: return new PolyID(m_Quad.m_NearbyQuadsCW[0], m_Height);
                case ECubeFacing.LF: return new PolyID(m_Quad.m_NearbyQuadsCW[1], m_Height);
                case ECubeFacing.FR: return new PolyID(m_Quad.m_NearbyQuadsCW[2], m_Height);
                case ECubeFacing.RB: return new PolyID(m_Quad.m_NearbyQuadsCW[3], m_Height);
                case ECubeFacing.T : return new PolyID(m_Quad.m_Quad.m_Identity,UByte.ForwardOne( m_Height));
                case ECubeFacing.D: return new PolyID(m_Quad.m_Quad.m_Identity, UByte.BackOne(m_Height));
            }
        }
        
        public void RefreshRelations(PilePool<TileCorner> _corners,PilePool<TileVoxel> _voxels)
        {
            Quad<bool> relationBottom = new Quad<bool>(false,false,false,false);
            if (m_Height != 0)
                relationBottom =new Quad<bool>(_corners.Contains(m_QubeCorners[0]),_corners.Contains(m_QubeCorners[1]),
                    _corners.Contains(m_QubeCorners[2]),_corners.Contains(m_QubeCorners[3]));
            Quad<bool> relationTop = new Quad<bool>(_corners.Contains(m_QubeCorners[4]),_corners.Contains(m_QubeCorners[5]),
                _corners.Contains(m_QubeCorners[6]),_corners.Contains(m_QubeCorners[7]));
            m_CornerRelation = new Qube<bool>(relationBottom, relationTop);

            m_CubeSides = new CubeFacing<PolyID>(new PolyID(m_Quad.m_NearbyQuadsCW[0], m_Height),new PolyID(m_Quad.m_NearbyQuadsCW[1], m_Height),new PolyID(m_Quad.m_NearbyQuadsCW[2], m_Height),
                new PolyID(m_Quad.m_NearbyQuadsCW[3], m_Height), new PolyID(m_Quad.m_Quad.m_Identity,UByte.ForwardOne( m_Height)), new PolyID(m_Quad.m_Quad.m_Identity, UByte.BackOne(m_Height))) ;
            m_SideRelation = CubeFacing<bool>.Create(m_CubeSides, _voxels.Contains);
        }
        public PolyID Identity => m_PoolID;
        public PolyQuad Quad => m_Quad.m_Quad;
        public Qube<PolyID> QubeCorners => m_QubeCorners;
        public Qube<bool> CornerRelations => m_CornerRelation;
        public CubeFacing<PolyID> CubeSides => m_CubeSides;
        public CubeFacing<bool> SideRelations => m_SideRelation;
        public Transform Transform => transform;
        public Quad<Vector2>[] CornerShapeLS =>  m_Quad.m_SplitQuadLS;
    }
}