using System;
using System.Linq;
using System.Runtime.InteropServices;
using Geometry;
using Geometry.Extend;
using Geometry.Pixel;
using Geometry.Voxel;
using LinqExtentions;
using Procedural;
using TPool;
using Procedural.Hexagon;
using TPoolStatic;
using UnityEngine;

namespace ConvexGrid
{
    public class TileVoxel : PoolBehaviour<PileID>,IVoxel
    {
        private TileQuad m_Quad { get; set; }
        private byte m_Height => m_PoolID.height;
        private PileQube m_QubeCorners;
        private BoolQube m_CornerRelation;
        private BCubeFacing m_SideRelation;

        public TileVoxel Init(TileQuad _srcQuad)
        {
            m_Quad = _srcQuad;
            transform.SetParent(m_Quad.transform);
            transform.localPosition = UTile.GetVoxelHeight(m_PoolID);
            transform.localRotation = Quaternion.identity;
            m_QubeCorners = new PileQube();
            for (int i = 0; i < 8; i++)
            {
                HexCoord corner = m_Quad.m_NearbyVertsCW[i % 4];
                byte height = i >= 4 ? m_Height : UByte.BackOne(m_Height);
                m_QubeCorners.SetCorner(i, new PileID(corner,height));
            }
            return this;
        }

        public override void OnPoolRecycle()
        {
            base.OnPoolRecycle();
            m_Quad = null;
        }

        public PileID GetFacingID(int _index)
        {
            var _facing = UQube.IndexToFacing(_index);
            switch (_facing)
            {
                default: throw new Exception("Invalid Facing:" + _facing);
                case ECubeFacing.BL: return new PileID(m_Quad.m_NearbyQuadsCW[0], m_Height);
                case ECubeFacing.LF: return new PileID(m_Quad.m_NearbyQuadsCW[1], m_Height);
                case ECubeFacing.FR: return new PileID(m_Quad.m_NearbyQuadsCW[2], m_Height);
                case ECubeFacing.RB: return new PileID(m_Quad.m_NearbyQuadsCW[3], m_Height);
                case ECubeFacing.T : return new PileID(m_Quad.m_Quad.m_Identity,UByte.ForwardOne( m_Height));
                case ECubeFacing.D: return new PileID(m_Quad.m_Quad.m_Identity, UByte.BackOne(m_Height));
            }
        }
        
        public void RefreshRelations(PilePool<TileCorner> _corners,PilePool<TileVoxel> _voxels)
        {
            BoolQuad relationBottom = new BoolQuad(false,false,false,false);
            if (m_Height != 0)
                relationBottom =new BoolQuad(_corners.Contains(m_QubeCorners[0]),_corners.Contains(m_QubeCorners[1]),
                    _corners.Contains(m_QubeCorners[2]),_corners.Contains(m_QubeCorners[3]));
            BoolQuad relationTop = new BoolQuad(_corners.Contains(m_QubeCorners[4]),_corners.Contains(m_QubeCorners[5]),
                _corners.Contains(m_QubeCorners[6]),_corners.Contains(m_QubeCorners[7]));
            m_CornerRelation = UQube.CombineToQube<BoolQube,bool>(relationBottom, relationTop);
            
            m_SideRelation = new BCubeFacing(_voxels.Contains(GetFacingID(0)),_voxels.Contains(GetFacingID(1)),_voxels.Contains(GetFacingID(2)),
                _voxels.Contains(GetFacingID(3)),_voxels.Contains(GetFacingID(4)),_voxels.Contains(GetFacingID(5)));
        }

        
        public PileID Identity => m_PoolID;
        public PileQube QubeCorners => m_QubeCorners;
        public BoolQube CornerRelations => m_CornerRelation;
        public BCubeFacing SideRelations => m_SideRelation;
        public Transform Transform => transform;
        public G2Quad[] CornerShapeLS =>  m_Quad.m_SplitQuadLS;
    }
}