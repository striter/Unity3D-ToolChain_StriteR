using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry;
using Runtime.Geometry.Extension;
using Procedural;
using Procedural.Hexagon;
using Procedural.Hexagon.Area;
using Procedural.Hexagon.Geometry;
using Runtime.Random;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.Extensions;
using UnityEngine;
using Gizmos = UnityEngine.Gizmos;
using Random = System.Random;

namespace TechToys.ThePlanet.Grid
{
    public enum EConvexIterate
    {
        Empty = 0,
        Tesselation = 1,
        Relaxed = 2,
    }

    [Serializable]
    public class DisorderedHexagonGridGenerator : IGridGenerator
    {
        public Transform transform { get; set; }

        private readonly Dictionary<EConvexIterate, Stack<IEnumerator>> m_ConvexIterator =
            new Dictionary<EConvexIterate, Stack<IEnumerator>>()
            {
                {EConvexIterate.Tesselation, new Stack<IEnumerator>()},
                {EConvexIterate.Relaxed, new Stack<IEnumerator>()},
            };

        public bool m_Flat = false;
        public int m_AreaRadius = 8;

        [Header("Smoothen")] public int m_SmoothenTimes = 300;
        [Range(0.001f, 0.5f)] public float m_SmoothenFactor = .1f;

        [Header("Iterate")] public int m_IteratePerFrame = 8;

        private Counter m_IterateCounter = new Counter(1f / 60f);
        private readonly Dictionary<HexCoord, RelaxArea> m_Chunks = new Dictionary<HexCoord, RelaxArea>();
        public readonly Dictionary<HexCoord, Coord> m_ExistVertices = new Dictionary<HexCoord, Coord>();

        public void Setup()
        {
            UProcedural.InitMatrix(transform.localToWorldMatrix, transform.worldToLocalMatrix, 1f / 6f * kmath.kSQRT2);
            UHexagon.flat = m_Flat;
            UHexagonArea.Init(m_AreaRadius, 6, true);
        }

        public void Tick(float _deltaTime)
        {
            m_IterateCounter.Tick(_deltaTime);
            if (m_IterateCounter.Playing)
                return;
            m_IterateCounter.Replay();

            int index = m_IteratePerFrame;
            while (index-- > 0)
            {
                EConvexIterate curState = UEnum.GetEnums<EConvexIterate>()
                    .Find(p => m_ConvexIterator.ContainsKey(p) && m_ConvexIterator[p].Count > 0);
                if (curState == 0)
                    break;

                if (!m_ConvexIterator[curState].First().MoveNext())
                    m_ConvexIterator[curState].Pop();
            }
        }

        public void Clear()
        {
            m_ExistVertices.Clear();
            m_Chunks.Clear();
            foreach (var key in m_ConvexIterator.Keys)
                m_ConvexIterator[key].Clear();
        }

        public void OnSceneGUI(SceneView _sceneView)
        {
            GRay ray = _sceneView.camera.ScreenPointToRay(_sceneView.GetScreenPoint());
            GPlane plane = new GPlane(Vector3.up, transform.position);
            ray.IntersectPoint(plane,out var hitPos);
            var hitCoord = ((Vector3)hitPos).ToCoord();
            var hitHex = hitCoord.ToCube();
            var hitArea = UHexagonArea.GetBelongAreaCoord(hitHex);
            if (Event.current.type == EventType.MouseDown)
                switch (Event.current.button)
                {
                    case 0:
                        ValidateArea(hitArea);
                        break;
                    case 1: break;
                }

            if (Event.current.type == EventType.KeyDown)
            {
                switch (Event.current.keyCode)
                {
                    case KeyCode.R:
                    {
                        Clear();
                        break;
                    }

                }
            }
        }

        public void OnGizmos()
        {
            foreach (RelaxArea area in m_Chunks.Values)
                area.DrawGizmos();
        }

