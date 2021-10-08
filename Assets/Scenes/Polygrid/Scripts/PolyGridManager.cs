using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LinqExtension;
using PolyGrid.Module;
using PolyGrid.Tile;
using Procedural;
using Procedural.Hexagon;
using TTouchTracker;
using UnityEngine;
using TDataPersistent;
namespace PolyGrid
{
    [Serializable]
    public struct CornerPersistent
    {
        public PileID identity;
        public EModuleType type;
    }
    public class PersistentData:CDataSave<PersistentData>
    {
        public override bool DataCrypt() => false;
        public List<CornerPersistent> m_CornerData=new List<CornerPersistent>();

        public void Record(IEnumerable<CornerPersistent> _corners)
        {
            m_CornerData.Clear();
            m_CornerData.AddRange(_corners);
        }
    }
    public class PolyGridManager : MonoBehaviour
    {
        public GridRuntimeData m_GridData;
        private readonly PersistentData m_PersistentData=new PersistentData();
        
        private TileManager m_TileManager;
        private SelectionManager m_SelectionManager;
        private ModuleManager m_ModuleManager;
        private CameraManager m_CameraManager;
        private MeshConstructor m_MeshConstructor;
        private RenderManager m_RenderManager;
        
        private IPolyGridControl[] m_Controls;
        private IPolyGridVertexCallback[] m_VertexCallbacks;
        private IPolyGridQuadCallback[] m_QuadCallbacks;
        private IPolyGridCornerCallback[] m_CornerCallbacks;
        private IPolyGridVoxelCallback[] m_VoxelCallbacks;
        private IPolyGridModifyCallback[] m_ModifyCallbacks;
        
        private readonly Dictionary<HexCoord, PolyArea> m_Areas = new Dictionary<HexCoord, PolyArea>();
        private readonly Dictionary<HexCoord, PolyVertex> m_Vertices = new Dictionary<HexCoord, PolyVertex>();
        private readonly Dictionary<HexCoord,PolyQuad> m_Quads = new Dictionary<HexCoord, PolyQuad>();
        private void Awake()
        {
            m_TileManager = GetComponent<TileManager>();
            m_ModuleManager = GetComponent<ModuleManager>();
            m_CameraManager = new CameraManager();
            m_MeshConstructor = GetComponent<MeshConstructor>();
            m_SelectionManager = new SelectionManager();
            m_RenderManager = GetComponent<RenderManager>();
            m_Controls = new IPolyGridControl[]{ m_TileManager,m_SelectionManager,m_ModuleManager,m_CameraManager,m_MeshConstructor,m_RenderManager};
            m_Controls.Traversal(p=>p.Init(transform));

            m_VertexCallbacks = m_Controls.Collect<IPolyGridControl, IPolyGridVertexCallback>().ToArray();
            m_QuadCallbacks = m_Controls.Collect<IPolyGridControl, IPolyGridQuadCallback>().ToArray();
            m_CornerCallbacks = m_Controls.Collect<IPolyGridControl, IPolyGridCornerCallback>().ToArray();
            m_VoxelCallbacks = m_Controls.Collect<IPolyGridControl, IPolyGridVoxelCallback>().ToArray();
            m_ModifyCallbacks=m_Controls.Collect<IPolyGridControl, IPolyGridModifyCallback>().ToArray();
            
            UIT_TouchConsole.InitDefaultCommands();
            UIT_TouchConsole.Command("Reset",KeyCode.R).Button(Clear);

            LoadArea(m_GridData);
            this.StartCoroutine(Generate());
        }

        IEnumerator Generate()
        {
            m_PersistentData.ReadPersistentData();
            m_PersistentData.m_CornerData.Sort((a,b)=>(a.identity.height-b.identity.height));
            foreach (var cornerData in m_PersistentData.m_CornerData)
            {
                m_ModuleManager.m_SpawnModule = cornerData.type;
                DoCornerConstruction(cornerData.identity,true);
                yield return new WaitForSeconds(.1f);
            }
        }

        void Clear()
        {
            m_Areas.Clear();
            m_Vertices.Clear();
            m_Quads.Clear();
            m_Controls.Traversal(p=>p.Clear());
            LoadArea(m_GridData);
        }

        void LoadArea(GridRuntimeData _data)
        {
            foreach (var _area in _data.areaData)
            {
                var areaCoord = _area.identity.coord;
                var area = new PolyArea(_area.identity);
                m_Areas.Add(areaCoord,area);
                //Insert Vertices&Quads
                foreach (var data in _area.m_Vertices)
                {
                    var identity = data.identity;
                    var coord = data.coord*KPolyGrid.tileSize;
                
                    m_Vertices.TryAdd(identity, () => new PolyVertex() {m_Identity = identity,m_Coord = coord,m_Invalid = data.invalid});
                    area.m_Vertices.Add(m_Vertices[identity]);
                }

                foreach (var quad in _area.m_Quads)
                {
                    var polyQuad = new PolyQuad(quad,m_Vertices);
                    m_Quads.Add(polyQuad.m_Identity,polyQuad);
                    area.m_Quads.Add(polyQuad);
                }

                //Fill Relations
                foreach (var areaQuad in _area.m_Quads)
                {
                    for (int i = 0; i < areaQuad.Length; i++)
                    {
                        var polyQuad = m_Quads[areaQuad.identity];
                        m_Vertices[areaQuad[i]].AddNearbyQuads(polyQuad);
                    }
                }

                m_MeshConstructor.ConstructArea(m_Areas[areaCoord]);
            }
        }
        private void Update()
        {
            InputTick();
            m_Controls.Traversal(p=>p.Tick(Time.deltaTime));
            // #if UNITY_EDITOR
            // EditorTick();
            // #endif
        }
        
