using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Geometry.Voxel;
using LinqExtension;
using Procedural;
using Procedural.Hexagon;
using TPool;
using TPoolStatic;
using UnityEngine;

namespace PolyGrid.Module
{
    public class ModuleManager : MonoBehaviour,IPolyGridControl
    {
        public EModuleType m_SpawnModule;
        public List<ModuleRuntimeData> m_Data;
        private TObjectPoolMono<PileID, ModuleVoxel> m_Voxels { get; set; }
        private TObjectPoolMono<PileID, ModuleCorner> m_Corners { get; set; }
        private readonly Queue<PileID> m_CornerPropaganda=new Queue<PileID>();
        private readonly Stack<Stack<PileID>> m_PropagandaChains = new Stack<Stack<PileID>>();
        public void Init(Transform _transform)
        {
            m_Voxels = new TObjectPoolMono<PileID, ModuleVoxel>(_transform.Find("Modules/Voxel/Item"));
            m_Corners = new TObjectPoolMono<PileID, ModuleCorner>(_transform.Find("Modules/Corner/Item"));
        }
        public void Clear()
        {
            m_Voxels.Clear();
            m_Corners.Clear();
        }

        private void OnValidate()
        {
            m_Data.Sort((a,b)=>a.m_Type-b.m_Type);
        }

        public void SpawnCorners(ICorner _corner)=> m_Corners.Spawn(_corner.Identity).Init(_corner,m_SpawnModule);
        public void RecycleCorners(PileID _cornerID)=>m_Corners.Recycle(_cornerID);
        public void SpawnModules(IVoxel _module)=>m_Voxels.Spawn(_module.Identity).Init(_module);
        public void RecycleModules(PileID _moduleID)=>m_Voxels.Recycle(_moduleID);
        
        public void Tick(float _deltaTime)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
                m_SpawnModule = m_SpawnModule.Next();
        }

        public void OnSelectVertex(PolyVertex _vertex, byte _height)
        {
            CollectPropagandaCorners(_vertex,_height);
            CollectPropagandaChain();
            PropagandaChainedModules(_vertex,_height);
        }

        void CollectPropagandaCorners(PolyVertex _vertex,byte _height)
        {
            m_CornerPropaganda.Clear();

            var propagandaStack = TSPoolStack<PileID>.Spawn();
            
            void TryPropaganda(PileID _cornerID)
            {
                if (!m_Corners.Contains(_cornerID))
                    return;
                
                if (m_CornerPropaganda.Contains(_cornerID))
                    return;
                    
                if (propagandaStack.Contains(_cornerID))
                    return;
                
                propagandaStack.Push(_cornerID);
            }

            var beginCorner = new PileID(_vertex.m_Identity, _height);
            if(m_Corners.Contains(beginCorner))
                TryPropaganda(beginCorner);
            else
                foreach (var _cornerID in _vertex.AllNearbyCorner(_height))
                    TryPropaganda(_cornerID);
            
            while (propagandaStack.Count>0)
            {
                var propagandaCornerID = propagandaStack.Pop();
                m_CornerPropaganda.Enqueue(propagandaCornerID);
                
                foreach (var corner in m_Corners[propagandaCornerID].m_Corner.NearbyCorners)
                    TryPropaganda(corner);
            }
            TSPoolStack<PileID>.Recycle(propagandaStack);
        }

        void CollectPropagandaChain()
        {
            foreach (var chain in m_PropagandaChains)
                TSPoolStack<PileID>.Recycle(chain);
            m_PropagandaChains.Clear();

            foreach (var propagandaCorner in m_CornerPropaganda)
            {
                var chain =  m_PropagandaChains.Find(chain=>chain.Any(chainedCorner => m_Corners[chainedCorner].m_Corner.NearbyCorners.Contains(propagandaCorner)));
                if (chain == null)
                {
                    chain = TSPoolStack<PileID>.Spawn();
                    m_PropagandaChains.Push(chain);
                }
                chain.Push(propagandaCorner);
            }
        }

        void PropagandaChainedModules(PolyVertex _vertex,byte _height)
        {
            Stack<PileID> affectedModules = new Stack<PileID>();
            foreach (var chain in m_PropagandaChains)
            {
                byte maxCornerHeight = chain.Max(p => p.height);
                foreach (var cornerID in chain)
                {
                    var corner = m_Corners[cornerID];
                    if (!corner.RefreshStatus(maxCornerHeight))
                        continue;
                    foreach (var voxelID in corner.m_Corner.NearbyVoxels)
                        affectedModules.TryPush(voxelID);
                }
            }

            foreach (var voxelID in _vertex.AllNearbyVoxels(_height))
            {
                if(!m_Voxels.Contains(voxelID))
                    continue;
                affectedModules.TryPush(voxelID);
            }
            
            foreach (var moduleID in affectedModules)
                m_Voxels[moduleID].ModuleValidate(m_Data,m_Corners.m_Dic);
        }
        
        public void OnAreaConstruct(PolyArea _area)
        {
        }

        public void UpdateModules(IEnumerable<PileID> _modules)
        {
            foreach (var pileID in _modules)
                m_Voxels[pileID].ModuleValidate(m_Data,m_Corners.m_Dic);
        }
        #if UNITY_EDITOR
        #region Gizmos

        public bool m_CornerGizmos;
        [MFoldout(nameof(m_CornerGizmos),true)]public bool m_CornerStatus;
        public bool m_VoxelGizmos;
        [MFoldout(nameof(m_VoxelGizmos),true)]public bool m_ShapeGizmos;
        public bool m_PropagandaGizmos;
        public bool m_PropagandaChainsGizmos;
        void OnDrawGizmos()
        {
            if (m_CornerGizmos && m_Corners != null)
            {
                Gizmos.color = Color.white;
                foreach (var corner in m_Corners)
                {
                    Gizmos.matrix = corner.transform.localToWorldMatrix;
                    if (m_CornerStatus)
                    {
                        Gizmos.color = corner.m_Type.ToColor();
                        Gizmos_Extend.DrawString(Vector3.up*.2f,$"{corner.m_Status}");
                    }
                    Gizmos.DrawSphere(Vector3.zero,.3f);
                }
            }

            if (m_VoxelGizmos && m_Voxels != null)
            {
                Gizmos.color = Color.white.SetAlpha(.5f);
                foreach (ModuleVoxel moduleContainer in m_Voxels)
                {
                    Gizmos.matrix = moduleContainer.transform.localToWorldMatrix;
                    Gizmos.DrawWireSphere(Vector3.zero,.3f);
                    if(m_ShapeGizmos)
                        foreach (var quad in moduleContainer.m_Voxel.CornerShapeLS)
                            Gizmos_Extend.DrawLinesConcat(quad.Iterate(p=>((Coord)p).ToPosition()));
                }
            }
            
            if (m_PropagandaGizmos && m_Corners != null)
            {
                Gizmos.matrix=Matrix4x4.identity;
                Gizmos.color = Color.white;
                Gizmos_Extend.DrawLines(m_CornerPropaganda.ToList(),p=>m_Corners[p].transform.position);
            }

            if (m_PropagandaChainsGizmos && m_Corners != null)
            {
                foreach (var tuple in m_PropagandaChains.LoopIndex())
                {
                    Gizmos.color = UColor.IndexToColor(tuple.index);
                    var chain = tuple.value;
                    foreach (var pile in chain)
                    {
                        Gizmos.matrix = m_Corners[pile].transform.localToWorldMatrix;
                        Gizmos.DrawWireSphere(Vector3.zero,.5f);
                    }
                }
            }
        }
        #endregion
        #endif
    }
}
