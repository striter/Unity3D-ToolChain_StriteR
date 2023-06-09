using System;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using TPool;
using TPoolStatic;
using Unity.Mathematics;
using UnityEngine;

namespace TechToys.ThePlanet.Module
{
    public class ModuleGridManager : MonoBehaviour,IModuleControl
    {
        public GridManager m_Grid { get; set; }
        
        private readonly Stack<ModuleCollapsePropagandaChain> m_PropagandaChains = new Stack<ModuleCollapsePropagandaChain>(12);
        //Local actors
        private ObjectPoolMono<GridID, ModuleVertex> m_GridVertices;
        private ObjectPoolMono<GridID,ModuleQuad> m_GridQuads;
        private PilePool<ModuleCorner> m_Corners;
        private PilePool<ModuleVoxel> m_Voxels;

        public void Init()
        {
            m_GridVertices = new ObjectPoolMono<GridID, ModuleVertex>(transform.Find("Vertex/Item"));
            m_GridQuads = new ObjectPoolMono<GridID,ModuleQuad>(transform.Find("Quad/Item"));
            m_Corners = new PilePool<ModuleCorner>(transform.Find("Corner/Item"));
            m_Voxels = new PilePool<ModuleVoxel>(transform.Find("Voxel/Item"));
        }

        public void Setup()
        {
        }

        public void Dispose()
        {
            TSPool<ModuleCollapsePropagandaChain>.Clear();
        }

        public void Tick(float _deltaTime)
        {
        }

        public void Clear()
        {
            m_GridQuads.Clear();
            m_GridVertices.Clear();
            m_Corners.Clear();
            m_Voxels.Clear();
            m_PropagandaChains.Clear();
            TSPool<ModuleCollapsePropagandaChain>.Clear();
        }
        
    #region LifeCycle

    public IEnumerable<ModuleInput> CollectPersistentData()
    {
        foreach (var corner in m_Corners)
            yield return new ModuleInput(){origin = corner.Identity,type = corner.m_Type};
    }
    
        public void CornerConstruction(PCGID _cornerID,int _cornerType, PCGVertex _vertex,Action<IVertex> _vertexSpawn,Action<IQuad> _quadSpawn, Action<ICorner> _cornerSpawn,Action<IVoxel> _moduleSpawn)
        {
            if (m_Corners.Contains(_cornerID))
                return;
            
            FillVertex(_vertex,_vertexSpawn);
            FillQuads(_vertex,_quadSpawn);
            
            FillCorner(_cornerID,_cornerType,_cornerSpawn);
            FillVoxels(_vertex,_moduleSpawn);
            
            RefreshCornerRelations(_vertex,_cornerID);
            RefreshVoxelRelations(_vertex);
        }
        
        public void CornerDeconstruction(PCGID _cornerID,PCGVertex _vertex,Action<GridID> _vertexRecycle,Action<GridID> _quadRecycle,Action<PCGID> _cornerRecycle,Action<PCGID> _moduleRecycle)
        {
            if (!m_Corners.Contains(_cornerID))
                return;
            
            RemoveCorner(_cornerID,_cornerRecycle);
            RemoveVoxels(_vertex,_moduleRecycle);
            
            RemoveVertex(_vertex,_vertexRecycle);
            RemoveQuads(_vertex,_quadRecycle);
            
            RefreshCornerRelations(_vertex,_cornerID);
            RefreshVoxelRelations(_vertex);
        }

        byte GetMaxCornerHeight(GridID _quadID)
        {
            var maxHeight = byte.MinValue;
            for (int i = 0; i < 4; i++)
            {
                var vertexID = m_GridQuads[_quadID].Quad.m_Indexes[i];
                if (m_Corners.Contains(vertexID))
                    maxHeight = Math.Max(maxHeight, UByte.ForwardOne( m_Corners.Max(vertexID)));
            }
            return maxHeight;
        }
        