        public void Output(GridCollection _collection)
        {
            //Check Invalid Quads
            List<HexQuad> quads = new List<HexQuad>();
            foreach (var area in m_Chunks.Values)
                quads.AddRange(area.m_Quads);

            List<HexCoord> invalidCoords = new List<HexCoord>();
            foreach (var quad in quads)
            {
                if (quads.Count(p => p.MatchVertexCount(quad) == 2) == 4)
                    continue;
                invalidCoords.TryAddRange(quad);
            }

            Dictionary<HexCoord, GridVertexData> vertices = new Dictionary<HexCoord, GridVertexData>();
            Dictionary<HexCoord, int> vertexHelper = new Dictionary<HexCoord, int>();
            List<GridChunkData> chunks = new List<GridChunkData>();

            foreach (var area in m_Chunks.Values)
            {
                if (area.m_State != EConvexIterate.Relaxed)
                    continue;

                GridChunkData chunkData = new GridChunkData  {
                    quads = new GridQuadData[area.m_Quads.Count],
                };

                foreach (var (vertexID, vertexCoord) in area.m_Vertices)
                {
                    if(vertices.ContainsKey(vertexID))
                        continue;

                    int gridVertexID = vertices.Count;
                    vertices.Add(vertexID, new GridVertexData() {
                        position = (vertexCoord ).ToPosition(), 
                        normal = Vector3.up,
                        invalid = invalidCoords.Contains(vertexID)
                    });
                    vertexHelper.Add(vertexID,gridVertexID);
                }

                int quadIndex = 0;
                foreach (var quad in area.m_Quads)
                {
                    var gridQuad = new PQuad(quad.Convert(p => vertexHelper[p] ));
                    chunkData.quads[quadIndex++] = new GridQuadData() {vertices = gridQuad};
                }
                chunks.Add(chunkData);
            }

            _collection.vertices = vertices.Values.ToArray();
            _collection.chunks = chunks.ToArray();
        }

        IEnumerator TessellateArea(HexCoord _areaCoord)
        {
            if(!m_Chunks.ContainsKey(_areaCoord))
                m_Chunks.Add(_areaCoord,new RelaxArea(UHexagonArea.GetArea(_areaCoord)));

            var area = m_Chunks[_areaCoord];
            if (area.m_State >= EConvexIterate.Tesselation)
                yield break;
            
            var iterator = area.Tesselation();
            while (iterator.MoveNext())
                yield return null;
        }

        IEnumerator RelaxArea(HexCoord _areaCoord,Dictionary<HexCoord,Coord> _vertices,Action<RelaxArea> _onAreaFinish)
        {
            if (!m_Chunks.ContainsKey(_areaCoord))
                yield break;
            var area = m_Chunks[_areaCoord];
            if (area.m_State >= EConvexIterate.Relaxed)
                yield break;
            
            var iterator = area.Relax(m_Chunks,_vertices,m_SmoothenTimes,m_SmoothenFactor);
            while (iterator.MoveNext())
                yield return null;
            
            _onAreaFinish(area);
        }

        public void ValidateArea(HexCoord _areaCoord)
        {
            foreach (HexagonArea tuple in _areaCoord.GetCoordsInRadius(1).Select(UHexagonArea.GetArea))
                m_ConvexIterator[EConvexIterate.Tesselation].Push(TessellateArea(tuple.coord));
            m_ConvexIterator[EConvexIterate.Relaxed].Push(RelaxArea(_areaCoord,m_ExistVertices,area =>
            {
                Debug.Log($"Area{area.m_Area.coord} Constructed!");
                foreach (var pair in area.m_Vertices)
                    m_ExistVertices.TryAdd(pair.Key,pair.Value);
            }));
        }

    }

    public class RelaxArea
    {
        public HexagonArea m_Area { get; }
        public IRandomGenerator m_Random { get; }
        public EConvexIterate m_State { get; private set; }
        public readonly List<HexQuad> m_Quads = new List<HexQuad>();
        public readonly Dictionary<HexCoord, Coord> m_Vertices = new Dictionary<HexCoord, Coord>();
        
        public readonly List<HexCoord> m_ProceduralVertices = new List<HexCoord>(); 
        public readonly List<HexTriangle> m_ProceduralTriangles = new List<HexTriangle>(); 
        public readonly List<HexQuad> m_ProceduralQuads = new List<HexQuad>();
        
