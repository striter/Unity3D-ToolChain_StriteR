using System.Collections.Generic;
using System.Linq;
using Geometry;
using Geometry.Voxel;
using Procedural;
using Procedural.Hexagon;
using Procedural.Hexagon.Area;
using TTouchTracker;
using UnityEngine;

namespace ConvexGrid
{
    public interface IConvexGridControl
    {
        void Init(Transform _transform);
        void Tick(float _deltaTime);
        void OnSelectVertex(ConvexVertex _vertex,byte _height);
        void OnAreaConstruct(ConvexArea _area);
        void Clear();
    }
    
    public interface IGridRaycast
    {
        (HexCoord,byte) GetCornerData();
        (HexCoord,byte) GetNearbyCornerData(ref RaycastHit _hit);
    }
    public class ConvexGridManager : MonoBehaviour
    {
        private CameraManager m_CameraManager;
        private MeshConstructor m_MeshConstructor;
        private ModuleManager m_ModuleManager;
        private TileManager m_TileManager;
        private RenderManager m_RenderManager;
        private IConvexGridControl[] m_Controls;
        
        private readonly Dictionary<HexCoord, ConvexArea> m_Areas = new Dictionary<HexCoord, ConvexArea>();
        private readonly Dictionary<HexCoord, ConvexVertex> m_Vertices = new Dictionary<HexCoord, ConvexVertex>();
        private readonly Dictionary<HexCoord,ConvexQuad> m_Quads = new Dictionary<HexCoord, ConvexQuad>();

        public GridRuntimeData m_GridData;
        private void Awake()
        {
            m_CameraManager = new CameraManager(); 
            m_MeshConstructor = new MeshConstructor();
            m_TileManager = GetComponent<TileManager>();
            m_ModuleManager = GetComponent<ModuleManager>();
            m_RenderManager = GetComponent<RenderManager>();
            m_Controls = new IConvexGridControl[]{ m_CameraManager, m_MeshConstructor,m_TileManager,m_RenderManager,m_ModuleManager};
            m_Controls.Traversal(p=>p.Init(transform));
            UIT_TouchConsole.InitDefaultCommands();
            UIT_TouchConsole.Command("Reset",KeyCode.R).Button(Clear);

            LoadArea(m_GridData);
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
                var area = new ConvexArea(_area.identity);
                m_Areas.Add(areaCoord,area);
                //Insert Vertices&Quads
                foreach (var pair in _area.m_Vertices)
                {
                    var identity = pair.identity;
                    var coord = pair.coord;
                
                    m_Vertices.TryAdd(identity, () => new ConvexVertex() {m_Hex = identity,m_Coord = coord});
                    area.m_Vertices.Add(m_Vertices[identity]);
                }

                foreach (var quad in _area.m_Quads)
                {
                    var convexQuad = new ConvexQuad(quad,m_Vertices);
                    m_Quads.Add(convexQuad.m_Identity,convexQuad);
                    area.m_Quads.Add(convexQuad);
                }

                //Fill Relations
                foreach (var areaQuad in _area.m_Quads)
                {
                    for (int i = 0; i < areaQuad.Length; i++)
                    {
                        var convexQuad = m_Quads[areaQuad.identity];
                        m_Vertices[areaQuad[i]].AddNearbyQuads(convexQuad);
                    }
                }

                m_Controls.Traversal(p=>p.OnAreaConstruct(m_Areas[areaCoord]));
            }
        }
        private void Update()
        {
            InputTick();
            m_Controls.Traversal(p=>p.Tick(Time.deltaTime));
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
        
        void Click(Vector2 screenPos,bool construct)
        {
            Ray ray=m_CameraManager.m_Camera.ScreenPointToRay(screenPos);
            if(Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, int.MaxValue))
            {
                var raycast = hit.collider.GetComponent<IGridRaycast>();
                var tuple = construct? raycast.GetNearbyCornerData(ref hit):raycast.GetCornerData();
                DoSelectVertex(m_Vertices[tuple.Item1],tuple.Item2,construct);
                return;
            }

            GPlane plane = new GPlane(Vector3.up, transform.position);
            var hitPos = ray.GetPoint(UGeometryIntersect.RayPlaneDistance(plane, ray));
            var hitCoord =  hitPos.ToCoord();
            if (ValidateGridSelection(hitCoord, out HexCoord selectCoord))
                DoSelectVertex(m_Vertices[selectCoord],0,construct);
            
            //DoAreaConstruct(UHexagonArea.GetBelongAreaCoord(hitHex));
        }

        void DoSelectVertex(ConvexVertex _vertex,byte _height ,bool _construct)
        {
            if(_construct)
                m_TileManager.CornerConstruction(_vertex,_height,m_ModuleManager.SpawnCorners,m_ModuleManager.SpawnModules);
            else
                m_TileManager.CornerDeconstruction(_vertex,_height,m_ModuleManager.RecycleCorners,m_ModuleManager.RecycleModules);
            
            m_Controls.Traversal(p=>p.OnSelectVertex(_vertex,_height));
            m_ModuleManager.ValidateModules(m_TileManager.CollectAvailableModules(_vertex,_height));
        }

        public bool ValidateGridSelection(Coord _localPos,out HexCoord coord)
        {
            coord=HexCoord.zero;
            var quad= m_Quads.Values.Find(p =>p.m_CoordQuad.IsPointInside(_localPos),out int quadIndex);
            if (quadIndex != -1)
            {
                var quadVertexIndex = quad.m_CoordQuad.NearestPointIndex(_localPos);
                coord=quad.m_HexQuad[quadVertexIndex];
                return true;
            }
            return false;
        }

        
#if UNITY_EDITOR
        public bool m_Gizmos=false;
        private void OnGUI()
        {
            TouchTracker.DrawDebugGUI();
        }

        private void OnDrawGizmos()
        {
            if (!m_Gizmos)
                return;
            Gizmos.color = Color.green.SetAlpha(.3f);
            foreach (var vertex in m_Vertices.Values)
                Gizmos.DrawSphere(vertex.m_Coord.ToPosition(),.2f);
            foreach (var quad in m_Quads)
                Gizmos_Extend.DrawLinesConcat(quad.Value.m_HexQuad.Iterate(p=>m_Vertices[p].m_Coord.ToPosition()));
        }
#endif
    }
    
}
