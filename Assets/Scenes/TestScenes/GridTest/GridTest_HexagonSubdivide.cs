using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Geometry.Three;
using Geometry.Two;
using ObjectPool;
using ObjectPoolStatic;
using UnityEngine;
using Procedural;
using Procedural.Hexagon;
using Procedural.Hexagon.Area;
using Procedural.Hexagon.Geometry;
using Procedural.Hexagon.ConvexGrid;
using TEditor;
using UnityEditor;

namespace GridTest
{
    [ExecuteInEditMode]
    public partial class GridTest_HexagonSubdivide:MonoBehaviour
    {
        public bool m_Flat = false;
        public float m_CellRadius = 1;
        public int m_AreaRadius = 8;
        [Header("Smoothen")] 
        public int m_SmoothenTimes = 300;
        [Range(0.001f,0.5f)]
        public float m_SmoothenFactor = .1f;
        
        [Header("Iterate")] 
        public int m_IteratePerFrame = 8;
        
        readonly Dictionary<PHexCube, Vertex> m_RelaxVertices = new Dictionary<PHexCube, Vertex>();
        readonly List<HexTriangle> m_RelaxTriangles = new List<HexTriangle>(); 
        readonly List<HexQuad> m_RelaxQuads = new List<HexQuad>();
        
        readonly Dictionary<PHexCube,AreaContainer> m_Areas = new Dictionary<PHexCube, AreaContainer>();
        readonly Dictionary<PHexCube, Vertex> m_Vertices = new Dictionary<PHexCube, Vertex>();
        readonly List<Quad> m_Quads = new List<Quad>();

        private readonly LinkedList<IEnumerator> m_Iterator = new LinkedList<IEnumerator>();
        private readonly Timer m_IterateTimer = new Timer(1f/60f);

        private int m_QuadSelected=-1;
        private ValueChecker<PHexCube> m_GridSelected =new ValueChecker<PHexCube>(PHexCube.zero);
        private Matrix4x4 m_TransformMatrix;
        void Setup()
        {
            UHexagon.flat = m_Flat;
            UHexagonArea.Init(m_AreaRadius,6,true);
            m_TransformMatrix = transform.localToWorldMatrix*Matrix4x4.Scale(m_CellRadius*Vector3.one);
        } 
        PHexCube ValidateSelection(Coord _localPos,out int quadIndex)
        {
            quadIndex = -1;
            foreach (var tuple in m_Quads.LoopIndex())
            {
                var quad =  tuple.value;
                if (quad.m_GeometryQuad.IsPointInside(_localPos))
                {
                     var sideIndex= quad.m_GeometryQuad.NearestPointIndex(_localPos);
                     quadIndex= tuple.index;
                     return quad.m_HexQuad[sideIndex]; 
                }
            }
            return PHexCube.zero;
        }
        void Clear()
        {
            m_QuadSelected = -1;
            m_GridSelected.Check(PHexCube.zero);
            m_Iterator.Clear();
            m_Areas.Clear();
            m_Vertices.Clear();
            m_Quads.Clear();
            m_RelaxQuads.Clear();
            m_RelaxTriangles.Clear();
            m_RelaxVertices.Clear();
            m_AreaMeshes?.Clear();
            if(m_Selection!=null)
                m_SelectionMesh.Clear();
        }
        private void Tick(float _deltaTime)
        {
            m_IterateTimer.Tick(_deltaTime);
            if (m_IterateTimer.m_Timing)
                return;
            m_IterateTimer.Replay();
            
            int index = m_IteratePerFrame;
            while (index-- > 0)
            {
                if (m_Iterator.Count==0)
                    return;

                if (!m_Iterator.Last().MoveNext())
                    m_Iterator.RemoveLast();
            }
        }
        
        void ValidateArea(PHexCube _areaCoord)
        {
            m_Iterator.AddLast(Tesselation(_areaCoord));
            m_Iterator.AddFirst(Relax(_areaCoord));
        }
        
