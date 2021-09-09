using System;
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
        public QubeRelations m_CornerRelations { get; private set; }
        public MeshFilter m_MeshFilter { get; private set; }
        public Mesh m_Mesh;
        public override void OnPoolInit(Action<PileID> _DoRecycle)
        {
            base.OnPoolInit(_DoRecycle);
            m_Mesh = new Mesh() {hideFlags = HideFlags.HideAndDontSave};
            m_Mesh.MarkDynamic();
            GetComponent<MeshFilter>().sharedMesh = m_Mesh;
        }

        public TileVoxel Init(TileQuad _srcQuad)
        {
            m_Quad = _srcQuad;
            m_Mesh.name = $"Voxel:{_srcQuad.m_Quad.m_Identity}";
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
        public void RefreshRelations(PilePool<TileCorner> _corners)
        {
            QuadRelations relationTop = new QuadRelations(_corners.Contains(GetCornerID(4)),_corners.Contains(GetCornerID(5)),
                _corners.Contains(GetCornerID(6)),_corners.Contains(GetCornerID(7)));
            QuadRelations relationBottom = new QuadRelations(false,false,false,false);
            if (m_Height != 0)
                relationBottom =new QuadRelations(_corners.Contains(GetCornerID(0)),_corners.Contains(GetCornerID(1)),
                    _corners.Contains(GetCornerID(2)),_corners.Contains(GetCornerID(3)));
            m_CornerRelations = new QubeRelations(relationBottom, relationTop);

            var vertices = TSPoolList<Vector3>.Spawn();
            var indexes = TSPoolList<int>.Spawn();
            var uvs = TSPoolList<Vector2>.Spawn();
            var normals = TSPoolList<Vector3>.Spawn();
            for (int i = 0; i < 8; i++)
            {
                if(!m_CornerRelations[i])
                    continue;
                var corner = _corners.Get(GetCornerID(i));
                
            }
            
            TSPoolList<Vector3>.Recycle(vertices);
            TSPoolList<int>.Recycle(indexes);
            TSPoolList<Vector2>.Recycle(uvs);
            TSPoolList<Vector3>.Recycle(normals);
        }
    }
}