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
    public class TileVoxel : PoolBehaviour<PileID>,IModuleCollector
    {
        public TileQuad m_Quad { get; private set; }
        public byte m_Height => m_PoolID.height;
        public BoolQube m_CornerRelation { get;  set; }
        public BCubeFacing m_SideRelation { get; set; }

        public TileVoxel Init(TileQuad _srcQuad)
        {
            m_Quad = _srcQuad;
            transform.SetParent(m_Quad.transform);
            transform.localPosition = ConvexGridHelper.GetVoxelHeight(m_PoolID);
            transform.localRotation = Quaternion.identity;
            return this;
        }

        public override void OnPoolRecycle()
        {
            base.OnPoolRecycle();
            m_Quad = null;
        }

        public PileID GetCornerID(int _index)
        {
            HexCoord corner = m_Quad.m_NearbyVertsCW[_index % 4];
            byte height = _index >= 4 ? m_Height : UByte.BackOne(m_Height);
            return new PileID(corner,height);
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
                relationBottom =new BoolQuad(_corners.Contains(GetCornerID(0)),_corners.Contains(GetCornerID(1)),
                    _corners.Contains(GetCornerID(2)),_corners.Contains(GetCornerID(3)));
            BoolQuad relationTop = new BoolQuad(_corners.Contains(GetCornerID(4)),_corners.Contains(GetCornerID(5)),
                _corners.Contains(GetCornerID(6)),_corners.Contains(GetCornerID(7)));
            m_CornerRelation = UQube.CombineToQube<BoolQube,bool>(relationBottom, relationTop);
            
            m_SideRelation = new BCubeFacing(_voxels.Contains(GetFacingID(0)),_voxels.Contains(GetFacingID(1)),_voxels.Contains(GetFacingID(2)),
                _voxels.Contains(GetFacingID(3)),_voxels.Contains(GetFacingID(4)),_voxels.Contains(GetFacingID(5)));
        }

        
        public PileID m_Identity => m_PoolID;
        public Transform m_ModuleTransform => transform;
        public byte m_ModuleByte => m_CornerRelation.ToByte();
        public G2Quad[] m_ModuleShapeLS =>  m_Quad.m_SplitQuadLS;
    }
}