        void FillVertex(PCGVertex _vertex,Action<IVertex> _vertexSpawn)
        {
            var vertexID = _vertex.m_Identity;
            if (m_GridVertices.Contains(vertexID))
                return;
            var vertex=m_GridVertices.Spawn(vertexID).Init(_vertex);
            _vertexSpawn(vertex);
        }

        void FillQuads(PCGVertex _vertex,Action<IQuad> _quadSpawn)
        {
            foreach (var nearbyQuad in _vertex.m_NearbyQuads)
            {
                var quadID = nearbyQuad.m_Identity;
                if (m_GridQuads.Contains(quadID))
                    continue;
                var quad=m_GridQuads.Spawn(quadID).Init(nearbyQuad);
                _quadSpawn(quad);
            }
        }

        void FillCorner(PCGID _cornerID,int _cornerType,Action<ICorner> _cornerSpawn)
        {
            if (m_Corners.Contains(_cornerID))
                return;
            var vertex = m_GridVertices[_cornerID.location];
            var corner = m_Corners.Spawn(_cornerID).Init(vertex,_cornerType,ConstructCornerCollider);
            _cornerSpawn(corner);
        }

        void FillVoxels(PCGVertex _vertex,Action<IVoxel> _voxelSpawn)
        {
            foreach (var quadID in _vertex.m_NearbyQuads.Select(p=>p.m_Identity))
            {
                var maxHeight = GetMaxCornerHeight(quadID);
                for (byte i = 0; i <= maxHeight; i++)
                {
                    var voxelID = new PCGID(quadID, i);
                    if(m_Voxels.Contains(voxelID))
                        continue;
                    var voxel = m_Voxels.Spawn(voxelID).Init(m_GridQuads[quadID]);
                    _voxelSpawn(voxel);
                }
            }
        }
        
        void RemoveVertex(PCGVertex _vertex,Action<GridID> _vertexRecycle)
        {
            var vertexID = _vertex.m_Identity;
            if (m_Corners.Contains(vertexID)||!m_GridVertices.Contains(vertexID))
                return;
            m_GridVertices.Recycle(vertexID);
            _vertexRecycle(vertexID);
        }

        void RemoveQuads(PCGVertex _vertex,Action<GridID> _quadRecycle)
        {
            foreach (var quadID in _vertex.m_NearbyQuads.Select(p => p.m_Identity))
            {
                if (m_Voxels.Contains(quadID)||!m_GridQuads.Contains(quadID))
                    continue;
                m_GridQuads.Recycle(quadID);
                _quadRecycle(quadID);
            }
        }
        
        void RemoveCorner(PCGID _cornerID,Action<PCGID> _cornerRecycle)
        {
            if (!m_Corners.Contains(_cornerID))
                return;
            m_Corners.Recycle(_cornerID);
            _cornerRecycle(_cornerID);
        }

        void RemoveVoxels(PCGVertex _vertex,Action<PCGID> _voxelRecycle)
        {
            foreach (var quadID in _vertex.m_NearbyQuads.Select(_p => _p.m_Identity))
            {
                var maxHeight = GetMaxCornerHeight(quadID);
                maxHeight = maxHeight == byte.MinValue ? byte.MinValue : UByte.ForwardOne(maxHeight);
                var srcHeight = m_Voxels.Max(quadID);
                for (var i = maxHeight; i <= srcHeight; i++)
                {
                    var voxelID = new PCGID(quadID, i);
                    m_Voxels.Recycle(voxelID);
                    _voxelRecycle(voxelID);
                }
            }
        }
        
        void RefreshCornerRelations(PCGVertex _vertex,PCGID _cornerID)
        {
            TSPoolList<PCGID>.Spawn(out var corners);

            var height = _cornerID.height;
            corners.AddRange(_vertex.IterateAdjacentCorners(height));
            corners.AddRange(_vertex.IterateIntervalCorners(height));
            corners.Add(_cornerID);
            for (int i = 0; i < corners.Count; i++)
            {
                var cornerID = corners[i];
                if(!m_Corners.Contains(cornerID))
                    continue;
                m_Corners[cornerID].RefreshRelations(m_Corners,m_Voxels);
            }
            TSPoolList<PCGID>.Recycle(corners);
        }
        
