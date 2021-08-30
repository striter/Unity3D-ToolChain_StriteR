using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Geometry.Three;
using Geometry.Two;
using OSwizzling;
using UnityEngine;
using Procedural;
using Procedural.Hexagon;
using Procedural.Hexagon.Area;
using Procedural.Hexagon.Geometry;
using TEditor;
using UnityEditor;
using Random = System.Random;

namespace GridTest.HexagonSubdivide
{
    enum EAreaState
    {
        Empty=0,
        Tesselation=1,
        Relaxed=2,
        Meshed=3,
    }
    class Vertex
    {
        public Coord m_Position;
        public readonly List<Quad> m_RelativeQuads=new List<Quad>(6);
    }
    class Area
    {
        public EAreaState m_State { get; private set; }
        public HexagonArea area { get; private set; }
        public Random m_Random { get; private set; }
        public Mesh m_Mesh { get; private set; }

        public readonly List<PHexCube> m_ProceduralVertices = new List<PHexCube>(); 
        public readonly List<HexTriangle> m_ProceduralTriangles = new List<HexTriangle>(); 
        public readonly List<HexQuad> m_ProceduralQuads = new List<HexQuad>();
        public Area(HexagonArea _area)
        {
            m_State = EAreaState.Empty;
            area = _area;
            m_Random= new Random(("Test"+area.coord).GetHashCode());
        }
        public IEnumerator SplitQuads()
        {
            if (m_State >= EAreaState.Tesselation)
                yield break;
            m_State = EAreaState.Tesselation;
            
            var random =  m_Random;
                
            //Generate Vertices
            foreach (var tuple in area.IterateAllCoordsCSRinged(false))
            {
                m_ProceduralVertices.TryAdd(tuple.coord);
                yield return null;
            }

            //Generate Triangles
            foreach (var tuple in area.IterateAllCoordsCSRinged(true))
            {
                var radius = tuple.radius;
                var direction = tuple.dir;
                bool first = tuple.first;

                if (radius == UHexagonArea.radius)
                    break;
    
                var startCoordCS = tuple.coord;
                m_ProceduralVertices.TryAdd(startCoordCS);
                if (radius == 0)
                {
                    var nearbyCoords = UHexagonArea.GetNearbyAreaCoordsCS(startCoordCS);
        
                    for (int i = 0; i < 6; i++)
                    {
                        var coord1 = nearbyCoords[i];
                        var coord2 = nearbyCoords[(i + 1) % 6];
                        m_ProceduralVertices.TryAdd(coord1);
                        m_ProceduralVertices.TryAdd(coord2);
                        m_ProceduralTriangles.Add(new HexTriangle(startCoordCS,coord1,coord2));
                        yield return null;
                    }
                }
                else
                {
                    int begin = first ? 0:1;
                    for (int i = begin; i < 3; i++)
                    {
                        var coord1=UHexagonArea.GetNearbyAreaCoordCS(startCoordCS,direction+3+i);
                        var coord2=UHexagonArea.GetNearbyAreaCoordCS(startCoordCS,direction+4+i);
                        m_ProceduralVertices.TryAdd(coord1);
                        m_ProceduralVertices.TryAdd(coord2);
                        m_ProceduralTriangles.Add(new HexTriangle(startCoordCS,coord1,coord2));
                        yield return null;
                    }
                }
            }
            
            List<HexTriangle> availableTriangles = m_ProceduralTriangles.DeepCopy();
            //Remove Edge Triangles
            foreach (var tuple in area.IterateAreaCoordsCSRinged(area.RadiusAS))
            {
                 if(!tuple.first)
                     continue;
            
                 var triangles = availableTriangles.Collect(p => p.vertices.All(p=>area.InRange(p))&&p.vertices.Contains(tuple.coord)).ToArray();
                 availableTriangles.RemoveRange(triangles);
            }

            //Combine Center Triangles
            while (availableTriangles.Count > 0)
            {
                int validateIndex = availableTriangles.RandomIndex(random);
                var curTriangle = availableTriangles[validateIndex];
                availableTriangles.RemoveAt(validateIndex);
        
                var relativeTriangleIndex = availableTriangles.FindIndex(p => p.MatchVertexCount(curTriangle)==2&&p.vertices.All(p=>area.InRange(p)));
                if(relativeTriangleIndex==-1)
                    continue;
        
                var nextTriangle = availableTriangles[relativeTriangleIndex];
                availableTriangles.RemoveAt(relativeTriangleIndex);

                m_ProceduralQuads.Add(UHexagonGeometry.CombineTriangle(curTriangle, nextTriangle));
                m_ProceduralTriangles.Remove(curTriangle);
                m_ProceduralTriangles.Remove(nextTriangle);
                yield return null;
            }
            
            //Split Quads
            int quadCount = m_ProceduralQuads.Count;
            while (quadCount-->0)
            {
                var splitQuad = m_ProceduralQuads[0];
                m_ProceduralQuads.RemoveAt(0);
            
                var index0 = splitQuad.vertex0;
                var index1 = splitQuad.vertex1;
                var index2 = splitQuad.vertex2;
                var index3 = splitQuad.vertex3;
                var midTuple = splitQuad.GetQuadMidVertices();
                
                var index01 = midTuple.m01;
                var index12 = midTuple.m12;
                var index23 = midTuple.m23;
                var index30 = midTuple.m30;
                var index0123 = midTuple.m0123;
                
                m_ProceduralVertices.TryAdd(index01);
                m_ProceduralVertices.TryAdd(index12);
                m_ProceduralVertices.TryAdd(index23);
                m_ProceduralVertices.TryAdd(index30);
                m_ProceduralVertices.TryAdd(index0123);
                
                m_ProceduralQuads.Add(new HexQuad(index3,index23,index0123,index30));
                yield return null;
                m_ProceduralQuads.Add(new HexQuad(index23,index2,index12,index0123));
                yield return null;
                m_ProceduralQuads.Add(new HexQuad(index30,index0123,index01,index0));
                yield return null;
                m_ProceduralQuads.Add(new HexQuad(index0123,index12,index1,index01));
                yield return null;
            }
            
            //Split Triangles
            while (m_ProceduralTriangles.Count > 0)
            {
                var splitTriangle = m_ProceduralTriangles[0];
                m_ProceduralTriangles.RemoveAt(0);
                var index0 = splitTriangle.vertex0;
                var index1 = splitTriangle.vertex1;
                var index2 = splitTriangle.vertex2;
            
                var midTuple = splitTriangle.GetTriangleMidVertices();
                var index01 = midTuple.m01;
                var index12 = midTuple.m12;
                var index20 = midTuple.m20;
                var index012 = midTuple.m012;
                
                m_ProceduralVertices.TryAdd(index01);
                m_ProceduralVertices.TryAdd(index12);
                m_ProceduralVertices.TryAdd(index20);
                m_ProceduralVertices.TryAdd(index012);
                
                m_ProceduralQuads.Add(new HexQuad(index0,index20,index012,index01));
                yield return null;
                m_ProceduralQuads.Add(new HexQuad(index01,index012,index12,index1));
                yield return null;
                m_ProceduralQuads.Add(new HexQuad(index012,index20,index2,index12));
                yield return null;
            }
        }

