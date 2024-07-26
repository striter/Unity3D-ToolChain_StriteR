using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry;
using TPool;
using TObjectPool;
using UnityEditor;
using UnityEngine;
using Gizmos = UnityEngine.Gizmos;

namespace TechToys.ThePlanet.Module.Prop
{
    public class ModulePropManager : MonoBehaviour,IModuleControl,IModuleVoxelCallback,IModuleCollapse,IModuleStructure
    {
        public GridManager m_Grid { get; set; }
        private ObjectPoolBehaviour<PCGID, ModulePropContainer> m_PropContainers;
        private ObjectPoolClass<int, ModulePropElement> m_PropElements;
        public IModuleStructureElement CollectStructure(PCGID _voxels)=>m_PropContainers[_voxels];

        private readonly List<PCGID> m_PropPropaganda = new List<PCGID>();
        private readonly Dictionary<PCGID, VoxelPropCollapse> m_PropCollapsing = new Dictionary<PCGID, VoxelPropCollapse>();
        private readonly List<IEnumerator> m_CollapseIterators = new List<IEnumerator>();
        
        public void OnVoxelConstruct(IVoxel _voxel)=>m_PropContainers.Spawn(_voxel.Identity).Init(_voxel);
        public void OnVoxelDeconstruct(PCGID _voxelID)=>m_PropContainers.Recycle(_voxelID);
        public void Init()
        {
            m_PropContainers = new ObjectPoolBehaviour<PCGID, ModulePropContainer>(transform.Find("Container/Item"));
            m_PropElements = new ObjectPoolClass<int, ModulePropElement>(transform.Find("Element/Item"));
        }

        public void Setup()
        {
        }

        public void Dispose()
        {
            ObjectPool<VoxelPropCollapse>.Clear();
        }

        public void Clear()
        {
            m_PropContainers.Clear();
            m_PropElements.Clear();
            m_CollapseIterators.Clear();
            ClearPropCollapse();
        }

        public void Tick(float _deltaTime)
        {
            TSPoolList<int>.Spawn(out var recycleList);

            foreach (var element in m_PropElements)
            {
                if(element.TickRecycle(_deltaTime))
                    recycleList.Add(element.identity);
            }

            foreach (var recycleElement in recycleList)
                m_PropElements.Recycle(recycleElement);
            
            TSPoolList<int>.Recycle(recycleList);
        }

        
        public void Propaganda(float _deltaTime, Stack<ModuleCollapsePropagandaChain> _propagandaChains)
        {
            m_CollapseIterators.Clear();
            foreach (var chain in _propagandaChains)
            {
                foreach (var voxel in chain.voxels)
                {
                    if(m_PropContainers.Contains(voxel))
                        m_PropContainers[voxel].BeginCollapse(m_PropElements);
                }
                
                if (chain.chainType == -1)
                    continue;
                m_CollapseIterators.Add(CollapseProps(chain));
            }
        }

        public bool Collapse(float _deltaTime)
        {
            foreach (var collapseIterator in m_CollapseIterators)
            {
                if (collapseIterator.MoveNext())
                    return false;
            }
            return true;
        }

        public bool Finalize(float _deltaTime)
        {
            // foreach (var finalizeIterator in m_FinalizeIterators)
            // {
            //     if(finalizeIterator.MoveNext())
            //        return false;
            // }
            return true;
        }
        
        void ClearPropCollapse()
        {
            m_PropPropaganda.Clear();
            foreach (var decorationCollapse in m_PropCollapsing.Values)
                ObjectPool<VoxelPropCollapse>.Recycle(decorationCollapse);
            m_PropCollapsing.Clear();
        }
        