        void RefreshVoxelRelations(PCGVertex _vertex)
        {
            TSPoolHashset<GridID>.Spawn(out var quadRefreshing);
            TSPoolHashset<GridID>.Spawn(out var vertexRefreshing);
            
            foreach (var nearbyQuad in _vertex.m_NearbyQuads)
                foreach (var intervalVertex in nearbyQuad.m_Vertices)
                {
                    if(vertexRefreshing.Contains(intervalVertex.m_Identity))
                        continue;
                    vertexRefreshing.Add(intervalVertex.m_Identity);
                    foreach (var intervalQuad in intervalVertex.m_NearbyQuads)
                        quadRefreshing.TryAdd(intervalQuad.m_Identity);
                }

            foreach (var quadID in quadRefreshing)
            {
                if (!m_Voxels.Contains(quadID))
                   continue;
                var maxHeight = GetMaxCornerHeight(quadID);
                for (byte i = 0; i <= maxHeight; i++)
                    m_Voxels[new PCGID(quadID, i)].RefreshRelations(m_Corners,m_Voxels);
            }
            
            TSPoolHashset<GridID>.Recycle(quadRefreshing);
            TSPoolHashset<GridID>.Recycle(vertexRefreshing);
        }

    #endregion
        
    #region Helper
        public Stack<ModuleCollapsePropagandaChain> CollectPropagandaRelations(IList<PCGID> _cornerIDs)
        {
            TSPoolHashset<PCGID>.Spawn(out var propagationCorners);
            TSPoolHashset<PCGID>.Spawn(out var deconstructionCorners);
            propagationCorners.Clear();

            void AppendPropagationCorners(PCGID _nearbyCornerID)
            {
                if(propagationCorners.Contains(_nearbyCornerID)) return;
                if (!m_Corners.Contains(_nearbyCornerID)) return;
                propagationCorners.Add(_nearbyCornerID);
            }

            var cornerCount = _cornerIDs.Count;
            for(int i=0;i<cornerCount;i++)
            {
                var cornerID = _cornerIDs[i];
                //Collect deconstruction corners
                if (!m_Corners.Contains(cornerID))
                    deconstructionCorners.TryAdd(cornerID);
                
                //Collect propagation corners (3x3 range <square grid>)
                AppendPropagationCorners(cornerID);
                var vertex = m_Grid.m_Vertices[cornerID.location];
                foreach (var nearbyCorner in vertex.IterateAdjacentCorners(cornerID.height))
                    AppendPropagationCorners(nearbyCorner);
                foreach (var adjacentCorner in vertex.IterateIntervalCorners(cornerID.height))
                    AppendPropagationCorners(adjacentCorner);
            }

            m_PropagandaChains.Clear();
            TSPool<ModuleCollapsePropagandaChain>.Clear();
            
            //Pass Deconstruction Corners First
            if (deconstructionCorners.Count > 0)
            {
                var deconstructChain = TSPool<ModuleCollapsePropagandaChain>.Spawn();
                deconstructChain.chainType = -1;        //Mark As Deconstruction Chains
                foreach (var cornerID in deconstructionCorners)
                    deconstructChain.voxels.TryAddRange(m_Grid.m_Vertices[cornerID.location].IterateRelativeVoxels(cornerID.height).Collect(m_Voxels.Contains));
                
                m_PropagandaChains.Push(deconstructChain);
            }

            //Collect Propagation Corners By Stack Iteration
            TSPoolStack<PCGID>.Spawn(out var modifyPropagations);
            TSPoolStack<PCGID>.Spawn(out var propagandaStack);
            TSPoolHashset<PCGID>.Spawn(out var validateCorners);
            modifyPropagations.PushRange(propagationCorners.Collect(_p=>m_Corners.Contains(_p)));
            while (modifyPropagations.Count > 0)
            {
                var chainCornerBegin = modifyPropagations.Pop();
                if (validateCorners.Contains(chainCornerBegin))
                    continue;
                
                propagandaStack.Clear();
                var modifyChain = TSPool<ModuleCollapsePropagandaChain>.Spawn();
                void TryPushIntoChain(PCGID corner)
                {
                    if(modifyChain.corners.Contains(corner))
                        return;
                        
                    modifyChain.corners.Add(corner);
                    validateCorners.Add(corner);
                    propagandaStack.Push(corner);
                }
                
                modifyChain.chainType = m_Corners[chainCornerBegin].m_Type;
                TryPushIntoChain(chainCornerBegin);
                while (propagandaStack.Count>0)
                {
                    var propagandaCornerID = propagandaStack.Pop();
                    var propagandaCorner = m_Corners[propagandaCornerID];
                    for (int i = 0; i < propagandaCorner.m_AdjacentConnectedCorners.Count; i++)
                        TryPushIntoChain(propagandaCorner.m_AdjacentConnectedCorners[i]);
                    for (int i = 0; i < propagandaCorner.m_IntervalConnectedCorners.Count; i++)
                        TryPushIntoChain(propagandaCorner.m_IntervalConnectedCorners[i]);
                }
                CollectAffectedVoxels(modifyChain.voxels,modifyChain.corners);

                m_PropagandaChains.Push(modifyChain);
            }
            TSPoolStack<PCGID>.Recycle(propagandaStack);
            TSPoolStack<PCGID>.Recycle(modifyPropagations);
            TSPoolHashset<PCGID>.Recycle(validateCorners);

            TSPoolHashset<PCGID>.Recycle(propagationCorners);
            TSPoolHashset<PCGID>.Recycle(deconstructionCorners);

            return m_PropagandaChains;
        }

