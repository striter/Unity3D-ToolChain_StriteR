using System.Collections;
using System.Collections.Generic;
using TPool;
using UnityEngine;
namespace PCG.Module.Cluster
{
    using static PCGDefines<int>;
    public class ModuleClusterManager : MonoBehaviour, IModuleControl,IModuleCornerCallback,IModuleVoxelCallback,IModuleCollapse,IModuleStructure
    {
        public GridManager m_Grid { get; set; }
        private TObjectPoolMono<PCGID, ModuleClusterCorner> m_ClusterCorners;
        private TObjectPoolMono<PCGID, ModuleClusterContainer> m_ClusterContainers;
        public IModuleStructureElement CollectStructure(PCGID _voxelID)=>m_ClusterContainers[_voxelID];

        private readonly List<IEnumerator> m_ClusterIterators = new List<IEnumerator>();
        public void Init()
        {
            m_ClusterContainers = new TObjectPoolMono<PCGID, ModuleClusterContainer>(transform.Find("Voxel/Item"));
            m_ClusterCorners = new TObjectPoolMono<PCGID, ModuleClusterCorner>(transform.Find("Corner/Item"));
        }

        public void Setup()
        {
        }

        public void Dispose()
        {
        }
        public void Clear()
        {
            m_ClusterContainers.Clear();
            m_ClusterCorners.Clear();
            m_ClusterIterators.Clear();
        }

        public void OnCornerConstruct(ICorner _corner)=>m_ClusterCorners.Spawn(_corner.Identity).Init(_corner);
        public void OnCornerDeconstruct(PCGID _cornerID)=>m_ClusterCorners.Recycle(_cornerID);
        public void OnVoxelConstruct(IVoxel _voxel)=>m_ClusterContainers.Spawn(_voxel.Identity).Init(_voxel);
        public void OnVoxelDeconstruct(PCGID _voxelID)=>m_ClusterContainers.Recycle(_voxelID);
        
        public void Propaganda(float _deltaTime, Stack<ModuleCollapsePropagandaChain> _propagandaChains)
        {
            m_ClusterIterators.Clear();
            foreach (var chain in _propagandaChains)
            {
                if (chain.chainType != -1)
                {
                    byte minHeight = byte.MaxValue;
                    byte maxHeight = byte.MinValue;
                    foreach (var corner in chain.corners)
                    {
                        minHeight = corner.height < minHeight ? corner.height : minHeight;
                        maxHeight = corner.height > maxHeight ? corner.height : maxHeight;
                    }
            
                    foreach (var cornerID in chain.corners)
                        m_ClusterCorners[cornerID].RefreshStatus(minHeight, maxHeight, m_ClusterCorners.m_Dic);
                }
                
                m_ClusterIterators.Add(CollapseClusters(chain));
                foreach (var voxelID in chain.voxels)
                    m_ClusterContainers[voxelID].Prepare(m_ClusterCorners.m_Dic);
                
            }
        }
        IEnumerator CollapseClusters(ModuleCollapsePropagandaChain _chain)
        {
            foreach (var voxelID in _chain.voxels)
            {
                m_ClusterContainers[voxelID].Collapse();
                yield return null;
            }
        }
        
        public bool Collapse(float _deltaTime)
        {
            foreach (var clusterIterator in m_ClusterIterators)
            {
                if (clusterIterator.MoveNext())
                    return false;
            }

            return true;
        }

        public bool Finalize(float _deltaTime)
        {
            return true;
        }

        public void Tick(float _deltaTime)
        {
        }


#if UNITY_EDITOR
        #region Gizmos
        [Header("Gizmos")]
        public bool m_CornerGizmos;
        [MFoldout(nameof(m_CornerGizmos),true)]public bool m_CornerStatus;
        public bool m_VoxelGizmos;
        void OnDrawGizmos()
        {
            if (m_ClusterCorners==null)
                return;
            
            if (m_CornerGizmos)
            {
                foreach (var corner in m_ClusterCorners)
                {
                    Gizmos.matrix = corner.transform.localToWorldMatrix;
                    Gizmos.color = Color.white;
                    Gizmos.DrawWireSphere(Vector3.zero, .2f);
                    
                    if (m_CornerStatus)
                        Gizmos_Extend.DrawString(Vector3.up * .2f, $"{corner.m_Status}");
                }
            }

            foreach (var container in m_ClusterContainers)
                container.m_VoxelGizmos = m_VoxelGizmos;
        }
    #endregion
#endif
    }
}