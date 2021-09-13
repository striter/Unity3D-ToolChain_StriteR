using System;
using System.Linq;
using System.Runtime.InteropServices;
using Geometry;
using Geometry.Voxel;
using LinqExtentions;
using TPool;
using Procedural.Hexagon;
using TPoolStatic;
using UnityEngine;

namespace ConvexGrid
{
    public class TileVoxel : PoolBehaviour<PileID>
    {
        public TileQuad m_Quad { get; private set; }
        public byte m_Height => m_PoolID.height;
        public VoxelCornerRelation m_CornerRelations { get; private set; }
        public VoxelSideRelation m_SideRelations { get; private set; }
        public MeshFilter m_MeshFilter { get; private set; }
        public Mesh m_VoxelMesh { get; private set; }
        public override void OnPoolInit(Action<PileID> _DoRecycle)
        {
            base.OnPoolInit(_DoRecycle);
            m_VoxelMesh = new Mesh() {hideFlags = HideFlags.HideAndDontSave};
            m_VoxelMesh.MarkDynamic();
            GetComponent<MeshFilter>().sharedMesh = m_VoxelMesh;
        }

        public TileVoxel Init(TileQuad _srcQuad)
        {
            m_Quad = _srcQuad;
            m_VoxelMesh.name = $"Voxel:{_srcQuad.m_Quad.m_Identity}";
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
            QuadRelations relationBottom = new QuadRelations(false,false,false,false);
            if (m_Height != 0)
                relationBottom =new QuadRelations(_corners.Contains(GetCornerID(0)),_corners.Contains(GetCornerID(1)),
                    _corners.Contains(GetCornerID(2)),_corners.Contains(GetCornerID(3)));
            QuadRelations relationTop = new QuadRelations(_corners.Contains(GetCornerID(4)),_corners.Contains(GetCornerID(5)),
                _corners.Contains(GetCornerID(6)),_corners.Contains(GetCornerID(7)));
            m_CornerRelations = new VoxelCornerRelation(relationBottom, relationTop);

            m_SideRelations = new VoxelSideRelation(_voxels.Contains(GetFacingID(0)),_voxels.Contains(GetFacingID(1)),_voxels.Contains(GetFacingID(2)),
                _voxels.Contains(GetFacingID(3)),_voxels.Contains(GetFacingID(4)),_voxels.Contains(GetFacingID(5)));
            
            RefreshCubes();
        }

        void RefreshCubes()
        {
            var vertices = TSPoolList<Vector3>.Spawn();
            var indexes = TSPoolList<int>.Spawn();
            var uvs = TSPoolList<Vector2>.Spawn();
            var normals = TSPoolList<Vector3>.Spawn();
            var qubes = TSPoolList<GQube>.Spawn(8);
    
            qubes.AddRange(m_Quad.m_OrientedShapeOS.SplitToQubes(ConvexGridHelper.m_TileHeightHalf));

            for (int i = 0; i < 8; i++)
            {
                if(!m_CornerRelations[i])
                    continue;
                
                qubes[i].FillFacingQuad(ECubeFacing.T,vertices,indexes,uvs,normals);
                qubes[i].FillFacingQuad(ECubeFacing.D,vertices,indexes,uvs,normals);
                qubes[i].FillFacingQuad(ECubeFacing.BL,vertices,indexes,uvs,normals);
                qubes[i].FillFacingQuad(ECubeFacing.LF,vertices,indexes,uvs,normals);
                qubes[i].FillFacingQuad(ECubeFacing.FR,vertices,indexes,uvs,normals);
                qubes[i].FillFacingQuad(ECubeFacing.RB,vertices,indexes,uvs,normals);
            }
            TSPoolList<GQube>.Recycle(qubes);

            m_VoxelMesh.Clear();
            m_VoxelMesh.SetVertices(vertices);
            m_VoxelMesh.SetIndices(indexes,MeshTopology.Quads,0,false);
            m_VoxelMesh.SetUVs(0,uvs);
            m_VoxelMesh.SetNormals(normals);
            m_VoxelMesh.RecalculateBounds();
            m_VoxelMesh.MarkModified();
            
            TSPoolList<Vector3>.Recycle(vertices);
            TSPoolList<int>.Recycle(indexes);
            TSPoolList<Vector2>.Recycle(uvs);
            TSPoolList<Vector3>.Recycle(normals);
        }
    }
}