        void InputTick()
        {
            float deltaTime = Time.unscaledDeltaTime;
            var touch=TouchTracker.Execute(deltaTime);
            foreach (var clickPos in touch.ResolveClicks(.2f)) 
                Click(clickPos,touch.Count==1);

            int dragCount = touch.Count;
            var drag = touch.CombinedDrag()*deltaTime*5f;
            if (dragCount == 1)
            {
                m_CameraManager.Rotate(drag.y,drag.x);
            }
            else
            {
                var pinch= touch.CombinedPinch()*deltaTime*5f;
                m_CameraManager.Pinch(pinch);
                m_CameraManager.Move(drag);
            }
        }
        
        void Click(Vector2 _screenPos,bool _construct)
        {
            var ray = m_CameraManager.m_Camera.ScreenPointToRay(_screenPos);
            PileID selection=default;
            if (_construct&&!m_SelectionManager.VerifyConstruction(ray, m_Quads.Values, out selection))
                return;
            if(!_construct&&!m_SelectionManager.VerifyDeconstruction(ray, out selection))
                return;
            
            DoCornerConstruction(selection, _construct);
            m_PersistentData.Record(m_ModuleManager.CollectAllCornerData());
            m_PersistentData.SavePersistentData();
        }

        bool DoCornerConstruction(PileID _selection,bool _construct)
        {
            var vertex = m_Vertices[_selection.location];
            if (vertex.m_Invalid)
                return false;

            if(_construct)
                m_TileManager.CornerConstruction(vertex,_selection.height,OnVertexSpawn,OnQuadSpawn,OnCornerSpawn,OnVoxelSpawn);
            else
                m_TileManager.CornerDeconstruction(vertex,_selection.height,OnVertexRecycle,OnQuadRecycle,OnCornerRecycle,OnVoxelRecycle);
            m_ModifyCallbacks.Traversal(p=>p.OnVertexModify(vertex,_selection.height,_construct));
            return true;
        }

        void OnVertexSpawn(PolyVertex _vertex) => m_VertexCallbacks.Traversal(p => p.OnPopulateVertex(_vertex));
        void OnVertexRecycle(HexCoord _vertexID) => m_VertexCallbacks.Traversal(p => p.OnDeconstructVertex(_vertexID));
        void OnQuadSpawn(PolyQuad _quad) => m_QuadCallbacks.Traversal(p => p.OnPopulateQuad(_quad));
        void OnQuadRecycle(HexCoord _quadID) => m_QuadCallbacks.Traversal(p => p.OnDeconstructQuad(_quadID));
        void OnCornerSpawn(ICorner _corner)=>m_CornerCallbacks.Traversal(p=>p.OnPopulateCorner(_corner));
        void OnCornerRecycle(PileID _cornerID)=>m_CornerCallbacks.Traversal(p=>p.OnDeconstructCorner(_cornerID));
        void OnVoxelSpawn(IVoxel _voxel)=>m_VoxelCallbacks.Traversal(p=>p.OnPopulateVoxel(_voxel));
        void OnVoxelRecycle(PileID _voxelID)=>m_VoxelCallbacks.Traversal(p=>p.OnDeconstructVoxel(_voxelID));

        
#if UNITY_EDITOR
        private void EditorTick()
        {
            var ray = m_CameraManager.m_Camera.ScreenPointToRay(Input.mousePosition);
            if (!m_SelectionManager.VerifyConstruction(ray, m_Quads.Values, out var _selection))
                return;
            m_MeshConstructor.ConstructCornerMarkup(m_Vertices[_selection.location],_selection.height);
        }
        
        #region Gizmos
        public bool m_Gizmos;
        private void OnGUI()
        {
            TouchTracker.DrawDebugGUI();
        }

        private void OnDrawGizmos()
        {
            if (!m_Gizmos)
                return;
            foreach (var vertex in m_Vertices.Values)
            {
                Gizmos.color = vertex.m_Invalid?Color.red:Color.green;
                Gizmos.DrawSphere(vertex.m_Coord.ToPosition(),.2f);
            }
            Gizmos.color = Color.white.SetAlpha(.3f);
            foreach (var quad in m_Quads)
                Gizmos_Extend.DrawLinesConcat(quad.Value.m_HexQuad.Iterate(p=>m_Vertices[p].m_Coord.ToPosition()));
        }
        #endregion
#endif
    }
    
}