        IEnumerator CollapseProps(ModuleCollapsePropagandaChain _chain)
        {
            ClearPropCollapse();
            var chainType = _chain.chainType;
            var chainVoxels = _chain.voxels;
            
            var moduleSet= DModule.Collection[chainType];
            if (!moduleSet.m_Decorations.Available)
                yield break;
            
            var type = moduleSet.m_ClusterType;
            var collection = moduleSet.m_Decorations;

            foreach (var voxelID in chainVoxels)
            {
                if (!m_PropContainers.Contains(voxelID))
                    continue;
                m_PropPropaganda.Add(voxelID);
                m_PropCollapsing.Add(voxelID,ObjectPool<VoxelPropCollapse>.Spawn().Init(chainType,m_PropContainers[voxelID].m_Voxel));
            }
            m_PropPropaganda.Sort((_a,_b)=>m_PropCollapsing[_a].m_Priority>m_PropCollapsing[_b].m_Priority?1:-1);

            TSPoolHashset<PCGID>.Spawn(out var decorationCollapse);
            for(int i=0;i<collection.decorationSets.Length;i++)
            {
                var decorationSet = collection.decorationSets[i];
                decorationCollapse.Clear();
                foreach (var voxelID in m_PropPropaganda)
                {
                    if(!m_PropCollapsing[voxelID].Available(type,decorationSet.masks))
                        continue;
                    decorationCollapse.Add(voxelID);
                }

                int iteration = 0;
                while (iteration++<512)
                {
                    if(decorationCollapse.Count==0)
                        continue;

                    var toCollapse = decorationCollapse.First();
                    var toCollapseCorner = m_PropCollapsing[toCollapse];
                    toCollapseCorner.Collapse(i);
                    decorationCollapse.Remove(toCollapse);
                    decorationCollapse.RemoveRange(UModuleProp.GetAdjacentVoxelInRange(toCollapse,decorationSet.density,m_PropContainers.m_Dic));
                }

                if (decorationSet.maskRightAvailablity) //Mask Right Voxels Adjacent To Empty
                {
                    foreach (var collapse in m_PropCollapsing.Values.Collect(p=>p.m_Result.Available))
                    {
                        var indexer=  UModulePropByte.GetOrientedPropIndex(type,collapse.m_Voxel.m_TypedCluster[chainType]);
                        var facing = UEnum.IndexToEnum<EQuadFacing>((indexer.orientation+1)%4);
                        if(!collapse.m_Voxel.m_CubeSidesExists.IsFlagEnable((int)facing))
                            continue;
                        var rightVoxelID = collapse.m_Voxel.m_CubeSides[facing];
                        if (!m_PropCollapsing.ContainsKey(rightVoxelID))
                            continue;
                        var rightCollapse = m_PropCollapsing[rightVoxelID];
                        if(rightCollapse.m_Collapsed)
                            continue;
                        rightCollapse.AppendCollapseMask(collapse.m_Voxel.Identity);
                    }
                }
                
                yield return null;
            }
            TSPoolHashset<PCGID>.Recycle(decorationCollapse);
            foreach (var voxelID in chainVoxels)
            {
                m_PropContainers[voxelID].Finalize(chainType,m_PropCollapsing[voxelID].m_Result,m_PropElements);
                yield return null;
            }
        }
        
        #if UNITY_EDITOR
        #region Gizmos

        [Header("Gizmos")]
        public bool m_LastPropCollapseGizmos;
        private void OnDrawGizmos()
        {
            if (m_PropContainers==null)
                return;
            

            if (m_LastPropCollapseGizmos)
                LastPropCollapseGizmos();

            DrawSelectedProps();
        }
        
        void DrawSelectedProps()
        {
            ModulePropContainer selectedContainer = m_PropContainers.Find(p=>p.m_Props.Any(p=>p.transform.gameObject == Selection.activeObject));
            if (selectedContainer == null)
                return;

            Gizmos.matrix = selectedContainer.transform.localToWorldMatrix;
            Gizmos.color = Color.white;
            Gizmos.DrawCube(Vector3.zero,Vector3.one*.2f);
            int index = 0;
            foreach (var type in selectedContainer.m_PropIndexes.Keys)
            {
                if (!selectedContainer.m_PropIndexes[type].Available)
                    continue;
                    
                Gizmos.color = UColor.IndexToColor(type);
                var propByte = selectedContainer.m_PropIndexes[type].propByte;
                var propIndexer = UModulePropByte.GetOrientedPropIndex(DModule.Collection.m_ModuleLibrary[type].m_ClusterType,propByte);
                UGizmos.DrawString($"{type},{propByte}|{propIndexer.srcByte},{propIndexer.orientation}", Vector3.up*.1f*(4+index++));
            }
                
            Gizmos.matrix = transform.worldToLocalMatrix;
            Gizmos.color = Color.white;
            foreach (var prop in selectedContainer.m_Props)
            {
                var position = prop.transform.position;
                Gizmos.DrawWireSphere(position,.1f);
                Gizmos.DrawLine(selectedContainer.transform.position,position);
            }
        }
        
        void LastPropCollapseGizmos()
        {
            foreach (var voxelID in m_PropPropaganda)
            {
                if(!m_PropContainers.Contains(voxelID))
                    continue;
                
                var voxel = m_PropContainers[voxelID];
                Gizmos.matrix = voxel.transform.localToWorldMatrix;
                Gizmos.DrawWireCube(Vector3.zero,Vector3.one*.25f);

                var collapse = m_PropCollapsing[voxelID];
                var finalByte = collapse.VoxelByte;
                var index = UModulePropByte.GetOrientedPropIndex( DModule.Collection.m_ModuleLibrary[collapse.m_Type].m_ClusterType,finalByte);
                UGizmos.DrawString($"B:{collapse.m_BaseByte}|M:{collapse.m_MaskByte} \nF:{finalByte}|I:{index.srcByte},{index.orientation}", Vector3.up*.2f);
            }
        }
        #endregion
        #endif
    }

}