        public IEnumerator Relaxed()
        {
            if (m_State >= EAreaState.Relaxed)
                yield break;
            m_State = EAreaState.Relaxed;
            
            m_ProceduralVertices.Clear();
            m_ProceduralTriangles.Clear();
            m_ProceduralQuads.Clear();
        }
    }
    class Quad
    {
        public HexQuad m_HexQuad { get; private set; }
        public G2Quad m_GeometryQuad { get; private set; }
        public Quad(HexQuad _hexQuad,Dictionary<PHexCube,Vertex> _vertices)
        {
            m_HexQuad = _hexQuad;
            m_GeometryQuad=new G2Quad(
                _vertices[m_HexQuad.vertex0].m_Position,
                _vertices[m_HexQuad.vertex1].m_Position,
                _vertices[m_HexQuad.vertex2].m_Position,
                _vertices[m_HexQuad.vertex3].m_Position);
        }
    }
    
    [ExecuteInEditMode]
    public class GridTest_HexagonSubdivide:MonoBehaviour
    {
        public bool m_Flat = false;
        public float m_CellRadius = 1;
        public int m_AreaRadius = 8;
        [Header("Random")]
        public string m_RandomValue="Test";
        [Header("Smoothen")] 
        public int m_SmoothenTimes = 300;
        [Range(0.001f,0.5f)]
        public float m_SmoothenFactor = .1f;
        
        [Header("Iterate")] 
        public int m_IteratePerFrame = 8;
        
        readonly Dictionary<PHexCube, Vertex> m_RelaxVertices = new Dictionary<PHexCube, Vertex>();
        readonly List<HexTriangle> m_RelaxTriangles = new List<HexTriangle>(); 
        readonly List<HexQuad> m_RelaxQuads = new List<HexQuad>();
        
        readonly Dictionary<PHexCube,Area> m_Areas = new Dictionary<PHexCube, Area>();
        readonly Dictionary<PHexCube, Vertex> m_Vertices = new Dictionary<PHexCube, Vertex>();
        readonly List<Quad> m_Quads = new List<Quad>();

