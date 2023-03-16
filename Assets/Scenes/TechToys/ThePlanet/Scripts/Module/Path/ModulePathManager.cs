using System.Collections;
using System.Collections.Generic;
using TechToys.ThePlanet.Module.Prop;
using TPool;
using TPoolStatic;
using UnityEngine;

namespace TechToys.ThePlanet.Module.Path
{
    public class ModulePathManager : MonoBehaviour, IModuleControl, IModuleVoxelCallback, IModuleCollapse , IModuleStructure
    {
        public GridManager m_Grid { get; set; }

        private ObjectPoolMono<PCGID, ModulePath> m_Paths;
        public IModuleStructureElement CollectStructure(PCGID _voxelID)=> m_Paths[_voxelID];
        
        private readonly List<PCGID> m_VoxelPathPropaganda = new List<PCGID>();
        private readonly Dictionary<PCGID, ModulePathCollapse> m_VoxelPathCollapsing = new Dictionary<PCGID, ModulePathCollapse>();
        private readonly List<IEnumerator> m_PathIterator = new List<IEnumerator>();

        public void Init()
        {
            m_Paths = new ObjectPoolMono<PCGID, ModulePath>(transform.Find("Item"));
        }

        public void Setup()
        {
        }

        public void Dispose()
        {
            TSPool<ModulePathCollapse>.Clear();
        }

        public void Clear()
        {
            m_Paths.Clear();
            m_PathIterator.Clear();
            ClearPathCollapse();
        }

        public void Tick(float _deltaTime)
        {
            
        }

        public void OnVoxelConstruct(IVoxel _voxel) => m_Paths.Spawn(_voxel.Identity).Init(_voxel);
        public void OnVoxelDeconstruct(PCGID _voxelID) => m_Paths.Recycle(_voxelID);

        
        void ClearPathCollapse()
        {
            m_VoxelPathPropaganda.Clear();
            foreach (var voxel in m_VoxelPathCollapsing.Values)
                TSPool<ModulePathCollapse>.Recycle(voxel);
            m_VoxelPathCollapsing.Clear();
        }

        public void Propaganda(float _deltaTime, Stack<ModuleCollapsePropagandaChain> _propagandaChains)
        {
            m_PathIterator.Clear();
            foreach (var propagandaChain in _propagandaChains)
            {
                if(propagandaChain.chainType==-1)
                    continue;
                m_PathIterator.Add(CollapsePaths(propagandaChain));
            }
        }

        IEnumerator CollapsePaths(ModuleCollapsePropagandaChain _chain)
        {
            var type = _chain.chainType;
            var collapseData = DModule.Collection[type];
            if (!collapseData.m_Paths.Available)
                yield break;

            var voxels = _chain.voxels;
            ClearPathCollapse();
            foreach (var voxelID in voxels)
            {
                var voxel = m_Paths[voxelID];
                if (!voxel.m_Voxel.m_ClusterUnitBaseBytes.IsPath())
                {
                    voxel.Clear();
                    continue;
                }
                m_VoxelPathCollapsing.Add(voxelID, TSPool<ModulePathCollapse>.Spawn().Init(voxel.m_Voxel));
                m_VoxelPathPropaganda.Add(voxelID);
            }

            foreach (var collapse in m_VoxelPathCollapsing.Values)
                collapse.Fill(m_VoxelPathCollapsing);
            m_VoxelPathPropaganda.Sort((_a, _b) => m_VoxelPathCollapsing[_a].m_Priority > m_VoxelPathCollapsing[_b].m_Priority ? 1 : -1);

            TSPoolStack<PCGID>.Spawn(out var propagandaStack);
            int iteration = 0;
            while (iteration++ < 1024)
            {
                var curCollapseID = m_VoxelPathPropaganda.FindIndex(p => !m_VoxelPathCollapsing[p].m_Collapsed);
                if (curCollapseID == -1)
                    break;

                var curCollapse = m_VoxelPathPropaganda[curCollapseID];
                var curCollapsed = m_VoxelPathCollapsing[curCollapse];
                curCollapsed.Collapse();

                propagandaStack.Push(curCollapsed.m_Voxel.Identity);
                while (propagandaStack.Count > 0)
                {
                    var propaganda = m_VoxelPathCollapsing[propagandaStack.Pop()];
                    for (int i = 0; i < 4; i++)
                    {
                        if (!m_VoxelPathCollapsing.TryGetValue(propaganda.m_Voxel.m_CubeSides[i], out var sidePropaganda))
                            continue;

                        if (!sidePropaganda.Propaganda(m_VoxelPathCollapsing))
                            continue;

                        propagandaStack.Push(sidePropaganda.m_Voxel.Identity);
                    }
                }
                yield return null;
            }
            TSPoolStack<PCGID>.Recycle(propagandaStack);

            foreach (var voxelID in m_VoxelPathPropaganda)
                m_Paths[voxelID].Validate(m_VoxelPathCollapsing[voxelID].m_Result, collapseData, DModule.Collection.m_MaterialLibrary);
        }

        public bool Collapse(float _deltaTime)
        {
            foreach (var collapseIterator in m_PathIterator)
            {
                if (collapseIterator.MoveNext())
                    return false;
            }

            return true;
        }

        public bool Finalize(float _deltaTime)
        {
            return true;
        }

        #region Gizmos

        [Header("Gizmos")]
        public bool m_LastPathCollapseGizmos;

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;


            if (m_LastPathCollapseGizmos)
                LastPathCollapseGizmos();
        }

        void LastPathCollapseGizmos()
        {
#if UNITY_EDITOR
            int index = 0;
            foreach (var voxelID in m_VoxelPathPropaganda)
            {
                Gizmos.color = index == 0 ? Color.green : Color.white;
                var voxel = m_Paths[voxelID];
                Gizmos.matrix = voxel.transform.localToWorldMatrix;
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one * .25f);

                Gizmos.matrix = Matrix4x4.identity;
                var collapse = m_VoxelPathCollapsing[voxelID];
                if (collapse.m_Possibilities.Count > 0)
                    Gizmos_Extend.DrawString(voxel.transform.position, collapse.m_Possibilities.Count.ToString());

                for (int i = 0; i < 4; i++)
                {
                    if (!collapse.m_Result[i])
                        continue;

                    Gizmos.color = UColor.IndexToColor(i);
                    Gizmos_Extend.DrawLine(collapse.m_Voxel.Transform.position, m_Paths[collapse.m_Voxel.m_CubeSides[i]].transform.position, .4f);
                }

                index++;
            }
#endif
        }

        #endregion

    }

}