        public void CollectAffectedVoxel(HashSet<PCGID> _voxelSet, PCGID _corner)
        {
            foreach (var voxelID in m_Corners[_corner].m_Vertex.Vertex.IterateRelativeVoxels(_corner.height))
                _voxelSet.Add(voxelID);
        }
        
        public void CollectAffectedVoxels(HashSet<PCGID> _voxelSet, IEnumerable<PCGID> _corners)
        {
            foreach (var corner in _corners)
            {
                foreach (var voxelID in m_Corners[corner].m_Vertex.Vertex.IterateRelativeVoxels(corner.height))
                {
                    if (_voxelSet.Contains(voxelID))
                        continue;
                    
                    if(!m_Voxels.Contains(voxelID))
                        continue;
                    
                    _voxelSet.Add(voxelID);
                }
            }
        }
        
        private const int kLayer = int.MaxValue;
        public bool ConstructRaycast(Ray _ray, out PCGID _selectionID)
        {
            _selectionID = default;
            if (Physics.Raycast(_ray, out var hit, float.MaxValue, kLayer))
            {
                var corner = hit.collider.GetComponent<ModuleCorner>();
                var vertical = Vector3.Dot(hit.normal, corner.m_Vertex.Vertex.m_Normal);
                if (vertical > .95f && corner.Identity.TryUpward(out _selectionID))
                    return true;
                if (vertical < -.95f && corner.Identity.TryDownward(out _selectionID))
                    return true;

                if (!m_Grid.ValidateSideVertex(corner.Identity.location, hit.point, out var sideLocation))
                    return false;

                _selectionID = new PCGID(sideLocation, corner.Identity.height);
                return true;
            }

            if (m_Grid.ValidateGridSelection(_ray,out var groundCorner))
            {
                _selectionID = new PCGID(groundCorner, 0);
                return true;
            }

            return false;
        }
        public bool DeconstructRaycast(Ray _ray, out PCGID _selectionID)
        {
            _selectionID = default;
            if (!Physics.Raycast(_ray, out var _hit, float.MaxValue))
                return false;

            if (!_hit.collider)
                return false;
            var corner = _hit.collider.GetComponent<ModuleCorner>();
            _selectionID = corner.Identity;
            return true;
        }
        
