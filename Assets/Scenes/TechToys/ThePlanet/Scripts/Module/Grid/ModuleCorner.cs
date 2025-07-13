using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using TPool;
using UnityEngine;

namespace TechToys.ThePlanet.Module
{
    [RequireComponent(typeof(MeshCollider))]
    public class ModuleCorner : APoolBehaviour<PCGID>,ICorner
    {
        public PCGID Identity => identity;
        private byte m_Height => identity.height;
        public IVertex m_Vertex  { get; private set; }
        public int m_Type  { get; private set; }
        public List<PCGID> m_AdjacentConnectedCorners { get; } = new List<PCGID>();
        public List<PCGID> m_IntervalConnectedCorners { get; } = new List<PCGID>();
        public List<PCGID> m_RelativeVoxels { get; } = new List<PCGID>();
        public List<PCGID> m_ClusterRelativeVoxels { get; } = new List<PCGID>();

        
        // public bool m_UpperCornerRelation { get; private set; }
        // public bool m_LowerCornerRelation  { get; private set; }
        private Mesh m_ColliderMesh;
        private MeshCollider m_Collider;
        public override void OnPoolCreate()
        {
            base.OnPoolCreate();
            m_ColliderMesh = new Mesh();
            m_ColliderMesh.MarkDynamic();
            m_Collider = GetComponent<MeshCollider>();
        }

        public ModuleCorner Init(IVertex _vertex,int _type,Action<Mesh,PCGVertex,PCGID> _constructCollider)
        {
            m_Vertex = _vertex;
            m_Type = _type;
            transform.SetPositionAndRotation( _vertex.Vertex.GetCornerPosition(identity.height),_vertex.transform.rotation);
            _constructCollider(m_ColliderMesh,_vertex.Vertex, Identity);
            m_Collider.sharedMesh = m_ColliderMesh;
            return this;
        }

        public override void OnPoolRecycle()
        {
            base.OnPoolRecycle();
            m_Type = -1;
            m_Vertex = null;
        }

        public void RefreshRelations(PilePool<ModuleCorner> _corners,PilePool<ModuleVoxel> _voxels)
        {
            var height = m_Height;
            // m_UpperCornerRelation = identity.TryUpward(out var topID) && _corners.Contains(topID) && DModule.IsCornerAdjacent(this.m_Type, _corners[topID].m_Type);
            // m_LowerCornerRelation = !identity.TryDownward(out var bottomID) || (_corners.Contains(bottomID) && DModule.IsCornerAdjacent(this.m_Type, _corners[bottomID].m_Type));
            
            m_Vertex.Vertex.IterateAdjacentCorners(height).Collect(p=>_corners.Contains(p)&&DModule.IsCornerAdjacent(m_Type,_corners[p].m_Type)).FillList(m_AdjacentConnectedCorners);
            m_Vertex.Vertex.IterateIntervalCorners(height).Collect(p=>_corners.Contains(p)&&DModule.IsCornerAdjacent(m_Type,_corners[p].m_Type)).FillList(m_IntervalConnectedCorners);
            m_Vertex.Vertex.IterateRelativeVoxels(height).Collect(_voxels.Contains).FillList(m_RelativeVoxels);
            m_AdjacentConnectedCorners.Extend(Identity).Select(p=>_corners[p].m_Vertex.Vertex.IterateRelativeVoxels(height)).Resolve().Collect(_voxels.Contains).FillList(m_ClusterRelativeVoxels,true);
        }

    }
}