        IEnumerator Tesselation(PHexCube _areaCoord)
        {
            foreach (HexagonArea tuple in _areaCoord.GetCoordsInRadius(1).Select(UHexagonArea.GetArea))
            {
                var aCoord = tuple.coord;
                if(!m_Areas.ContainsKey(aCoord))
                    m_Areas.Add(aCoord,new AreaContainer(tuple));
                
                m_Iterator.AddLast(m_Areas[aCoord].SplitQuads());
                yield return null;
            }
        }
        IEnumerator Relax(PHexCube _areaCoord)
        {
            var destArea = m_Areas[_areaCoord];
            if (destArea.m_State >= EAreaState.Relaxed)
                yield break;

            //Push Coords
            foreach (var tuple in  _areaCoord.GetCoordsInRadius(1).Select(UHexagonArea.GetArea))
            {
                var area = m_Areas[tuple.coord];
                if(area.m_State== EAreaState.Relaxed)
                    continue;
                
                foreach (var quad in area.m_ProceduralQuads)
                {
                    m_RelaxQuads.Add(quad);
                    
                    AddCoord(quad.vertex0);
                    AddCoord(quad.vertex1);
                    AddCoord(quad.vertex2);
                    AddCoord(quad.vertex3);
                }

                foreach (var triangle in area.m_ProceduralTriangles)
                {
                    m_RelaxTriangles.Add(triangle);
                    AddCoord(triangle.vertex0);
                    AddCoord(triangle.vertex1);
                    AddCoord(triangle.vertex2);
                }
            }
            
            void AddCoord(PHexCube p)
            {
                var coord = p.ToPixel();
                if (m_Vertices.ContainsKey(p))
                    coord = m_Vertices[p].m_Coord;
                m_RelaxVertices.TryAdd(p, new Vertex(){m_Coord = coord});
            }
            
            //Relaxing
            Dictionary<PHexCube, Coord> relaxDirections = new Dictionary<PHexCube, Coord>();
            Coord[] origins = new Coord[4];
            Coord[] offsets = new Coord[4];
            Coord[] directions = new Coord[4];
            Coord[] relaxOffsets = new Coord[4];
            for (int i = 0; i < m_SmoothenTimes; i++)
            {
                relaxDirections.Clear();
                foreach (var quad in m_RelaxQuads)
                { 
                    quad.Select(p => m_RelaxVertices[p].m_Coord).Fill(origins);
                    var center = origins.Average((a, b) => a + b, (a, divide) => a / divide);
                    origins.Select(p => p - center).Fill(offsets);

                    directions[0] = offsets[0];
                    directions[1] = UMath.m_Rotate270CW.Multiply(offsets[1]);
                    directions[2] = UMath.m_Rotate180CW.Multiply(offsets[2]);
                    directions[3] = UMath.m_Rotate90CW.Multiply(offsets[3]);
                    
                    var average = Coord.Normalize( directions.Sum((a,b)=>a+b))*UMath.SQRT2*3;

                    directions[0] = average - offsets[0];
                    directions[1] = UMath.m_Rotate90CW.Multiply(average) - offsets[1];
                    directions[2] = UMath.m_Rotate180CW.Multiply(average) - offsets[2];
                    directions[3] = UMath.m_Rotate270CW.Multiply(average) - offsets[3];
            
                    relaxOffsets =  directions.MemberCopy(relaxOffsets);
                    for (int j = 0; j < 4; j++)
                        if (m_Vertices.ContainsKey(quad[j]))
                            for (int k = 0; k < 3; k++)
                                relaxOffsets[(j + k)%4] -= directions[j];
                    for (int j = 0; j < 4; j++)
                        if (m_Vertices.ContainsKey(quad[j]))
                            relaxOffsets[j]=Coord.zero;
                    
                    foreach (var tuple in quad.LoopIndex())
                    {
                        if (m_Vertices.ContainsKey(quad[tuple.index]))
                            continue;
                        relaxDirections.TryAdd(tuple.value, Coord.zero);
                        relaxDirections[tuple.value] += relaxOffsets[tuple.index];
                    }
                }
            
                foreach (var pair in relaxDirections)
                    m_RelaxVertices[pair.Key].m_Coord += pair.Value * m_SmoothenFactor;
                yield return null;
            }
            
            //Finalize Result
            foreach (HexQuad hexQuad in m_RelaxQuads)
            {
                if(hexQuad.Any(p=>!destArea.m_Area.InRange(p)))
                    continue;

                foreach (var vertex in hexQuad)
                    if(!m_Vertices.ContainsKey(vertex))
                        m_Vertices.Add(vertex,m_RelaxVertices[vertex]);

                var quad = new Quad(hexQuad, m_Vertices);
                m_Quads.Add(quad);
                destArea.m_Quads.Add(quad);

                foreach (PHexCube vertex in hexQuad)
                    m_Vertices[vertex].m_RelativeQuads.Add(quad);
                yield return null;
            }

            //Inform Area
            m_Iterator.AddLast(m_Areas[_areaCoord].Relaxed());
            m_RelaxTriangles.Clear();
            m_RelaxVertices.Clear();
            m_RelaxQuads.Clear();
            yield return null;
        }
    }

