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
        void OnSelectVertex(ConvexVertex _vertex,byte _height ,bool _construct);
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
        private GridManager m_GridManager;
        private IConvexGridControl[] m_Controls;
        
        public float m_CellRadius = 1;
        private readonly Dictionary<HexCoord, ConvexArea> m_Areas = new Dictionary<HexCoord, ConvexArea>();
        private readonly Dictionary<HexCoord, ConvexVertex> m_Vertices = new Dictionary<HexCoord, ConvexVertex>();
        private readonly Dictionary<HexCoord,ConvexQuad> m_Quads = new Dictionary<HexCoord, ConvexQuad>();
        
        private void OnValidate() => ConvexGridHelper.InitMatrix(transform, m_CellRadius);

        private void Awake()
        {
            OnValidate();
            m_GridGenerator =  GetComponent<GridGenerator>();
            m_CameraControl = new CameraControl(); 
            m_MeshConstructor = new MeshConstructor();
            m_GridManager = GetComponent<GridManager>();
            m_Controls = new IConvexGridControl[]{m_GridGenerator,  m_CameraControl, m_MeshConstructor,m_GridManager};
            m_Controls.Traversal(p=>p.Init(transform));
            UIT_TouchConsole.InitDefaultCommands();
            UIT_TouchConsole.Command("Reset Grid",KeyCode.R).Button(m_GridManager.Clear);
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
                m_Controls.Traversal(p=>p.OnSelectVertex(m_Vertices[tuple.Item1],tuple.Item2,construct));
                return;
            }

            GPlane plane = new GPlane(Vector3.up, transform.position);
            var hitPos = ray.GetPoint(UGeometryVoxel.RayPlaneDistance(plane, ray));
            var hitCoord =  hitPos.ToCoord();
            var hitHex=hitCoord.ToCube();
            if (ValidateGridSelection(hitCoord, out HexCoord selectCoord))
                m_Controls.Traversal(p=>p.OnSelectVertex(m_Vertices[selectCoord],0,construct));
            DoAreaConstruct(UHexagonArea.GetBelongAreaCoord(hitHex));
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
            var area = new ConvexArea(areaCoord);
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
                Gizmos_Extend.DrawLines(quad.Value.m_HexQuad.ConstructIteratorArray(p=>m_Vertices[p].m_Coord.ToPosition()));
        }
#endif
    }
    
}
