using System;
using System.Collections;
using System.Collections.Generic;
using LinqExtension;
using TPool;
using Procedural.Hexagon;
using UnityEngine;

namespace PolyGrid.Tile
{
    public class TileCorner : PoolBehaviour<PileID>,IGridRaycast,ICorner
    {
        public byte m_Height => m_PoolID.height;
        public HexCoord m_VertID => m_BaseVertex.m_Vertex.m_Identity;

        public TileVertex m_BaseVertex { get; private set; }
        public MeshCollider m_Collider { get; private set; }
        private readonly List<PileID> m_NearbyValidCorners = new List<PileID>();
        private readonly List<PileID> m_NearbyValidVoxels = new List<PileID>();

        public override void OnPoolInit(Action<PileID> _DoRecycle)
        {
            base.OnPoolInit(_DoRecycle);
            m_Collider = GetComponent<MeshCollider>();
        }
        
        public TileCorner Init(TileVertex _vertex)
        {
            m_BaseVertex = _vertex;
            transform.SetParent(m_BaseVertex.transform);
            transform.localPosition = DPolyGrid.GetCornerHeight(m_PoolID);
            transform.localRotation = Quaternion.identity;
            m_Collider.sharedMesh = _vertex.m_CornerMesh;
            return this;
        }

        public override void OnPoolRecycle()
        {
            base.OnPoolRecycle();
            m_Collider.sharedMesh = null;
        }

        public void RefreshRelations(PilePool<TileCorner> _corners,PilePool<TileVoxel> _voxels)
        {
            m_NearbyValidCorners.Clear();
            m_NearbyValidVoxels.Clear();
            
            void AddCorner(PilePool<TileCorner> _corners,PileID _cornerID)
            {
                if (!_corners.Contains(_cornerID))
                    return;
                m_NearbyValidCorners.Add(_cornerID);
            }

            void AddQuad(PilePool<TileVoxel> _voxels, PileID _voxelID)
            {
                if (!_voxels.Contains(_voxelID))
                    return;
                m_NearbyValidVoxels.Add(_voxelID);
            }

            var height = m_Height;
            foreach (var corner in m_BaseVertex.m_Vertex.AllNearbyCorner(height))
                AddCorner(_corners,corner);

            foreach (var quad in m_BaseVertex.m_Vertex.AllNearbyVoxels(height))
                AddQuad(_voxels,quad);
        }
        public (HexCoord, byte) GetCornerData() => (m_VertID,  m_Height);
        public (HexCoord, byte) GetNearbyCornerData(ref RaycastHit _hit)
        {
            if (Vector3.Dot(_hit.normal, Vector3.up) > .95f)
                return (m_VertID, UByte.ForwardOne(m_Height));

            if (Vector3.Dot(_hit.normal, Vector3.down) > .95f)
                return (m_VertID, UByte.BackOne(m_Height));
            
            var localPoint = transform.InverseTransformPoint(_hit.point);
            float minSqrDistance = float.MaxValue;
            (Vector3 position, HexCoord vertex) destCorner = default;
            foreach (var tuple in m_BaseVertex.m_RelativeCornerDirections)
            {
                var sqrDistance = (localPoint - tuple.position).sqrMagnitude;
                if (minSqrDistance < sqrDistance)
                    continue;
                minSqrDistance = sqrDistance;
                destCorner = tuple;
            }
            
            return (destCorner.vertex,m_Height);
        }

        public Transform Transform => transform;
        public PileID Identity => m_PoolID;
        public List<PileID> NearbyCorners => m_NearbyValidCorners;
        public List<PileID> NearbyVoxels => m_NearbyValidVoxels;
    }
}