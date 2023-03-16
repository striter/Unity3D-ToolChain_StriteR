using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TechToys.ThePlanet.Module.BOIDS;
using TechToys.ThePlanet.Module.Cluster;
using TechToys.ThePlanet.Module.Path;
using TechToys.ThePlanet.Module.Prop;
using UnityEngine;

namespace TechToys.ThePlanet.Module
{
    public class ModuleManager : MonoBehaviour,IPCGControl
    {
        [ColorUsage(false,true)]public Color[] m_EmissionColors;
        [Header("Wave Function Collapse")]
        public bool m_ForceIteration = false;
        [MFoldout(nameof(m_ForceIteration),false)] [Clamp(1,1024)]public int m_IterationPerFrame =1;
        
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
        private IModuleStructure[] m_StructureManagers;
        
        private IModuleCollapse[] m_ModuleCollapses;
        private EModuleCollapseStatus m_CollapseStatus;
        
        private readonly Dictionary<int, IModuleStructureElement> m_Structures = new Dictionary<int, IModuleStructureElement>();
        private readonly List<PCGID> m_DirtyCorners = new List<PCGID>();
        
        private readonly Dictionary<string, int> m_ModuleIndexer = new Dictionary<string, int>();
        
        public void Init()
        {
            m_GridManager = transform.Find("Grid").GetComponent<ModuleGridManager>();
            m_ClusterManger = transform.Find("Cluster").GetComponent<ModuleClusterManager>();
            m_PropManager = transform.Find("Prop").GetComponent<ModulePropManager>();
            m_PathManager = transform.Find("Path").GetComponent<ModulePathManager>();
            m_BoidsManager = transform.Find("Boids").GetComponent<ModuleBoidsManager>();
            
            m_Controls = new IModuleControl[] { m_GridManager , m_ClusterManger, m_PropManager , m_PathManager,m_BoidsManager,transform.Find("Static").GetComponent<ModuleStatic>()  };
            m_VertexCallbacks = m_Controls.CollectAs<IModuleControl, IModuleVertexCallback>().ToArray();
            m_QuadCallbacks = m_Controls.CollectAs<IModuleControl, IModuleQuadCallback>().ToArray();
            m_CornerCallbacks = m_Controls.CollectAs<IModuleControl, IModuleCornerCallback>().ToArray();
            m_VoxelCallbacks = m_Controls.CollectAs<IModuleControl, IModuleVoxelCallback>().ToArray();
            m_StructureManagers = m_Controls.CollectAs<IModuleControl, IModuleStructure>().ToArray();
            m_ModuleCollapses = m_Controls.CollectAs<IModuleControl, IModuleCollapse>().ToArray();
            
            m_Controls.Traversal(_p => _p.Init());
        }
        
        public ModuleManager Setup(ModuleCollection _collection, GridManager _grid)
        {
            Clear();
            DModule.Collection = _collection;
            DModule.EmissionColors = m_EmissionColors;
            
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
            m_Structures.Clear();
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
                if (m_GridManager.ConstructRaycast(_ray, out selection))
                {
                    Construct(moduleType,selection);
                    PCGAudios.Play(KPCGAudios.kClick);
                }
                return;
            }

            if (!m_GridManager.DeconstructRaycast(_ray, out selection))
                return;
            Deconstruct(selection);
            PCGAudios.Play(KPCGAudios.kClick2);
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
            int iterationTimes = m_ForceIteration ? kMaxIterationTimes : m_IterationPerFrame;
            while (iterationTimes-- > 0)
                TickCollapse(_deltaTime);

            m_Controls.Traversal(_p => _p.Tick(_deltaTime));
        }

        public void TickEnvironment(float _deltaTime, Vector3 _lightDir)
        {
            m_Structures.Values.Traversal(p=>p.TickLighting(_deltaTime,_lightDir));
        }
        
        private void OnVertexSpawn(IVertex _vertex) => m_VertexCallbacks.Traversal(_p => _p.OnPopulateVertex(_vertex));
        private void OnVertexRecycle(GridID _vertexID) => m_VertexCallbacks.Traversal(_p => _p.OnDeconstructVertex(_vertexID));
        private void OnQuadSpawn(IQuad _quad) => m_QuadCallbacks.Traversal(_p => _p.OnPopulateQuad(_quad));
        private void OnQuadRecycle(GridID _quadID) => m_QuadCallbacks.Traversal(_p => _p.OnDeconstructQuad(_quadID));
        private void OnCornerSpawn(ICorner _corner) => m_CornerCallbacks.Traversal(_p => _p.OnCornerConstruct(_corner));
        private void OnCornerRecycle(PCGID _cornerID) => m_CornerCallbacks.Traversal(_p => _p.OnCornerDeconstruct(_cornerID));

        private void OnVoxelSpawn(IVoxel _voxel)
        {
            m_VoxelCallbacks.Traversal(_p => _p.OnVoxelConstruct(_voxel));
            foreach (var structureManager in m_StructureManagers)
            {
                var structure = structureManager.CollectStructure(_voxel.Identity);
                m_Structures.Add(structure.Identity,structure);
                m_BoidsManager.OnModuleConstruct(structure);
            }
        }

        private void OnVoxelRecycle(PCGID _voxelID)
        {
            foreach (var structureManager in m_StructureManagers)
            {
                var structure = structureManager.CollectStructure(_voxelID);
                m_Structures.Remove(structure.Identity);
                m_BoidsManager.OnModuleDeconstruct(structure);
            }
            m_VoxelCallbacks.Traversal(_p => _p.OnVoxelDeconstruct(_voxelID));
        }
    }
}
