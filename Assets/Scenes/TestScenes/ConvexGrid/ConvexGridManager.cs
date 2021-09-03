using System.Collections.Generic;
using Geometry;
using Geometry.Voxel;
using Procedural;
using Procedural.Hexagon;
using Procedural.Hexagon.Area;
using TTouchTracker;
using UnityEngine;

namespace  ConvexGrid
{
    public interface IConvexGridControl
    {
        void Init(Transform _transform);
        void Tick(float _deltaTime);
        void Select(bool _valid,HexCoord _coord,ConvexVertex _vertex);
        void Clear();
    }

    public class ConvexGridManager : MonoBehaviour
    {
        private GridGenerator m_GridGenerator;
        private CameraControl m_CameraControl;
        private MeshConstructor m_MeshConstructor;
        private IConvexGridControl[] m_Controls;
        
        public float m_CellRadius = 1;
        private readonly ValueChecker<HexCoord> m_GridSelected =new ValueChecker<HexCoord>(HexCoord.zero);
        private readonly Dictionary<HexCoord, ConvexVertex> m_Vertices = new Dictionary<HexCoord, ConvexVertex>();

        private readonly List<ConvexQuad> m_Quads = new List<ConvexQuad>();

        private void OnValidate() => ConvexGridHelper.InitMatrix(transform, m_CellRadius);

        private void Awake()
        {
            OnValidate();
            m_GridGenerator =  GetComponent<GridGenerator>();
            m_CameraControl = new CameraControl(); 
            m_MeshConstructor = new MeshConstructor();
            m_Controls = new IConvexGridControl[]{m_GridGenerator,  m_CameraControl, m_MeshConstructor};
            m_Controls.Traversal(p=>p.Init(transform));
            UIT_TouchConsole.InitDefaultCommands();
            UIT_TouchConsole.Command("Reset",KeyCode.R).Button(Clear);
        }

        void Clear()
        {
            m_GridSelected.Check(HexCoord.zero);
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
                Click(clickPos);

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
        
        void Click(Vector2 screenPos)
        {
            GRay ray = m_CameraControl.m_Camera.ScreenPointToRay(screenPos);
            GPlane plane = new GPlane(Vector3.up, transform.position);
            var hitPos = ray.GetPoint(UGeometryVoxel.RayPlaneDistance(plane, ray));
            var hitCoord =  hitPos.ToCoord();
            var hitHex=hitCoord.ToCube();
            var hitArea = UHexagonArea.GetBelongAreaCoord(hitHex);

            if (m_GridSelected.Check(ValidateSelection(hitCoord, out bool valid,out int quadIndex)))
                m_Controls.Traversal(p=>p.Select(valid,m_GridSelected,m_Vertices[m_GridSelected]));
            m_GridGenerator.ValidateArea(hitArea,m_Vertices,OnAreaConstruct);
        }

        public HexCoord ValidateSelection(Coord _localPos,out bool valid,out int quadIndex)
        {
            valid = false;
            var quad= m_Quads.Find(p =>p.m_CoordQuad.IsPointInside(_localPos),out quadIndex);
            if (quadIndex != -1)
            {
                valid = true;
                var quadVertexIndex = quad.m_CoordQuad.NearestPointIndex(_localPos);
                return quad.m_HexQuad[quadVertexIndex];
            }
            return HexCoord.zero;
        }
        void OnAreaConstruct(ConvexArea _area)
        {
            foreach (var quad in _area.m_Quads)
            {
                for (int i = 0; i < quad.Length; i++)
                {
                    var vertex = quad[i];
                    m_Vertices.TryAdd(vertex, () => new ConvexVertex() {m_Coord = _area.m_Vertices[vertex]});
                }
                var convexQuad = new ConvexQuad(quad,m_Vertices);
                m_Quads.Add(convexQuad);
                for (int i = 0; i < quad.Length; i++)
                    m_Vertices[ quad[i]].m_RelativeQuads.Add(convexQuad);
            }
            m_MeshConstructor.ConstructArea(_area,m_Vertices);   
            _area.CleanData();
        }
        
#if UNITY_EDITOR
        private void OnGUI()
        {
            TouchTracker.DrawDebugGUI();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green.SetAlpha(.3f);
            foreach (var vertex in m_Vertices.Values)
                Gizmos.DrawSphere(vertex.m_Coord.ToWorld(),.2f);
            foreach (var quad in m_Quads)
                Gizmos_Extend.DrawLines(quad.m_HexQuad.ConstructIteratorArray(p=>m_Vertices[p].m_Coord.ToWorld()));
        }
#endif
    }
    
}