        private readonly LinkedList<IEnumerator> m_Iterator = new LinkedList<IEnumerator>();
        private readonly Timer m_IterateTimer = new Timer(1f/60f);

        private int m_QuadSelected=-1;
        private PHexCube m_GridSelected = PHexCube.zero;
        
        void ValidateSelection(Coord _localPos,out int quadIndex,out PHexCube gridIndex)
        {
            quadIndex = -1;
            gridIndex = PHexCube.zero;
            foreach (var tuple in m_Quads.LoopIndex())
            {
                var quad =  tuple.value;
                if (quad.m_GeometryQuad.IsPointInside(_localPos))
                {
                     var sideIndex= quad.m_GeometryQuad.NearestPointIndex(_localPos);
                     gridIndex = quad.m_HexQuad[sideIndex]; 
                     quadIndex= tuple.index;
                     break;
                }
            }
        }

        void Clear()
        {
            m_Iterator.Clear();
            m_Areas.Clear();
            m_Vertices.Clear();
            m_Quads.Clear();
            m_RelaxQuads.Clear();
            m_RelaxTriangles.Clear();
            m_RelaxVertices.Clear();
        }

        private void Tick()
        {
            UHexagon.flat = m_Flat;
            UHexagonArea.Init(m_AreaRadius,6,true);
            m_IterateTimer.Tick(EditorTime.deltaTime);
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
        
        void ValidateArea(PHexCube _positionCS)
        {
            var area = UHexagonArea.GetBelongingArea(_positionCS);
            m_Iterator.AddLast(Validate(area));
            m_Iterator.AddFirst(Relax(area));
        }
        IEnumerator Validate(HexagonArea _destArea)
        {
            var areas = _destArea.IterateNearbyAreas().Extend(_destArea);
            //Validate
            foreach (HexagonArea tuple in areas)
            {
                var aCoord = tuple.coord;
                if(!m_Areas.ContainsKey(aCoord))
                    m_Areas.Add(aCoord,new Area(tuple));
                
                m_Iterator.AddLast(m_Areas[aCoord].SplitQuads());
                yield return null;
            }
        }

        IEnumerator Relax(HexagonArea _destArea)
        {
            var areas = _destArea.IterateNearbyAreas().Extend(_destArea);
            //Push Coords
            foreach (var tuple in areas)
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
                    coord = m_Vertices[p].m_Position;
                m_RelaxVertices.TryAdd(p, new Vertex(){m_Position = coord});
            }
            
            //Relaxing
            Dictionary<PHexCube, Coord> relaxDirections = new Dictionary<PHexCube, Coord>();
            for (int i = 0; i < m_SmoothenTimes; i++)
            {
                relaxDirections.Clear();
                foreach (var quad in m_RelaxQuads)
                { 
                    var origins = quad.GetVertices(p => m_RelaxVertices[p].m_Position);
                    var center = origins.Average((a, b) => a + b, (a, divide) => a / divide);
                    var offsets = origins.Select(p => p - center).ToArray();
            
                    var directions = new Coord[]
                    {
                        offsets[0],
                        UMath.m_Rotate270.Multiply(offsets[1]),
                        UMath.m_Rotate180.Multiply(offsets[2]),
                        UMath.m_Rotate90.Multiply(offsets[3])
                    };
                    
                    var average = Coord.Normalize( directions.Sum((a,b)=>a+b))*UMath.SQRT2*3;
                    directions = new Coord[]
                    {
                        average - offsets[0], 
                        UMath.m_Rotate90.Multiply(average) - offsets[1],
                        UMath.m_Rotate180.Multiply(average) - offsets[2],
                        UMath.m_Rotate270.Multiply(average) - offsets[3]
                    };
            
                    var vertices = quad.vertices;
                    var relaxOffsets =  directions.DeepCopy();
                    for (int j = 0; j < 4; j++)
                        if (m_Vertices.ContainsKey(vertices[j]))
                            for (int k = 0; k < 3; k++)
                                relaxOffsets[(j + k)%4] -= directions[j];
                    for (int j = 0; j < 4; j++)
                        if (m_Vertices.ContainsKey(vertices[j]))
                            relaxOffsets[j]=Coord.zero;
                    
                    foreach (var tuple in vertices.LoopIndex())
                    {
                        if (m_Vertices.ContainsKey(vertices[tuple.index]))
                            continue;
                        relaxDirections.TryAdd(tuple.value, Coord.zero);
                        relaxDirections[tuple.value] += relaxOffsets[tuple.index];
                    }
                }
            
                foreach (var pair in relaxDirections)
                    m_RelaxVertices[pair.Key].m_Position += pair.Value * m_SmoothenFactor;
                yield return null;
            }
            
            //Finalize Result
            foreach (HexQuad hexQuad in m_RelaxQuads)
            {
                if(hexQuad.vertices.Any(p=>!_destArea.InRange(p)))
                    continue;

                //A
                foreach (var vertex in hexQuad.vertices)
                    if(!m_Vertices.ContainsKey(vertex))
                        m_Vertices.Add(vertex,m_RelaxVertices[vertex]);

                var quad = new Quad(hexQuad, m_Vertices);
                m_Quads.Add(quad);
                
                foreach (PHexCube vertex in hexQuad.vertices)
                    m_Vertices[vertex].m_RelativeQuads.Add(quad);
                yield return null;
            }

            //Inform Area
            m_Iterator.AddLast(m_Areas[_destArea.coord].Relaxed());
            m_RelaxTriangles.Clear();
            m_RelaxVertices.Clear();
            m_RelaxQuads.Clear();
            yield return null;
        }
        
#if UNITY_EDITOR
        private void OnValidate() => Clear();
        private void OnEnable()
        {
            if (Application.isPlaying)
                return;
            
            EditorApplication.update += Tick;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            if (Application.isPlaying)
                return;

            EditorApplication.update -= Tick;
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            GRay ray = sceneView.camera.ScreenPointToRay(TEditor.UECommon.GetScreenPoint(sceneView));
            GPlane plane = new GPlane(Vector3.up, transform.position);
            var hitPos = ray.GetPoint(UGeometry.RayPlaneDistance(plane, ray));
            var hitCoord = (transform.InverseTransformPoint(hitPos) / m_CellRadius).ToPixel();
            var hitHex=hitCoord.ToAxial();
            if (Event.current.type == EventType.MouseDown)
                switch (Event.current.button)
                {
                    case 0: ValidateArea(hitHex);  break;
                    case 1: break;
                }

            if (Event.current.type == EventType.KeyDown)
            {
                switch (Event.current.keyCode)
                {
                    case KeyCode.R: Clear(); break;
                }
            }
            ValidateSelection(hitCoord,out m_QuadSelected,out m_GridSelected);
        }
        
