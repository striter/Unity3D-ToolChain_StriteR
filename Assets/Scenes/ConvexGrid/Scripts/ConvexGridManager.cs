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
        private GridGenerator m_GridGenerator;
        private CameraControl m_CameraControl;
        private MeshConstructor m_MeshConstructor;
        private ModuleManager m_ModuleManager;
        private TileManager m_TileManager;
        private IConvexGridControl[] m_Controls;
        
        private readonly Dictionary<HexCoord, ConvexArea> m_Areas = new Dictionary<HexCoord, ConvexArea>();
        private readonly Dictionary<HexCoord, ConvexVertex> m_Vertices = new Dictionary<HexCoord, ConvexVertex>();
        private readonly Dictionary<HexCoord,ConvexQuad> m_Quads = new Dictionary<HexCoord, ConvexQuad>();
        
        private void Awake()
        {
            m_GridGenerator =  GetComponent<GridGenerator>();
            m_CameraControl = new CameraControl(); 
            m_MeshConstructor = new MeshConstructor();
            m_TileManager = GetComponent<TileManager>();
            m_ModuleManager = GetComponent<ModuleManager>();
            m_Controls = new IConvexGridControl[]{m_GridGenerator,  m_CameraControl, m_MeshConstructor,m_TileManager,m_ModuleManager};
            m_Controls.Traversal(p=>p.Init(transform));
            UIT_TouchConsole.InitDefaultCommands();
            UIT_TouchConsole.Command("Reset Grid",KeyCode.R).Button(m_TileManager.Clear);
            UIT_TouchConsole.Command("Reset All",KeyCode.T).Button(Clear);

            DoAreaConstruct(new HexCoord(0, 0, 0));
        }

        void Clear()
        {
            m_Areas.Clear();
            m_Vertices.Clear();
            m_Quads.Clear();
            m_Controls.Traversal(p=>p.Clear());
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
            foreach (var clickPos in touch.ResolveClicks()) 
                Click(clickPos,touch.Count==1);

            int dragCount = touch.Count;
            var drag = touch.CombinedDrag()*deltaTime*5f;
            if (dragCount == 1)
            {
                m_CameraControl.Rotate(drag.y,drag.x);
            }
            else
            {
                var pinch= touch.CombinedPinch()*deltaTime*5f;
                m_CameraControl.Pinch(pinch);
                m_CameraControl.Move(drag);
            }
        }
        
        void Click(Vector2 screenPos,bool construct)
        {
            Ray ray=m_CameraControl.m_Camera.ScreenPointToRay(screenPos);
            if(Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, int.MaxValue))
            {
                var raycast = hit.collider.GetComponent<IGridRaycast>();
                var tuple = construct? raycast.GetNearbyCornerData(ref hit):raycast.GetCornerData();
                DoCornerConstruction(m_Vertices[tuple.Item1],tuple.Item2,construct);
                return;
            }

            GPlane plane = new GPlane(Vector3.up, transform.position);
            var hitPos = ray.GetPoint(UGeometryIntersect.RayPlaneDistance(plane, ray));
            var hitCoord =  hitPos.ToCoord();
            if (ValidateGridSelection(hitCoord, out HexCoord selectCoord))
                DoCornerConstruction(m_Vertices[selectCoord],0,construct);
            
            //DoAreaConstruct(UHexagonArea.GetBelongAreaCoord(hitHex));
        }

        void DoCornerConstruction(ConvexVertex _vertex,byte _height ,bool _construct)
        {
            if(_construct)
                m_TileManager.CornerConstruction(_vertex,_height,m_ModuleManager.SpawnModules);
            else
                m_TileManager.CornerDeconstruction(_vertex,_height,m_ModuleManager.RecycleModules);
            
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

        void DoAreaConstruct(HexCoord _areaCoord)
        {
            if (m_Areas.ContainsKey(_areaCoord))
                return;
            
            m_GridGenerator.ValidateArea(_areaCoord,m_Vertices,OnAreaConstruct);
        }
        
        void OnAreaConstruct(RelaxArea _area)
        {
            var areaCoord = _area.m_Area.m_Coord;
            var area = new ConvexArea(_area.m_Area.m_Coord,_area.m_Vertices[_area.m_Area.centerCS]);
            m_Areas.Add(areaCoord,area);
            //Insert Vertices&Quads
            foreach (var pair in _area.m_Vertices)
            {
                var hex = pair.Key;
                var coord = pair.Value;
                
                m_Vertices.TryAdd(hex, () => new ConvexVertex() {m_Hex = hex,m_Coord = coord});
                area.m_Vertices.Add(m_Vertices[hex]);
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
                    var convexQuad = m_Quads[areaQuad.m_Identity];
                    m_Vertices[areaQuad[i]].AddNearbyQuads(convexQuad);
                }
            }

            _area.CleanData();
            
            m_Controls.Traversal(p=>p.OnAreaConstruct(m_Areas[areaCoord]));
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
