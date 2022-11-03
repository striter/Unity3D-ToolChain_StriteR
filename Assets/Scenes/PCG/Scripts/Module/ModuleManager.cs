using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PCG.Module.BOIDS;
using PCG.Module.Cluster;
using PCG.Module.Path;
using PCG.Module.Prop;
using TDataPersistent;
using TPoolStatic;
using UnityEngine;

namespace PCG.Module
{
    using static PCGDefines<int>;
    public class ModuleManager : MonoBehaviour,IPolyGridControl
    {
        private IModuleControl[] m_Controls;
        
        public ModuleGridManager m_GridManager { get; private set; }
        private ModuleClusterManager m_ClusterManger;
        private ModulePropManager m_PropManager;
        private ModulePathManager m_PathManager;
        private ModuleBoidsManager m_BoidsManager;

        private IModuleVertexCallback[] m_VertexCallbacks;
        private IModuleQuadCallback[] m_QuadCallbacks;
        private IModuleCornerCallback[] m_CornerCallbacks;
        private IModuleVoxelCallback[] m_VoxelCallbacks;
        private IModuleStructure[] m_Structures;
        
        private IModuleCollapse[] m_ModuleCollapses;
        private EModuleCollapseStatus m_CollapseStatus;
        
        private readonly List<PCGID> m_DirtyCorners = new List<PCGID>();
        private readonly Dictionary<string, int> m_ModuleIndexer = new Dictionary<string, int>();

        [Header("Wave Function Collapse")]
        public bool m_ForceIteration = false;
        [MFoldout(nameof(m_ForceIteration),false)] [Clamp(1,1024)]public int m_IterationPerFrame =1;
        
        public void Init()
        {
            m_GridManager = transform.Find("Grid").GetComponent<ModuleGridManager>();
            m_ClusterManger = transform.Find("Cluster").GetComponent<ModuleClusterManager>();
            m_PropManager = transform.Find("Prop").GetComponent<ModulePropManager>();
            m_PathManager = transform.Find("Path").GetComponent<ModulePathManager>();
            m_BoidsManager = transform.Find("Boids").GetComponent<ModuleBoidsManager>();
            
            m_Controls = new IModuleControl[] { m_GridManager , m_ClusterManger, m_PropManager , m_PathManager,m_BoidsManager  };
            m_VertexCallbacks = m_Controls.CollectAs<IModuleControl, IModuleVertexCallback>().ToArray();
            m_QuadCallbacks = m_Controls.CollectAs<IModuleControl, IModuleQuadCallback>().ToArray();
            m_CornerCallbacks = m_Controls.CollectAs<IModuleControl, IModuleCornerCallback>().ToArray();
            m_VoxelCallbacks = m_Controls.CollectAs<IModuleControl, IModuleVoxelCallback>().ToArray();
            m_Structures = m_Controls.CollectAs<IModuleControl, IModuleStructure>().ToArray();
            m_ModuleCollapses = m_Controls.CollectAs<IModuleControl, IModuleCollapse>().ToArray();
            
            m_Controls.Traversal(_p => _p.Init());
        }
        
        public ModuleManager Setup(ModuleCollection _collection, GridManager _grid)
        {
            Clear();
            DModule.Collection = _collection;
            
            m_ModuleIndexer.Clear();
            foreach (var (type,data) in DModule.Collection.m_ModuleLibrary.LoopIndex())
                m_ModuleIndexer.Add(data.name, type);

            m_Controls.Traversal(_p=>
            {
                _p.m_Grid = _grid;
                _p.Setup();
            });
            return this;
        }

        public void Clear()
        {
            m_Controls.Traversal(_p => _p.Clear());
            m_DirtyCorners.Clear();
            m_CollapseStatus = EModuleCollapseStatus.Awaiting;
        }
        
        public void Dispose()
        {
            DModule.Collection = null;
            m_Controls.Traversal(_p=>_p.Dispose());
        }

        public void Input(string _moduleName,Ray _ray,bool _construct)
        {
            if (!m_ModuleIndexer.ContainsKey(_moduleName))
                throw new Exception($"Invalid Resource Name Found:{_moduleName},{DModule.Collection.name}");
            var moduleType = m_ModuleIndexer[_moduleName];
            PCGID selection;
            if (_construct)
            {
                if(m_GridManager.ConstructRaycast(_ray,out selection))
                    Construct(moduleType,selection);
                return;
            }

            if(m_GridManager.DeconstructRaycast(_ray, out selection))
                Deconstruct(selection);
        }

