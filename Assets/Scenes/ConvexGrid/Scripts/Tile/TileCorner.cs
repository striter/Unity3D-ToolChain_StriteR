using System;
using System.Collections;
using System.Collections.Generic;
using LinqExtentions;
using TPool;
using Procedural.Hexagon;
using UnityEngine;

namespace ConvexGrid
{
    public class TileCorner : PoolBehaviour<PileID>,IGridRaycast,ICorner,IPile
    {
        public byte m_Height => m_PoolID.height;
        public HexCoord m_VertID => m_BaseVertex.m_Vertex.m_Hex;
        public TileVertex m_BaseVertex { get; private set; }
        public MeshCollider m_Collider { get; private set; }

        public override void OnPoolInit(Action<PileID> _DoRecycle)
        {
            base.OnPoolInit(_DoRecycle);
            m_Collider = GetComponent<MeshCollider>();
        }
        
        public TileCorner Init(TileVertex _vertex)
        {
            m_BaseVertex = _vertex;
            transform.SetParent(m_BaseVertex.transform);
            transform.localPosition = UTile.GetCornerHeight(m_PoolID);
            transform.localRotation = Quaternion.identity;
            m_Collider.sharedMesh = _vertex.m_CornerMesh;
            return this;
        }

        public override void OnPoolRecycle()
        {
            base.OnPoolRecycle();
            m_Collider.sharedMesh = null;
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
        public EPileStatus Status { get; set; }
    }
}