        private readonly Dictionary<HexCoord, Coord> m_RelaxVertices = new Dictionary<HexCoord, Coord>();
        private readonly List<HexQuad> m_RelaxQuads = new List<HexQuad>();
        public RelaxArea(HexagonArea _area)
        {
            m_Area = _area;
            m_Random= new LCGRandom(("Test"+m_Area.coord).GetHashCode());
            m_State = EConvexIterate.Empty;
        }
        public IEnumerator Tesselation()
        {
            if (m_State != EConvexIterate.Empty)
                yield break;
            m_State += 1;
            
            //Generate Vertices
            foreach (var tuple in m_Area.IterateAllCoordsCSRinged(false))
            {
                m_ProceduralVertices.TryAdd(tuple.coord);
                yield return null;
            }

            //Generate Triangles
            foreach (var tuple in m_Area.IterateAllCoordsCSRinged(true))
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
            
            //Combine Triangles
            List<HexTriangle> availableTriangles = m_ProceduralTriangles.DeepCopyInstance();
            while (availableTriangles.Count > 0)
            {
                int validateIndex = availableTriangles.RandomIndex(m_Random);
                var curTriangle = availableTriangles[validateIndex];
                availableTriangles.RemoveAt(validateIndex);

                int relativeTriangleIndex = -1;
                int availableLength = availableTriangles.Count;
                for(int i=0;i<availableLength;i++)
                {
                    var triangle = availableTriangles[i];
                    if(triangle.MatchVertexCount(curTriangle)!=2)
                        continue;
                    relativeTriangleIndex = i;
                }
                
                if(relativeTriangleIndex==-1)
                    continue;
        
                var nextTriangle = availableTriangles[relativeTriangleIndex];
                availableTriangles.RemoveAt(relativeTriangleIndex);

                m_ProceduralQuads.Add(new HexQuad(curTriangle.CombineTriangle(nextTriangle)));
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

                foreach (var tuple in splitQuad.SplitToQuads(false))
                {
                    m_ProceduralVertices.TryAdd(tuple.vB);
                    m_ProceduralVertices.TryAdd(tuple.vL);
                    m_ProceduralVertices.TryAdd(tuple.vF);
                    m_ProceduralVertices.TryAdd(tuple.vR);
                    m_ProceduralQuads.Add(new HexQuad(tuple));
                }
                yield return null;
            }
            
            //Split Triangles
            while (m_ProceduralTriangles.Count > 0)
            {
                var splitTriangle = m_ProceduralTriangles[0];
                m_ProceduralTriangles.RemoveAt(0);

                foreach (var quad in splitTriangle.SplitToQuads())
                {
                    m_ProceduralVertices.TryAdd(quad[0]);
                    m_ProceduralVertices.TryAdd(quad[1]);
                    m_ProceduralVertices.TryAdd(quad[2]);
                    m_ProceduralVertices.TryAdd(quad[3]);
                    m_ProceduralQuads.Add(new HexQuad(quad));
                    yield return null;
                }
            }
            m_ProceduralTriangles.Clear();
        }