        public void Construct(int _type, PCGID _corner)
        {
            m_GridManager.CornerConstruction(_corner, _type, m_GridManager.m_Grid.m_Vertices[_corner.location], OnVertexSpawn, OnQuadSpawn, OnCornerSpawn, OnVoxelSpawn);
            m_DirtyCorners.TryAdd(_corner);
            m_CollapseStatus = EModuleCollapseStatus.Awaiting;
        }

        public void Deconstruct(PCGID _corner)
        {
            m_GridManager.CornerDeconstruction(_corner, m_GridManager.m_Grid.m_Vertices[_corner.location], OnVertexRecycle, OnQuadRecycle, OnCornerRecycle, OnVoxelRecycle);
            m_DirtyCorners.TryAdd(_corner);
            m_CollapseStatus = EModuleCollapseStatus.Awaiting;
        }

        bool TickCollapse(float _deltaTime)
        {
            switch (m_CollapseStatus)
            {
                case EModuleCollapseStatus.Awaiting:
                {
                    if (m_DirtyCorners.Count == 0)
                        return false;

                    var propagandaChains = m_GridManager.CollectPropagandaRelations(m_DirtyCorners);

                    for (int i = 0; i < m_ModuleCollapses.Length; i++)
                        m_ModuleCollapses[i].Propaganda(_deltaTime,propagandaChains);
                    
                    m_CollapseStatus = EModuleCollapseStatus.Collapsing;
                    return true;
                }
                case EModuleCollapseStatus.Collapsing:
                {
                    for(int i=0;i<m_ModuleCollapses.Length;i++)
                        if (!m_ModuleCollapses[i].Collapse(_deltaTime))
                            return true;
                    
                    m_CollapseStatus = EModuleCollapseStatus.Finalizing;
                    return true;
                }
                case EModuleCollapseStatus.Finalizing:
                {
                    for(int i=0;i<m_ModuleCollapses.Length;i++)
                        if (!m_ModuleCollapses[i].Finalize(_deltaTime))
                            return true;
                    
                    m_DirtyCorners.Clear();
                    m_CollapseStatus = EModuleCollapseStatus.Awaiting;
                    return false;
                }
            }
            return false;
        }
        
        private static readonly int kMaxIterationTimes = 4096;
        public void Tick(float _deltaTime)
        {
            bool collapsing = true;
            int iterationTimes = m_ForceIteration ? kMaxIterationTimes : m_IterationPerFrame;
            while (iterationTimes-- > 0)
            {
                collapsing &= TickCollapse(_deltaTime);
                if (!collapsing)
                    break;
            }

            if (collapsing)
                return;
            m_Controls.Traversal(_p => _p.Tick(_deltaTime));
        }

        private void OnVertexSpawn(IVertex _vertex) => m_VertexCallbacks.Traversal(_p => _p.OnPopulateVertex(_vertex));
        private void OnVertexRecycle(SurfaceID _vertexID) => m_VertexCallbacks.Traversal(_p => _p.OnDeconstructVertex(_vertexID));
        private void OnQuadSpawn(IQuad _quad) => m_QuadCallbacks.Traversal(_p => _p.OnPopulateQuad(_quad));
        private void OnQuadRecycle(SurfaceID _quadID) => m_QuadCallbacks.Traversal(_p => _p.OnDeconstructQuad(_quadID));
        private void OnCornerSpawn(ICorner _corner) => m_CornerCallbacks.Traversal(_p => _p.OnCornerConstruct(_corner));
        private void OnCornerRecycle(PCGID _cornerID) => m_CornerCallbacks.Traversal(_p => _p.OnCornerDeconstruct(_cornerID));

        private void OnVoxelSpawn(IVoxel _voxel)
        {
            m_VoxelCallbacks.Traversal(_p => _p.OnVoxelConstruct(_voxel));
            foreach (var structure in m_Structures)
                    m_BoidsManager.OnModuleConstruct(structure.CollectStructure(_voxel.Identity));
        }

        private void OnVoxelRecycle(PCGID _voxelID)
        {
            foreach (var structure in m_Structures)
                m_BoidsManager.OnModuleDeconstruct(structure.CollectStructure(_voxelID));
            m_VoxelCallbacks.Traversal(_p => _p.OnVoxelDeconstruct(_voxelID));
        }
    }
}