    //Runtime
    public partial class GridTest_HexagonSubdivide
    {
        private TObjectPoolClass<AreaMeshContainer> m_AreaMeshes;
        private Camera m_Camera;
        private Transform m_Selection;
        private Mesh m_SelectionMesh;
        
        private class AreaMeshContainer : ITransform
        {
            public Transform iTransform { get; }
            public MeshFilter m_MeshFilter { get; }
            public MeshRenderer m_MeshRenderer { get; }

            public AreaMeshContainer(Transform _transform)
            {
                iTransform = _transform;
                m_MeshFilter = _transform.GetComponent<MeshFilter>();
                m_MeshRenderer = _transform.GetComponent<MeshRenderer>();
            }

            public void ApplyMesh(Mesh _sharedMesh)
            {
                m_MeshFilter.sharedMesh = _sharedMesh;
            }
        }
        private void Awake()
        {
            if (!Application.isPlaying)
                return;
            Setup();
            m_Camera = transform.Find("Camera").GetComponent<Camera>();
            m_AreaMeshes = new TObjectPoolClass<AreaMeshContainer>(transform.Find("AreaContainer/AreaMesh"));
            m_Selection = transform.Find("Selection");
            m_SelectionMesh = new Mesh {name="Selection",hideFlags = HideFlags.HideAndDontSave};
            m_Selection.GetComponent<MeshFilter>().sharedMesh = m_SelectionMesh;
        }

        void Update()
        {
            if (!Application.isPlaying)
                return;
            Tick(Time.deltaTime);
            InputTick();
        }

        void InputTick()
        {
            GRay ray = m_Camera.ScreenPointToRay(Input.mousePosition);
            GPlane plane = new GPlane(Vector3.up, transform.position);
            var hitPos = ray.GetPoint(UGeometry.RayPlaneDistance(plane, ray));
            var hitCoord = (transform.InverseTransformPoint(hitPos) / m_CellRadius).ToPixel();
            var hitHex=hitCoord.ToAxial();
            var hitArea = UHexagonArea.GetBelongAreaCoord(hitHex);
            if (m_GridSelected.Check(ValidateSelection(hitCoord, out m_QuadSelected)))
                PopulateSelectionMesh();
            
            if (Input.GetMouseButton(0))
            {
                ValidateArea(hitArea);
                m_Iterator.AddFirst(PopulateGridMesh(hitArea));
            }
            
            if(Input.GetKeyDown(KeyCode.R))
                Clear();
        }

        void PopulateSelectionMesh()
        {
            bool valid = m_QuadSelected != -1;
            m_Selection.SetActive(valid);
            if (!valid)
                return;

            List<Vector3> vertices = TSPoolList<Vector3>.Spawn();
            List<int> indices = TSPoolList<int>.Spawn();
            List<Vector2> uvs = TSPoolList<Vector2>.Spawn();
            
            var selectVertex = m_Vertices[m_GridSelected];
            foreach (Quad quad in selectVertex.m_RelativeQuads)
            {
                int startIndex = vertices.Count;
                int[] indexes = {startIndex, startIndex + 1, startIndex + 2, startIndex + 3};

                var hexQuad = quad.m_HexQuad;
                var hexVertices = hexQuad;
                var offset = hexVertices.FindIndex(p=>p==m_GridSelected);
                for (int i = 0; i < 4; i++)
                {
                    int index=(i+offset)%4;
                    vertices.Add( m_TransformMatrix.MultiplyPoint( m_Vertices[hexQuad[index]].m_Coord.ToWorld()));
                    uvs.Add(URender.IndexToQuadUV(i));
                }

                indices.Add(indexes[0]);
                indices.Add(indexes[1]);
                indices.Add(indexes[3]);
                indices.Add(indexes[1]);
                indices.Add(indexes[2]);
                indices.Add(indexes[3]);
            }
            
            m_SelectionMesh.Clear();
            m_SelectionMesh.SetVertices(vertices);
            m_SelectionMesh.SetUVs(0,uvs);
            m_SelectionMesh.SetIndices(indices,MeshTopology.Triangles,0,false);
            
            TSPoolList<Vector3>.Recycle(vertices);
            TSPoolList<Vector2>.Recycle(uvs);
            TSPoolList<int>.Recycle(indices);
        }

        IEnumerator PopulateGridMesh(PHexCube _areaCoord)
        {
            if (!m_Areas.ContainsKey(_areaCoord))
                yield break;
            var area = m_Areas[_areaCoord];
            if (area.m_State >= EAreaState.Meshed)
                yield break;
            area.SetupMesh(area.m_Quads,m_Vertices,m_TransformMatrix);
            m_AreaMeshes.AddItem().ApplyMesh(area.m_Mesh);
            yield return null;
        }
    }
    