        public IEnumerator Relax(Dictionary<HexCoord,RelaxArea> _areas,Dictionary<HexCoord,Coord> _existVertices,int _smoothenTimes,float _smoothenFactor)
        {
            if (m_State != EConvexIterate.Tesselation)
                yield break;

            //Push Coords
            void AddCoord(HexCoord p)
            {
                var coord = p.ToCoord();
                if (_existVertices.ContainsKey(p))
                    coord = _existVertices[p];
                m_RelaxVertices.TryAdd(p, coord);
            }
            
            var areaCoord = m_Area.coord;
            foreach (var tuple in  areaCoord.GetCoordsInRadius(1).Select(UHexagonArea.GetArea))
            {
                var nearbyArea = _areas[tuple.coord];
                if (nearbyArea.m_Area.coord!=m_Area.coord&&nearbyArea.m_State>EConvexIterate.Tesselation)
                    continue;

                foreach (var quad in nearbyArea.m_ProceduralQuads)
                {
                    m_RelaxQuads.Add(quad);
                    AddCoord(quad[0]);
                    AddCoord(quad[1]);
                    AddCoord(quad[2]);
                    AddCoord(quad[3]);
                }
            }
            
            //Relaxing
            Coord[] origins = new Coord[4];
            Coord[] offsets = new Coord[4];
            Coord[] directions = new Coord[4];
            Coord[] relaxOffsets = new Coord[4];
            int count = _smoothenTimes;
            while (count-->1)
            {
                foreach (var quad in m_RelaxQuads)
                { 
                    //Get Offsets & Center
                    for (int i= 0; i < 4; i++)
                        origins[i] = m_RelaxVertices[quad[i]];
                    var center = origins.Average((a, b) => a + b, (a, divide) => a / divide);
                    for (int i = 0; i < 4; i++)
                        offsets[i] = origins[i]- center;

                    //Rotate To Sample Direction
                    directions[0] = offsets[0];
                    directions[1] = KRotation.kRotateCW270.mul(offsets[1]);
                    directions[2] = KRotation.kRotateCW180.mul(offsets[2]);
                    directions[3] = KRotation.kRotateCW90.mul(offsets[3]);
                    
                    var average = Coord.Normalize( directions.Sum((a,b)=>a+b))*kmath.kSQRT2*3;
                    
                    //Rotate back
                    directions[0] = average - offsets[0];
                    directions[1] = (Coord)KRotation.kRotateCW90.mul(average) - offsets[1];
                    directions[2] = (Coord)KRotation.kRotateCW180.mul(average) - offsets[2];
                    directions[3] = (Coord)KRotation.kRotateCW270.mul(average) - offsets[3];
                    
                    //Inform Relaxing
                    relaxOffsets =  directions.MemberCopy(relaxOffsets);
                    for (int j = 0; j < 4; j++)
                        if (_existVertices.ContainsKey(quad[j]))
                            for (int k = 0; k < 3; k++)
                                relaxOffsets[(j + k)%4] -= directions[j];
                    for (int j = 0; j < 4; j++)
                        if (_existVertices.ContainsKey(quad[j]))
                            relaxOffsets[j]=Coord.zero;

                    for (int i = 0; i < 4; i++)
                    {
                        var value = quad[i];
                        if (_existVertices.ContainsKey(value))
                            continue;
                        m_RelaxVertices[value] += relaxOffsets[i] * _smoothenFactor;
                    }
                }
                yield return null;
            }
            
            //Finalize Result
            foreach (HexQuad hexQuad in m_RelaxQuads)
            {
                if(hexQuad.Any(p=>!m_Area.InRange(p)))
                    continue;

                m_Quads.Add(hexQuad);
                hexQuad.Traversal(vertex => m_Vertices.TryAdd(vertex, m_RelaxVertices[vertex]));
                yield return null;
            }

            m_ProceduralVertices.Clear();
            m_ProceduralQuads.Clear();
            m_RelaxQuads.Clear();
            m_RelaxVertices.Clear();
            m_State += 1;
            yield return null;
        }

        #if UNITY_EDITOR
        public void DrawGizmos()
        {
            Gizmos.color = Color.white.SetA(.5f);
            foreach (var vertex in m_ProceduralVertices)
                Gizmos.DrawSphere(vertex.ToPosition(),.3f);
            Gizmos.color = Color.white.SetA(.2f);
            foreach (var triangle in m_ProceduralTriangles)
                UGizmos.DrawLinesConcat(triangle.Iterate(p=>p.ToPosition()));
            foreach (var quad in m_ProceduralQuads)
                UGizmos.DrawLinesConcat(quad.Iterate(p=>p.ToPosition()));
            
            Gizmos.color = Color.yellow;
            foreach (var vertex in m_RelaxVertices.Values)
                Gizmos.DrawSphere(vertex.ToPosition(),.2f);
            foreach (var quad in m_RelaxQuads)
                UGizmos.DrawLinesConcat(quad.Iterate(p=>m_RelaxVertices[p].ToPosition()));
            
            Gizmos.color = Color.green;
            foreach (var vertex in m_Vertices.Values)
                Gizmos.DrawSphere(vertex.ToPosition(),.2f);
            foreach (var quad in m_Quads)
                UGizmos.DrawLinesConcat(quad.Iterate(p=>m_Vertices[p].ToPosition()));
        }
        #endif
    }

    public static class Extension
    {
        public static float3 ToPosition(this HexCoord _hexCube)
        {
            return _hexCube.ToCoord().ToPosition();
        }
    }
}