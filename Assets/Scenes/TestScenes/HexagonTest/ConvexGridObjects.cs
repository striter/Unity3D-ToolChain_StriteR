using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Geometry.Two;
using GridTest;
using ObjectPoolStatic;
using Procedural;
using Procedural.Hexagon;
using UnityEngine;
using Procedural.Hexagon.Area;
using Procedural.Hexagon.Geometry;
using Random = System.Random;

namespace ConvexGrid
{
    enum EConvexIterate
    {
        Empty=0,
        Tesselation=1,
        Relaxed=2,
        Meshed=3,
    }
    class ConvexVertex
    {
        public Coord m_Coord;
        public readonly List<ConvexQuad> m_RelativeQuads=new List<ConvexQuad>(6);
    }
    class ConvexQuad
    {
        public HexQuad m_HexQuad { get; private set; }
        public G2Quad m_GeometryQuad { get; private set; }
        public ConvexQuad(HexQuad _hexQuad,Dictionary<HexagonCoordC,ConvexVertex> _vertices)
        {
            m_HexQuad = _hexQuad;
            m_GeometryQuad=new G2Quad(
                _vertices[m_HexQuad.vertex0].m_Coord,
                _vertices[m_HexQuad.vertex1].m_Coord,
                _vertices[m_HexQuad.vertex2].m_Coord,
                _vertices[m_HexQuad.vertex3].m_Coord);
        }
    }
    class ConvexArea
    {
        public HexagonArea m_Area { get; }
        public Random m_Random { get; }
        public EConvexIterate m_State { get; private set; }
        public readonly List<ConvexQuad> m_Quads = new List<ConvexQuad>();
        
        public readonly List<HexagonCoordC> m_ProceduralVertices = new List<HexagonCoordC>(); 
        public readonly List<HexTriangle> m_ProceduralTriangles = new List<HexTriangle>(); 
        public readonly List<HexQuad> m_ProceduralQuads = new List<HexQuad>();
        
        private readonly Dictionary<HexagonCoordC, ConvexVertex> m_RelaxVertices = new Dictionary<HexagonCoordC, ConvexVertex>();
        private readonly List<HexTriangle> m_RelaxTriangles = new List<HexTriangle>(); 
        private readonly List<HexQuad> m_RelaxQuads = new List<HexQuad>();
        public ConvexArea(HexagonArea _area)
        {
            m_Area = _area;
            m_Random= new Random(("Test"+m_Area.m_Coord).GetHashCode());
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
            
            List<HexTriangle> availableTriangles = m_ProceduralTriangles.DeepCopy();
            HexTriangle[] edgeTriangles = new HexTriangle[availableTriangles.Count];
            
            foreach (var tuple in m_Area.IterateAreaCoordsCSRinged(m_Area.RadiusAS))
            {
                if(!tuple.first)
                    continue;

                int length = -1;
                foreach (var p in availableTriangles)
                {
                    if(! p.All(k=>m_Area.InRange(k))&&p.Contains(tuple.coord))
                        continue;
                    length++;
                    edgeTriangles[length] = p;
                }
                
                availableTriangles.RemoveRange(edgeTriangles, length);
            }


            //Combine Center Triangles
            while (availableTriangles.Count > 0)
            {
                int validateIndex = availableTriangles.RandomIndex(m_Random);
                var curTriangle = availableTriangles[validateIndex];
                availableTriangles.RemoveAt(validateIndex);
        
                var relativeTriangleIndex = availableTriangles.FindIndex(p => p.MatchVertexCount( curTriangle)==2&&p.All(p=>m_Area.InRange(p)));
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
                
                m_ProceduralQuads.Add(new HexQuad(index0,index01,index0123,index30));
                yield return null;
                m_ProceduralQuads.Add(new HexQuad(index01,index1,index12,index0123));
                yield return null;
                m_ProceduralQuads.Add(new HexQuad(index30,index0123,index23,index3));
                yield return null;
                m_ProceduralQuads.Add(new HexQuad(index0123,index12,index2,index23));
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
                
                m_ProceduralQuads.Add(new HexQuad(index0,index01,index012,index20));
                yield return null;
                m_ProceduralQuads.Add(new HexQuad(index01,index1,index12,index012));
                yield return null;
                m_ProceduralQuads.Add(new HexQuad(index20,index012,index12,index2));
                yield return null;
            }
        }