    // Editor
#if UNITY_EDITOR
    public partial class GridTest_HexagonSubdivide
    {
        private void OnValidate()
        {
            Setup();
            Clear();
        }
        private void OnEnable()
        {
            if (Application.isPlaying)
                return;
            
            Setup();
            EditorApplication.update += EditorTick;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            if (Application.isPlaying)
                return;

            Clear();
            EditorApplication.update -= EditorTick;
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        void EditorTick() => Tick(EditorTime.deltaTime);

        private void OnSceneGUI(SceneView sceneView)
        {
            GRay ray = sceneView.camera.ScreenPointToRay(sceneView.GetScreenPoint());
            GPlane plane = new GPlane(Vector3.up, transform.position);
            var hitPos = ray.GetPoint(UGeometry.RayPlaneDistance(plane, ray));
            var hitCoord = (transform.InverseTransformPoint(hitPos) / m_CellRadius).ToPixel();
            var hitHex=hitCoord.ToAxial();
            var hitArea = UHexagonArea.GetBelongAreaCoord(hitHex);
            if (Event.current.type == EventType.MouseDown)
                switch (Event.current.button)
                {
                    case 0: ValidateArea(hitArea);  break;
                    case 1: break;
                }

            if (Event.current.type == EventType.KeyDown)
            {
                switch (Event.current.keyCode)
                {
                    case KeyCode.R: Clear(); break;
                }
            }
            m_GridSelected.Check( ValidateSelection(hitCoord,out m_QuadSelected));
        }
        
        #region Gizmos
        private void OnDrawGizmos()
        {
            Gizmos.matrix = m_TransformMatrix;
            DrawAreaProcedural();
            DrawRelaxProcedural();
            DrawGrid();
            DrawSelection();
        }
        
        void DrawAreaProcedural()
        {
            foreach (AreaContainer area in m_Areas.Values)
            {
                Gizmos.color = Color.white.SetAlpha(.5f);
                foreach (var vertex in area.m_ProceduralVertices)
                    Gizmos.DrawSphere(vertex.ToWorld(),.3f);
                Gizmos.color = Color.white.SetAlpha(.2f);
                foreach (var triangle in area.m_ProceduralTriangles)
                    Gizmos_Extend.DrawLines(triangle.ConstructIteratorArray(p=>p.ToWorld(),3));
                foreach (var quad in area.m_ProceduralQuads)
                    Gizmos_Extend.DrawLines(quad.ConstructIteratorArray(p=>p.ToWorld(),4));
            }
        }

        void DrawRelaxProcedural()
        {
            Gizmos.color = Color.yellow;
            foreach (var vertex in m_RelaxVertices.Values)
                Gizmos.DrawSphere(vertex.m_Coord.ToWorld(),.2f);
            foreach (var triangle in m_RelaxTriangles)
                Gizmos_Extend.DrawLines(triangle.ConstructIteratorArray(p=>m_RelaxVertices[p].m_Coord.ToWorld(),3));
            foreach (var quad in m_RelaxQuads)
                Gizmos_Extend.DrawLines(quad.ConstructIteratorArray(p=>m_RelaxVertices[p].m_Coord.ToWorld(),4));
        }

        void DrawGrid()
        {
            Gizmos.color = Color.green.SetAlpha(.3f);
            foreach (var vertex in m_Vertices.Values)
                Gizmos.DrawSphere(vertex.m_Coord.ToWorld(),.2f);
            foreach (var quad in m_Quads)
                Gizmos_Extend.DrawLines(quad.m_HexQuad.ConstructIteratorArray(p=>m_Vertices[p].m_Coord.ToWorld(),4));
        }

        void DrawSelection()
        {
            if (m_QuadSelected == -1)
                return;
            Gizmos.color = Color.white.SetAlpha(.3f);
            Gizmos_Extend.DrawLines(m_Quads[m_QuadSelected].m_HexQuad.ConstructIteratorArray(p=>m_Vertices[p].m_Coord.ToWorld(),4));
            Gizmos.color = Color.cyan;
            var vertex = m_Vertices[m_GridSelected];
            Gizmos.DrawSphere(vertex.m_Coord.ToWorld(),.5f);
            Gizmos.color = Color.yellow;
            foreach (var quad in vertex.m_RelativeQuads)
                Gizmos.DrawSphere(((Coord)(quad.m_GeometryQuad.center)).ToWorld(),.3f);
        }
        #endregion
    }
#endif
}