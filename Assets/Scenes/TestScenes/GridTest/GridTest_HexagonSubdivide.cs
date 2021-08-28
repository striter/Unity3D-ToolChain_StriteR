using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Procedural;
using Procedural.Hexagon;
using Procedural.Hexagon.Area;
using Procedural.Hexagon.Geometry;
using TEditor;
using UnityEditor;

namespace GridTest.HexagonSubdivide
{
    class Vertex
    {
        public Coord position;
    }
    #if UNITY_EDITOR
    [ExecuteInEditMode]
    public class GridTest_HexagonSubdivide:MonoBehaviour
    {
        public bool m_Flat = false;
        public float m_CellRadius = 1;
        public int m_AreaRadius = 8;
        public int m_MaxAreaRadius = 4;
        [Header("Random")]
        public string m_RandomValue="Test";
        [Header("Smoothen")] 
        public int m_SmoothenTimes = 300;
        [Range(0.001f,0.5f)]
        public float m_SmoothenFactor = .1f;
        
        [Header("Iterate")] 
        public int m_IteratePerFrame = 8;
        
        [NonSerialized] private readonly Dictionary<PHexCube,HexagonArea> m_Areas = new Dictionary<PHexCube, HexagonArea>();
        [NonSerialized] private readonly List<HexQuad> m_Quads = new List<HexQuad>();
        [NonSerialized] private readonly Dictionary<PHexCube, Vertex> m_Vertices = new Dictionary<PHexCube, Vertex>();
        
        [NonSerialized] private readonly Dictionary<PHexCube, Vertex> m_ProceduralVertices=new Dictionary<PHexCube, Vertex>();
        [NonSerialized] private readonly List<HexTriangle> m_ProceduralTriangles = new List<HexTriangle>();
        [NonSerialized] private readonly List<HexQuad> m_ProceduralQuads = new List<HexQuad>();

        private readonly Queue<IEnumerator> m_AreaIterate=new Queue<IEnumerator>();
        private readonly Timer m_IterateTimer = new Timer(1f/60f);

        private void OnValidate() => Clear();
        private void OnEnable()
        {
            EditorApplication.update += Tick;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            EditorApplication.update -= Tick;
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            GRay ray = sceneView.camera.ScreenPointToRay(TEditor.UECommon.GetScreenPoint(sceneView));
            GPlane plane = new GPlane(Vector3.up, transform.position);
            var hitPoint = ray.GetPoint(UGeometry.RayPlaneDistance(plane, ray));
            var hitPointCS=(transform.InverseTransformPoint(hitPoint) / m_CellRadius).ToPixel().ToAxial();
            if (Event.current.type == EventType.MouseDown)
                switch (Event.current.button)
                {
                    case 0:
                        ValidateArea(hitPointCS);
                        break;
                    case 1: break;
                }

            if (Event.current.type == EventType.KeyDown)
            {
                switch (Event.current.keyCode)
                {
                    case KeyCode.R: Clear(); break;
                }
            }
        }
        void ValidateArea(PHexCube _positionCS)
        {
            var area = UHexagonArea.GetBelongingArea(_positionCS);
            if (!area.coord.InRange(m_MaxAreaRadius))
                return;
            if (m_Areas.ContainsKey(area.coord))
                return;
                
            m_Areas.Add(area.coord,area);
            m_AreaIterate.Enqueue(Generate(area));
        }


        void Clear()
        {
            m_AreaIterate.Clear();
            m_Areas.Clear();
            m_Vertices.Clear();
            m_Quads.Clear();
            
            m_ProceduralVertices.Clear();
            m_ProceduralTriangles.Clear();
            m_ProceduralQuads.Clear();
        }

        private void Tick()
        {
            m_IterateTimer.Tick(EditorTime.deltaTime);
            if (m_IterateTimer.m_Timing)
                return;
            m_IterateTimer.Replay();
            
            int index = m_IteratePerFrame;
            while (index-- > 0)
            {
                if (m_AreaIterate.Count==0)
                    return;

                if (!m_AreaIterate.First().MoveNext())
                    m_AreaIterate.Dequeue();
            }

        }
        
        private void OnDrawGizmos()
        {
            UHexagon.flat = m_Flat;
            UHexagonArea.Init(m_AreaRadius,6,true);
            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Scale(Vector3.one*m_CellRadius);
            DrawProcedural();
            DrawAxis();
        }
        