        public IEnumerator Relax(Dictionary<HexagonCoordC,ConvexArea> _areas,Dictionary<HexagonCoordC,ConvexVertex> _existVertices,List<ConvexQuad> _existQuad,int _smoothenTimes,float _smoothenFactor)
        {
            if (m_State != EConvexIterate.Tesselation)
                yield break;

            //Push Coords
            void AddCoord(HexagonCoordC p)
            {
                var coord = p.ToPixel();
                if (_existVertices.ContainsKey(p))
                    coord = _existVertices[p].m_Coord;
                m_RelaxVertices.TryAdd(p, new ConvexVertex(){m_Coord = coord});
            }
            
            var areaCoord = m_Area.m_Coord;
            foreach (var tuple in  areaCoord.GetCoordsInRadius(1).Select(UHexagonArea.GetArea))
            {
                var nearbyArea = _areas[tuple.m_Coord];
                if (nearbyArea.m_State>EConvexIterate.Tesselation)
                    continue;

                foreach (var quad in nearbyArea.m_ProceduralQuads)
                {
                    m_RelaxQuads.Add(quad);
                    AddCoord(quad.vertex0);
                    AddCoord(quad.vertex1);
                    AddCoord(quad.vertex2);
                    AddCoord(quad.vertex3);
                }

                foreach (var triangle in nearbyArea.m_ProceduralTriangles)
                {
                    m_RelaxTriangles.Add(triangle);
                    AddCoord(triangle.vertex0);
                    AddCoord(triangle.vertex1);
                    AddCoord(triangle.vertex2);
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
                        origins[i] = m_RelaxVertices[quad.GetElement(i)].m_Coord;
                    var center = origins.Average((a, b) => a + b, (a, divide) => a / divide);
                    for (int i = 0; i < 4; i++)
                        offsets[i] = origins[i]- center;

                    //Rotate To Sample Direction
                    directions[0] = offsets[0];
                    directions[1] = UMath.m_Rotate270CW.Multiply(offsets[1]);
                    directions[2] = UMath.m_Rotate180CW.Multiply(offsets[2]);
                    directions[3] = UMath.m_Rotate90CW.Multiply(offsets[3]);
                    
                    var average = Coord.Normalize( directions.Sum((a,b)=>a+b))*UMath.SQRT2*3;
                    
                    //Rotate back
                    directions[0] = average - offsets[0];
                    directions[1] = UMath.m_Rotate90CW.Multiply(average) - offsets[1];
                    directions[2] = UMath.m_Rotate180CW.Multiply(average) - offsets[2];
                    directions[3] = UMath.m_Rotate270CW.Multiply(average) - offsets[3];
                    
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
                        m_RelaxVertices[value].m_Coord += relaxOffsets[i] * _smoothenFactor;
                    }
                }
                yield return null;
            }
            
            //Finalize Result
            foreach (HexQuad hexQuad in m_RelaxQuads)
            {
                if(hexQuad.Any(p=>!m_Area.InRange(p)))
                    continue;

                hexQuad.Traversal(vertex => _existVertices.TryAdd(vertex, m_RelaxVertices[vertex]));

                var quad = new ConvexQuad(hexQuad, _existVertices);
                m_Quads.Add(quad);
                
                _existQuad.Add(quad);

                hexQuad.Traversal(vertex =>_existVertices[vertex].m_RelativeQuads.Add(quad));
                yield return null;
            }

            m_RelaxQuads.Clear();
            m_RelaxTriangles.Clear();
            m_RelaxVertices.Clear();
            m_ProceduralVertices.Clear();
            m_ProceduralTriangles.Clear();
            m_ProceduralQuads.Clear();
            m_State += 1;
            yield return null;
        }

        public Mesh SetupMesh(Dictionary<HexagonCoordC,ConvexVertex> _vertices,Matrix4x4 _matrix)
        {
            if (m_State != EConvexIterate.Relaxed)
                return null;
            m_State += 1;
            var m_Mesh= new Mesh{hideFlags = HideFlags.HideAndDontSave};
            m_Mesh.name = m_Area.m_Coord.ToString();
            List<Vector3> vertices = TSPoolList<Vector3>.Spawn(); 
            List<Vector2> uvs = TSPoolList<Vector2>.Spawn(); 
            List<int> indices = TSPoolList<int>.Spawn();

            foreach (ConvexQuad quad in m_Quads)
            {
                var hexagonQuad = quad.m_HexQuad;
                int index0 = vertices.Count;
                int index1 = index0 + 1;
                int index2 = index0 + 2;
                int index3 = index0 + 3;
                hexagonQuad.Traversal((index, coord) =>
                {
                    var positionOS = _matrix*_vertices[coord].m_Coord.ToWorld();
                    vertices.Add(positionOS);
                    uvs.Add(URender.IndexToQuadUV(index));
                });
                indices.Add(index0);
                indices.Add(index1);
                indices.Add(index3);
                indices.Add(index1);
                indices.Add(index2);
                indices.Add(index3);
            }
            
            m_Mesh.Clear();
            m_Mesh.SetVertices(vertices);
            m_Mesh.SetUVs(0,uvs);
            m_Mesh.SetIndices(indices,MeshTopology.Triangles,0,true);
            return m_Mesh;
        }
        #if UNITY_EDITOR
        public void DrawProceduralGizmos()
        {
            Gizmos.color = Color.white.SetAlpha(.5f);
            foreach (var vertex in m_ProceduralVertices)
                Gizmos.DrawSphere(vertex.ToWorld(),.3f);
            Gizmos.color = Color.white.SetAlpha(.2f);
            foreach (var triangle in m_ProceduralTriangles)
                Gizmos_Extend.DrawLines(triangle.ConstructIteratorArray(p=>p.ToWorld()));
            foreach (var quad in m_ProceduralQuads)
                Gizmos_Extend.DrawLines(quad.ConstructIteratorArray(p=>p.ToWorld()));
            
            Gizmos.color = Color.yellow;
            foreach (var vertex in m_RelaxVertices.Values)
                Gizmos.DrawSphere(vertex.m_Coord.ToWorld(),.2f);
            foreach (var triangle in m_RelaxTriangles)
                Gizmos_Extend.DrawLines(triangle.ConstructIteratorArray(p=>m_RelaxVertices[p].m_Coord.ToWorld()));
            foreach (var quad in m_RelaxQuads)
                Gizmos_Extend.DrawLines(quad.ConstructIteratorArray(p=>m_RelaxVertices[p].m_Coord.ToWorld()));
        }
        #endif
    }
}