        void ConstructCornerCollider(Mesh _mesh,PCGVertex _vertex,PCGID _corner)
        {
            TSPoolList<Vector3>.Spawn(out var vertices);
            TSPoolList<int>.Spawn(out var indices);
            TSPoolList<TrapezoidQuad>.Spawn(out var cornerQuads);
            UBoundsIncrement.Begin();
            var center = m_Grid.m_Vertices[_corner.location].GetCornerPosition(_corner.height);
            
            var vertex = m_Grid.m_Vertices[_corner.location];
            foreach (var (index, quad) in vertex.m_NearbyQuads.LoopIndex())
                cornerQuads.Add(quad.m_ShapeWS.ConstructGeometry(vertex.GetQuadVertsArrayCW(index), EQuadGeometry.Half));

            vertices.Clear();
            foreach (var cornerQuad in cornerQuads)
            { 
                int indexOffset = vertices.Count;
                vertices.AddRange(cornerQuad.ExpandToQube(center,_corner.height,0f));
                UPolygon.QuadToTriangleIndices(indices, indexOffset + 0, indexOffset + 3, indexOffset + 2, indexOffset + 1); //Bottom
                UPolygon.QuadToTriangleIndices(indices, indexOffset + 4, indexOffset + 5, indexOffset + 6, indexOffset + 7); //Top
                UPolygon.QuadToTriangleIndices(indices, indexOffset + 1, indexOffset + 2, indexOffset + 6, indexOffset + 5); //Forward Left
                UPolygon.QuadToTriangleIndices(indices, indexOffset + 2, indexOffset + 3, indexOffset + 7, indexOffset + 6); //Forward Right
            }

            _mesh.Clear();
            _mesh.SetVertices(vertices);
            _mesh.SetIndices(indices, MeshTopology.Triangles, 0, false);
            TSPoolList<TrapezoidQuad>.Recycle(cornerQuads);
            TSPoolList<Vector3>.Recycle(vertices);
            TSPoolList<int>.Recycle(indices);
        }
    #endregion
        
    #if UNITY_EDITOR
    #region Gizmos
        [Header("Gizmos")] 
        [Header("2 Dimension")]
        public bool m_VertexGizmos;
        public bool m_QuadGizmos;
        [MFoldout(nameof(m_QuadGizmos),true)] public bool m_RelativeQuadGizmos;
        [Header("3 Dimension")]
        public bool m_CornerGizmos;
        [MFoldout(nameof(m_CornerGizmos), true)] public bool m_CornerAdjacentRelations;
        [MFoldout(nameof(m_CornerGizmos), true)] public bool m_CornerIntervalRelations;
        [MFoldout(nameof(m_CornerGizmos), true)] public bool m_CornerVoxelRelations;
        public bool m_VoxelGizmos;
        [MFoldout(nameof(m_VoxelGizmos),true)] public bool m_VoxelCornerRelations;
        [MFoldout(nameof(m_VoxelGizmos),true)] public bool m_VoxelVoxelRelation;

        [Header("Propaganda")]
        public bool m_PropagandaGizmos;
        public bool m_PropagandaVoxels;
        
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;
            
            if (m_VertexGizmos) 
            {
                Gizmos.color = Color.cyan;
                foreach (var vertex in m_GridVertices)
                    Gizmos.DrawWireSphere(vertex.m_Vertex.m_Position,.3f);
            }

            if (m_QuadGizmos)
            {
                foreach (var quad in m_GridQuads)
                {
                    Gizmos.color = Color.white;
                    Gizmos.matrix = quad.transform.localToWorldMatrix;
                    
                    Gizmos_Extend.DrawLinesConcat(quad.m_ShapeOS.positions.Iterate(p=>(float3)p));
                    // Gizmos.DrawLine(Vector3.up,Vector3.up+Vector3.forward);

                    if (m_RelativeQuadGizmos)
                    {
                        Gizmos.matrix = Matrix4x4.identity;
                        for(int i=0;i<quad.m_NearbyQuadCW.Length;i++)
                        {
                            Gizmos.color = UColor.IndexToColor(i);
                            if(m_GridQuads.Contains(quad.m_NearbyQuadCW[i]))
                                Gizmos_Extend.DrawLine(quad.transform.position,m_GridQuads[quad.m_NearbyQuadCW[i]].Quad.position,.4f);
                        }
                    }
                }
            }