        #region Gizmos
        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Scale(Vector3.one*m_CellRadius);
            DrawAreaProcedural();
            DrawRelaxProcedural();
            DrawGrid();
            DrawSelection();
        }
        
        void DrawAreaProcedural()
        {
            Gizmos.color = Color.grey;
            foreach (Area area in m_Areas.Values)
            {
                foreach (var vertex in area.m_ProceduralVertices)
                    Gizmos.DrawSphere(vertex.ToWorld(),.4f);
                Gizmos.color = Color.white.SetAlpha(.2f);
                foreach (var triangle in area.m_ProceduralTriangles)
                    Gizmos_Extend.DrawLines(triangle.GetVertices(p=>p.ToWorld()));
                foreach (var quad in area.m_ProceduralQuads)
                    Gizmos_Extend.DrawLines(quad.GetVertices(p=>p.ToWorld()));
            }
        }

        void DrawRelaxProcedural()
        {
            Gizmos.color = Color.yellow;
            foreach (var vertex in m_RelaxVertices.Values)
                Gizmos.DrawSphere(vertex.m_Position.ToWorld(),.2f);
            foreach (var triangle in m_RelaxTriangles)
                Gizmos_Extend.DrawLines(triangle.GetVertices(p=>m_RelaxVertices[p].m_Position.ToWorld()));
            foreach (var quad in m_RelaxQuads)
                Gizmos_Extend.DrawLines(quad.GetVertices(p=>m_RelaxVertices[p].m_Position.ToWorld()));
        }

        void DrawGrid()
        {
            Gizmos.color = Color.green.SetAlpha(.3f);
            foreach (var vertex in m_Vertices.Values)
                Gizmos.DrawSphere(vertex.m_Position.ToWorld(),.2f);
            foreach (var quad in m_Quads)
                Gizmos_Extend.DrawLines(quad.m_HexQuad.GetVertices(p=>m_Vertices[p].m_Position.ToWorld()));
        }

        void DrawSelection()
        {
            if (m_QuadSelected == -1)
                return;
            Gizmos.color = Color.white.SetAlpha(.3f);
            Gizmos_Extend.DrawLines(m_Quads[m_QuadSelected].m_HexQuad.GetVertices(p=>m_Vertices[p].m_Position.ToWorld()));
            Gizmos.color = Color.cyan;
            var vertex = m_Vertices[m_GridSelected];
            Gizmos.DrawSphere(vertex.m_Position.ToWorld(),.5f);
            Gizmos.color = Color.yellow;
            foreach (var quad in vertex.m_RelativeQuads)
                Gizmos.DrawSphere(((Coord)(quad.m_GeometryQuad.center)).ToWorld(),.3f);
        }
        #endregion
#endif
    }
}