        IEnumerator Generate(HexagonArea _destArea)
        {
            void AddCoord(PHexCube p)
            {
                var coord = p.ToPixel();
                if (m_Vertices.ContainsKey(p))
                    coord = m_Vertices[p].position;
                m_ProceduralVertices.TryAdd(p, new Vertex(){position = coord});
            }

            var area = _destArea;
                //Generate Vertices
                foreach (var tuple in area.IterateAllCoordsCSRinged(false))
                {
                    AddCoord(tuple.coord);
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
                    AddCoord(startCoordCS);
                    if (radius == 0)
                    {
                        var nearbyCoords = UHexagonArea.GetCoordsNearbyCS(startCoordCS);
                
                        for (int i = 0; i < 6; i++)
                        {
                            var coord1 = nearbyCoords[i];
                            var coord2 = nearbyCoords[(i + 1) % 6];
                            AddCoord(coord1);
                            AddCoord(coord2);
                            m_ProceduralTriangles.Add(new HexTriangle(startCoordCS,coord1,coord2));
                            yield return null;
                        }
                    }
                    else
                    {
                        direction += 3;
                        int begin = first ? 0:1;
                        for (int i = begin; i < 3; i++)
                        {
                            var coord1=UHexagonArea.GetCoordsNearby(startCoordCS,direction+i);
                            var coord2=UHexagonArea.GetCoordsNearby(startCoordCS,direction+i+1);
                            AddCoord(coord1);
                            AddCoord(coord2);
                            m_ProceduralTriangles.Add(new HexTriangle(startCoordCS,coord1,coord2));
                            yield return null;
                        }
                    }
                }

            //Combine Triangles
            System.Random random = new System.Random(m_RandomValue.GetHashCode());
            List<HexTriangle> availableTriangles = m_ProceduralTriangles.DeepCopy();
            int maxValidateCount = 1024;
            while (availableTriangles.Count > 0)
            {
                if (maxValidateCount-- <= 0)
                    throw new Exception("Validate Failed!");
                
                int validateIndex = availableTriangles.RandomIndex(random);
                var curTriangle = availableTriangles[validateIndex];
                availableTriangles.RemoveAt(validateIndex);
            
                var relativeTriangleIndex = availableTriangles.FindIndex(p => p.MatchVertexCount(curTriangle)==2);
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
                
                AddCoord(index01);
                AddCoord(index12);
                AddCoord(index23);
                AddCoord(index30);
                AddCoord(index0123);
                
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
                
                AddCoord(index01);
                AddCoord(index12);
                AddCoord(index20);
                AddCoord(index012);
                
                m_ProceduralQuads.Add(new HexQuad(index0,index20,index012,index01));
                yield return null;
                m_ProceduralQuads.Add(new HexQuad(index01,index012,index12,index1));
                yield return null;
                m_ProceduralQuads.Add(new HexQuad(index012,index20,index2,index12));
                yield return null;
            }

            //Relaxing
            Dictionary<PHexCube, Coord> directions = new Dictionary<PHexCube, Coord>();
            for (int i = 0; i < m_SmoothenTimes; i++)
            {
                directions.Clear();
                // var quad = m_Quads.Find();
                // var quad = m_Quads[0];
                foreach (var quad in m_ProceduralQuads)
                { 
                    var origins = quad.GetVertices(p => m_ProceduralVertices[p].position);
                    var center = origins.Average((a, b) => a + b, (a, divide) => a / divide);
                    var offsets = origins.Select(p => p - center).ToArray();
            
                    Coord direction0 = offsets[0];
                    Coord direction1 = UMath.m_Rotate270.Multiply(offsets[1]);
                    Coord direction2 = UMath.m_Rotate180.Multiply(offsets[2]);
                    Coord direction3 = UMath.m_Rotate90.Multiply(offsets[3]);
                    
                    var average = Coord.Normalize( direction0 + direction1 + direction2 + direction3)*UMath.SQRT2*3;
                    direction0 = average- offsets[0];
                    direction1 = UMath.m_Rotate90.Multiply(average)-offsets[1];
                    direction2 = UMath.m_Rotate180.Multiply(average)-offsets[2];
                    direction3 = UMath.m_Rotate270.Multiply(average)-offsets[3];
            
                    var vertices = quad.vertices;
                    directions.TryAdd(vertices[0], Coord.zero);
                    directions.TryAdd(vertices[1], Coord.zero);
                    directions.TryAdd(vertices[2], Coord.zero);
                    directions.TryAdd(vertices[3], Coord.zero);
            
                    directions[vertices[0]] += direction0;
                    directions[vertices[1]] += direction1;
                    directions[vertices[2]] += direction2;
                    directions[vertices[3]] += direction3;
                }
            
                foreach (var pair in directions)
                    m_ProceduralVertices[pair.Key].position += pair.Value * m_SmoothenFactor;
                yield return null;
            }


            int passIndex = m_ProceduralQuads.Count;
            int curIndex = 0;
            while (passIndex-- > 0)
            {
                var quad = m_ProceduralQuads[curIndex];
                if (quad.vertices.Any(p => _destArea.InRange(p)))
                {
                    m_Quads.Add(quad);
                    foreach (var vertex in quad.vertices)
                        if(!m_Vertices.ContainsKey(vertex))
                            m_Vertices.Add(vertex,m_ProceduralVertices[vertex]);
                    curIndex++;
                    continue;
                }

                m_ProceduralQuads.RemoveAt(curIndex);
                yield return null;
            }

            m_ProceduralTriangles.Clear();
            m_ProceduralVertices.Clear();
            m_ProceduralQuads.Clear();
        }
        
        void DrawProcedural()
        {
            Gizmos.color = Color.yellow;
            foreach (var vertex in m_ProceduralVertices.Values)
                Gizmos.DrawSphere(vertex.position.ToWorld(),.2f);
            Gizmos.color = Color.cyan;
            foreach (var area in m_Areas)
                foreach (var coord in UHexagonArea.IterateAreaCoordsCS(area.Value))
                    if(m_ProceduralVertices.ContainsKey(coord))
                        Gizmos.DrawSphere(m_ProceduralVertices[coord].position.ToWorld(),.4f);
            Gizmos.color = Color.white.SetAlpha(.2f);
            foreach (var triangle in m_ProceduralTriangles)
                Gizmos_Extend.DrawLines(triangle.GetVertices(p=>m_ProceduralVertices[p].position.ToWorld()));
            foreach (var quad in m_ProceduralQuads)
                Gizmos_Extend.DrawLines(quad.GetVertices(p=>m_ProceduralVertices[p].position.ToWorld()));
        }

        void DrawAxis()
        {
            Gizmos.color = Color.green;
            foreach (var vertex in m_Vertices.Values)
                Gizmos.DrawSphere(vertex.position.ToWorld(),.2f);
            Gizmos.color = Color.green.SetAlpha(.5f);
            foreach (HexQuad square in m_Quads)
                Gizmos_Extend.DrawLines(square.GetVertices(p=>m_Vertices[p].position.ToWorld()));
        }
    }
    #endif
}