            if (m_CornerGizmos)
            {
                foreach (var corner in m_Corners)
                {
                    Gizmos.color = UColor.IndexToColor(corner.m_Type);
                    Gizmos.matrix = corner.transform.localToWorldMatrix;
                    Gizmos.DrawWireSphere(Vector3.zero,.5f);
                    
                    if (m_CornerAdjacentRelations)
                    {
                        Gizmos.matrix = Matrix4x4.identity;
                        Gizmos.color = Color.green;
                        foreach (var cornerID in corner.m_AdjacentConnectedCorners)
                            Gizmos_Extend.DrawLine(corner.transform.position, m_Corners[cornerID].transform.position , .4f);
                    }
                    
                    if (m_CornerIntervalRelations)
                    {
                        Gizmos.matrix = Matrix4x4.identity;
                        Gizmos.color = Color.blue;
                        foreach (var cornerID in corner.m_IntervalConnectedCorners)
                            Gizmos_Extend.DrawLine(corner.transform.position, m_Corners[cornerID].transform.position , .4f);
                    }
                    
                    if(m_CornerVoxelRelations)
                    {
                        Gizmos.matrix = Matrix4x4.identity;
                        Gizmos.color = Color.yellow;
                        foreach (var voxel in corner.m_RelativeVoxels)
                            Gizmos_Extend.DrawLine(corner.transform.position,m_Voxels[voxel].transform.position,.8f);
                    }
                }
            }

            if (m_VoxelGizmos)
            {
                Gizmos.color = Color.white;
                foreach (var voxel in m_Voxels)
                {
                    Gizmos.color = Color.white;
                    Gizmos.matrix = voxel.transform.localToWorldMatrix;
                    Gizmos.DrawSphere(Vector3.zero,.3f);
                    Gizmos.matrix = Matrix4x4.identity;
                    
                    if (m_VoxelCornerRelations)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            Gizmos.color = UColor.IndexToColor(i%4);
                            if (voxel.m_Corners[i]!=null)
                                Gizmos_Extend.DrawLine(voxel.transform.position,voxel.m_Corners[i].Transform.position,.8f);
                        }
                    }

                    if (m_VoxelVoxelRelation)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            Gizmos.color = UColor.IndexToColor(i%4);
                            if(voxel.m_CubeSidesExists.IsFlagEnable(UEnum.IndexToEnum<ECubeFacing>(i)))
                                Gizmos_Extend.DrawLine(voxel.transform.position,m_Voxels[voxel.m_CubeSides[i]].transform.position,.8f);
                        }
                    }
                }
            }

            if (m_PropagandaGizmos)
            {
                
                foreach (var (index,chain) in m_PropagandaChains.LoopIndex())
                {
                    Gizmos.color = UColor.IndexToColor(index).SetAlpha(.5f);
                    if (m_PropagandaVoxels)
                    {
                        foreach (var voxel in chain.voxels)
                        {
                            Gizmos.matrix = m_Voxels[voxel].transform.localToWorldMatrix;
                            Gizmos.DrawWireCube(Vector3.zero,Vector3.one*.25f);
                            Gizmos.matrix = Matrix4x4.identity;
                        }
                        Gizmos_Extend.DrawLines(chain.voxels,_p=>m_Voxels[_p].transform.position);
                    }
                    else
                    {
                        foreach (var corner in chain.corners)
                        {
                            Gizmos.matrix = m_Corners[corner].transform.localToWorldMatrix;
                            Gizmos.DrawWireSphere(Vector3.zero,.5f);
                            Gizmos.matrix = Matrix4x4.identity;
                        }
                        Gizmos_Extend.DrawLines(chain.corners,_p=>m_Corners[_p].transform.position);
                    }
                }
            }
        }
    #endregion
    #endif
    }
}